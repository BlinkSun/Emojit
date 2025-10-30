using EmojitClient.Maui.Models;

namespace EmojitClient.Maui.Framework.Selectors;

public class SymbolTemplateSelector : DataTemplateSelector
{
    public DataTemplate? PngTemplate { get; set; }
    public DataTemplate? SvgTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is EmojItSymbol symbol)
        {
            if (symbol.MimeType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase))
                return SvgTemplate ?? PngTemplate;
        }
        return PngTemplate;
    }
}
