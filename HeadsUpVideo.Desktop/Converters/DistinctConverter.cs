using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HeadsUpVideo.Desktop.Converters
{
    public class DistinctConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string lang)
        {
            var values = value as IEnumerable;
            if (values == null)
                return null;

            var distinctList = values.Cast<object>().Distinct();
            return distinctList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();

        }
    }
}
