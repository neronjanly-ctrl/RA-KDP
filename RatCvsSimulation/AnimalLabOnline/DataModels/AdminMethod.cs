using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    /// <summary>
    /// Administration Method
    /// </summary>
    /// <seealso cref="https://www.clodrosome.com/animal-injection/"/>
    public enum AdminMethod
    {
        [Display(Name = "Unknown Administration", ShortName = "")]
        Unknown,

        [Display(Name = "Intravenous Bolus", ShortName = "i.v.")]
        [Match("i.v.", "i.v", "iv", "iv bolus", CaseSensitive = false)]
        IntravenousBolus, // 静脉推注

        [Display(Name = "Intravenous Infusion", ShortName = "i.v.i.")]
        [Match("i.v.i.", "i.v.i", "ivi", "iv infusion", CaseSensitive = false)]
        IntravenousInfusion, // 静脉滴注

        [Display(Name = "Intraperitoneal Injection", ShortName = "i.p.")]
        [Match("i.p.", "i.p", "ip", "ip injection", CaseSensitive = false)]
        IntraperitonealInjection, // 腹腔注射

        [Display(Name = "Intragastric Infusion", ShortName = "i.g.")]
        [Match("i.g.", "i.g", "ig", "ig infusion", CaseSensitive = false)]
        IntragastricInfusion, // 灌胃

        [Display(Name = "Oral", ShortName = "p.o.")]
        [Match("p.o.", "p.o", "po", CaseSensitive = false)]
        Oral, // 口服

        [Display(Name = "Subcutaneous Injection", ShortName = "s.c.")]
        [Match("s.c.", "s.c", "sc", "sc injection")]
        SubcutaneousInjection, // 皮下注射

        [Display(Name = "Intramuscular Injection", ShortName = "i.m.")]
        [Match("i.m.", "i.m", "im", "im injection", CaseSensitive = false)]
        IntramuscularInjection, // 肌肉注射

        [Display(Name = "Intravitreal Injection")]
        IntravitrealInjection, // 玻璃体注射

        [Display(Name = "Intracerebral Injection")]
        IntracerebralInjection, // 脑内注射
    }
}