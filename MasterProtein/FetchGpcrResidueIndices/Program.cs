using ChemicalFunctions;
using CommonTools;
using CommonTools.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ChemicalFunctions.GpcrDb;

namespace GenerateExcelSummary
{
    internal class Program
    {
        private const string defaultSchemeName = "Ballesteros-Weinstein (Class A)";
        private static readonly string[] stateColHeaders = new[] { null, null, null, "State->" };
        private static readonly string[] summaryRowHeaders = new[] { "Intra-Ligand Free", "Inter-Ligand Free", "Total Free Energy", "Normalized Total Free Energy" };
        private static readonly string[] csvColHeaders = new[] { "Chain ID", "Residue name", "Residue sequence", defaultSchemeName, "Conf 1(Input)", "Conf 1" };

        private static readonly string[] postfixList = new[]
        {
            "gauss1",
            "gauss2",
            "repulsion",
            "hydrophobic",
            "hbonding",
            "gauss",
            "steric",
            "nonsteric",
            "total",
        };

        // colors for GPCR classes
        private static readonly IReadOnlyDictionary<string, Color> gpcrClassColors = new Dictionary<string, Color>
        {
            { "A (Rhodopsin)", ColorHelper.ToColor("#edb2b2") },
            { "B1 (Secretin)", ColorHelper.ToColor("#f4d799") },
            { "C (Glutamate)", ColorHelper.ToColor("#add9aa") },
            { "F (Frizzled)", ColorHelper.ToColor("#aad8d9") },
        };

        private static Color ClassColorFn(string o) => gpcrClassColors.TryGetValue(o, out var val) ? val : Color.Black;

        // colors for GPCR states
        private static readonly IReadOnlyDictionary<string, Color> gpcrStateColors = new Dictionary<string, Color>
        {
            { "Active", ColorHelper.ToColor("#add9aa") },
            { "Intermediate", ColorHelper.ToColor("#f4d799") },
            { "Inactive", ColorHelper.ToColor("#edb2b2") },
        };

        private static Color StateColorFn(string o) => gpcrStateColors.TryGetValue(o, out var val) ? val : Color.Black;

        // color scale for excel score/similarity sheets
        private static readonly (double scale, string color)[] sheetColors = new[]
        {
            (-2, "FF0000FF" ),
            (.0, "FFFFFFFF" ),
            ( 2, "FFFF0000" ),
        };

        // color scale for heatmap cells
        private static readonly (double scale, Color color)[] heatmapColors = new (double scale, Color color)[]
        {
            (-1,   ColorHelper.ToColor((1.0, 0.0, 0.0, 0.3))), // 00004D, very dark blue
            (-0.5, ColorHelper.ToColor((1.0, 0.0, 0.0, 1.0))), // 0000FF, blue
            ( 0,   ColorHelper.ToColor((1.0, 1.0, 1.0, 1.0))), // FFFFFF, white
            ( 0.5, ColorHelper.ToColor((1.0, 1.0, 0.0, 0.0))), // FF0000, red
            ( 1,   ColorHelper.ToColor((1.0, 0.5, 0.0, 0.0))), // 800000, maroon
        };

        private const int rankingTop = 20;

        private static readonly IReadOnlyDictionary<string, string> shortKeys = new Dictionary<string, string>
        {
            { "All", "All" },
            { "Active", "Act" },
            { "Intermediate", "Intmd" },
            { "Inactive", "Inact" },
        };

        private static (string proteinName, string @class, string pdbId, string uniprotId, string species, string position) ExtractFields(string path)
        {
            // The path could be in the forms of:
            // C:\Users\MAC489\Downloads\GPCRdb_Proteins_JC031919_computed\ACM2_[A_(Rhodopsin)]\4MQT_P08172_[Human]\allosteric\out\ligand.csv
            // C:\Users\MAC489\Downloads\GPCRdb_Proteins_JC031919_computed\ACM2_[A_(Rhodopsin)]\5YC8_P08172_[Human]\out\ligand.csv
            // .\ACM2_[A_(Rhodopsin)]\4MQT_P08172_[Human]\allosteric\out\ligand.csv
            // .\ACM2_[A_(Rhodopsin)]\5YC8_P08172_[Human]\out\ligand.csv
            // /mnt/c/Users/MAC489/Downloads/GPCRdb_Proteins_JC031919_computed/ACM2_[A_(Rhodopsin)]/4MQT_P08172_[Human]/allosteric/out/ligand.csv
            // /mnt/c/Users/MAC489/Downloads/GPCRdb_Proteins_JC031919_computed/ACM2_[A_(Rhodopsin)]/5YC8_P08172_[Human]/out
            // ./ACM2_[A_(Rhodopsin)]/4MQT_P08172_[Human]/allosteric/out/ligand.csv
            // ./ACM2_[A_(Rhodopsin)]/5YC8_P08172_[Human]/out

            // The output fields example:
            // [0] proteinName: ACM2
            // [1] class: A (Rhodopsin)
            // [2] pdbId: 4MQT
            // [3] uniprotId: P08172
            // [4] species: Human
            // [5] position: allosteric/orthosteric/null
            string[] fields = Regex.Match(path, @"[/\\]([^/\\]+)_\[([^\]]+)\][/\\]([^/\\]+)_([^_/\\]+)_\[([^/\\]+)\][/\\]([^/\\]+)")
                .Groups
                .Cast<Group>()
                .Skip(1)
                .Select(o => o.Value.Replace('_', ' '))
                .ToArray();
            if (fields[5] == "out")
                fields[5] = null;
            return (fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]);
        }

        private static IReadOnlyDictionary<int, string> GetNumberingsForUniProtId(string uniProtId, ConcurrentDictionary<string, IReadOnlyDictionary<int, string>> numberingDict)
        {
            //       resSeq numbering
            IReadOnlyDictionary<int, string> numberings;

            if (numberingDict.ContainsKey(uniProtId))
            {
                numberings = numberingDict[uniProtId];
            }
            else
            {
                numberings = uniProtId.GetGpcrNumberings();
                numberingDict[uniProtId] = numberings;
            }

            return numberings;
        }

