using AnimalLabOnline.DataModels;
using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.EnumHelpers
{
    public static class TreatmentTypeExtensions
    {
        public static string GetName(this TreatmentType treatmentType)
        {
            return EnumAnnotationHelper<TreatmentType>.GetAttribute<DisplayAttribute>(treatmentType).Name;
        }

        public static string GetShortName(this TreatmentType treatmentType)
        {
            return EnumAnnotationHelper<TreatmentType>.GetAttribute<DisplayAttribute>(treatmentType).ShortName;
        }

        public static TreatmentType ParseTreatmentType(this string s)
        {
            return EnumByMatchParser<TreatmentType>.Parse(s);
        }

        public static bool TryParseTreatmentType(this string s, out TreatmentType value)
        {
            return EnumByMatchParser<TreatmentType>.TryParse(s, out value);
        }
    }
}
