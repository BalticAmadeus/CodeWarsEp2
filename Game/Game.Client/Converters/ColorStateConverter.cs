using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Game.AdminClient.ViewModels;

namespace Game.AdminClient.Converters
{
    public class ColorStateConverter : IValueConverter
    {
        public static Color GetPlayerColor(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0:
                    return Colors.OrangeRed;
                case 1:
                    return Colors.LawnGreen;
                case 2:
                    return Colors.Yellow;
                case 3:
                    return Colors.Teal;
                case 4:
                    return Colors.Magenta;
                case 5:
                    return Colors.Brown;
                case 6:
                    return Colors.Blue;
                case 7:
                    return Colors.Silver;
                default:
                    return Colors.Gray;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int code;
            if (value is int)
                code = (int)value;
            else
                code = -1;

            return new SolidColorBrush(GetPlayerColor(code));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}