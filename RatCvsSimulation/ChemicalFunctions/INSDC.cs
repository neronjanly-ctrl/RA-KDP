using System.Collections.Generic;

namespace ChemicalFunctions
{
    public static class INSDC
    {
        public static string GetINSDCId(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("INSDCId");
        }

        public static string GetENAUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetINSDCId();
            return id != null ? $"https://www.ebi.ac.uk/ena/data/view/{id}" : null;
        }

        public static string GetGenBankUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetINSDCId();
            return id != null ? $"https://www.ncbi.nlm.nih.gov/nuccore/{id}" : null;
        }

        public static string GetDDBJUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetINSDCId();
            return id != null ? $"http://getentry.ddbj.nig.ac.jp/getentry/na/{id}/" : null;
        }
    }
}
