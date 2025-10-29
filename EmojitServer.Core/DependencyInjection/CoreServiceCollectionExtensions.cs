using System;
using Microsoft.Extensions.DependencyInjection;

namespace EmojitServer.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for registering core gameplay services.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core layer services to the dependency injection container.
    /// Currently acts as a placeholder for future registrations while keeping configuration consistent.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddCoreLayer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
