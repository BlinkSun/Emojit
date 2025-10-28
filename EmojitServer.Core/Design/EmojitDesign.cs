using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EmojitServer.Core.Design;

/// <summary>
/// Provides deterministic card generation for the Emojit game, ensuring that any pair of cards shares exactly one symbol.
/// </summary>
public sealed class EmojitDesign
{
    private readonly ReadOnlyCollection<ReadOnlyCollection<int>> _cards;

    private EmojitDesign(int order, IReadOnlyList<ReadOnlyCollection<int>> cards)
    {
        Order = order;
        SymbolCount = order * order + order + 1;
        SymbolsPerCard = order + 1;
        CardCount = SymbolCount;
        _cards = new ReadOnlyCollection<ReadOnlyCollection<int>>(cards.ToList());
    }

    /// <summary>
    /// Gets the order of the finite projective plane used for the design.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the total number of cards produced by the design.
    /// </summary>
    public int CardCount { get; }

    /// <summary>
    /// Gets the total number of unique symbols contained in the design.
    /// </summary>
    public int SymbolCount { get; }

    /// <summary>
    /// Gets the number of symbols that appear on each card.
    /// </summary>
    public int SymbolsPerCard { get; }

    /// <summary>
    /// Creates a new deterministic Emojit design using the specified projective plane order.
    /// </summary>
    /// <param name="order">The order of the finite projective plane (must be a prime number).</param>
    /// <returns>An initialized <see cref="EmojitDesign"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided order is invalid.</exception>
    public static EmojitDesign Create(int order)
    {
        if (order < 2)
        {
            throw new ArgumentException("Order must be at least 2.", nameof(order));
        }

        if (!IsPrime(order))
        {
            throw new ArgumentException("Order must be a prime number to guarantee a valid design.", nameof(order));
        }

        try
        {
            IReadOnlyList<ReadOnlyCollection<int>> cards = BuildDeck(order);
            return new EmojitDesign(order, cards);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to construct the Emojit design deck.", ex);
        }
    }

