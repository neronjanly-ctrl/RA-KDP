using CommonTools;

namespace ChemicalFunctions
{
    public static class ChemicalElementExtensions
    {
        public static ChemicalElement ParseChemicalElement(this string s)
        {
            return EnumByMatchParser<ChemicalElement>.Parse(s);
        }

        public static bool TryParseChemicalElement(this string s, out ChemicalElement value)
        {
            return EnumByMatchParser<ChemicalElement>.TryParse(s, out value);
        }

        public static AtomAttribute GetAtom(this ChemicalElement element)
        {
            return EnumAnnotationHelper<ChemicalElement>.GetAttribute<AtomAttribute>(element);
        }
    }
}
