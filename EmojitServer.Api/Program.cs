using EmojitServer.Api.Configuration;
using EmojitServer.Api.Hubs;
using EmojitServer.Api.Middleware;
using EmojitServer.Application.DependencyInjection;
using EmojitServer.Application.Configuration;
using EmojitServer.Core.DependencyInjection;
using EmojitServer.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace EmojitServer.Api;

internal static class Program
{

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables("EMOJIT_");

        ConfigureLogging(builder);

        ConfigureServices(builder.Services, builder.Configuration);

        WebApplication app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddOptions<GameDefaultsOptions>()
            .Bind(configuration.GetSection(GameDefaultsOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddCors();

        services.AddControllers();
        services.AddSignalR();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Emojit Server API",
                Version = "v1",
                Description = "ASP.NET Core host powering the Emojit multiplayer experience.",
            });
        });

        services.AddCoreLayer();
        services.AddApplicationLayer();
        services.AddInfrastructureLayer(configuration);
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff zzz ";
            options.IncludeScopes = true;
            options.SingleLine = true;
        });
        builder.Logging.AddDebug();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;

        app.UseRouting();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseCors(policy => ConfigureCorsPolicy(policy, corsOptions));
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hubs/game");
    }

    private static void ConfigureCorsPolicy(CorsPolicyBuilder policyBuilder, CorsOptions corsOptions)
    {
        if (corsOptions.AllowedOrigins.Count == 0)
        {
            policyBuilder.AllowAnyOrigin();
        }
        else
        {
            policyBuilder.WithOrigins(corsOptions.AllowedOrigins.ToArray());
        }

        policyBuilder
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (corsOptions.AllowCredentials && corsOptions.AllowedOrigins.Count > 0)
        {
            policyBuilder.AllowCredentials();
        }
    }
}
