using System;
using System.Windows;
using System.Windows.Data;

namespace xTrade
{
    public class GroupToBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var group = value as PeopleInGroup;
            object result = null;

            if (group != null)
            {
                result = @group.Count == 0 ? Application.Current.Resources["PhoneChromeBrush"] : Application.Current.Resources["PhoneAccentBrush"];
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
