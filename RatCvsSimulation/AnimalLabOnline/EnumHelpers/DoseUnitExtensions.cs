using AnimalLabOnline.DataModels;
using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.EnumHelpers
{
    public static class DoseUnitExtensions
    {
        public static string GetName(this DoseUnit doseUnit)
        {
            return EnumAnnotationHelper<DoseUnit>.GetAttribute<DisplayAttribute>(doseUnit).Name;
        }

        public static string GetShortName(this DoseUnit doseUnit)
        {
            return EnumAnnotationHelper<DoseUnit>.GetAttribute<DisplayAttribute>(doseUnit).ShortName;
        }

        public static DoseUnit ParseDoseUnit(this string s)
        {
            return EnumByMatchParser<DoseUnit>.Parse(s);
        }

        public static bool TryParseDoseUnit(this string s, out DoseUnit value)
        {
            return EnumByMatchParser<DoseUnit>.TryParse(s, out value);
        }
    }
}
