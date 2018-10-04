using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LFMSync
{
    public class DateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dto = value as DateTimeOffset?;
            if (dto == null)
                return null;
            var v = dto.Value;
            var diff = DateTimeOffset.Now - v;
            if (diff.TotalSeconds < 60)
                return $"{diff.TotalSeconds:0}s";
            else if (diff.TotalMinutes < 60)
                return $"{diff.Minutes}m";
            else if (diff.TotalHours < 24)
                return $"{diff.Hours}h";
            else if (diff.TotalDays < 30)
                return $"{diff.Days}d";
            else return "Old";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
