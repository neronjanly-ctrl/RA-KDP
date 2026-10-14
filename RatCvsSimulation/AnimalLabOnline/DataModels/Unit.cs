using CommonTools;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    /// <summary>
    /// Units
    /// µ &micro; &#181; &#xB5; \xB5
    /// μ &mu; &#956; &#x3BC; \u03BC
    /// Μ &Mu; &#924; &#x39C; \u039C
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/International_System_of_Units#Prefixes"/>
    /// <seealso cref="https://en.wikipedia.org/wiki/Mu_(letter)"/>
    public enum Unit
    {
        [Display(Name = "", ShortName = "unknown unit")]
        Unknown,

        // units of length
        [Display(Name = "kilometer", ShortName = "km")] // = 1000 m
        [Match("km")]
        KiloMeter,
        [Display(Name = "meter", ShortName = "m")] // 1 m
        [Match("m")]
        Meter,
        [Display(Name = "decimeter", ShortName = "dm")] // = 0.1 m
        [Match("dm")]
        DeciMeter,
        [Display(Name = "centimeter", ShortName = "cm")] // = 0.01 m
        [Match("cm")]
        CentiMeter,
        [Display(Name = "millimeter", ShortName = "mm")] // = 0.001 m
        [Match("mm")]
        MilliMeter,
        [Display(Name = "micrometer", ShortName = "µm")] // = 0.001 mm
        [Match("[µμu]m", UseRegex = true)]
        MicroMeter,
        [Display(Name = "nanometer", ShortName = "nm")] // = 0.001 µm
        [Match("nm")]
        NanoMeter,
        [Display(Name = "picometer", ShortName = "pm")] // = 0.001 nm
        [Match("pm")]
        PicoMeter,

        // units of area
        [Display(Name = "square meter", ShortName = "m^2")] // = 1 m^2
        [Match("m2", "m^2")]
        SquareMeter,
        [Display(Name = "square decimeter", ShortName = "dm^2")] // = 0.01 m^2
        [Match("dm2", "dm^2")]
        SquareDeciMeter,
        [Display(Name = "square centimeter", ShortName = "cm^2")] // = 0.01 dm^2
        [Match("cm2", "cm^2")]
        SquareCentiMeter,
        [Display(Name = "square millimeter", ShortName = "mm^2")] // = 0.01 cm^2
        [Match("mm2", "mm^2")]
        SquareMilliMeter,
        [Display(Name = "square micrometer", ShortName = "µm^2")] // = 0.01 mm^2
        [Match(@"[µμu]m\^?2", UseRegex = true)]
        SquareMicroMeter,
        [Display(Name = "square nanometer", ShortName = "nm^2")] // = 0.01 µm^2
        [Match("nm2", "nm^2")]
        SquareNanoMeter,
        [Display(Name = "square picometer", ShortName = "pm^2")] // = 0.01 nm^2
        [Match("pm2", "pm^2")]
        SquarePicoMeter,

        // units of volume
        [Display(Name = "kiloliter", ShortName = "kL")] // = 1000 L = m^3
        [Match("kl", "kL", "m3", "m^3")]
        KiloLiter,
        [Display(Name = "liter", ShortName = "L")] // 1 L = dm^3
        [Match("l", "L", "dm3", "dm^3")]
        Liter,
        [Display(Name = "deciliter", ShortName = "dL")] // = 0.1 L
        [Match("dl", "dL")]
        DeciLiter,
        [Display(Name = "centiliter", ShortName = "cL")] // = 0.01 L
        [Match("cl", "cL")]
        CentiLiter,
        [Display(Name = "milliliter", ShortName = "mL")] // = 0.001 L = cm^3
        [Match("ml", "mL", "cm3", "cm^3")]
        MilliLiter,
        [Display(Name = "microliter", ShortName = "µL")] // = 0.001 mL = mm^3
        [Match(@"[µμu][lL]|mm\^?3", UseRegex = true)]
        MicroLiter,
        [Display(Name = "nanoliter", ShortName = "nL")] // = 0.001 µL
        [Match("nl", "nL")]
        NanoLiter,
        [Display(Name = "picoliter", ShortName = "pL")] // = 0.001 nL
        [Match("pl", "pL")]
        PicoLiter,

        // units of voltage
        [Display(Name = "kilovolt", ShortName = "kV")] // = 1000 V
        [Match("kv", "kV")]
        KiliVolt,
        [Display(Name = "volt", ShortName = "V")] // = 1 V
        [Match("v", "V")]
        Volt,
        [Display(Name = "millivolt", ShortName = "mV")] // = 0.001 V
        [Match("mv", "mV")]
        MilliVolt,
        [Display(Name = "microvolt", ShortName = "µV")] // = 0.001 mV
        [Match("[µμu][vV]", UseRegex = true)]
        MicroVolt,

        // units of weight
        [Display(Name = "kilogram", ShortName = "kg")] // 1000 g
        [Match("kg")]
        KiloGram,
        [Display(Name = "gram", ShortName = "g")] // = 1 g
        [Match("g")]
        Gram,
        [Display(Name = "milligram", ShortName = "mg")] // = 0.001 g
        [Match("mg")]
        MilliGram,
        [Display(Name = "microgram", ShortName = "µg")] // = 0.001 mg
        [Match("[µμu]g", UseRegex = true)]
        MicroGram,
        [Display(Name = "nanogram", ShortName = "ng")] // = 0.001 µg
        [Match("ng")]
        NanoGram,
        [Display(Name = "picogram", ShortName = "pg")] // = 0.001 ng
        [Match("pg")]
        PicoGram,

        // units of time
        [Display(Name = "day", ShortName = "day")] // = 24 hour
        [Match("day")]
        Day,
        [Display(Name = "hour", ShortName = "hour")] // = 60 min
        [Match("h", "hour")]
        Hour,
        [Display(Name = "minute", ShortName = "min")] // = 60 s
        [Match("min", "minute", "minutes")]
        Minute,
        [Display(Name = "second", ShortName = "s")] // = 1 s
        [Match("s", "sec", "second", "seconds")]
        Second,
        [Display(Name = "millisecond", ShortName = "ms")] // = 0.001 s
        [Match("ms", "msec")]
        MilliSecond,
        [Display(Name = "microsecond", ShortName = "µs")] // = 0.001 ms
        [Match("[µμu]s(ec)?", UseRegex = true)]
        MicroSecond,
        [Display(Name = "nanosecond", ShortName = "ns")] // = 0.001 µs
        [Match("ns", "nsec")]
        NanoSecond,
        [Display(Name = "picosecond", ShortName = "ps")] // = 0.001 ns
        [Match("ps", "psec")]
        PicoSecond,

        // units of amount of substance
        [Display(Name = "kilomole", ShortName = "kmol")] // = 1000 mol
        [Match("k(M|[mM]ol(es?)?)", UseRegex = true)]
        KiloMole,
        [Display(Name = "mole", ShortName = "mol")] // = 1 mol = 6.02214076e23 particles
        [Match("(M|[mM]ol(es?)?)", UseRegex = true)]
        Mole,
        [Display(Name = "millimole", ShortName = "mmol")] // = 0.001 mol
        [Match("m(M|[mM]ol(es?)?)", UseRegex = true)]
        MilliMole,
        [Display(Name = "micromole", ShortName = "µmol")] // = 0.001 mmol
        [Match("[µμu](M|[mM]ol(es?)?)", UseRegex = true)]
        MicroMole,
        [Display(Name = "nanomole", ShortName = "nmol")] // = 0.001 µmol
        [Match("n(M|[mM]ol(es?)?)", UseRegex = true)]
        NanoMole,
        [Display(Name = "picomole", ShortName = "pmol")] // = 0.001 nmol
        [Match("p(M|[mM]ol(es?)?)", UseRegex = true)]
        PicoMole,
        [Display(Name = "international unit", ShortName = "IU")] // = UI in Romance languages https://en.wikipedia.org/wiki/International_unit
        [Match("UI", "IU")]
        [Match(@"([iI]nternational)?[uU]nits?", UseRegex = true)]
        IU,
        [Display(Name = "kilounit", ShortName = "kU")] // = 1000 U
        [Match("kU")]
        KiloU,
        [Display(Name = "unit", ShortName = "U")] // = U is a special unit for enzyme
        [Match("U")]
        U,
        [Display(Name = "milliunit", ShortName = "mU")] // = 0.001 U
        [Match("mU")]
        MilliU,

        // units of catalytic activity
        [Display(Name = "kilokatal", ShortName = "kkat")] // = 1000 kat
        [Match("k[kK]at(al)?", UseRegex = true)]
        KiloKatal,
        [Display(Name = "katal", ShortName = "kat")] // = 1 kat
        [Match("[kK]at(al)?", UseRegex = true)]
        Katal,
        [Display(Name = "millikatal", ShortName = "mkat")] // = 0.001 kat
        [Match("m[kK]at(al)?", UseRegex = true)]
        MilliKatal,
        [Display(Name = "microkatal", ShortName = "µkat")] // = 0.001 mkat
        [Match("[µμu][kK]at(al)?", UseRegex = true)]
        MicroKatal,
        [Display(Name = "nanokatal", ShortName = "nkat")] // = 0.001 µkat
        [Match("n[kK]at(al)?", UseRegex = true)]
        NanoKatal,

        // units of amount or ratio
        [Display(Name = "percent", ShortName = "%")] // = 0.01
        [Match("%")]
        Percent,
        [Display(Name = "permille", ShortName = "‰")] // = 0.001
        [Match("‰")]
        Permille,
        [Display(Name = "permyriad", ShortName = "‱")] // = 0.0001
        [Match("‱")]
        Permyriad,
        [Display(Name = "count", ShortName = "cnt")] // = 1
        [Match("cnt", "count", "counts", "number")]
        Count,
        [Display(Name = "ratio", ShortName = "ratio")] // https://en.wikipedia.org/wiki/Fold_change
        [Match("foldof", "normalizedto")]
        Ratio,

        // units of pressure
        [Display(Name = "millimeter of mercury", ShortName = "mmHg")] // = 1/760 standard atmospheric pressure = 101325 / 760 pascals = 133.322368421053 pascals
        [Match("mmHg")]
        MilliMeterOfMercury,

        // units of temperature
        [Display(Name = "degrees celcius", ShortName = "°C")]
        [Match("°C")]
        CelciusDegree,
        [Display(Name = "degrees fahrenheit", ShortName = "°F")]
        [Match("°F")]
        FahrenheitDegree,
        [Display(Name = "degree kelvin", ShortName = "K")]
        [Match("K")]
        KelvinDegree,

        [Display(Name = "beats", ShortName = "beats")]
        [Match("beat", "beats")]
        Beats,
        [Display(Name = "beats per minute", ShortName = "beats/min")]
        [Match("bpm")] // = beats/min
        BeatsPerMin,
    }
}
