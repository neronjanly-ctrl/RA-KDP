using AnimalLabOnline.DataModels;
using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.EnumHelpers
{
    public static class AdminMethodExtensions
    {
        public static string GetName(this AdminMethod adminMethod)
        {
            return EnumAnnotationHelper<AdminMethod>.GetAttribute<DisplayAttribute>(adminMethod).Name;
        }

        public static string GetShortName(this AdminMethod adminMethod)
        {
            return EnumAnnotationHelper<AdminMethod>.GetAttribute<DisplayAttribute>(adminMethod).ShortName;
        }

        public static AdminMethod ParseAdminMethod(this string s)
        {
            return EnumByMatchParser<AdminMethod>.Parse(s);
        }

        public static bool TryParseAdminMethod(this string s, out AdminMethod value)
        {
            return EnumByMatchParser<AdminMethod>.TryParse(s, out value);
        }
    }
}
