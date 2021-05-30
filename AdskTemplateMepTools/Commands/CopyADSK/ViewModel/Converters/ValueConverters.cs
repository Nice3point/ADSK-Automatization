using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Markup;
using AdskTemplateMepTools.Utils;

namespace AdskTemplateMepTools.Commands.CopyADSK.ViewModel.Converters
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Enum @enum) return string.Empty;

            return !@enum.GetAttributeOfType<EnumMemberAttribute>(out var attribute) ? string.Empty : attribute.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}