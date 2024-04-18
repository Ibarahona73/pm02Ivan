using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pm02Ivan.Convertir
{
    public class base64Image : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ImageSource imageSource = null;
            if (value != null)
            {
                String base64 = (String)value;
                byte[] fotobyte = System.Convert.FromBase64String(base64);
                var stream = new MemoryStream(fotobyte);

                imageSource = ImageSource.FromStream(() => stream);
            }

            return imageSource;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}