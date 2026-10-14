using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    public enum DoseUnit
    {
        [Display(Name = "unknown unit", ShortName = "")]
        Unknown,

        [Display(Name = "gram per kilogram of body weight", ShortName = "g/kg")]
        [Match("g/kg")]
        GPerKg,

        [Display(Name = "milligram per kilogram of body weight", ShortName = "mg/kg")]
        [Match("mg/kg")]
        MgPerKg,

        [Display(Name = "microgram per kilogram of body weight", ShortName = "µg/kg")]
        [Match("[µμu]g/kg", UseRegex = true)]
        UgPerKg,

        [Display(Name = "nanogram per kilogram of body weight", ShortName = "ng/kg")]
        [Match("ng/kg")]
        NgPerKg,

        [Display(Name = "milligram per kilogram of body weight per day", ShortName = "mg/kg/d")]
        [Match("mg/kg/d")]
        MgPerKgPerDay,
    }
}
