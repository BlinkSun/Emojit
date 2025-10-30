namespace EmojitClient.Maui.Framework;

/// <summary>
/// Encapsulates the combinatorial design (Projective Plane of order n) behind Spot It!/Dobble.
/// Generates the full incidence structure (symbols and cards) using GF(n) arithmetic (n must be prime).
/// Symbols are represented by integers [0..(n^2+n)].
/// Cards are lists of symbol ids, each of size (n+1).
/// </summary>
public sealed class EmojItDesign
{
    private readonly int order;
    private readonly int symbolCount;
    private readonly int cardCount;
    private readonly int symbolsPerCard;
    private readonly List<List<int>> cards;

    /// <summary>
    /// Gets the order n of the projective plane (must be a prime).
    /// </summary>
    public int Order
    {
        get { return order; }
    }

    /// <summary>
    /// Gets the total count of distinct symbols (n^2 + n + 1).
    /// </summary>
    public int SymbolCount
    {
        get { return symbolCount; }
    }

    /// <summary>
    /// Gets the total count of cards (n^2 + n + 1).
    /// </summary>
    public int CardCount
    {
        get { return cardCount; }
    }

    /// <summary>
    /// Gets the number of symbols per card (n + 1).
    /// </summary>
    public int SymbolsPerCard
    {
        get { return symbolsPerCard; }
    }

    /// <summary>
    /// Creates a new design for a given prime order n and builds all cards.
    /// </summary>
    /// <param name="n">Prime order of the projective plane (e.g., 2, 3, 5, 7...).</param>
    /// <returns>A fully built SpotItDesign.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if n &lt; 2.</exception>
    /// <exception cref="ArgumentException">Thrown if n is not prime.</exception>
    public static EmojItDesign Create(int n)
    {
        if (n < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Order n must be >= 2.");
        }

        if (!IsPrime(n))
        {
            throw new ArgumentException("Order n must be prime for this construction (GF(n)).", nameof(n));
        }

        return new EmojItDesign(n);
    }

