using CommonTools;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalFunctions
{
    public static class ChemicalFormatExtensions
    {
        // ".pdbqt", ChemicalFormat.PDBQT
        public static IDictionary<string, ChemicalFormat> Formats { get; } = EnumAnnotationHelper<ChemicalFormat>
            .GetWithAttribute<MimeAttribute>()
            .SelectMany(o => o.GetMimeType().Extensions.Select(p => new { ext = $".{p}", format = o }))
            .ToDictionary(o => o.ext, o => o.format);

        // ".pdbqt", "chemical/x-pdbqt"
        public static IDictionary<string, string> MimeTypes { get; } = EnumAnnotationHelper<ChemicalFormat>
            .GetWithAttribute<MimeAttribute>()
            .Select(o => new { format = o, mime = o.GetMimeType() })
            .SelectMany(o => o.mime.Extensions.Select(p => new { ext = $".{p}", mime = o.mime.MimeType }))
            .ToDictionary(o => o.ext, o => o.mime);

        public static ChemicalFormat ParseChemicalFormat(this string s)
        {
            return EnumByMatchParser<ChemicalFormat>.Parse(s);
        }

        public static bool TryParseChemicalFormat(this string s, out ChemicalFormat value)
        {
            return EnumByMatchParser<ChemicalFormat>.TryParse(s, out value);
        }

        public static MimeAttribute GetMimeType(this ChemicalFormat format)
        {
            return EnumAnnotationHelper<ChemicalFormat>.GetAttribute<MimeAttribute>(format);
        }
    }
}
