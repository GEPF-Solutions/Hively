using Hively.Server.DbModel;
using Hively.Server.Repository;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services;
using Hively.Server.Services.Abstractions;
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

        app.UseAuthorization();


        app.MapControllers();
        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