        /// <summary>
        /// Read from <paramref name="csvFilePath"/> and emit xlsx rows <paramref name="xlsxRows"/> and headers <paramref name="xlsxHeaders"/>
        /// All columns in <paramref name="csvRows"/> will be copied.
        /// The first header row in <paramref name="xlsxHeaders"/> is the pdb state row.
        /// The second header row in <paramref name="xlsxHeaders"/> is the pdb id row.
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="uniProtId"></param>
        /// <param name="pdbId"></param>
        /// <param name="pdbStateDict"></param>
        /// <param name="numberingDict"></param>
        /// <param name="xlsxRows"></param>
        /// <param name="xlsxHeaders"></param>
        private static (int rows, int columns) GenerateNumberingsXlsxRows(
            string csvFilePath,
            string uniProtId,
            string pdbId,
            IReadOnlyDictionary<string, ReceptorState> pdbStateDict,
            ConcurrentDictionary<string, IReadOnlyDictionary<int, string>> numberingDict,
            out IEnumerable<object>[] xlsxRows,
            out IEnumerable<string>[] xlsxHeaders)
        {
            var numberings = GetNumberingsForUniProtId(uniProtId, numberingDict);

            string[] csvRows = File.ReadAllLines(csvFilePath);

            string[] headers = CsvHelper.SplitCsvFields(csvRows[0]);
            var outRows = new List<IEnumerable<object>>();

            // Transform rows by inserting the BW numbering at [3]
            foreach (string line in csvRows.Skip(1))
            {
                string[] fields = CsvHelper.SplitCsvFields(line);
                IEnumerable<object> objs;
                var scores = fields.Skip(3).Select(o => string.IsNullOrEmpty(o) ? string.Empty : (object)double.Parse(o));

                if (fields.Length < 4) // blank rows or unrecognized rows
                {
                    objs = Array.Empty<object>();
                }
                else if (string.IsNullOrEmpty(fields[1]) || string.IsNullOrEmpty(fields[2])) // 4 summary rows: [0] title, [1] empty, [2] empty, [3] score
                {
                    objs = new[] { fields[0], null, null, null }.Concat(scores);
                }
                else // score rows: [0] chain, [1] resName, [2] resSeq, [3] score
                {
                    int resSeq = int.Parse(fields[2]);
                    if (!numberings.TryGetValue(resSeq, out string num))
                    {
                        if (!fields[1].TryParseAminoAcid(out var aa))
                            throw new Exception($"Unrecognized amino acid {fields[1]}");
                        num = $"{aa.GetCode()}{fields[2]}";
                    }
                    objs = new object[] { fields[0], fields[1], resSeq, num }.Concat(scores);
                }
                outRows.Add(objs);
            }

            var outHeaders1 = headers.Take(3).Append(defaultSchemeName).Concat(headers.Skip(3));
            var outHeaders2 = stateColHeaders.Concat(headers.Skip(3).Select(o => pdbStateDict.GetValueOrDefault(pdbId, ReceptorState.Unknown).ToString()));

            xlsxRows = outRows.ToArray();
            xlsxHeaders = new[] { outHeaders1, outHeaders2 };

            return (xlsxHeaders.Length + xlsxRows.Length, headers.Length + 1);
        }

