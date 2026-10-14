using System.Collections.Generic;

namespace ChemicalFunctions
{
    public static class Ensembl
    {
        public static string GetEnsemblId(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("EnsemblId");
        }

        public static string GetEnsemblUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetEnsemblId();
            return id != null ? $"https://useast.ensembl.org/Homo_sapiens/Gene/Summary?g={id}" : null;
        }

        public static string GetEnsemblRegionInDetailUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetEnsemblId();
            return id != null ? $"https://useast.ensembl.org/Homo_sapiens/Location/View?db=core;g={id}" : null;
        }

        public static string GetEnsemblGeneSequenceUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetEnsemblId();
            return id != null ? $"https://useast.ensembl.org/Homo_sapiens/Gene/Sequence?g={id}" : null;
        }
    }
}
