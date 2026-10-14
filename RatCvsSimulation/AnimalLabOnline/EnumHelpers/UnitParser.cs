using AnimalLabOnline.DataModels;
using CommonTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimalLabOnline.EnumHelpers
{
    public static class UnitParser
    {
        static (Unit unit, int exponent, string speciality) ParseAtomUnitField(this string field)
        {
            var ss = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 0)
                throw new Exception($"Wrong unit format '{field}'");

            int exp = 0;
            var m = Regex.Match(ss[0], @"^(\d+)(.*)$");
            if (m.Success)
            {
                exp = (int)Math.Log10(int.Parse(m.Groups[1].Value));
                ss[0] = m.Groups[2].Value;
                if (string.IsNullOrEmpty(ss[0]))
                    ss = ss.Skip(1).ToArray();
            }

            Unit unit = Unit.Unknown;
            if (ss.Length > 1)
            {
                if ((ss[0] + ss[1]).TryParseUnit(out unit)) // first two segment, i.e. µ mol, mm Hg
                {
                    if (ss.Length > 2 && ss[2].ToLower() == "of")
                        return (unit, exp, string.Join(' ', ss.Skip(3)));
                    return (unit, exp, string.Join(' ', ss.Skip(2)));
                }
                else if ((ss[ss.Length - 2] + ss[ss.Length - 1]).TryParseUnit(out unit)) // last two segment, i.e. µ mol, mm Hg
                {
                    return (unit, exp, string.Join(' ', ss.Take(ss.Length - 2)));
                }
            }
            if (ss[0].TryParseUnit(out unit)) // first one segment
            {
                if (ss.Length > 1 && ss[1].ToLower() == "of")
                    return (unit, exp, string.Join(' ', ss.Skip(2)));
                return (unit, exp, string.Join(' ', ss.Skip(1)));
            }
            if (ss[ss.Length - 1].TryParseUnit(out unit)) // last one segment
            {
                return (unit, exp, string.Join(' ', ss.Take(ss.Length - 1)));
            }

            throw new Exception($"Unknown unit '{field}'");
        }

        public static IList<(Unit unit, int exponent, string speciality)> ParseUnitFull(this string s)
        {
            if (s == null)
                return null;

            var ret = new List<(Unit unit, int exponent, string speciality)>();

            int exp = 0;
            var fs = s.Split('*');
            if (fs.Length == 2)
            {
                if (!double.TryParse(fs[1], out double f))
                    throw new Exception($"Invalid unit factor '{fs[1]}' in '{s}'");
                exp = (int)Math.Log10(f);
                s = fs[0];
            }
            else if (fs.Length != 1)
            {
                throw new Exception($"Too many *'s in '{s}'");
            }

            var fields = s.Split('/');
            foreach (var field in fields)
            {
                ret.Add(field.ParseAtomUnitField());
            }

            if (ret.Count == 0 || exp != 0 && ret[0].exponent != 0)
                throw new Exception($"Duplicate factor for unit '{s}'");

            ret[0] = (ret[0].unit, ret[0].exponent + exp, ret[0].speciality);

            return ret;
        }

        static string GetFactor(this int exp, bool dot)
        {
            string s;
            switch (exp)
            {
                case 0: return null;
                case 1: s = "10"; break;
                case 2: s = "100"; break;
                case 3: s = "10&sup3;"; break;
                case -1: s = "0.1"; break;
                case -2: s = "0.01"; break;
                case -3: s = "0.001"; break;
                default: s = $"10{exp.ToSuperscript()}"; break;
            }
            return dot ? $"{s}&sdot;" : $"{s} ";
        }

        static string TransformScripts(this string s)
        {
            s = s.Replace("O2", $"O{2.ToSubscript()}");
            s = s.Replace("H2", $"H{2.ToSubscript()}");
            s = Regex.Replace(s, @"\^(\d+)", o => int.Parse(o.Groups[1].Value).ToSuperscript());
            return s;
        }

        static string GetSpeciality(this string speciality)
        {
            if (string.IsNullOrEmpty(speciality))
                return null;
            return $" of {speciality.TransformScripts()}";
        }

        public static string FormatUnit(this IEnumerable<(Unit unit, int exp, string speciality)> units)
        {
            if (units == null)
                return null;
            return string.Join('/', units.Select(o => $"{o.exp.GetFactor(true)}{o.unit.GetShortName().TransformScripts()}"));
        }

        public static string FormatUnitFull(this IEnumerable<(Unit unit, int exp, string speciality)> units)
        {
            if (units == null)
                return null;
            return string.Join(" per ", units.Select(o => $"{o.exp.GetFactor(false)}{o.unit.GetName()}{o.speciality.GetSpeciality()}"));
        }
    }
}
