using CommonTools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalFunctions
{
    public static class AsdDb
    {
        public static string GetAsdDbUri(this IDictionary<string, object> bag)
        {
            return "http://mdl.shsmu.edu.cn/ASD/BrowseSite?page=1&limit=9999";
        }

        public static IDictionary<string, object> FetchAsdDbJson(this IDictionary<string, object> bag)
        {
            try
            {
                bag["AsdDbJson"] = bag.GetAsdDbUri().DownloadString();
            }
            catch (Exception)
            {
                bag["AsdDbJson"] = null;
                Logger.Current.LogError($"Failed to get ASDdb data.");
            }
            return bag;
        }

        public class AsdRecordSet
        {
            [JsonProperty("totalCount")]
            public int TotalCount { get; set; }

            [JsonProperty("data")]
            public List<AsdRecord> Data { get; set; }
        }

        public class AsdRecord
        {
            [JsonProperty("substrate_pdb")]
            public string SubstratePdb { get; set; }

            [JsonProperty("substrate_alias")]
            public string SubstrateAlias { get; set; }

            [JsonProperty("target_name")]
            public string TargetName { get; set; }

            [JsonProperty("allosteric_pdb")]
            public string AllostericPdb { get; set; }

            [JsonProperty("substrate_site")]
            public string SubstrateSite { get; set; }

            [JsonProperty("organism")]
            public string Organism { get; set; }

            [JsonProperty("target_id")]
            public int TargetId { get; set; }

            [JsonProperty("modulator_id")]
            public string ModulatorId { get; set; }

            [JsonProperty("modulator_serial")]
            public string ModulatorSerial { get; set; }

            [JsonProperty("modulator_alias")]
            public string ModulatorAlias { get; set; }

            [JsonProperty("pubmed_id")]
            public string PubmedId { get; set; }

            [JsonProperty("target_domain")]
            public string TargetDomain { get; set; }

            [JsonProperty("ref_title")]
            public string RefTitle { get; set; }

            [JsonProperty("allosteric_site")]
            public string AllostericSite { get; set; }

            public override bool Equals(object obj)
            {
                return obj is AsdRecord that
                    && SubstratePdb == that.SubstratePdb
                    && SubstrateAlias == that.SubstrateAlias
                    && TargetName == that.TargetName
                    && AllostericPdb == that.AllostericPdb
                    && SubstrateSite == that.SubstrateSite
                    && Organism == that.Organism
                    && TargetId == that.TargetId
                    //&& ModulatorId == that.ModulatorId
                    //&& ModulatorSerial == that.ModulatorSerial
                    //&& ModulatorAlias == that.ModulatorAlias
                    && PubmedId == that.PubmedId
                    //&& TargetDomain == that.TargetDomain
                    && RefTitle == that.RefTitle
                    //&& AllostericSite == that.AllostericSite
                    ;
            }

            public override int GetHashCode()
            {
                return SubstratePdb?.GetHashCode() ?? 0
                    ^ SubstrateAlias?.GetHashCode() ?? 0
                    ^ TargetName?.GetHashCode() ?? 0
                    ^ AllostericPdb?.GetHashCode() ?? 0
                    ^ SubstrateSite?.GetHashCode() ?? 0
                    ^ Organism?.GetHashCode() ?? 0
                    ^ TargetId.GetHashCode()
                    //^ ModulatorId?.GetHashCode() ?? 0
                    //^ ModulatorSerial?.GetHashCode() ?? 0
                    //^ ModulatorAlias?.GetHashCode() ?? 0
                    ^ PubmedId?.GetHashCode() ?? 0
                    //^ TargetDomain?.GetHashCode() ?? 0
                    ^ RefTitle?.GetHashCode() ?? 0
                    //^ AllostericSite?.GetHashCode() ?? 0;
                    ;
            }

            public static bool operator ==(AsdRecord a, AsdRecord b) => a.Equals(b);
            public static bool operator !=(AsdRecord a, AsdRecord b) => !a.Equals(b);
        }

        public static IDictionary<string, object> ParseAsdDbStructures(this IDictionary<string, object> bag)
        {
            var rs = JsonConvert.DeserializeObject<AsdRecordSet>(bag.Get<string>("AsdDbJson").Replace("\"null\"", "null"));

            if (rs.TotalCount != rs.Data.Count)
                Logger.Current.LogWarning($"Wrong PDBs number reported on ASDdb: {rs.TotalCount} != {rs.Data.Count}");

            // Make sure all allosteric pdbs for a target do not conflict.
            var dict = new Dictionary<string, AsdRecord>();
            foreach (var r in rs.Data)
            {
                string key = $"{r.TargetName.ToLower().Trim()}_{r.AllostericPdb}";
                if (!dict.ContainsKey(key))
                {
                    dict[key] = r;
                }
                else if (dict[key] != r)
                {
                    Logger.Current.LogWarning($"Duplicate Allosteric PDB {r.AllostericPdb} found for target {r.TargetName} but with different description.");
                }
            }

            bag["AsdDbPdbList"] = dict.Values.ToArray();
            if (rs.Data.Count == 0)
                Logger.Current.LogWarning($"No PDBs found on ASDdb");

            return bag;
        }

        public static AsdRecord[] GetAsdDbPdbList(this IDictionary<string, object> bag)
        {
            return bag.Get<AsdRecord[]>("AsdDbPdbList");
        }
    }
}
