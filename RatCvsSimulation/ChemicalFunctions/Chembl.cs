using CommonTools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChemicalFunctions
{
    public static class Chembl
    {
        public static string GetChemblId(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("ChemblId");
        }

        public static IDictionary<string, object> SetChemblId(this IDictionary<string, object> bag, string value)
        {
            return bag.Set("ChemblId", value);
        }

        public static string GetChemblTargetActivityUri(this IDictionary<string, object> bag)
        {
            string id = bag.GetChemblId();
            return id != null ? $"https://www.ebi.ac.uk/chembl/g/#browse/activities/filter/target_chembl_id%3A{id}" : null;
        }


        public static async Task<IDictionary<string, object>> FetchChemblActivityData(this IDictionary<string, object> bag)
        {
            string id = bag.GetChemblId();
            string uri = "https://www.ebi.ac.uk/chembl/g/";
            string downloadEndpoint = "https://www.ebi.ac.uk/chembl/interface_api/delayed_jobs/submit/download_job";
            string downloadUri = "https://www.ebi.ac.uk/chembl/interface_api/delayed_jobs/outputs/{0}/{0}.zip";

            Logger.Current.LogInformation("Fetching activity data for {id}", id);

            bag["ChemblUri"] = uri;
            bag["ChemblTargetActivityUri"] = null;
            bag["ChemblBioactivityData"] = null;

            if (id == null)
            {
                return bag;
            }

            using HttpClient hc = new()
            {
                Timeout = TimeSpan.FromMinutes(3)
            };

            Dictionary<string, string> dict = new()
            {
                    { "index_name", "chembl_activity" },
                    { "query", @"{""bool"":{""must"":[{""query_string"":{""analyze_wildcard"":true,""query"":""target_chembl_id:" + id + @"""}}],""filter"":[]}}" },
                    { "format", "CSV" },
                    { "dl__ignore_cache", "false" },
                };

            using HttpRequestMessage reqMessage = new(HttpMethod.Post, downloadEndpoint);
            using FormUrlEncodedContent content = new(dict);
            reqMessage.Content = content;
            reqMessage.Headers.Add("Referer", "https://www.ebi.ac.uk/");

            using HttpResponseMessage resMessage = await hc.SendAsync(reqMessage);
            string json = await resMessage.Content.ReadAsStringAsync();

            Match m2 = Regex.Match(json, @"{""job_id"": *""([^""]+)""}");
            if (m2.Success)
            {
                string downloadId = m2.Groups[1].Value;
                string downloadLink = string.Format(downloadUri, downloadId);
                bag["ChemblTargetActivityUri"] = downloadLink;

                Stopwatch sw = Stopwatch.StartNew();
                byte[] bytes = null;
                int retries = 20;

                for (int i = 1; i <= retries; i++)
                {
                    try
                    {
                        bytes = await hc.GetByteArrayAsync(downloadLink);

                        if (bytes == null || bytes.Length == 0)
                            throw new Exception("Zero downloaded size");

                        Logger.Current.LogInformation("Downloaded {bytesLength} bytes for {id} from {downloadLink} in {timeElapsed}", bytes.Length, id, downloadLink, sw.Elapsed);

                        bag["ChemblBioactivityData"] = CompressionHelper.DecompressZip(bytes);

                        break;
                    }
                    catch (Exception ex)
                    {
                        if (i < retries)
                        {
                            if (i % 10 == 0)
                                Logger.Current.LogWarning("Failed to download activity data for {id} from {downloadLink} for {times} times, will retry in 3 seconds. {message}", id, downloadLink, i, ex.Message);
                            await Task.Delay(3000);
                        }
                        else
                        {
                            Logger.Current.LogError("Failed to download activity data for {id} from {downloadLink} in {timeElapsed}. {message}", id, downloadLink, sw.Elapsed, ex.Message);
                        }
                    }
                }
            }

            return bag;
        }

        public static byte[] GetChemblBioactivityData(this IDictionary<string, object> bag)
        {
            return bag.Get<byte[]>("ChemblBioactivityData");
        }

        public static IDictionary<string, object> SaveChemblBioactivityData(this IDictionary<string, object> bag, string path)
        {
            byte[] data = bag.GetChemblBioactivityData();
            if (data == null || data.Length == 0)
            {
                Logger.Current.LogError("Failed to save file {path}", path);
                return bag;
            }
            File.WriteAllBytes(path, data);
            return bag;
        }
    }
}
