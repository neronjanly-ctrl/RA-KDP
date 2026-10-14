using CommonTools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ChemicalFunctions
{
    public static class GeneNames
    {
        private static string SimplifyA(string s)
        {
            s = Regex.Replace(s, @"\s*<a [^<>]*href=""([^""]+)""[^<>]*>\s*", @"<a href=""$1"">");
            s = Regex.Replace(s, @"\s*</?(?!a)>\s*", string.Empty);
            s = Regex.Replace(s, @"\s*<a[^<>]+>\s*[CD]?\s*</a>\s*", string.Empty);
            return s.Trim();
        }

        private static IEnumerable<(string Key, string Url)> CutLinks(string s)
        {
            const string pattern = @"<a href=""([^""]+)"">([^<>]+)</a>";

            yield return ("Title", Regex.Replace(s, pattern, string.Empty));

            var ms = Regex.Matches(s, pattern);
            foreach (Match m in ms)
            {
                yield return (m.Groups[2].Value, m.Groups[1].Value);
            }
        }

        private static string Cleanup(string s)
        {
            return WebUtility.HtmlDecode(Regex.Replace(s, @"\s*<[^<>]+>\s*", string.Empty).Trim());
        }

        private static IEnumerable<(string Key, string[] Values)> InternalParseGeneNameJson(string html)
        {
            html = Regex.Replace(html, @"<!--[\s\S]*?-->", string.Empty);

            var ms = Regex.Matches(html, @"<fieldset class=""[^""]+"">([\s\S]+?)</fieldset>");
            var symbolData = Regex.Matches(ms[0].Value, @"<dt[^<>]*>([\s\S]+?)</dt>\s*<dd[^<>]*>([\s\S]+?)</dd>");

            foreach (Match m in symbolData)
            {
                string key = Cleanup(m.Groups[1].Value);
                string value = Cleanup(m.Groups[2].Value);
                yield return (key, new[] { value });
            }

            var extLinks = Regex.Matches(ms[1].Value, @"<dt[^<>]*>([\s\S]+?)</dt>(\s*<dd[^<>]*>\s*(<dl[^<>]*>([\s\S]+?)</dl>|([\s\S]+?))\s*</dd>)+");

            foreach (Match m in extLinks)
            {
                string key = Cleanup(m.Groups[1].Value);

                var list = new List<string>();
                foreach (Capture c in m.Groups[3].Captures)
                {
                    string subvalue = string.Empty;

                    if (c.Value.StartsWith("<dl"))
                    {
                        var dl = Regex.Matches(c.Value, @"<dt[^<>]*>([\s\S]+?)</dt>(\s*<dd[^<>]*>([\s\S]+?)</dd>)*");

                        foreach (Match m2 in dl)
                        {
                            subvalue = SimplifyA(m2.Groups[1].Value);

                            foreach (Capture c2 in m2.Groups[3].Captures)
                            {
                                subvalue += SimplifyA(c2.Value);
                            }
                        }
                    }
                    else
                    {
                        subvalue = SimplifyA(c.Value);
                    }

                    if (subvalue.StartsWith("<") && !subvalue.StartsWith("<a "))
                        continue;

                    list.Add(subvalue);
                }

                yield return (key, list.ToArray());
            }
        }

        public static IDictionary<string, object> FetchGeneNamesJson(this IDictionary<string, object> bag)
        {
            string uri = bag.GetGeneNamesJsonUri();
            try
            {
                bag["GeneNamesJson"] = uri.DownloadString();
            }
            catch (Exception)
            {
                bag["GeneNamesJson"] = null;
                Logger.Current.LogError($"Failed to get genenames json for HGNC id {bag["HgncId"]}");
            }
            return bag;
        }

        public static string GetGeneNamesJsonUri(this IDictionary<string, object> bag)
        {
            int? id = bag.GetHgncId();
            return id != null ? $"https://www.genenames.org/cgi-bin/gene/symbol-report?id={id}" : null;
        }

        public static string GetGeneNamesHtmlUri(this IDictionary<string, object> bag)
        {
            int? id = bag.GetHgncId();
            return id != null ? $"https://www.genenames.org/data/gene-symbol-report/#!/hgnc_id/{id}" : null;
        }

        public static string GetGeneNamesJson(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("GeneNamesJson");
        }

        public static IDictionary<string, object>  SetGeneName(this IDictionary<string, object> bag, string geneName)
        {
            return bag.Set<string>("GeneName", geneName);
        }

        public static IDictionary<string, object> ParseGeneNamesData(this IDictionary<string, object> bag)
        {
            // GeneName
            string geneName = bag.Get<string>("GeneName");

            // GeneNamesHtmlUri
            bag.GetGeneNamesHtmlUri();

            // EnsemblId
            var json = JObject.Parse(bag.Get<string>("GeneNamesJson"));

            var proteins = json["external_resources"]["proteins"]
                .SelectMany(o => o.Select(p => new { type = p["type"]?.Value<string>(), id = p["id"]?.Value<string>() }).ToArray());
            var genes = json["external_resources"]["genes"]
                .SelectMany(o => o.Select(p => new { type = p["type"]?.Value<string>(), id = p["id"]?.Value<string>() }).ToArray());
            var nucleotides = json["external_resources"]["nucleotides"]
                .SelectMany(o => o.Select(p => new { type = p["type"]?.Value<string>(), id = p["id"]?.Value<string>() }).ToArray());

            // UniProt/Swiss-Prot
            bag["UniProtId"] = proteins.FirstOrDefault(o => o.type == "UniProt/Swiss-Prot")?.id;

            // Ensembl
            bag["EnsemblId"] = genes.FirstOrDefault(o => o.type == "Ensembl")?.id;

            // INSDC
            bag["INSDCId"] = nucleotides.FirstOrDefault(o => o.type == "INSDC")?.id;

            // RefSeq
            bag["RefSeqId"] = nucleotides.FirstOrDefault(o => o.type == "RefSeq")?.id;

            // Locus group
            bag["LocusGroup"] = json["locus_group"]?.Value<string>();

            // Locus type
            bag["LocusType"] = json["locus_type"]?.Value<string>();

            // Alias names, previously Synonyms
            bag["AliasNames"] = json["alias_names"]?.Select(o => o.Value<string>()).ToArray();

            // Gene groups, previously Gene family
            bag["GeneGroups"] = json["groups"]?.Select(o => o["name"].Value<string>()).ToArray();

            // Alias symbols
            bag["AliasSymbols"] = json["alias_symbols"]?.Select(o => o.Value<string>()).ToArray();

            // Approved symbol
            bag["ApprovedSymbol"] = json["symbol"]?.Value<string>();

            // Approved name
            bag["ApprovedName"] = json["name"]?.Value<string>();

            // Previous names
            bag["PreviousNames"] = json["prev_names"]?.Select(o => o.Value<string>()).ToArray();

            // Chromosomal location
            bag["ChromosomeLocation"] = json["chromosome_location"]?.Value<string>();

            //Logger.Current.LogWarning($"Failed to parse genenames information for HGNC {bag["HgncId"]}");

            return bag;
        }

        public static string GetLocusGroup(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("LocusGroup");
        }

        public static string GetLocusType(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("LocusType");
        }

        public static string GetAliasNames(this IDictionary<string, object> bag)
        {
            string[] names = bag.Get<string[]>("AliasNames");
            return names != null ? string.Join(";", names) : null;
        }

        public static string GetGeneGroups(this IDictionary<string, object> bag)
        {
            string[] names = bag.Get<string[]>("GeneGroups");
            return names != null ? string.Join(";", names) : null;
        }

        public static string GetAliasSymbols(this IDictionary<string, object> bag)
        {
            string[] names = bag.Get<string[]>("AliasSymbols");
            return names != null ? string.Join(";", names) : null;
        }

        public static string GetApprovedSymbol(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("ApprovedSymbol");
        }

        public static string GetApprovedName(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("ApprovedName");
        }

        public static string GetChromosomeLocation(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("ChromosomeLocation");
        }
    }
}
