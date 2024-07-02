using System.Globalization;

namespace ChatLink.Client.Converters;

public class BooleanToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCurrentUser)
        {
            return isCurrentUser ? new Thickness(50, 0, 0, 10) : new Thickness(0, 0, 50, 10);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

