using EmojitClient.Maui.Framework.Abstractions.Authentication;
using EmojitClient.Maui.Framework.Abstractions.Leaderboard;
using EmojitClient.Maui.Framework.Abstractions.Realtime;
using EmojitClient.Maui.Framework.Abstractions.Stats;
using EmojitClient.Maui.Framework.Options;
using EmojitClient.Maui.Framework.Services.Authentication;
using EmojitClient.Maui.Framework.Services.Leaderboard;
using EmojitClient.Maui.Framework.Services.Realtime;
using EmojitClient.Maui.Framework.Services.Stats;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace EmojitClient.Maui.Framework.DependencyInjection;

/// <summary>
/// Provides dependency injection helpers for configuring the Emojit client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required to communicate with the Emojit server.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Optional configuration source used to bind <see cref="EmojitApiOptions"/>.</param>
    /// <param name="configureOptions">Optional delegate used to further configure <see cref="EmojitApiOptions"/>.</param>
    /// <returns>The provided service collection for chaining.</returns>
    public static IServiceCollection AddEmojitClientServices(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<EmojitApiOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        OptionsBuilder<EmojitApiOptions> optionsBuilder = services.AddOptions<EmojitApiOptions>();

        if (configuration is not null)
        {
            optionsBuilder.Bind(configuration.GetSection(EmojitApiOptions.SectionName));
        }

        if (configureOptions is not null)
        {
            optionsBuilder.Configure(configureOptions);
        }

        optionsBuilder.PostConfigure(options => options.Validate());

        services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(ConfigureHttpClient);
        services.AddHttpClient<ILeaderboardApiClient, LeaderboardApiClient>(ConfigureHttpClient);
        services.AddHttpClient<IDesignStatsApiClient, DesignStatsApiClient>(ConfigureHttpClient);

        services.AddSingleton<IGameHubClient, GameHubClient>();

        return services;
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        EmojitApiOptions options = provider.GetRequiredService<IOptions<EmojitApiOptions>>().Value;
        options.Validate();

        client.BaseAddress = new Uri(options.BaseAddress, UriKind.Absolute);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