    /// <summary>
    /// Returns an immutable snapshot of a card's symbol ids by its index.
    /// </summary>
    /// <param name="cardIndex">Card index in [0..CardCount-1].</param>
    /// <returns>Read-only list of symbol ids.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If index is out of range.</exception>
    public IReadOnlyList<int> GetCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(cardIndex), "Card index out of range.");
        }

        return cards[cardIndex];
    }

    /// <summary>
    /// Attempts to get the unique common symbol between two cards (guaranteed by the design).
    /// </summary>
    /// <param name="cardIndexA">First card index.</param>
    /// <param name="cardIndexB">Second card index.</param>
    /// <returns>The unique common symbol id.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If any index is out of range.</exception>
    /// <exception cref="InvalidOperationException">If cards don't have exactly one common symbol.</exception>
    public int FindCommonSymbol(int cardIndexA, int cardIndexB)
    {
        if (cardIndexA < 0 || cardIndexA >= cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(cardIndexA), "Card index out of range.");
        }

        if (cardIndexB < 0 || cardIndexB >= cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(cardIndexB), "Card index out of range.");
        }

        if (cardIndexA == cardIndexB)
        {
            throw new InvalidOperationException("A card compared with itself has all symbols in common.");
        }

        HashSet<int> setA = [.. cards[cardIndexA]];
        int count = 0;
        int last = -1;

        foreach (int s in cards[cardIndexB])
        {
            if (setA.Contains(s))
            {
                count++;
                last = s;
            }
        }

        if (count != 1)
        {
            throw new InvalidOperationException("Design integrity broken: pair does not have exactly one common symbol.");
        }

        return last;
    }

    /// <summary>
    /// Enumerates all cards (read-only views). Not memory-copying, so do not modify internally.
    /// </summary>
    public IEnumerable<IReadOnlyList<int>> EnumerateCards()
    {
        foreach (List<int> c in cards)
        {
            yield return c;
        }
    }

    /// <summary>
    /// Validates the incidence structure: each pair of distinct cards intersects in exactly one symbol.
    /// </summary>
    /// <param name="message">Diagnostic message when validation fails; empty when success.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public bool Validate(out string message)
    {
        try
        {
            int total = cards.Count;
            for (int i = 0; i < total; i++)
            {
                HashSet<int> a = [.. cards[i]];
                for (int j = i + 1; j < total; j++)
                {
                    int inter = 0;
                    foreach (int s in cards[j])
                    {
                        if (a.Contains(s))
                        {
                            inter++;
                            if (inter > 1)
                            {
                                message = $"Invalid: cards {i} and {j} share more than 1 symbol.";
                                return false;
                            }
                        }
                    }

                    if (inter != 1)
                    {
                        message = $"Invalid: cards {i} and {j} share {inter} symbols (expected 1).";
                        return false;
                    }
                }
            }

            message = String.Empty;
            return true;
        }
        catch (Exception ex)
        {
            message = "Validation error: " + ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Lists all symbol ids [0..SymbolCount-1].
    /// </summary>
    public IReadOnlyList<int> ListAllSymbols()
    {
        List<int> result = new(symbolCount);
        for (int i = 0; i < symbolCount; i++)
        {
            result.Add(i);
        }
        return result;
    }

    /// <summary>
    /// Builds a human-friendly label for a symbol id. Replace with image lookup later if needed.
    /// </summary>
    /// <param name="symbolId">Symbol id in [0..SymbolCount-1].</param>
    /// <returns>String label.</returns>
    public string GetSymbolLabel(int symbolId)
    {
        if (symbolId < 0 || symbolId >= symbolCount)
        {
            return $"[invalid:{symbolId}]";
        }
        return $"Symbol {symbolId}";
    }

    /// <summary>
    /// Convenience: returns design statistics as a dictionary of key/value strings.
    /// </summary>
    public IDictionary<string, string> GetStats()
    {
        Dictionary<string, string> d = new()
        {
            ["Order(n)"] = order.ToString(),
            ["Symbols(n^2+n+1)"] = symbolCount.ToString(),
            ["Cards(n^2+n+1)"] = cardCount.ToString(),
            ["SymbolsPerCard(n+1)"] = symbolsPerCard.ToString(),
            ["Expected Common Symbol"] = "Exactly 1 for any pair of distinct cards"
        };
        return d;
    }

    // ======= Internals / Construction =======

    private EmojItDesign(int n)
    {
        order = n;
        symbolCount = n * n + n + 1;
        cardCount = symbolCount;
        symbolsPerCard = n + 1;
        cards = new List<List<int>>(cardCount);

        BuildDesign();
    }

    /// <summary>
    /// Checks if n is prime (deterministic simple check, adequate for small n).
    /// </summary>
    private static bool IsPrime(int n)
    {
        if (n <= 1) { return false; }
        if (n <= 3) { return true; }
        if (n % 2 == 0 || n % 3 == 0) { return false; }
        int i = 5;
        while (i * i <= n)
        {
            if (n % i == 0 || n % (i + 2) == 0)
            {
                return false;
            }
            i += 6;
        }
        return true;
    }

    /// <summary>
    /// Maps finite points (x,y) where x,y in GF(p) to ids [0..p^2-1].
    /// </summary>
    private int PointId(int x, int y)
    {
        return x * order + y; // 0..(p^2-1)
    }

    /// <summary>
    /// Maps the "point at infinity" associated with slope m (0..p-1) to ids [p^2..p^2+p-1].
    /// </summary>
    private int InfinitySlopeId(int m)
    {
        return order * order + m; // p^2 .. p^2 + (p-1)
    }

    /// <summary>
    /// Maps the special "vertical slope at infinity" point to id p^2 + p.
    /// </summary>
    private int InfinityVerticalId()
    {
        return order * order + order; // last id
    }

    /// <summary>
    /// Builds all cards using the canonical projective plane construction over GF(p):
    /// - For each slope m and intercept b, line y = m*x + b plus the slope-infinity point.
    /// - For each vertical x = a, all points with that x plus the vertical-infinity point.
    /// - The line at infinity: all infinity points (slopes + vertical).
    /// </summary>
    private void BuildDesign()
    {
        try
        {
            int p = order;

            // 1) Affine lines: y = m*x + b for all m,b in GF(p), plus infinity point for slope m.
            for (int m = 0; m < p; m++)
            {
                for (int b = 0; b < p; b++)
                {
                    List<int> card = new(symbolsPerCard);
                    for (int x = 0; x < p; x++)
                    {
                        int y = Mod(m * x + b, p);
                        card.Add(PointId(x, y));
                    }
                    card.Add(InfinitySlopeId(m));
                    cards.Add(card);
                }
            }

            // 2) Vertical lines: x = a for all a in GF(p), plus vertical infinity point.
            for (int a = 0; a < p; a++)
            {
                List<int> card = new(symbolsPerCard);
                for (int y = 0; y < p; y++)
                {
                    card.Add(PointId(a, y));
                }
                card.Add(InfinityVerticalId());
                cards.Add(card);
            }

            // 3) Line at infinity: all infinity slope points + vertical infinity.
            {
                List<int> card = new(symbolsPerCard);
                for (int m = 0; m < p; m++)
                {
                    card.Add(InfinitySlopeId(m));
                }
                card.Add(InfinityVerticalId());
                cards.Add(card);
            }

            if (cards.Count != cardCount)
            {
                throw new InvalidOperationException($"Internal build produced {cards.Count} cards, expected {cardCount}.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to build SpotIt design: " + ex.Message, ex);
        }
    }

    private static int Mod(int a, int p)
    {
        int r = a % p;
        if (r < 0) { r += p; }
        return r;
    }
}

/// <summary>
/// Provides utility methods to drive UI-agnostic "rounds" of the game using a SpotItDesign.
/// </summary>
public sealed class EmojItManager
{
    private readonly EmojItDesign design;
    private readonly Random random;

    /// <summary>
    /// Creates a manager bound to a given design.
    /// </summary>
    /// <param name="design">A valid SpotItDesign instance.</param>
    public EmojItManager(EmojItDesign design)
    {
        ArgumentNullException.ThrowIfNull(design);

        this.design = design;
        random = new Random();
    }

    /// <summary>
    /// Gets the underlying design (read-only usage recommended).
    /// </summary>
    public EmojItDesign Design
    {
        get { return design; }
    }

    /// <summary>
    /// Generates a random pair of distinct card indices and the unique common symbol.
    /// </summary>
    /// <param name="cardIndexA">Output first card index.</param>
    /// <param name="cardIndexB">Output second card index.</param>
    /// <param name="commonSymbol">Output unique common symbol id.</param>
    public void NextRandomPair(out int cardIndexA, out int cardIndexB, out int commonSymbol)
    {
        int total = design.CardCount;
        if (total < 2)
        {
            throw new InvalidOperationException("Design must contain at least 2 cards.");
        }

        cardIndexA = random.Next(0, total);
        do
        {
            cardIndexB = random.Next(0, total);
        } while (cardIndexB == cardIndexA);

        commonSymbol = design.FindCommonSymbol(cardIndexA, cardIndexB);
    }

    /// <summary>
    /// Computes the unique common symbol between two provided card indices.
    /// </summary>
    /// <param name="cardIndexA">First card index.</param>
    /// <param name="cardIndexB">Second card index.</param>
    /// <returns>Common symbol id.</returns>
    public int GetCommonSymbol(int cardIndexA, int cardIndexB)
    {
        return design.FindCommonSymbol(cardIndexA, cardIndexB);
    }
}