        /// <summary>
        /// Summarize all reports in <paramref name="csvFilePaths"/> into a single xlsx file with rows <paramref name="summaryRows"/> and headers <paramref name="summaryHeaders"/>.
        /// Take only the first 4 columns in each file of <paramref name="csvFilePaths"/>.
        /// Add pdb state to the pre header row.
        /// Add average columns to the right of the scores.
        /// </summary>
        /// <param name="csvFilePaths"></param>
        /// <param name="uniProtId"></param>
        /// <param name="pdbStateDict"></param>
        /// <param name="numberingDict"></param>
        /// <param name="summaryRows"></param>
        /// <param name="summaryHeaders"></param>
        /// <param name="rankingRows"></param>
        /// <param name="similarityRows"></param>
        /// <returns>Total rows rendered.</returns>
        private static (int rows, int columns) GeneratePdbSummaryXlsxRows(
            string[] csvFilePaths,
            string uniProtId,
            IReadOnlyDictionary<string, ReceptorState> pdbStateDict,
            ConcurrentDictionary<string, IReadOnlyDictionary<int, string>> numberingDict,
            IVectorDistanceMeasure[] similarityVdms,
            out IEnumerable<object>[] summaryRows,
            out IEnumerable<string>[] summaryHeaders,
            out IEnumerable<object>[] rankingRows,
            out IEnumerable<object>[][] similarityRows,
            out MapDataFrame<string, string> scoreDataFrame,
            out MapDataFrame<string, string>[] similarityDataFrames,
            int rankingTop)
        {
            // Only headers for pdb scores.
            // summary: [0] Intra-Ligand Free, [1] Inter-Ligand Free, [2] Total Free Energy, [3] Normalized Total Free Energy
            var headers = new List<(string name, ReceptorState state, double[] summary)>();

            //                             resSeq          pdbName score
            var scoreDict = new Dictionary<int, Dictionary<string, double>>();

            //                           resSeq       chainId
            var dChains = new Dictionary<int, HashSet<string>>();
            //                             resSeq       resName
            var dResNames = new Dictionary<int, HashSet<string>>();
            //                               resSeq       BW_numbering
            var dNumberings = new Dictionary<int, HashSet<string>>(); // actually, a resSeq should not have multiple numberings

            //                                  name
            var rankings = new SortedDictionary<string, (string numbering, string resName, char resCode, int resSeq, double score)[]>();

            var numberings = GetNumberingsForUniProtId(uniProtId, numberingDict);

            foreach (string file in csvFilePaths)
            {
                string[][] rows = File.ReadAllLines(file).Select(o => CsvHelper.SplitCsvFields(o)).Where(o => o.Length > 1).ToArray();
                if (rows.Length - 7 < rankingTop)
                {
                    Logger.Current.LogWarning($"File {file} has less than {rankingTop} valid result lines");
                    continue;
                }

                var (_, _, pdbId, _, _, position) = ExtractFields(file);
                string name = position == null ? pdbId : $"{pdbId}_{position}";

                // (pdb name, state)
                headers.Add((name, pdbStateDict.GetValueOrDefault(pdbId, ReceptorState.Unknown), new double[summaryRowHeaders.Length]));

                var scoreList = new List<(string numbering, string resName, char resCode, int resSeq, double score)>();

                if (!rows.All(o => o.Length >= 4))
                    throw new Exception($"Not all rows are with 4 or more fields in file {file}");
                if (!Enumerable.Range(0, 3).All(i => rows[0][i] == csvColHeaders[i]))
                    throw new Exception($"Header {Enumerable.Range(0, 3).First(i => rows[0][i] != csvColHeaders[i])} is not recognized in file {file}");
                if (rows[0][3] != csvColHeaders[4] && rows[0][3] != csvColHeaders[5])
                    throw new Exception($"Header {rows[0][3]} is not recognized in file {file}");
                if (rows.Count(o => string.IsNullOrEmpty(o[1]) && string.IsNullOrEmpty(o[2])) != 4)
                    throw new Exception($"Not exactly 4 summary rows in file {file}");

                int summaryRowId = 0;

                // Skipping the header row.
                foreach (string[] row in rows.Skip(1))
                {
                    // For a score row.
                    if (!string.IsNullOrEmpty(row[1]) && !string.IsNullOrEmpty(row[2]))
                    {
                        string chain = row[0];
                        string resName = row[1];
                        int resSeq = int.Parse(row[2]);
                        if (!double.TryParse(row[3], out double score))
                            score = 0;

                        if (!scoreDict.TryGetValue(resSeq, out var sdict))
                            sdict = scoreDict[resSeq] = new Dictionary<string, double>();
                        sdict[name] = score;

                        if (dChains.TryGetValue(resSeq, out var hsChains))
                            hsChains.Add(row[0]);
                        else
                            dChains[resSeq] = new HashSet<string> { chain };

                        if (dResNames.TryGetValue(resSeq, out var hsResNames))
                            hsResNames.Add(resName);
                        else
                            dResNames[resSeq] = new HashSet<string> { resName };

                        if (!numberings.TryGetValue(resSeq, out string numbering))
                            numbering = $"{resName.ParseAminoAcid().GetCode()}{resSeq}";

                        if (dNumberings.TryGetValue(resSeq, out var hsNumberings))
                            hsNumberings.Add(numbering);
                        else
                            dNumberings[resSeq] = new HashSet<string> { numbering };

                        scoreList.Add((numbering, resName, resName.ParseAminoAcid().GetCode(), resSeq, score));
                    }
                    // For a summary row.
                    else
                    {
                        if (summaryRowHeaders[summaryRowId] != row[0])
                            throw new Exception($"Summary header '{row[0]}' not recognized.");
                        headers.Last().summary[summaryRowId++] = double.Parse(row[3]);
                    }
                }

                rankings[name] = scoreList.OrderBy(o => o.score).ToArray();
            }

            // Sort the columns and calculate column index
            headers.Sort((a, b) => a.state != b.state ? a.state.CompareTo(b.state) : a.name.CompareTo(b.name));
            var stateCounts = headers.GroupBy(o => o.state).OrderBy(o => o.Key).Select(o => (state: o.Key, count: o.Count(), col: 0)).ToList();
            for (int i = 0, col = 5; i < stateCounts.Count; i++)
            {
                stateCounts[i] = (stateCounts[i].state, stateCounts[i].count, col);
                col += stateCounts[i].count;
            }

            // header rows
            var outHeaders1 = csvColHeaders.Take(4).Concat(headers.Select(o => o.name)).Concat(stateCounts.Select(o => $"Average({o.count})")).Append($"Average({headers.Count})");
            var outHeaders2 = stateColHeaders.Concat(headers.Select(o => o.state.ToString())).Concat(stateCounts.Select(o => o.state.ToString())).Append("All");

            // score rows
            var outRows = scoreDict
                .OrderBy(o => o.Key)
                .Select((o, i) => new object[]
                    {
                        string.Join('/', dChains[o.Key].OrderBy(p => p)),
                        string.Join('/', dResNames[o.Key].OrderBy(p => p)),
                        o.Key,
                        string.Join('/', dNumberings[o.Key].OrderBy(p => p)),
                    }
                    .Concat(headers.Select(p => p.name).Select(p => o.Value.TryGetValue(p, out double v) ? (object)v : 0.0))
                    .Concat(stateCounts.Select(p => $"=AVERAGE({ExcelColumnHelper.ToCellRef(p.col, i + 3)}:{ExcelColumnHelper.ToCellRef(p.col + p.count - 1, i + 3)})"))
                    .Append($"=AVERAGE({ExcelColumnHelper.ToCellRef(5, i + 3)}:{ExcelColumnHelper.ToCellRef(4 + headers.Count, i + 3)})")
                )
                // 1 blank row, for the convenience of sorting in Excel
                .Append(Array.Empty<object>())
                // 4 summary rows
                .Concat(summaryRowHeaders.Select((o, i) => new object[]
                    {
                        o,
                        null,
                        null,
                        null
                    }
                    .Concat(headers.Select(p => (object)p.summary[i]))
                    .Concat(stateCounts.Select(p => $"=AVERAGE({ExcelColumnHelper.ToCellRef(p.col, i + 4 + dChains.Count)}:{ExcelColumnHelper.ToCellRef(p.col + p.count - 1, i + 4 + dChains.Count)})"))
                    .Append($"=AVERAGE({ExcelColumnHelper.ToCellRef(5, i + 4 + dChains.Count)}:{ExcelColumnHelper.ToCellRef(4 + headers.Count, i + 4 + dChains.Count)})")
                ));

            summaryRows = outRows.ToArray();
            summaryHeaders = new[] { outHeaders1, outHeaders2 };

            rankingRows = Array.Empty<IEnumerable<object>>()
                // Scores
                .Append(rankings
                        .Select(o => o.Key)
                        .Prepend($"Top {rankingTop} Scores"))
                .Append(rankings
                        .Select(o => pdbStateDict.GetValueOrDefault(o.Key.Split('_')[0], ReceptorState.Unknown).ToString())
                        .Prepend("State->"))
                .Concat(Enumerable.Range(0, rankingTop)
                        .Select(i => rankings.Select(o => (object)o.Value[i].score).Prepend(i + 1).ToArray()))
                .Append(Array.Empty<object>())
                // Numberings
                .Append(rankings
                        .Select(o => o.Key)
                        .Prepend($"Top {rankingTop} Numberings"))
                .Append(rankings
                        .Select(o => pdbStateDict.GetValueOrDefault(o.Key.Split('_')[0], ReceptorState.Unknown).ToString())
                        .Prepend("State->"))
                .Concat(Enumerable.Range(0, rankingTop)
                        .Select(i => rankings.Select(o => (object)o.Value[i].numbering).Prepend(i + 1).ToArray()))
                .Append(Array.Empty<object>())
                // ResSeqs
                .Append(rankings
                        .Select(o => o.Key)
                        .Prepend($"Top {rankingTop} ResSeqs"))
                .Append(rankings
                        .Select(o => pdbStateDict.GetValueOrDefault(o.Key.Split('_')[0], ReceptorState.Unknown).ToString())
                        .Prepend("State->"))
                .Concat(Enumerable.Range(0, rankingTop)
                        .Select(i => rankings.Select(o => (object)$"{o.Value[i].resCode}{o.Value[i].resSeq}").Prepend(i + 1).ToArray()))
                .Append(Array.Empty<object>())
                // ResNames
                .Append(rankings
                        .Select(o => o.Key)
                        .Prepend($"Top {rankingTop} ResNames"))
                .Append(rankings
                        .Select(o => pdbStateDict.GetValueOrDefault(o.Key.Split('_')[0], ReceptorState.Unknown).ToString())
                        .Prepend("State->"))
                .Concat(Enumerable.Range(0, rankingTop)
                        .Select(i => rankings.Select(o => (object)o.Value[i].resName).Prepend(i + 1).ToArray()))
                .ToArray();

            var scoDataFrame = new MapDataFrame<string, string>
            (
                headers.Select // column vectors, no specific order required since they're being reordered while clustering
                (
                    o => new MapVector<string>
                    (
                        rankings[o.name].ToDictionary
                        (
                            p => p.numbering, // row key
                            p => p.score // score value
                        ),
                        o.name, // column key
                        o.state.ToString() // column tag
                    )
                ),
                null, // no row tags
                null, // no row tag type
                "PDB ID" // column tag type
            );

            var simDataFrames = similarityVdms.Select
            (
                vdm => new MapDataFrame<string, string>
                (
                    headers.Select
                    (
                        a => new MapVector<string>
                        (
                            headers.ToDictionary
                            (
                                b => b.name, // row key
                                b => 1 - vdm.Measure(scoDataFrame.GetColumn(a.name), scoDataFrame.GetColumn(b.name)) // similarity value
                            ),
                            a.name, // column key
                            a.state.ToString() // column tag
                        )
                    ),
                    headers.ToDictionary(o => o.name, o => o.state.ToString()), // row tags
                    "State", // row tag type
                    "State" // column tag type
                )
            ).ToArray();

            var orderedKeys = rankings
                .Select(o => (key: o.Key, state: pdbStateDict.GetValueOrDefault(o.Key.Split('_')[0], ReceptorState.Unknown)))
                .OrderBy(o => o.state)
                .ThenBy(o => o.key)
                .ToArray();

            similarityRows = simDataFrames.Select
            (
                df => Array.Empty<IEnumerable<object>>()
                .Append
                (
                    orderedKeys // pdb id headers
                        .Select(o => o.key)
                        .Prepend("PDB Id")
                        .Prepend("State")
                )
                .Append
                (
                    orderedKeys // state headers
                        .Select(o => o.state.ToString())
                        .Prepend("State->")
                        .Prepend(null)
                )
                .Concat
                (
                    orderedKeys // similarity data
                        .Select(a => orderedKeys
                            .Select(b => (object)df[a.key, b.key])
                            .Prepend(a.key)
                            .Prepend(a.state.ToString())
                        ).ToArray()
                )
                .ToArray()
            ).ToArray();

            scoreDataFrame = scoDataFrame;
            similarityDataFrames = simDataFrames;

            return (summaryHeaders.Length + scoreDict.Count + 1 + 4, stateColHeaders.Length + headers.Count + stateCounts.Count + 1);
        }

