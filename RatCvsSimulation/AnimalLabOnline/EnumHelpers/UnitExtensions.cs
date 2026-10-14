using AnimalLabOnline.DataModels;
using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.EnumHelpers
{
    public static class UnitExtensions
    {
        public static string GetName(this Unit unit)
        {
            return EnumAnnotationHelper<Unit>.GetAttribute<DisplayAttribute>(unit).Name;
        }

        public static string GetShortName(this Unit unit)
        {
            return EnumAnnotationHelper<Unit>.GetAttribute<DisplayAttribute>(unit).ShortName;
        }

        public static Unit ParseUnit(this string s)
        {
            return EnumByMatchParser<Unit>.Parse(s);
        }

        public static bool TryParseUnit(this string s, out Unit value)
        {
            return EnumByMatchParser<Unit>.TryParse(s, out value);
        }
    }
}
