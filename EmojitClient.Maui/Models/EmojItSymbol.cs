namespace EmojitClient.Maui.Models;

/// <summary>
/// Represents a symbol on a Spot It card.
/// </summary>
public class EmojItSymbol
{
    public int Id { get; set; }
    public ImageSource? Image { get; set; }
    public View? View { get; set; }
    public string Label { get; set; } = string.Empty;
    public byte[] ImageBlob { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
}