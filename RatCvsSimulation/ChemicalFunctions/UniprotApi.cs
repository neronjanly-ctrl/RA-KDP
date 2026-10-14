using CommonTools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace ChemicalFunctions
{
    public static class UniprotApi
    {
        public static string GetUniProtApiUri(this IDictionary<string, object> bag, bool reviewed)
        {
            string id = bag.GetUniProtId();
            return id != null ? $"https://rest.uniprot.org/uniprotkb/search?query=reviewed:{reviewed.ToString().ToLower()}%20AND%20{id}" : null;
        }

        public static string GetUniProtJson(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("UniProtJson");
        }

        public static IDictionary<string, object> FetchUniProtJson(this IDictionary<string, object> bag, bool reviewed)
        {
            int[] waitTime = new int[] { 0, 500, 2000 };
            bag["UniProtJson"] = null;

            for (int i = 0; bag["UniProtJson"] == null; i++)
            {
                if (i == 3)
                {
                    Logger.Current.LogError($"Failed to get uniProt data for uniProt Id {bag["UniProtId"]} after 3 attempts");
                    break;
                }

                if (i > 0 && i < 3)
                {
                    Logger.Current.LogError($"Failed to get uniProt data for uniProt Id {bag["UniProtId"]}, retry in {waitTime[i]}ms");
                    Thread.Sleep(waitTime[i]);
                }

                try
                {
                    string json = bag.GetUniProtApiUri(reviewed).DownloadString();
                    if (!string.IsNullOrEmpty(json) && json.Trim() != "{\"results\":[]}")
                        bag["UniProtJson"] = json;
                }
                catch (Exception)
                {
                }
            }
            return bag;
        }

        public static IDictionary<string, object> ParseUniProtJson(this IDictionary<string, object> bag)
        {
            string json = bag.Get<string>("UniProtJson");

            bag["HgncId"] = null;
            bag["GeneName"] = null;
            bag["UniProtSymbol"] = null;
            bag["ProteinName"] = null;
            bag["Species"] = null;
            bag["SubmittedProteinName"] = null;
            bag["ProteinNameFull"] = null;
            bag["ProteinSymbol"] = null;
            bag["OrganismSymbol"] = null;
            bag["ChemblId"] = null;
            bag["Organisms"] = null;

            string uniprotId = bag.GetUniProtId();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                // Access properties using the DOM API
                JsonElement root = doc.RootElement;
                JsonElement results = root.GetProperty("results");

                JsonElement[] matchedResults = results
                    .EnumerateArray()
                    .Where(o => o.GetProperty("uniProtkbId").GetString() == uniprotId
                        || o.GetProperty("primaryAccession").GetString() == uniprotId)
                    .ToArray();

                if (matchedResults.Length == 0)
                {
                    Logger.Current.LogError("No matching result for uniProt uniProt Id {uniprotId}", uniprotId);
                    return bag;
                }

                JsonElement result = matchedResults[0];

                string uniprotKbId = result.GetProperty("uniProtkbId").GetString();
                JsonElement organism = result.GetProperty("organism");

                var uniProtKBCrossReferences = result
                    .GetProperty("uniProtKBCrossReferences")
                    .EnumerateArray()
                    .Select(o => new
                    {
                        database = o.GetProperty("database").GetString(),
                        id = o.GetProperty("id").GetString(),
                        properties = o.GetProperty("properties").EnumerateArray().ToDictionary(p => p.GetProperty("key").GetString(), p => p.GetProperty("value").GetString())
                    })
                    .ToArray();

                var hgnc = uniProtKBCrossReferences.FirstOrDefault(o => o.database == "HGNC");
                if (hgnc != null)
                    bag["HgncId"] = int.Parse(hgnc.id.Split(":")[1]);

                var chembl = uniProtKBCrossReferences.FirstOrDefault(o => o.database == "ChEMBL");
                if (chembl != null)
                    bag["ChemblId"] = chembl.id;

                string? commonNameStr = null;
                if (organism.TryGetProperty("commonName", out JsonElement commonName))
                    commonNameStr = commonName.GetString();

                /*if (result.TryGetProperty("genes", out JsonElement genes) && genes.GetArrayLength() > 0)
                    bag["GeneName"] = genes[0].GetProperty("geneName").GetProperty("value").GetString();*/

                if (result.TryGetProperty("genes", out JsonElement genes) && genes.GetArrayLength() > 0)
                {
                    var geneObj = genes[0];
                    if (geneObj.TryGetProperty("geneName", out JsonElement geneNameObj))
                    {
                        if (geneNameObj.TryGetProperty("value", out JsonElement geneValueObj))
                        {
                            bag["GeneName"] = geneValueObj.GetString();
                        }
                        else
                        {
                            bag["GeneName"] = null; // 或者留空
                        }
                    }
                    else
                    {
                        bag["GeneName"] = null; // 或者留空
                    }
                }
                else
                {
                    bag["GeneName"] = null; // 或者留空
                }


                JsonElement proteinDesc = result.GetProperty("proteinDescription");

                bag["UniProtSymbol"] = uniprotKbId;

                if (proteinDesc.TryGetProperty("recommendedName", out JsonElement recommendedName))
                    bag["ProteinName"] = recommendedName.GetProperty("fullName").GetProperty("value");
                else if (proteinDesc.TryGetProperty("submissions", out JsonElement submissions) && submissions.GetArrayLength() > 0)
                    bag["ProteinName"] = submissions[0].GetProperty("full").GetProperty("value").GetString();

                bag["Species"] = organism.GetProperty("scientificName").GetString();
                bag["SubmittedProteinName"] = null;
                bag["ProteinNameFull"] = null;
                bag["ProteinSymbol"] = uniprotKbId.Split('_')[0];
                bag["OrganismSymbol"] = uniprotKbId.Split('_')[1];
                bag["Organisms"] = new[] { organism.GetProperty("scientificName").GetString(), commonNameStr }.Where(o => !string.IsNullOrEmpty(o)).ToArray();
            }

            return bag;
        }
    }
}
