using System.Globalization;

namespace ChatLink.Client.Converters;

public class RandomColorConverter : IValueConverter
{
    private static readonly Random Random = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? value : Color.FromRgb(Random.Next(100,200), Random.Next(100,200), Random.Next(100,200));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
