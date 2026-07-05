using System.Security.Claims;
using Hively.Server.DbModel;
using Hively.Server.Hubs;
using Hively.Server.Infrastructure;
using Hively.Server.Repository;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services;
using Hively.Server.Services.Abstractions;
using Hively.Server.Services.Ingestion;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<HivelyContext>(options =>
        {
            options.UseNpgsql(connectionString);

            // Dev only: logs full SQL text (incl. parameter values) for every query — noisy
            // and can expose sensitive data, never enable outside Development.
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
        builder.Services.AddScoped<IProducerService, ProducerService>();
        builder.Services.AddScoped<IConsumerRepository, ConsumerRepository>();
        builder.Services.AddScoped<IConsumerService, ConsumerService>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<ISchemaRepository, SchemaRepository>();
        builder.Services.AddScoped<ISchemaService, SchemaService>();
        builder.Services.AddScoped<IMatchRepository, MatchRepository>();
        builder.Services.AddScoped<IMatchService, MatchService>();
        builder.Services.AddScoped<IMatchNotifier, MatchNotifier>();
        builder.Services.AddScoped<ITopicRepository, TopicRepository>();
        builder.Services.AddScoped<ITopicService, TopicService>();
        builder.Services.AddScoped<ITopicNotifier, TopicNotifier>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAutoMatchApplier, AutoMatchApplier>();

        builder.Services.Configure<MqttBrokerSettings>(builder.Configuration.GetSection("MqttBroker"));
        builder.Services.AddScoped<ITopicIngestionService, TopicIngestionService>();
        builder.Services.AddSingleton<IMqttStatusService, MqttStatusService>();
        builder.Services.AddHostedService<MqttIngestionService>();

        builder.Services.AddSignalR();

        // Each provider is opt-in via Authentication:{Provider}:Enabled (default
        // false) — a deployment only offering Entra, say, shouldn't even register
        // Google's handler, let alone need real credentials configured for it.
        var googleEnabled = builder.Configuration.GetValue<bool>("Authentication:Google:Enabled");
        var entraEnabled = builder.Configuration.GetValue<bool>("Authentication:Entra:Enabled");

        // A provider can be enabled in config without its ClientId/ClientSecret actually
        // being set yet (e.g. a fresh dev machine with no user-secrets configured) — only
        // register the handler once both are present. Registering it anyway with an empty
        // ClientId would make the handler's own option validation throw on every single
        // request (UseAuthentication resolves every remote-scheme handler per-request to
        // check its callback path, not just on challenge), taking the whole app down
        // instead of just leaving that one sign-in option unavailable. AuthController
        // reports this same "configured" state so the login page can show the button
        // disabled rather than hiding it outright.
        var googleConfigured = googleEnabled
            && !string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientId"])
            && !string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientSecret"]);
        var entraConfigured = entraEnabled
            && !string.IsNullOrEmpty(builder.Configuration["Authentication:Entra:ClientId"])
            && !string.IsNullOrEmpty(builder.Configuration["Authentication:Entra:ClientSecret"])
            && !string.IsNullOrEmpty(builder.Configuration["Authentication:Entra:TenantId"]);

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // API-only backend, no server-rendered login page — return plain status
            // codes instead of redirecting to a login/access-denied page that doesn't exist.
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        if (googleConfigured)
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // Restricts Google login to a single Google Workspace domain, mirroring
                // what Entra's TenantId already does — optional so local dev without a
                // Workspace domain configured still allows any Google account.
                var allowedHostedDomain = builder.Configuration["Authentication:Google:AllowedHostedDomain"];
                if (!string.IsNullOrEmpty(allowedHostedDomain))
                {
                    options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                    {
                        var uri = QueryHelpers.AddQueryString(ctx.RedirectUri, "hd", allowedHostedDomain);
                        ctx.Response.Redirect(uri);
                        return Task.CompletedTask;
                    };
                }

                options.Events.OnCreatingTicket = async ctx =>
                {
                    if (!string.IsNullOrEmpty(allowedHostedDomain))
                    {
                        var hostedDomain = ctx.User.TryGetProperty("hd", out var hd) ? hd.GetString() : null;
                        if (!string.Equals(hostedDomain, allowedHostedDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Fail($"Google account is not part of the '{allowedHostedDomain}' organization.");
                            return;
                        }
                    }

                    var email = ctx.Identity!.FindFirst(ClaimTypes.Email)?.Value
                        ?? throw new InvalidOperationException("Google login did not return an email claim.");
                    var subject = ctx.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? throw new InvalidOperationException("Google login did not return a subject claim.");

                    var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var user = await userService.UpsertFromExternalLoginAsync("google", subject, email, ctx.HttpContext.RequestAborted);
                    ctx.Identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                };
                options.Events.OnRemoteFailure = ctx =>
                {
                    ctx.HandleResponse();
                    ctx.Response.Redirect("/login?error=access_denied");
                    return Task.CompletedTask;
                };
            });
        }

        if (entraConfigured)
        {
            authBuilder.AddOpenIdConnect("Entra", options =>
            {
                var tenantId = builder.Configuration["Authentication:Entra:TenantId"];
                options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                options.ClientId = builder.Configuration["Authentication:Entra:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Entra:ClientSecret"];
                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.SaveTokens = false;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Scope.Add("email");
                options.Events.OnTokenValidated = async ctx =>
                {
                    var principal = ctx.Principal!;
                    var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.FindFirst("preferred_username")?.Value
                        ?? throw new InvalidOperationException("Entra login did not return an email claim.");
                    var subject = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? throw new InvalidOperationException("Entra login did not return a subject claim.");

                    var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var user = await userService.UpsertFromExternalLoginAsync("entra", subject, email, ctx.HttpContext.RequestAborted);
                    ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(ClaimTypes.Role, user.Role));
                };
            });
        }

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(options => { });
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseHttpLogging();
        }

        app.UseDefaultFiles();
        app.MapStaticAssets();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        app.MapHub<TopicHub>("/hubs/topic");
        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
