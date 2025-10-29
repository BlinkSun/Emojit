using EmojitServer.Core.Design;
using EmojitServer.Core.GameModes;
using EmojitServer.Core.Managers;
using EmojitServer.Domain.Entities;
using EmojitServer.Domain.Enums;
using EmojitServer.Domain.ValueObjects;

namespace EmojitServer.Tests.Core;

/// <summary>
/// Exercises the primary flow of the <see cref="TowerGameManager"/> implementation.
/// </summary>
public sealed class TowerGameManagerTests
{
    /// <summary>
    /// Ensures that the tower manager resolves a round and records scoring when a player selects the correct symbol.
    /// </summary>
    [Fact]
    public void RegisterAttempt_ShouldResolveRound_WhenSymbolMatches()
    {
        TowerGameManager manager = new();
        EmojitDesign design = EmojitDesign.Create(order: 3);
        GameSession session = GameSession.Schedule(GameId.New(), GameMode.Tower, maxPlayers: 4, maxRounds: 3, DateTimeOffset.UtcNow);

        PlayerId playerOne = PlayerId.New();
        PlayerId playerTwo = PlayerId.New();
        session.AddParticipant(playerOne);
        session.AddParticipant(playerTwo);

        manager.Initialize(
            session,
            [playerOne, playerTwo],
            design,
            new GameModeConfiguration(maxRounds: 1, shuffleDeck: false));

        GameRoundState roundState = manager.StartNextRound(DateTimeOffset.UtcNow);

        Assert.Equal(1, roundState.RoundNumber);
        Assert.Equal(2, roundState.PlayerCardIndexes.Count);

        int playerOneCard = roundState.PlayerCardIndexes[playerOne];
        int matchingSymbol = design.FindCommonSymbol(roundState.SharedCardIndex, playerOneCard);
        int incorrectSymbol = GetAlternateSymbol(design, playerOneCard, matchingSymbol);

        RoundResolutionResult incorrectAttempt = manager.RegisterAttempt(playerTwo, incorrectSymbol, DateTimeOffset.UtcNow);
        Assert.False(incorrectAttempt.RoundResolved);
        Assert.True(incorrectAttempt.AttemptAccepted);
        Assert.Null(incorrectAttempt.ResolvingPlayerId);

        RoundResolutionResult resolution = manager.RegisterAttempt(playerOne, matchingSymbol, DateTimeOffset.UtcNow);

        Assert.True(resolution.RoundResolved);
        Assert.True(resolution.AttemptAccepted);
        Assert.Equal(playerOne, resolution.ResolvingPlayerId);
        Assert.Equal(playerOneCard, resolution.ResolvingPlayerCardIndex);
        Assert.Equal(matchingSymbol, resolution.MatchingSymbolId);

        ScoreSnapshot snapshot = manager.GetScoreSnapshot();
        Assert.Equal(1, snapshot.Scores[playerOne]);
        Assert.True(manager.IsGameOver);
    }

    private static int GetAlternateSymbol(EmojitDesign design, int cardIndex, int excludedSymbol)
    {
        IReadOnlyList<int> symbols = design.GetCard(cardIndex);
        foreach (int symbol in symbols)
        {
            if (symbol != excludedSymbol)
            {
                return symbol;
            }
        }

        throw new InvalidOperationException("Unable to locate an alternate symbol on the card for testing.");
    }
}