    /// <summary>
    /// Gets the immutable list of symbols for the card at the specified index.
    /// </summary>
    /// <param name="cardIndex">The zero-based index of the card.</param>
    /// <returns>A read-only collection representing the card.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the card index is outside the deck bounds.</exception>
    public IReadOnlyList<int> GetCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= _cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(cardIndex), cardIndex, "Card index is outside the bounds of the deck.");
        }

        return _cards[cardIndex];
    }

    /// <summary>
    /// Finds the single common symbol shared between the two specified cards.
    /// </summary>
    /// <param name="firstCardIndex">The zero-based index of the first card.</param>
    /// <param name="secondCardIndex">The zero-based index of the second card.</param>
    /// <returns>The symbol identifier shared by the two cards.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any provided card index is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the cards do not share exactly one symbol.</exception>
    public int FindCommonSymbol(int firstCardIndex, int secondCardIndex)
    {
        if (firstCardIndex < 0 || firstCardIndex >= _cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(firstCardIndex), firstCardIndex, "First card index is outside the bounds of the deck.");
        }

        if (secondCardIndex < 0 || secondCardIndex >= _cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(secondCardIndex), secondCardIndex, "Second card index is outside the bounds of the deck.");
        }

        if (firstCardIndex == secondCardIndex)
        {
            throw new InvalidOperationException("A card cannot be compared with itself when searching for a common symbol.");
        }

        ReadOnlyCollection<int> firstCard = _cards[firstCardIndex];
        ReadOnlyCollection<int> secondCard = _cards[secondCardIndex];

        try
        {
            HashSet<int> lookup = new HashSet<int>(firstCard);
            int? commonSymbol = null;

            foreach (int symbol in secondCard)
            {
                if (!lookup.Contains(symbol))
                {
                    continue;
                }

                if (commonSymbol.HasValue)
                {
                    throw new InvalidOperationException("Cards share more than one common symbol, violating design invariants.");
                }

                commonSymbol = symbol;
            }

            return commonSymbol ?? throw new InvalidOperationException("Cards do not share a common symbol, violating design invariants.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Failed to compute the common symbol between cards.", ex);
        }
    }

    /// <summary>
    /// Validates the internal integrity of the design, ensuring that all invariants hold.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the design contains invalid cards or mismatched symbols.</exception>
    public void Validate()
    {
        try
        {
            for (int cardIndex = 0; cardIndex < _cards.Count; cardIndex++)
            {
                ReadOnlyCollection<int> card = _cards[cardIndex];

                if (card.Count != SymbolsPerCard)
                {
                    throw new InvalidOperationException($"Card at index {cardIndex} does not contain the expected number of symbols.");
                }

                HashSet<int> uniquenessCheck = new HashSet<int>(card);
                if (uniquenessCheck.Count != SymbolsPerCard)
                {
                    throw new InvalidOperationException($"Card at index {cardIndex} contains duplicate symbols.");
                }

                for (int otherIndex = cardIndex + 1; otherIndex < _cards.Count; otherIndex++)
                {
                    ReadOnlyCollection<int> otherCard = _cards[otherIndex];

                    int sharedCount = 0;
                    foreach (int symbol in otherCard)
                    {
                        if (!uniquenessCheck.Contains(symbol))
                        {
                            continue;
                        }

                        sharedCount++;
                        if (sharedCount > 1)
                        {
                            throw new InvalidOperationException($"Cards at indexes {cardIndex} and {otherIndex} share more than one symbol.");
                        }
                    }

                    if (sharedCount == 0)
                    {
                        throw new InvalidOperationException($"Cards at indexes {cardIndex} and {otherIndex} do not share a symbol.");
                    }
                }
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Validation of the Emojit design failed due to an unexpected error.", ex);
        }
    }

    /// <summary>
    /// Provides diagnostic statistics about the design.
    /// </summary>
    /// <returns>A <see cref="EmojitDesignStats"/> instance containing aggregated information.</returns>
    public EmojitDesignStats GetStats()
    {
        return new EmojitDesignStats(Order, CardCount, SymbolCount, SymbolsPerCard);
    }

    private static IReadOnlyList<ReadOnlyCollection<int>> BuildDeck(int order)
    {
        int symbolsPerCard = order + 1;
        int totalCards = order * order + order + 1;
        List<ReadOnlyCollection<int>> cards = new List<ReadOnlyCollection<int>>(totalCards);

        // First card contains symbols 0..order
        int[] firstCard = new int[symbolsPerCard];
        for (int i = 0; i < symbolsPerCard; i++)
        {
            firstCard[i] = i;
        }

        cards.Add(Array.AsReadOnly(firstCard));

        // Cards containing the zeroth symbol paired with unique symbol groups.
        for (int i = 0; i < order; i++)
        {
            int[] card = new int[symbolsPerCard];
            card[0] = 0;
            for (int j = 0; j < order; j++)
            {
                card[j + 1] = order + 1 + (i * order) + j;
            }

            cards.Add(Array.AsReadOnly(card));
        }

        // Remaining cards built using finite projective plane construction.
        for (int a = 0; a < order; a++)
        {
            for (int b = 0; b < order; b++)
            {
                int[] card = new int[symbolsPerCard];
                card[0] = a + 1;

                for (int c = 0; c < order; c++)
                {
                    int symbol = order + 1 + (c * order) + ((a * c + b) % order);
                    card[c + 1] = symbol;
                }

                cards.Add(Array.AsReadOnly(card));
            }
        }

        return cards;
    }

    private static bool IsPrime(int value)
    {
        if (value < 2)
        {
            return false;
        }

        if (value == 2)
        {
            return true;
        }

        if (value % 2 == 0)
        {
            return false;
        }

        int limit = (int)Math.Sqrt(value);
        for (int divisor = 3; divisor <= limit; divisor += 2)
        {
            if (value % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Represents aggregated statistics about an <see cref="EmojitDesign"/> instance.
/// </summary>
/// <param name="Order">The order of the finite projective plane.</param>
/// <param name="CardCount">The number of cards generated.</param>
/// <param name="SymbolCount">The number of unique symbols used.</param>
/// <param name="SymbolsPerCard">The count of symbols placed on each card.</param>
public readonly record struct EmojitDesignStats(int Order, int CardCount, int SymbolCount, int SymbolsPerCard);
