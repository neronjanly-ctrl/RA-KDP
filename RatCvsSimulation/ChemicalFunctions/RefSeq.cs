using System.Collections.Generic;

namespace ChemicalFunctions
{
    public static class RefSeq
    {
        public static string GetRefSeqId(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("RefSeqId");
        }

        public static string GetRefSeqUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetRefSeqId();
            return id != null ? $"https://www.ncbi.nlm.nih.gov/nuccore/{id}" : null;
        }

        public static string GetNcbiSequenceViewerUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetRefSeqId();
            return id != null ? $"https://www.ncbi.nlm.nih.gov/projects/sviewer/?id={id}" : null;
        }
    }
}
