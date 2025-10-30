using EmojitClient.Maui.Models;
using System.Globalization;

namespace EmojitClient.Maui.Framework.Converters;

public class SymbolToViewConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<EmojItSymbol> s)
        {
            return s.Select(x => ImageSource.FromStream(() => new MemoryStream(x.ImageBlob)));
            //View view = CreateViewFromSymbol(s);
            //return view;
        }

        return null!;
    }

    //private static View CreateViewFromSymbol(EmojItSymbol s)
    //{
    //    if (s.ImageBlob?.Length > 0)
    //    {
    //        return new Image
    //        {
    //            Source = ImageSource.FromStream(() => new MemoryStream(s.ImageBlob)),
    //            Aspect = Aspect.AspectFit
    //        };
    //    }

    //    return new Label { Text = s.Label, TextColor = Colors.White };
    //}

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

