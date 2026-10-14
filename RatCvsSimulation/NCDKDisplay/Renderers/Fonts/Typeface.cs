using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace NCDK.Renderers.Fonts
{
    public class Typeface
    {
        public FontFamily FontFamily { get; set; }
        public FontStyle Style { get; set; }
        public FontWeight Weight { get; set; }
        public FontStretch Stretch { get; set; }

        public Typeface(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            FontFamily = fontFamily;
            Style = fontStyle;
            Weight = fontWeight;
            Stretch = fontStretch;
        }
    }
}
