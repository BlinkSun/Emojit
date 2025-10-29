using System;
using System.Collections.Generic;
using EmojitServer.Core.Design;
using Xunit;

namespace EmojitServer.Tests.Core;

/// <summary>
/// Provides coverage for the deterministic Emojit deck design implementation.
/// </summary>
public sealed class EmojitDesignTests
{
    /// <summary>
    /// Ensures that validation completes successfully for a well-formed design of prime order.
    /// </summary>
    [Fact]
    public void Validate_ShouldConfirmDeckIntegrity_ForPrimeOrder()
    {
        EmojitDesign design = EmojitDesign.Create(order: 3);

        design.Validate();
    }

    /// <summary>
    /// Confirms that the deterministic design correctly identifies the shared symbol between two distinct cards.
    /// </summary>
    [Fact]
    public void FindCommonSymbol_ShouldReturnSharedSymbol_ForDistinctCards()
    {
        EmojitDesign design = EmojitDesign.Create(order: 3);

        IReadOnlyList<int> sharedCard = design.GetCard(0);
        IReadOnlyList<int> secondCard = design.GetCard(5);

        HashSet<int> sharedSymbols = new(sharedCard);
        sharedSymbols.IntersectWith(secondCard);

        Assert.Single(sharedSymbols);

        int expectedSymbol = Assert.Single(sharedSymbols);
        int actualSymbol = design.FindCommonSymbol(0, 5);

        Assert.Equal(expectedSymbol, actualSymbol);
    }

    /// <summary>
    /// Verifies that comparing a card with itself triggers an exception as expected.
    /// </summary>
    [Fact]
    public void FindCommonSymbol_ShouldThrow_WhenComparingSameCard()
    {
        EmojitDesign design = EmojitDesign.Create(order: 3);

        Assert.Throws<InvalidOperationException>(() => design.FindCommonSymbol(2, 2));
    }
}
