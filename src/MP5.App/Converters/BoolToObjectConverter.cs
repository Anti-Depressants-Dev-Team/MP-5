using System.Globalization;

namespace MP5.App.Converters;

public class BoolToObjectConverter : IValueConverter
{
    public object? TrueObject { get; set; }
    public object? FalseObject { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isTrue = false;

        if (value is bool b)
        {
            isTrue = b;
        }
        else if (value is int i)
        {
            isTrue = i > 0;
        }

        if (parameter != null)
        {
            // Support "TrueValue|FalseValue" parameter format for quick usage
            var paramStr = parameter.ToString();
            if (paramStr != null && paramStr.Contains('|'))
            {
                var parts = paramStr.Split('|');
                return isTrue ? parts[0] : parts[1];
            }
        }
        
        return isTrue ? TrueObject : FalseObject;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
