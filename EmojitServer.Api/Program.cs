using EmojitServer.Api.Hubs;
using EmojitServer.Application.DependencyInjection;
using EmojitServer.Core.DependencyInjection;
using EmojitServer.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EmojitServer.Api;

internal static class Program
{
    private const string DefaultCorsPolicyName = "EmojitCorsPolicy";

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        WebApplication app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

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

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors(DefaultCorsPolicyName);
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hubs/game");
    }
}
