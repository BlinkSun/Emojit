using System;
using EmojitServer.Application.Abstractions.Services;
using EmojitServer.Application.Contracts.Mapping;
using EmojitServer.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Mapster;

namespace EmojitServer.Application.DependencyInjection;

/// <summary>
/// Provides extension methods to register application layer services with the dependency injection container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the application services responsible for orchestrating game flows, leaderboard updates, and validation logic.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <returns>The provided service collection instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        ContractMappingConfiguration.Register(config);
        services.AddSingleton(config);

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IValidationService, ValidationService>();

        return services;
    }
}
