using HeadsUpVideo.Desktop.Models;
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
    public class DistinctCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string lang)
        {
            var values = value as IEnumerable;
            if (values == null)
                return null;

            var distinctList = values.Cast<BreakdownModel.Instance>().Select(x => x.Category).Distinct();
            return distinctList.OrderBy(s => s);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();

        }
    }
}
