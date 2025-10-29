using EmojitServer.Application.Contracts.Leaderboard;
using EmojitServer.Application.Contracts.Realtime;
using EmojitServer.Application.Contracts.Stats;
using EmojitServer.Application.Services.Models;
using EmojitServer.Core.Design;
using EmojitServer.Core.GameModes;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.ValueObjects;
using Mapster;

namespace EmojitServer.Application.Contracts.Mapping;

/// <summary>
/// Provides Mapster configuration to translate domain models into contract payloads.
/// </summary>
public static class ContractMappingConfiguration
{
    private static bool _configured;
    private static readonly object SyncRoot = new();

    /// <summary>
    /// Registers mapping configuration for contract payloads.
    /// </summary>
    /// <param name="config">The Mapster configuration instance to populate.</param>
    public static void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_configured)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_configured)
            {
                return;
            }

            config.NewConfig<GameSession, GameCreatedResponse>()
                .Map(dest => dest.GameId, src => src.Id.Value.ToString())
                .Map(dest => dest.Mode, src => src.Mode)
                .Map(dest => dest.MaxPlayers, src => src.MaxPlayers)
                .Map(dest => dest.MaxRounds, src => src.MaxRounds);

            config.NewConfig<LeaderboardEntry, LeaderboardEntryDto>()
                .Map(dest => dest.PlayerId, src => src.PlayerId.Value)
                .Map(dest => dest.TotalPoints, src => src.TotalPoints)
                .Map(dest => dest.GamesPlayed, src => src.GamesPlayed)
                .Map(dest => dest.GamesWon, src => src.GamesWon)
                .Map(dest => dest.LastUpdatedAtUtc, src => src.LastUpdatedAtUtc);

            config.NewConfig<EmojitDesignStats, DesignStatsDto>();

            config.NewConfig<(GameId GameId, GameRoundState Round), RoundStartEvent>()
                .Map(dest => dest.GameId, src => src.GameId.Value.ToString())
                .Map(dest => dest.RoundNumber, src => src.Round.RoundNumber)
                .Map(dest => dest.SharedCardIndex, src => src.Round.SharedCardIndex)
                .Map(dest => dest.PlayerCardIndexes, src => ConvertScores(src.Round.PlayerCardIndexes))
                .Map(dest => dest.StartedAtUtc, src => src.Round.StartedAtUtc);

            config.NewConfig<(GameId GameId, GameAttemptResult Attempt), RoundResultEvent>()
                .Map(dest => dest.GameId, src => src.GameId.Value.ToString())
                .Map(dest => dest.RoundResolved, src => src.Attempt.Resolution.RoundResolved)
                .Map(dest => dest.AttemptAccepted, src => src.Attempt.Resolution.AttemptAccepted)
                .Map(dest => dest.ResolvingPlayerId, src => src.Attempt.Resolution.ResolvingPlayerId.HasValue
                    ? src.Attempt.Resolution.ResolvingPlayerId.Value.Value.ToString()
                    : null)
                .Map(dest => dest.ResolvingPlayerCardIndex, src => src.Attempt.Resolution.ResolvingPlayerCardIndex)
                .Map(dest => dest.MatchingSymbolId, src => src.Attempt.Resolution.MatchingSymbolId)
                .Map(dest => dest.RoundNumber, src => src.Attempt.Resolution.RoundNumber)
                .Map(dest => dest.ProcessedAtUtc, src => src.Attempt.Resolution.ProcessedAtUtc)
                .Map(dest => dest.ResolutionDurationMilliseconds, src => src.Attempt.Resolution.ResolutionDuration.HasValue
                    ? src.Attempt.Resolution.ResolutionDuration.Value.TotalMilliseconds
                    : (double?)null)
                .Map(dest => dest.Scores, src => src.Attempt.ScoreSnapshot == null
                    ? null
                    : ConvertScores(src.Attempt.ScoreSnapshot.Scores))
                .Map(dest => dest.GameCompleted, src => src.Attempt.GameCompleted);

            config.NewConfig<(GameId GameId, ScoreSnapshot Snapshot), GameOverEvent>()
                .Map(dest => dest.GameId, src => src.GameId.Value.ToString())
                .Map(dest => dest.FinalScores, src => ConvertScores(src.Snapshot.Scores))
                .Map(dest => dest.CompletedAtUtc, src => src.Snapshot.CapturedAtUtc);

            _configured = true;
        }
    }

    private static IReadOnlyDictionary<string, int> ConvertScores(IReadOnlyDictionary<PlayerId, int> scores)
    {
        return scores.ToDictionary(pair => pair.Key.Value.ToString(), pair => pair.Value);
    }
}
