namespace EmojitClient.Maui.Data.Entities;

/// <summary>
/// Represents a visual symbol used in the SpotIt game.
/// Can be an emoji or an image blob.
/// </summary>
public class Symbol
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public required string Label { get; set; }
    public required string Emoji { get; set; }
    public required byte[] ImageBlob { get; set; }
    public required string MimeType { get; set; }
}