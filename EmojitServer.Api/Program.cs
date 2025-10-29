using EmojitServer.Api.Configuration;
using EmojitServer.Api.Hubs;
using EmojitServer.Api.Middleware;
using EmojitServer.Application.Configuration;
using EmojitServer.Application.DependencyInjection;
using EmojitServer.Core.DependencyInjection;
using EmojitServer.Infrastructure.DependencyInjection;
using EmojitServer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Threading.RateLimiting;

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
        services.AddOptions<Configuration.CorsOptions>()
            .Bind(configuration.GetSection(Configuration.CorsOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddOptions<GameDefaultsOptions>()
            .Bind(configuration.GetSection(GameDefaultsOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddOptions<SignalRMessageOptions>()
            .Bind(configuration.GetSection(SignalRMessageOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddCors();

        services.AddControllers();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();
        services.AddAuthorization();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigurator>();
        services.AddHealthChecks()
            .AddDbContextCheck<EmojitDbContext>(); // <-- Fixed line

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiterOptions.OnRejected = (context, _) =>
            {
                HttpContext httpContext = context.HttpContext;
                try
                {
                    RateLimitingOptions limiterOptions = httpContext.RequestServices
                        .GetRequiredService<IOptions<RateLimitingOptions>>()
                        .Value;

                    string retryAfter = limiterOptions.WindowInSeconds
                        .ToString(CultureInfo.InvariantCulture);
                    httpContext.Response.Headers.RetryAfter = retryAfter;
                }
                catch (Exception exception)
                {
                    ILogger logger = httpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(Program));
                    logger.LogError(exception, "Failed to compute rate limiting retry-after header.");
                }

                return ValueTask.CompletedTask;
            };

            rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                try
                {
                    RateLimitingOptions limiterOptions = httpContext.RequestServices
                        .GetRequiredService<IOptions<RateLimitingOptions>>()
                        .Value;

                    string partitionKey = ResolveClientPartitionKey(httpContext);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limiterOptions.PermitLimit,
                            QueueProcessingOrder = limiterOptions.QueueProcessingOrder,
                            QueueLimit = limiterOptions.QueueLimit,
                            Window = limiterOptions.ToWindowTimeSpan(),
                        });
                }
                catch (Exception exception)
                {
                    ILogger logger = httpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(Program));
                    logger.LogError(exception, "Failed to resolve rate limiter configuration. Allowing request without throttling.");

                    string fallbackPartitionKey = ResolveClientPartitionKey(httpContext);
                    return RateLimitPartition.GetNoLimiter(fallbackPartitionKey);
                }
            });
        });

        // Use the registered IOptions<SignalRMessageOptions>
        services.AddOptions<HubOptions>()
            .PostConfigure<IOptions<SignalRMessageOptions>>((hubOptions, messageOptions) =>
            {
                hubOptions.MaximumReceiveMessageSize = messageOptions.Value.MaximumReceiveMessageSizeInBytes;
            });

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

        Configuration.CorsOptions corsOptions = app.Services.GetRequiredService<IOptions<Configuration.CorsOptions>>().Value;

        app.UseRouting();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseCors(policy => ConfigureCorsPolicy(policy, corsOptions));
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/healthz");
        app.MapHub<GameHub>("/hubs/game");
    }

    private static void ConfigureCorsPolicy(CorsPolicyBuilder policyBuilder, Configuration.CorsOptions corsOptions)
    {
        ArgumentNullException.ThrowIfNull(policyBuilder);
        ArgumentNullException.ThrowIfNull(corsOptions);

        policyBuilder
            .WithOrigins(corsOptions.AllowedOrigins.ToArray())
            .WithMethods(corsOptions.AllowedMethods.ToArray())
            .WithHeaders(corsOptions.AllowedHeaders.ToArray());

        if (corsOptions.AllowCredentials)
        {
            policyBuilder.AllowCredentials();
        }
        else
        {
            policyBuilder.DisallowCredentials();
        }
    }

    private static string ResolveClientPartitionKey(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        try
        {
            string? authenticatedName = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User?.Identity?.Name
                : null;

            if (!string.IsNullOrWhiteSpace(authenticatedName))
            {
                return $"user:{authenticatedName}";
            }

            string? remoteAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteAddress))
            {
                return $"ip:{remoteAddress}";
            }
        }
        catch (Exception exception)
        {
            ILogger logger = httpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(Program));
            logger.LogWarning(exception, "Unable to resolve rate limiter partition key from request context.");
        }

        return "anonymous";
    }
}
