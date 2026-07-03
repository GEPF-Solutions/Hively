using System.Security.Claims;
using Hively.Server.DbModel;
using Hively.Server.Repository;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
        builder.Services.AddScoped<IRuleRepository, RuleRepository>();
        builder.Services.AddScoped<IRuleService, RuleService>();
        builder.Services.AddScoped<ITopicRepository, TopicRepository>();
        builder.Services.AddScoped<ITopicService, TopicService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddAuthentication(options =>
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
        })
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Events.OnCreatingTicket = async ctx =>
            {
                var email = ctx.Identity!.FindFirst(ClaimTypes.Email)?.Value
                    ?? throw new InvalidOperationException("Google login did not return an email claim.");
                var subject = ctx.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new InvalidOperationException("Google login did not return a subject claim.");

                var userService = ctx.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var user = await userService.UpsertFromExternalLoginAsync("google", subject, email, ctx.HttpContext.RequestAborted);
                ctx.Identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            };
        })
        .AddOpenIdConnect("Entra", options =>
        {
            var tenantId = builder.Configuration["Authentication:Entra:TenantId"];
            options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            options.ClientId = builder.Configuration["Authentication:Entra:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:Entra:ClientSecret"];
            options.ResponseType = "code";
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
        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