        private static Dictionary<string, (int rows, int columns)> GenerateUniProtSummaryXlsxRows(
            IReadOnlyDictionary<string, string[]> csvFilePathSets,
            IReadOnlyDictionary<string, ReceptorState> pdbStateDict,
            ConcurrentDictionary<string, IReadOnlyDictionary<int, string>> numberingDict,
            IVectorDistanceMeasure[] similarityVdms,
            out IReadOnlyDictionary<string, Dictionary<string, double>> averageDictSets,
            out IReadOnlyDictionary<string, IEnumerable<object>[]> summaryRowSets,
            out IReadOnlyDictionary<string, IEnumerable<string>[]> summaryHeaderSets,
            out IReadOnlyDictionary<string, IEnumerable<object>[]> rankingRowSets,
            out IReadOnlyDictionary<string, IEnumerable<object>[][]> similarityRowSets,
            out IReadOnlyDictionary<string, MapDataFrame<string, string>> scoreDataFrames,
            out IReadOnlyDictionary<string, MapDataFrame<string, string>[]> similarityDataFrames,
            int rankingTop)
        {
            const string allState = "All";

            //                       (K)state_str  (V)uniprot_id (V)count
            var countSets = new Dictionary<string, List<int>>();

            // Only headers for uniprot average scores.
            //                     (K)state_str  (V)key list (proteinName,uniProtId,species,class)
            var keySets = new Dictionary<string, List<string>>();

            //                           (K)state_str (V)uniprot_id (K)numbering (V)total_score
            var scoreDictSets = new Dictionary<string, List<Dictionary<string, double>>>();

            //                         (K)state_str       (K)numbering  (V)average_score
            var avgDictSets = new Dictionary<string, Dictionary<string, double>>();

            // [0] Intra-Ligand Free, [1] Inter-Ligand Free, [2] Total Free Energy, [3] Normalized Total Free Energy
            //                       (K)state_str (V)id  (V)total_score[4]
            var summarySets = new Dictionary<string, List<double[]>>();

            // The key is in format: proteinName,uniProtId,species,class
            foreach (var (key, csvFilePaths) in csvFilePathSets.OrderBy(o => o.Key))
            {
                string uniProtId = key.Split(',')[1];

                Logger.Current.LogInformation($"Summarizing {uniProtId}");
                var numberings = GetNumberingsForUniProtId(uniProtId, numberingDict);

                //                        (K)state_str  (V)count
                var stateCount = new Dictionary<string, int>();
                //                       (K)state_str       (K)numbering  (V)total_score
                var scoreDict = new Dictionary<string, Dictionary<string, double>>();
                //                     (K)state_str  (V)total_score[4]
                var summary = new Dictionary<string, double[]>();

                foreach (string csvFilePath in csvFilePaths)
                {
                    string[][] rows = File.ReadAllLines(csvFilePath).Select(o => CsvHelper.SplitCsvFields(o)).Where(o => o.Length > 1).ToArray();
                    if (rows.Length - 7 < rankingTop)
                    {
                        Logger.Current.LogWarning($"File {csvFilePath} has less than {rankingTop} valid result lines");
                        continue;
                    }

                    var (_, _, pdbId, _, _, _) = ExtractFields(csvFilePath);
                    string state = pdbStateDict.GetValueOrDefault(pdbId, ReceptorState.Unknown).ToString();

                    stateCount.TryAdd(state, 0);
                    stateCount[state]++;
                    stateCount.TryAdd(allState, 0);
                    stateCount[allState]++;

                    scoreDict.TryAdd(state, new Dictionary<string, double>());
                    scoreDict.TryAdd(allState, new Dictionary<string, double>());

                    summary.TryAdd(state, new double[4]);
                    summary.TryAdd(allState, new double[4]);

                    if (!rows.All(o => o.Length >= 4))
                        throw new Exception($"Not all rows are with 4 or more fields in file {csvFilePath}");
                    if (!Enumerable.Range(0, 3).All(i => rows[0][i] == csvColHeaders[i]))
                        throw new Exception($"Header {Enumerable.Range(0, 3).First(i => rows[0][i] != csvColHeaders[i])} is not recognized in file {csvFilePath}");
                    if (rows[0][3] != csvColHeaders[4] && rows[0][3] != csvColHeaders[5])
                        throw new Exception($"Header {rows[0][3]} is not recognized in file {csvFilePath}");
                    if (rows.Count(o => string.IsNullOrEmpty(o[1]) && string.IsNullOrEmpty(o[2])) != 4)
                        throw new Exception($"Not exactly 4 summary rows in file {csvFilePath}");

                    // Skipping the header row.
                    int summaryId = 0;
                    foreach (string[] row in rows.Skip(1))
                    {
                        // For a score row.
                        if (!string.IsNullOrEmpty(row[1]) && !string.IsNullOrEmpty(row[2]))
                        {
                            int resSeq = int.Parse(row[2]);
                            if (!double.TryParse(row[3], out double score))
                                score = 0.0;

                            // Only scores with numbering will be stated.
                            if (numberings.TryGetValue(resSeq, out string numbering))
                            {
                                scoreDict[state].TryAdd(numbering, 0);
                                scoreDict[allState].TryAdd(numbering, 0);
                                scoreDict[state][numbering] += score;
                                scoreDict[allState][numbering] += score;
                            }
                        }
                        // For a summary row.
                        else
                        {
                            if (summaryRowHeaders[summaryId] != row[0])
                                throw new Exception($"Summary header '{row[0]}' not recognized.");
                            double score = double.Parse(row[3]);
                            summary[state][summaryId] += score;
                            summary[allState][summaryId] += score;
                            summaryId++;
                        }
                    }
                }

                // Merge into global results and calculate sum from average.
                foreach (var (state, count) in stateCount)
                {
                    countSets.TryAdd(state, new List<int>());
                    countSets[state].Add(count);

                    keySets.TryAdd(state, new List<string>());
                    keySets[state].Add(key);

                    summarySets.TryAdd(state, new List<double[]>());
                    summarySets[state].Add(summary[state].Select(o => o / count).ToArray());

                    scoreDictSets.TryAdd(state, new List<Dictionary<string, double>>());
                    scoreDictSets[state].Add(scoreDict[state].ToDictionary(o => o.Key, o => o.Value / count));

                    avgDictSets.TryAdd(state, new Dictionary<string, double>());
                    foreach (var (numbering, totalScore) in scoreDict[state])
                    {
                        avgDictSets[state].TryAdd(numbering, 0);
                        avgDictSets[state][numbering] += totalScore;
                    }
                }
            }

            // Transform total scores into average scores.
            foreach (string state in countSets.Keys)
            {
                foreach (string numbering in avgDictSets[state].Keys.ToArray())
                {
                    avgDictSets[state][numbering] /= countSets[state].Sum();
                }
            }

            averageDictSets = avgDictSets;

            summaryRowSets = scoreDictSets.ToDictionary(o => o.Key, o =>
                // average score rows
                o.Value
                    .SelectMany(p => p.Keys)
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(numbering =>
                        new object[] { numbering, avgDictSets[o.Key][numbering] }
                        .Concat(o.Value.Select(q => q.TryGetValue(numbering, out double v) ? v : (object)0.0))
                    )
                // 1 blank row, for the convenience of sorting in Excel
                .Append(Array.Empty<object>())
                // 4 summary rows
                .Concat(Enumerable.Range(0, 4).Select(i => new object[] { summaryRowHeaders[i], null }.Concat(summarySets[o.Key].Select(p => (object)p[i]))))
                .ToArray()
            );

            summaryHeaderSets = keySets.ToDictionary(o => o.Key, o => new[]
            {
                new[] { defaultSchemeName, "Average" }.Concat(o.Value.Select(p => p.Split(',')[0])),
                new[] { "UniProtId->", $"{allState}({countSets[o.Key].Sum()})" }.Concat(o.Value.Select((p, i) => $"{p.Split(',')[1]}({countSets[o.Key][i]})")),
                new[] { "Species->", null }.Concat(o.Value.Select((p, i) => $"{p.Split(',')[2]}")),
                Array.Empty<string>(),
            });

            var scoreRankingSets = scoreDictSets.ToDictionary(
                o => o.Key,
                o => o.Value.Select(
                    p => p.Select(
                        q => (numbering: q.Key, score: q.Value)
                    )
                    .OrderBy(q => q.score)
                    .ToArray()
                )
                .ToArray()
            );

            rankingRowSets = keySets.Keys.ToDictionary(key => key, key =>
                Array.Empty<IEnumerable<object>>()
                // Protein Names
                .Append(keySets[key]
                    .Select(o => o.Split(',')[0])
                    .Prepend($"Top {rankingTop} Scores"))
                // UniProt Ids
                .Append(keySets[key]
                    .Select((o, i) => $"{o.Split(',')[1]}({countSets[key][i]})")
                    .Prepend("UniProtId->"))
                // Species
                .Append(keySets[key]
                    .Select(o => o.Split(',')[2])
                    .Prepend("Species->"))
                // Scores
                .Concat(
                    Enumerable.Range(0, rankingTop)
                    .Select
                    (
                        i => Enumerable
                            .Range(0, scoreRankingSets[key].Length)
                            .Select(j => (object)scoreRankingSets[key][j][i].score)
                            .Prepend(i + 1)
                    )
                )
                .Append(Array.Empty<object>())
                // Protein Names
                .Append(keySets[key]
                    .Select(o => o.Split(',')[0])
                    .Prepend($"Top {rankingTop} Numberings"))
                // UniProt Ids
                .Append(keySets[key]
                    .Select((o, i) => $"{o.Split(',')[1]}({countSets[key][i]})")
                    .Prepend("UniProtId->"))
                // Species
                .Append(keySets[key]
                    .Select(o => o.Split(',')[2])
                    .Prepend("Species->"))
                // Numberings
                .Concat(
                    Enumerable.Range(0, rankingTop)
                    .Select
                    (
                        i => Enumerable
                            .Range(0, scoreRankingSets[key].Length)
                            .Select(j => (object)scoreRankingSets[key][j][i].numbering)
                            .Prepend(i + 1)
                    )
                )
                .ToArray()
            );

            var scoDataFrames = scoreRankingSets
                .ToDictionary
                (
                    o => o.Key,
                    o => new MapDataFrame<string, string>
                    (
                        o.Value.Select
                        (
                            (p, i) => new MapVector<string>
                            (
                                p.ToDictionary
                                (
                                    q => q.numbering, // row key
                                    q => q.score // score value
                                ),
                                keySets[o.Key][i].Split(',')[0], // column key
                                keySets[o.Key][i].Split(',')[3] // column tag
                            )
                        ),
                        null, // no row tags
                        null, // no row tag type
                        "Class" // column tag type
                    )
                );

            var simDataFrames = keySets.Keys.ToDictionary(
                key => key,
                key => similarityVdms.Select
                (
                    vdm => new MapDataFrame<string, string>
                    (
                        Enumerable
                            .Range(0, keySets[key].Count)
                            .Select
                            (
                                a => new MapVector<string>
                                (
                                    Enumerable.Range(0, keySets[key].Count)
                                        .ToDictionary
                                        (
                                            b => keySets[key][b].Split(',')[0], // row key
                                            b => 1 - vdm.Measure(scoDataFrames[key].GetColumnAt(a), scoDataFrames[key].GetColumnAt(b)) // similarity value
                                        ),
                                    keySets[key][a].Split(',')[0], // column key
                                    keySets[key][a].Split(',')[3] // column tag
                                )
                            ),
                        keySets[key].Select(o => o.Split(',')).ToDictionary(o => o[0], o => o[3]), // row tags
                        "Class", // row tag type
                        "Class" // column tag type
                    )
                ).ToArray()
            );

            similarityRowSets = keySets.Keys.ToDictionary
            (
                key => key,
                key => simDataFrames[key].Select
                (
                    df => Array.Empty<IEnumerable<object>>()
                    .Append // Protein Names
                    (
                        keySets[key]
                            .Select(o => o.Split(',')[0])
                            .Prepend("Species")
                            .Prepend("UniProtId")
                            .Prepend("Class")
                            .Prepend("Protein")
                    )
                    .Append // Classes
                    (
                        keySets[key]
                            .Select(o => o.Split(',')[3])
                            .Prepend("Class->")
                            .Prepend(null)
                            .Prepend(null)
                            .Prepend(null)
                    )
                    .Append // UniProt Ids
                    (
                        keySets[key]
                            .Select((o, i) => $"{o.Split(',')[1]}({countSets[key][i]})")
                            .Prepend("UniProtId->")
                            .Prepend(null)
                            .Prepend(null)
                            .Prepend(null)
                    )
                    .Append // Species
                    (
                        keySets[key]
                            .Select(o => o.Split(',')[2])
                            .Prepend("Species->")
                            .Prepend(null)
                            .Prepend(null)
                            .Prepend(null)
                    )
                    .Concat // Similarity data
                    (
                        keySets[key]
                            .Select(a => a.Split(','))
                            .Select
                            (
                                (a, i) => keySets[key]
                                .Select(b => b.Split(',')[0])
                                .Select(b => (object)df[a[0], b])
                                .Prepend(a[2])
                                .Prepend($"{a[1]}({countSets[key][i]})")
                                .Prepend(a[3])
                                .Prepend(a[0])
                            )
                            .ToArray()
                    )
                    .ToArray()
                )
                .ToArray()
            );

            scoreDataFrames = scoDataFrames;
            similarityDataFrames = simDataFrames;

            int numHeaderRows = summaryHeaderSets.First().Value.Length;
            return scoreDictSets.ToDictionary(o => o.Key, o => (numHeaderRows + avgDictSets[o.Key].Keys.Count + 1 + 4, 2 + o.Value.Count));
        }

