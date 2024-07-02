using System.Globalization;

namespace ChatLink.Client.Converters;

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        CultureInfo englishCulture = new CultureInfo("en-US");

        if (value is DateTime dateTime)
        {
            var now = DateTime.Now;
            if (dateTime.Date == now.Date)
            {
                return dateTime.ToString("HH:mm");
            }

            if (dateTime.Date == now.AddDays(-1).Date)
            {
                return dateTime.ToString("MMMM d, HH:mm", englishCulture);
            }

            if (dateTime.Date >= now.AddDays(-7).Date)
            {
                return dateTime.ToString("dddd");
            }

            if (dateTime.Year == now.Year)
            {
                return dateTime.ToString("MMMM d", englishCulture);
            }

            return dateTime.ToString("MMMM d, yyyy", englishCulture);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


