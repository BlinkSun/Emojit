using System.Collections.ObjectModel;

namespace EmojitClient.Maui.Models;

/// <summary>
/// Represents a card containing a collection of symbols.
/// </summary>
public class EmojItCard
{
    public int Id { get; set; }
    public ObservableCollection<EmojItSymbol> Symbols { get; set; } = [];
}