        private static readonly IDictionary<string, string> nameCache = new ConcurrentDictionary<string, string>();

        private static string GetColumnName(string proteinName, string uniProtId)
        {
            if (nameCache.ContainsKey(uniProtId))
                return nameCache[uniProtId];
            var (_, species, _, _) = GpcrnHelper.GetProperties(uniProtId);
            if (species == "HUMAN")
                return nameCache[uniProtId] = proteinName;
            return nameCache[uniProtId] = $"{proteinName}({species[0]})";
        }

        // All 11 number schemes except the Common arrestin numbering scheme can be fetched.
        // data from https://gpcrdb.org/residue/residuetable
        private static void Main(string[] args)
        {
            string workDir;
            string[] ligandNames;

            if (args.Length == 0)
            {
                Console.WriteLine("program <dir-path> <ligand-set>");
                return;
            }

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            Logger.Current = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            workDir = args[0];

            if (args.Length == 1)
            {
                ligandNames = new[] { "ligand" };
            }
            else
            {
                ligandNames = new[]
                {
                    "DMT_NONselec",
                    "Ergotamine_5HT2Bselec",
                    "MDA_NONSELEC",
                    "donitriptan_5HT1Bselec",
                    "serotonin_NONselec",
                };
            };

            Logger.Current.LogInformation($"Fetching numbering schemes for all GPCR crystal structures...");

            // Pipeline bag for fetching chemical data.
            // pdb id => pdb state
            var pdbStateDict = (IReadOnlyDictionary<string, ReceptorState>)new Dictionary<string, object>()
                .FetchGpcrDbHtml()
                .ParseGpcrDbStructures()
                .GetGpcrDbPdbList()
                .ToDictionary(o => o.PdbEntry, o => o.State);

            var sw = Stopwatch.StartNew();
            Logger.Current.LogInformation("Timing started");

            var numberingDict = new ConcurrentDictionary<string, IReadOnlyDictionary<int, string>>(); // uniprot id => numberings

            foreach (string ligand in ligandNames)
            {
                Parallel.ForEach(Directory.GetDirectories(workDir), proteinDir =>
                {
                    string proteinName = Path.GetFileNameWithoutExtension(proteinDir);
                    Logger.Current.LogInformation($"Working on protein {proteinName}");

                    /**********************************/
                    /* residue numberings for protein */
                    /**********************************/

                    // Read {ligand}_total.csv, add a numbering column and emit a new xlsx file.
                    foreach (string file in Directory.GetFiles(proteinDir, $"{ligand}_total.csv", SearchOption.AllDirectories))
                    {
                        var (_, _, pdbId, uniProtId, _, position) = ExtractFields(file);
                        string target = Path.Combine(Path.GetDirectoryName(file), $"{(position == null ? pdbId : $"{pdbId}_{position}")}_{ligand}+numbering.xlsx");
                        if (File.Exists(target))
                        {
                            Logger.Current.LogInformation($"Skipping existing report {Path.GetRelativePath(workDir, target)}");
                            continue;
                        }

                        Logger.Current.LogInformation($"Found PDB file '{Path.GetRelativePath(workDir, file)}' {uniProtId}");

                        var (rows, columns) = GenerateNumberingsXlsxRows(
                            file,
                            uniProtId,
                            pdbId,
                            pdbStateDict,
                            numberingDict,
                            out var outRows,
                            out var outHeaders);

                        using var xlsxDoc = target.OpenXlsxFile();
                        xlsxDoc
                            .AppendWorksheet($"{pdbId} Residue Report", outRows, outHeaders)
                            .FreezePanel(0, 4, 2)
                            .AddColorScales(0, 5, (uint)columns, 3, (uint)rows - 5, sheetColors)
                            .Close();
                    }

                    /********************************/
                    /* summary for per protein pdbs */
                    /********************************/
                    foreach (string postfix in postfixList)
                    {
                        foreach (var group in Directory.GetFiles(proteinDir, $"{ligand}_{postfix}.csv", SearchOption.AllDirectories).GroupBy(o => ExtractFields(o).uniprotId))
                        {
                            // initiate measure instances
                            IVectorDistanceMeasure cosineVdm = new CachedVectorDistanceMeasure(new CosineDistanceMeasure());
                            IVectorDistanceMeasure correlationVdm = new CachedVectorDistanceMeasure(new CorrelationDistanceMeasure());
                            IClusterDistanceMeasure farthestCdm = new FarthestPointMethod();

                            // score vectors => clustering score vectors
                            var scoreClustColVdms = new[] { correlationVdm, cosineVdm };
                            var scoreClustColCdm = farthestCdm;

                            // score vectors => similarity matrix => clustering similarity matrix
                            var similarityVdms = new[] { correlationVdm, cosineVdm };
                            var simClustRowVdm = cosineVdm;
                            var simClustRowCdm = farthestCdm;
                            var simClustColVdm = correlationVdm;
                            var simClustColCdm = farthestCdm;


                            string uniProtId = group.Key;

                            Logger.Current.LogInformation($"Summarizing uniprot {uniProtId}_{postfix}");

                            // Read every {ligand}_{postfix}.csv, extract all the first score columns and put together into uniprot summary xlsx file.
                            var (rows, columns) = GeneratePdbSummaryXlsxRows(
                                group.ToArray(),
                                uniProtId,
                                pdbStateDict,
                                numberingDict,
                                similarityVdms,
                                out var summaryRows,
                                out var summaryHeaders,
                                out var rankingRows,
                                out var similarityRows,
                                out var scoreDataFrame,
                                out var similarityDataFrames,
                                rankingTop);

                            // Write out to an xlsx document.
                            string target = Path.Combine(proteinDir, $"{uniProtId}+{ligand}_{postfix}_summary.xlsx");
                            if (File.Exists(target))
                            {
                                Logger.Current.LogInformation($"Skipping existing summary {Path.GetRelativePath(workDir, target)}");
                            }
                            else
                            {
                                using var xlsxDoc = target.OpenXlsxFile();
                                xlsxDoc
                                    .AppendWorksheet($"{uniProtId}_{postfix} Summary", summaryRows, summaryHeaders)
                                    .FreezePanel(0, 4, 2)
                                    .AddColorScales(0, 5, (uint)columns, 3, (uint)rows - 5, sheetColors)

                                    .AppendWorksheet($"{uniProtId}_{postfix} Rankings", rankingRows)
                                    .AddColorScales(1, 2, (uint)similarityRows[0].Length - 1, 3, 2 + rankingTop, sheetColors);

                                for (int i = 0; i < scoreClustColVdms.Length; i++)
                                {
                                    var vdm = scoreClustColVdms[i];
                                    var simRows = similarityRows[i];

                                    xlsxDoc
                                        .AppendWorksheet($"{uniProtId}_{postfix} Sml {vdm.Name.Substring(0, 6)}", simRows)
                                        .FreezePanel(2 + i, 2, 2)
                                        .AddColorScales(2 + i, 3, (uint)simRows.Length, 3, (uint)simRows.Length, sheetColors);
                                }

                                xlsxDoc.Close();
                            }

                            // Plot the cluster maps to png files: 1st degree, only column distance measure
                            foreach (var vdm in scoreClustColVdms)
                            {
                                string pngfile = Path.Combine(proteinDir, $"{uniProtId}+{ligand}_{postfix}_[col_{vdm.Name}+{scoreClustColCdm.Name}].png");
                                if (File.Exists(pngfile))
                                {
                                    Logger.Current.LogInformation($"Skipping existing heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                }
                                else if (scoreDataFrame.ColumnCount < 2 || scoreDataFrame.RowCount < 2)
                                {
                                    Logger.Current.LogInformation($"Skipping blank heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                }
                                else
                                {
                                    // A row corresponds to a residue and the row elements each corresponds to a PDB id,
                                    // So the average is simply the sum over the row length.
                                    scoreDataFrame.OrderRowsBy(o => o.Values.Sum() / o.Length);

                                    var columnClustInfo = new ClusteringInfo<string, string>(scoreClustColCdm, vdm, scoreDataFrame.Columns);
                                    if (columnClustInfo.Nodes.Count < 1)
                                    {
                                        Logger.Current.LogInformation($"Skipping blank heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                    }
                                    else
                                    {
                                        Logger.Current.LogInformation($"Emitting heatmap {Path.GetRelativePath(workDir, pngfile)}");

                                        ClusteringPlot.PlotHeatmap(
                                            pngfile,
                                            scoreDataFrame,
                                            6000, // canvasWidth
                                            true, // autoHeight
                                            false, // leftAlignedBadVectors
                                            false, // upToDownForVerticalText
                                            heatmapColors,
                                            gpcrStateColors.Select(o => (o.Key, o.Value)).ToArray(),
                                            Color.Black,
                                            Color.Black,
                                            null,
                                            columnClustInfo,
                                            // in case no state color is available, hide the color bar
                                            null,
                                            scoreDataFrame.Columns.Any(o => gpcrStateColors.ContainsKey(o.Tag)) ? (Func<string, Color>)StateColorFn : null
                                        );
                                    }
                                }
                            }

                            // Plot the cluster maps to png files: 2nd degree => similarity matrix + row distance measure + column distance measure
                            for (int i = 0; i < similarityVdms.Length; i++)
                            {
                                var vdm = similarityVdms[i];
                                var df = similarityDataFrames[i];

                                string pngfile = Path.Combine(proteinDir, $"{uniProtId}+{ligand}_{postfix}_[sim_{vdm.Name}][row_{simClustRowVdm.Name}+{simClustRowCdm.Name},col_{simClustColVdm.Name}+{simClustColCdm.Name}].png");
                                if (File.Exists(pngfile))
                                {
                                    Logger.Current.LogInformation($"Skipping existing heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                }
                                else if (df.ColumnCount < 2 || df.RowCount < 2)
                                {
                                    Logger.Current.LogInformation($"Skipping blank heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                }
                                else
                                {
                                    var rowClustInfo = new ClusteringInfo<string, string>(simClustRowCdm, simClustRowVdm, df.Rows);
                                    var columnClustInfo = new ClusteringInfo<string, string>(simClustColCdm, simClustColVdm, df.Columns);
                                    if (rowClustInfo.NodeCount < 1 || columnClustInfo.NodeCount < 1)
                                    {
                                        Logger.Current.LogInformation($"Skipping blank heatmap {Path.GetRelativePath(workDir, pngfile)}");
                                    }
                                    else
                                    {
                                        Logger.Current.LogInformation($"Emitting heatmap {Path.GetRelativePath(workDir, pngfile)}");

                                        ClusteringPlot.PlotHeatmap(
                                            pngfile,
                                            df,
                                            6000, // canvasWidth
                                            false, // autoHeight
                                            false, // leftAlignedBadVectors
                                            false, // upToDownForVerticalText
                                            heatmapColors,
                                            gpcrStateColors.Select(o => (o.Key, o.Value)).ToArray(),
                                            Color.Black,
                                            Color.Black,
                                            rowClustInfo,
                                            columnClustInfo,
                                            // in case no state color is available, hide the color bar
                                            df.Rows.Any(o => gpcrStateColors.ContainsKey(o.Tag)) ? (Func<string, Color>)StateColorFn : null,
                                            df.Columns.Any(o => gpcrStateColors.ContainsKey(o.Tag)) ? (Func<string, Color>)StateColorFn : null
                                        );
                                    }
                                }
                            }
                        }
                    }
                });

                /****************************/
                /* summary for all uniprots */
                /****************************/
                Parallel.ForEach(postfixList, postfix =>
                {
                    // initiate measure instances
                    IVectorDistanceMeasure cosineVdm = new CachedVectorDistanceMeasure(new CosineDistanceMeasure());
                    IVectorDistanceMeasure correlationVdm = new CachedVectorDistanceMeasure(new CorrelationDistanceMeasure());
                    IClusterDistanceMeasure nearestCdm = new NearestPointMethod();
                    IClusterDistanceMeasure farthestCdm = new FarthestPointMethod();

                    // score vectors => clustering score vectors
                    var scoreClustColVdms = new[] { correlationVdm, cosineVdm };
                    var scoreClustColCdm = farthestCdm;

                    // score vectors => similarity matrix => clustering similarity matrix
                    var similarityVdms = new[] { correlationVdm, cosineVdm };
                    var simClustRowVdm = cosineVdm;
                    var simClustRowCdm = farthestCdm;
                    var simClustColVdm = correlationVdm;
                    var simClustColCdm = farthestCdm;

                    Logger.Current.LogInformation($"Summarizing all_{postfix} reports");

                    // Keys in format proteinName,uniProtId,species
                    var csvPathSets = Directory
                        .GetFiles(workDir, $"{ligand}_{postfix}.csv", SearchOption.AllDirectories)
                        .Select(o => new { key = ExtractFields(o), path = o })
                        .GroupBy(o => $"{GetColumnName(o.key.proteinName, o.key.uniprotId)},{o.key.uniprotId},{o.key.species},{o.key.@class}", o => o.path)
                        .ToDictionary(o => o.Key, o => o.ToArray());

                    // Summarize all receptors into a single xlsx file.
                    var dimensions = GenerateUniProtSummaryXlsxRows(
                        csvPathSets,
                        pdbStateDict,
                        numberingDict,
                        similarityVdms,
                        out var averageDictSets,
                        out var summaryRowSets,
                        out var summaryHeaderSets,
                        out var rankingRowSets,
                        out var similarityRowSets,
                        out var scoreDataFrames,
                        out var similarityDataFrames,
                        rankingTop);

                    // Write out to an xlsx document with sheets named in the order of all(unknown)/active/intermediate/inactive.
                    string target = Path.Combine(workDir, $"all+{ligand}_{postfix}_summary.xlsx");
                    if (File.Exists(target))
                    {
                        Logger.Current.LogInformation($"Skipping existing summary {Path.GetRelativePath(workDir, target)}");
                    }
                    else
                    {
                        using var xlsxDoc = target.OpenXlsxFile();
                        Logger.Current.LogInformation($"Emittig summary {Path.GetRelativePath(workDir, target)}");
                        int n = 0;

                        foreach (var (state, summaryRows) in summaryRowSets.OrderBy(o => (int)o.Key.ParseState()))
                        {
                            xlsxDoc
                                .AppendWorksheet($"{shortKeys[state]} Smr", summaryRows, summaryHeaderSets[state])
                                .FreezePanel(n, 2, 4)
                                .AddColorScales(n++, 2, (uint)dimensions[state].columns, 5, (uint)dimensions[state].rows - 5, sheetColors);
                        }

                        foreach (var (state, rankingRows) in rankingRowSets.OrderBy(o => (int)o.Key.ParseState()))
                        {
                            xlsxDoc
                                .AppendWorksheet($"{shortKeys[state]} Rnk", rankingRows)
                                .AddColorScales(n++, 2, (uint)similarityRowSets[state][0].Length - 1, 4, 3 + rankingTop, sheetColors);
                        }

                        foreach (var (state, similarityRows) in similarityRowSets.OrderBy(o => (int)o.Key.ParseState()))
                        {
                            for (int i = 0; i < similarityVdms.Length; i++)
                            {
                                xlsxDoc
                                    .AppendWorksheet($"{shortKeys[state]} Sml {similarityVdms[i].Name.Substring(0, 3)}", similarityRows[i])
                                    .FreezePanel(n, 4, 4)
                                    .AddColorScales(n++, 5, (uint)similarityRows[i].Length, 5, (uint)similarityRows[i].Length, sheetColors);
                            }
                        }

                        xlsxDoc.Close();
                    }

                    // Plot the cluster maps to png files: 1st degree, only column distance measure
                    foreach (var vdm in scoreClustColVdms)
                    {
                        foreach (var (key, df) in scoreDataFrames)
                        {
                            string pngfile = Path.Combine(workDir, $"{key.ToLower()}+{ligand}_{postfix}_[col_{vdm.Name}+{scoreClustColCdm.Name}].png");
                            if (File.Exists(pngfile))
                            {
                                Logger.Current.LogInformation($"Skipping existing heatmap {Path.GetRelativePath(workDir, pngfile)}");
                            }
                            else
                            {
                                // A row corresponds to a residue and the row elements each corresponds to a target,
                                // However the ordering should be by the weighted average, i.e. the average of the score across all PDBs.
                                df.OrderRowsBy(o => averageDictSets[key][o.Name]);
                                df.OrderColumnsBy(o => (o as IVector<string, string, string>).Tag + o.Name);

                                var columnClustInfo = new ClusteringInfo<string, string>(scoreClustColCdm, vdm, df.Columns);
                                Logger.Current.LogInformation($"Emitting heatmap {Path.GetRelativePath(workDir, pngfile)}");

                                ClusteringPlot.PlotHeatmap(
                                    pngfile,
                                    df,
                                    6000, // canvasWidth
                                    true, // autoHeight
                                    false, // leftAlignedBadVectors
                                    false, // upToDownForVerticalText
                                    heatmapColors,
                                    gpcrClassColors.Select(o => (o.Key, o.Value)).ToArray(),
                                    Color.Black,
                                    Color.Black,
                                    null,
                                    columnClustInfo,
                                    // in case no class color is available, hide the color bar
                                    null,
                                    df.Columns.Any(o => gpcrClassColors.ContainsKey(o.Tag)) ? (Func<string, Color>)ClassColorFn : null
                                );
                            }
                        }
                    }

                    // Plot the cluster maps to png files: 2nd degree => similarity matrix + row distance measure + column distance measure
                    foreach (var (key, dfs) in similarityDataFrames)
                    {
                        for (int i = 0; i < similarityVdms.Length; i++)
                        {
                            var vdm = similarityVdms[i];
                            var df = dfs[i];

                            string pngfile = Path.Combine(workDir, $"{key.ToLower()}+{ligand}_{postfix}_[sim_{vdm.Name}][row_{simClustRowVdm.Name}+{simClustRowCdm.Name},col_{simClustColVdm.Name}+{simClustColCdm.Name}].png");
                            if (File.Exists(pngfile))
                            {
                                Logger.Current.LogInformation($"Skipping existing heatmap {Path.GetRelativePath(workDir, pngfile)}");
                            }
                            else
                            {
                                var rowClustInfo = new ClusteringInfo<string, string>(simClustRowCdm, simClustRowVdm, df.Rows);
                                var columnClustInfo = new ClusteringInfo<string, string>(simClustColCdm, simClustColVdm, df.Columns);
                                Logger.Current.LogInformation($"Emitting heatmap {Path.GetRelativePath(workDir, pngfile)}");

                                ClusteringPlot.PlotHeatmap(
                                    pngfile,
                                    df,
                                    6000, // canvasWidth
                                    false, // autoHeight
                                    false, // leftAlignedBadVectors
                                    false, // upToDownForVerticalText
                                    heatmapColors,
                                    gpcrClassColors.Select(o => (o.Key, o.Value)).ToArray(),
                                    Color.Black,
                                    Color.Black,
                                    rowClustInfo,
                                    columnClustInfo,
                                    // in case no class color is available, hide the color bar
                                    df.Rows.Any(o => gpcrClassColors.ContainsKey(o.Tag)) ? (Func<string, Color>)ClassColorFn : null,
                                    df.Columns.Any(o => gpcrClassColors.ContainsKey(o.Tag)) ? (Func<string, Color>)ClassColorFn : null
                                );
                            }
                        }
                    }
                });
            } // end of foreach in ligandNames

            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
            Thread.Sleep(500);
        } // end of Main()
    }
}
