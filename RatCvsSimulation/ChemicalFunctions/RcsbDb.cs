using ChemicalFunctions.PdbFormat;
using CommonTools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChemicalFunctions
{
    public static class RcsbDb
    {
        public static IDictionary<string, object> SetPdbEntry(this IDictionary<string, object> bag, string pdbEntry)
        {
            return bag.Set("PdbEntry", pdbEntry);
        }

        public static string GetPdbDownloadUri(this IDictionary<string, object> bag)
        {
            string pdbEntry = bag.Get<string>("PdbEntry");
            if (!string.IsNullOrEmpty(pdbEntry))
            {
                string uri = $"https://files.rcsb.org/download/{pdbEntry}.pdb";
                bag.Set("PdbDownloadUri", uri);
                return uri;
            }
            return null;
        }

        public static string GetCifDownloadUri(this IDictionary<string, object> bag)
        {
            string pdbEntry = bag.Get<string>("PdbEntry");
            if (!string.IsNullOrEmpty(pdbEntry))
            {
                string uri = $"https://files.rcsb.org/download/{pdbEntry}.cif";
                bag.Set("CifDownloadUri", uri);
                return uri;
            }
            return null;
        }

        public static string GetRcsbDownloadUri(this IDictionary<string, object> bag)
        {
            string pdbEntry = bag.Get<string>("PdbEntry");
            if (!string.IsNullOrEmpty(pdbEntry))
            {
                string uri = $"https://www.rcsb.org/structure/{pdbEntry}";
                bag.Set("RcsbDownloadUri", uri);
                return uri;
            }
            return null;
        }

        private static bool InternalDownloadFile(string uri, string fileName, bool skipIfExist)
        {
            if (skipIfExist && File.Exists(fileName))
                return true;
            try
            {
                uri.DownloadFile(fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetPdbOrCifFilePath(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("PdbFileName") ?? bag.Get<string>("CifFileName");
        }

        public static bool DownloadPdbFile(this IDictionary<string, object> bag, string fileName, bool skipIfExist = true)
        {
            bool r = InternalDownloadFile(bag.GetPdbDownloadUri(), fileName, skipIfExist);
            if (r)
                bag.Set("PdbFileName", fileName);
            return r;
        }

        public static bool DownloadCifFile(this IDictionary<string, object> bag, string fileName, bool skipIfExist = true)
        {
            bool r = InternalDownloadFile(bag.GetCifDownloadUri(), fileName, skipIfExist);
            if (r)
                bag.Set("CifFileName", fileName);
            return r;
        }

        public static bool SplitPdbChains(this IDictionary<string, object> bag, string destDir = null, bool skipIfExist = true)
        {
            string pdbFilePath = bag.Get<string>("PdbFileName");

            if (string.IsNullOrEmpty(pdbFilePath))
                return false;

            bool usePrefix = destDir == null;
            if (destDir == null)
                destDir = Path.GetDirectoryName(pdbFilePath);

            //if (skipIfExist && Directory.Exists(destDir))
            //{
            //    Logger.Current.LogDebug($"Skipping splitting for existing {destDir}");
            //    return true;
            //}

            string pdbName = Path.GetFileNameWithoutExtension(pdbFilePath);
            string pdbOutFilePath = Path.Combine(destDir, $"{pdbName}_ALL.pdb");

            string[] blackList = new[] { "HOH", "H2O", "WATER" };

            using (var pr = new PdbReader(pdbFilePath))
            using (var allWriter = new PdbWriter(pdbOutFilePath))
            {
                var records = new List<Record>();
                var chainWriters = new Dictionary<(string, string, int), PdbWriter>();

                while (true)
                {
                    var record = pr.ReadRecord();
                    if (record == null)
                        break;

                    if (record is AbstractChainedCoordinateRecord chainedRecord)
                    {
                        string chainId = chainedRecord.ChainId.ToString();
                        if (char.IsLower(chainId[0]))
                            chainId = "@" + chainId;
                        if (string.IsNullOrWhiteSpace(chainId))
                            chainId = "Global";

                        string resName = chainedRecord.ResidueName;
                        int resSeq = chainedRecord.ResidueSequence;

                        if (!blackList.Contains(resName))
                        {
                            // All amoni acids are placed in the same file
                            if (resName.TryParseAminoAcid(out _))
                            {
                                resName = "AminoAcids";
                                resSeq = 0;
                            }

                            var key = (chainId, resName, resSeq);

                            if (!chainWriters.ContainsKey(key))
                            {
                                string resFileName = $"{(usePrefix ? $"{pdbName}_" : "")}{chainId}_{resName}{(resSeq == 0 ? "" : $"_{resSeq}")}.pdb";
                                string resFilePath = Path.Combine(destDir, resFileName);
                                var pw2 = new PdbWriter(resFilePath);
                                chainWriters[key] = pw2;

                                foreach (var rec in records)
                                {
                                    pw2.WriteRecord(rec);
                                }

                                Logger.Current.LogInformation($"Output: {resFileName}");
                            }

                            chainWriters[key].WriteRecord(record);
                        }
                    }
                    //else
                    //{
                    //    records.Add(record);
                    //    foreach (var chainWriter in chainWriters)
                    //    {
                    //        chainWriter.Value.WriteRecord(record);
                    //    }
                    //}

                    allWriter.WriteRecord(record);
                }

                allWriter.Close();
                foreach (var chainWriter in chainWriters)
                {
                    chainWriter.Value.Close();
                    chainWriter.Value.Dispose();
                }

                pr.Close();
            }


            if (!IsSameFile(pdbOutFilePath, pdbFilePath))
                Logger.Current.LogWarning($"{pdbOutFilePath} signature not matched");
            else
                File.Delete(pdbOutFilePath);

            return true;
        }

        private static bool IsSameFile(string path1, string path2)
        {
            string[] newLines = File.ReadAllLines(path1);
            string[] oldLines = File.ReadAllLines(path2);

            if (newLines.Length != oldLines.Length)
                return false;

            for (int i = 0; i < newLines.Length; i++)
            {
                newLines[i] = newLines[i].TrimEnd();
                oldLines[i] = oldLines[i].TrimEnd();

                if (oldLines[i].Length != newLines[i].Length)
                    return false;

                for (int j = 0; j < newLines[i].Length; j++)
                    if (newLines[i][j] != oldLines[i][j] && !(newLines[i][j] == ' ' && oldLines[i][j] == '-'))
                        return false;
            }

            return true;
        }

        public static IDictionary<string, object> DownloadPdbOrCifAndSplit(this IDictionary<string, object> bag, string destinationDir, string proteinName, string? subfolderName, bool skipIfExist = true, bool split = true)
        {
            string pdbEntry = bag.Get<string>("PdbEntry");
            string downloadUri;

            string downloadDir = subfolderName == null ? destinationDir : Path.Combine(destinationDir, "PDB");
            if (!Directory.Exists(downloadDir))
                Directory.CreateDirectory(downloadDir);

            string pdbFilePath = Path.Combine(downloadDir, $"{pdbEntry}.pdb");
            string cifFilePath = Path.Combine(downloadDir, $"{pdbEntry}.cif");

            if (bag.DownloadPdbFile(pdbFilePath, skipIfExist))
            {
                downloadUri = bag.GetPdbDownloadUri();
                Logger.Current.LogInformation($"Downloaded PDB {pdbEntry} for {proteinName}");

                string fragmentDir = subfolderName == null ? destinationDir : Path.Combine(destinationDir, subfolderName);

                if (!Directory.Exists(fragmentDir))
                    Directory.CreateDirectory(fragmentDir);

                if (split && bag.SplitPdbChains(fragmentDir, skipIfExist))
                    Logger.Current.LogInformation($"Splitted {pdbEntry} for {proteinName}");
            }
            else if (bag.DownloadCifFile(cifFilePath, skipIfExist))
            {
                downloadUri = bag.GetCifDownloadUri();
                Logger.Current.LogInformation($"Downloaded CIF {pdbEntry} for {proteinName}");
            }
            else
            {
                downloadUri = null;
                Logger.Current.LogError($"Failed: cannot download {pdbEntry} PDB/CIF for {proteinName}");
            }

            bag.Set("PdbOrCifDownloadUri", downloadUri);
            return bag;
        }

        public static string GetPdbOrCifDownloadUri(this IDictionary<string, object> bag)
        {
            return bag.Get<string>("PdbOrCifDownloadUri");
        }

        public static IDictionary<string, object> DownloadRcsbProfileHtml(this IDictionary<string, object> bag)
        {
            var url = bag.GetRcsbDownloadUri();
            try
            {
                var html = url.DownloadString();
                if (!string.IsNullOrEmpty(html))
                    bag.Set("RcsbProfileHtml", html);
            }
            catch (Exception)
            {
                bag.Set<string>("RcsbProfileHtml", null);
            }

            return bag;
        }

        public static IDictionary<string, object> ParseRcsbProfileHtml(this IDictionary<string, object> bag)
        {
            var html = bag.Get<string>("RcsbProfileHtml");

            var pattern = @"Go to UniProtKB: &nbsp<span[^<>]*>\s*<a href=""https?://www.uniprot.org/uniprot/([^""]+)""[^<>]*>\1</a>\s*</span>";
            var ms = Regex.Matches(html, pattern);

            var ids = ms.Cast<Match>().Select(o => o.Groups[1].Value).OrderBy(o => o).Distinct().ToArray();
            if (ids.Length != 0)
            {
                bag.Set("UniProtIds", ids);
            }
            else
            {
                bag.Set<string[]>("UniProtIds", null);
            }
            return bag;
        }
    }
}
