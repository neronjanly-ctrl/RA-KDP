using ChemicalFunctions;
using CommonTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FetchPDBsForGPCRASD
{
    internal class Program
    {
        private static void FetchGpcrDbPdbs()
        {
            var sw = Stopwatch.StartNew();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "GPCRdb_Proteins");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var bag = new ConcurrentBag<string[]>();

            var rs = new Dictionary<string, object>()
                .FetchGpcrDbHtml()
                .ParseGpcrDbStructures()
                .GetGpcrDbPdbList();

            string[] headers = new[] { "UniProtId", "Symbol", "Species", "PDB Entry", "IUPHAR", "Class", "Family", "Resolution", "Chain", "State", "Distance", "Download Url", "View PDB Url" };

            Parallel.ForEach(rs, pdb =>
            {
                // use gene name if protein name is uniprot id
                if (pdb.ProteinName == pdb.UniProtId)
                {
                    pdb.ProteinName = new Dictionary<string, object>()
                        .SetUniProtId(pdb.UniProtId)
                        .FetchUniProtHtml()
                        .ParseHgncIdAndGeneName()
                        .GetGeneName();
                }
                pdb.ProteinName = pdb.ProteinName.ToUpper();

                string proteinOutput = Path.Combine(outputDir, pdb.ProteinName);
                if (!Directory.Exists(proteinOutput))
                    Directory.CreateDirectory(proteinOutput);

                var b = new Dictionary<string, object>()
                    .SetPdbEntry(pdb.PdbEntry)
                    .DownloadPdbOrCifAndSplit(proteinOutput, pdb.ProteinName, $"{pdb.PdbEntry}_{pdb.UniProtId}_[{pdb.Species.Replace(' ', '_')}]");

                string[] infoLines = new string[] {
                    pdb.UniProtId, pdb.ProteinName, pdb.Species, pdb.PdbEntry, pdb.Iuphar, pdb.Class, pdb.Family, pdb.Resolution, pdb.Chain, pdb.State.ToString(), pdb.Distance, b.GetPdbOrCifDownloadUri(), b.GetRcsbDownloadUri()
                };
                bag.Add(infoLines);
                File.WriteAllLines(Path.Combine(proteinOutput, $"{pdb.PdbEntry}.txt"), infoLines);

                if (Directory.GetFiles(proteinOutput).IsEmpty())
                    Directory.Delete(proteinOutput);
            });

            File.WriteAllLines(Path.Combine(outputDir, "GPCR_Proteins.csv"), CsvHelper.FormatCsvRows(bag.OrderBy(o => o[0]).ThenBy(o => o[3]), headers));
            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
        }

        private static void FetchAsdDbPdbs()
        {
            var sw = Stopwatch.StartNew();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "ASD_Proteins");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var bag = new ConcurrentBag<string[]>();

            var rs = new Dictionary<string, object>()
                .FetchAsdDbJson()
                .ParseAsdDbStructures()
                .GetAsdDbPdbList()
                .GroupBy(o => o.TargetName.ToLower().Trim());

            string[] headers = new[] { "Target Name", "Target Domain", "Reference Title", "PubMed Id", "Organism", "Modulator Alias", "Allosteric Site", "Substrate Site", "Download Uri", "RCSB Uri" };

            Parallel.ForEach(rs, target =>
            {
                string proteinName = target.First().TargetName;
                string proteinOutput = Path.Combine(outputDir, proteinName), substrateDir = Path.Combine(proteinOutput, "Substrate");
                if (!Directory.Exists(proteinOutput))
                    Directory.CreateDirectory(proteinOutput);

                if (!Directory.Exists(substrateDir))
                    Directory.CreateDirectory(substrateDir);

                Parallel.ForEach(target.Select(o => o.SubstratePdb).Where(o => o != null).Distinct(), pdb =>
                {
                    if (pdb == null)
                        return;
                    var b = new Dictionary<string, object>()
                        .SetPdbEntry(pdb)
                        .DownloadPdbOrCifAndSplit(substrateDir, proteinName, null);
                });

                if (Directory.GetFiles(substrateDir).IsEmpty())
                    Directory.Delete(substrateDir);

                Parallel.ForEach(target, pdb =>
                {
                    var b = new Dictionary<string, object>()
                        .SetPdbEntry(pdb.AllostericPdb)
                        .DownloadPdbOrCifAndSplit(proteinOutput, pdb.TargetName, null);

                    string[] infoLines = new[] {
                        pdb.TargetName, pdb.TargetDomain, pdb.RefTitle, pdb.PubmedId, pdb.Organism, pdb.ModulatorAlias, pdb.AllostericSite, pdb.SubstrateSite, b.GetPdbOrCifDownloadUri(), b.GetRcsbDownloadUri()
                    };
                    bag.Add(infoLines);
                    File.WriteAllLines(Path.Combine(proteinOutput, $"{pdb.AllostericPdb}.txt"), infoLines);
                });

                if (Directory.GetFiles(proteinOutput).IsEmpty())
                    Directory.Delete(proteinOutput);
            });

            File.WriteAllLines(Path.Combine(outputDir, "ASD_Proteins.csv"), CsvHelper.FormatCsvRows(bag, headers));
            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
        }

        private static void FetchDrugBankPdbs(Dictionary<string, string> uniprots, string dbname)
        {
            var sw = Stopwatch.StartNew();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", dbname);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] headers = new[] { "UniProtId", "Method", "PDB Entry", "Species", "Organisms", "Resolution", "Chain", "Positions", "Categories", "Download Url", "View PDB Url" };

            var summary = new ConcurrentBag<string[]>();

            Parallel.ForEach(uniprots, kv =>
            {
                var (uniprotid, cates) = kv;
                var bag = new Dictionary<string, object>()
                    .SetUniProtId(uniprotid)
                    .FetchUniProtHtml();

                string geneName = bag.ParseHgncIdAndGeneName().GetGeneName();
                string proteinName = bag.ParseProteinName().GetProteinName();
                string proteinName2 = bag.GetSubmittedProteinName();
                string species = bag.GetSpecies();

                if (proteinName == uniprotid || proteinName == null)
                    proteinName = geneName;
                if (proteinName == null)
                    proteinName = proteinName2;

                var pdbs = bag.ParseUniProtStructures().GetUniProtPdbList();
                string uniprotDir = Path.Combine(outputDir, proteinName);
                if (!Directory.Exists(uniprotDir))
                    Directory.CreateDirectory(uniprotDir);

                string[] organisms = bag.ParseOrganisms().GetOrganisms();
                string organism = organisms.Length == 0 ? species : organisms.Length == 1 ? organisms[0] : organisms[1];
                organism = organism.Split('/', StringSplitOptions.RemoveEmptyEntries).Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToArray().First();

                Parallel.ForEach(pdbs, pdb =>
                {
                    if (pdb == null)
                        return;

                    var b = new Dictionary<string, object>()
                        .SetPdbEntry(pdb.PdbEntry)
                        .DownloadPdbOrCifAndSplit(uniprotDir, proteinName, $"{pdb.PdbEntry}_{pdb.UniProtId}_[{organism.Replace(' ', '_')}]");

                    if (b.GetPdbOrCifDownloadUri() == null)
                        return;

                    string[] infoLines = new[] {
                        pdb.UniProtId, pdb.Method, pdb.PdbEntry, species, string.Join("|", organisms), pdb.Resolution, pdb.Chain, pdb.Positions, cates, b.GetPdbOrCifDownloadUri(), b.GetRcsbDownloadUri()
                    }.Select(o => o ?? "").ToArray();

                    summary.Add(infoLines);
                    string txtFileName = Path.ChangeExtension(b.GetPdbOrCifFilePath(), ".txt");
                    File.WriteAllLines(txtFileName, infoLines);
                });

                if (Directory.GetFiles(uniprotDir).IsEmpty() && Directory.GetDirectories(uniprotDir).IsEmpty())
                    Directory.Delete(uniprotDir);
            });

            File.WriteAllLines(Path.Combine(outputDir, $"{dbname}.csv"), CsvHelper.FormatCsvRows(summary, headers));
            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
        }

        private static void FetchTinasPdbs()
        {
            string[] targets = new[]
            {
                "O00206", "O15399", "O60858", "P01189", "P03372", "P07204", "P07359", "P08183", "P10415", "P11229",
                "P12104", "P13569", "P16220", "P19652", "P20309", "P23141", "P23219", "P23975", "P28335", "P30926",
                "P31151", "P31645", "P32297", "P35354", "P35372", "P37231", "P41143", "P41145", "P41146", "P46098",
                "Q01959", "Q05586", "Q07869", "Q12879", "Q13224", "Q14957", "Q5T1J1", "Q693P7", "Q8TCU5", "Q99720",
                "Q9GZZ6", "Q9Y6Y9",
            };

            string[] enzymes = new[]
            {
                "D6RB81", "O60656", "P05177", "P06133", "P08684", "P10632", "P10635", "P11509", "P11511", "P11712",
                "P16435", "P16662", "P20813", "P20815", "P22309", "P23219", "P24462", "P33260", "P33261", "P35354",
                "P35503",
            };

            var uniprots = targets.Union(enzymes).Distinct().OrderBy(o => o).ToDictionary(o => o, o =>
            {
                var vals = new List<string>();
                if (targets.Contains(o))
                    vals.Add("Targets");
                if (enzymes.Contains(o))
                    vals.Add("Enzymes");
                return string.Join(' ', vals);
            });

            FetchDrugBankPdbs(uniprots, "TinasOpioids_Proteins");
        }

        private static void FetchMingzhePdbs()
        {
            // PR drug list - mingzhe
            string[] targets = new string[]
            {
                "D2YW37", "G1SNM3", "O00206", "O00408", "O00519", "O00555", "O00591", "O14684", "O14764", "O14788",
                "O14920", "O14939", "O15111", "O15399", "O15440", "O15530", "O15554", "O35433", "O43497", "O43525",
                "O43526", "O43741", "O60218", "O60391", "O60656", "O60760", "O60858", "O75311", "O75469", "O75762",
                "O75899", "O76074", "O95180", "O95264", "P00734", "P00742", "P00748", "P00918", "P01034", "P01106",
                "P01137", "P01138", "P01178", "P01189", "P01258", "P01266", "P01282", "P01375", "P01579", "P01584",
                "P01834", "P01859", "P02741", "P02788", "P03372", "P03886", "P04054", "P04278", "P04629", "P04637",
                "P05067", "P05106", "P05129", "P05164", "P05231", "P05305", "P05362", "P05979", "P06133", "P06241",
                "P06276", "P06307", "P06401", "P06870", "P06881", "P07204", "P07359", "P07451", "P07477", "P07550",
                "P07711", "P08138", "P08172", "P08173", "P08183", "P08473", "P08648", "P08684", "P08908", "P08909",
                "P08912", "P08913", "P09210", "P09211", "P09917", "P09958", "P0DMS8", "P10092", "P10145", "P10276",
                "P10415", "P10478", "P10826", "P11021", "P11166", "P11229", "P11388", "P11473", "P11511", "P11712",
                "P12004", "P12104", "P12821", "P13500", "P13569", "P13631", "P14174", "P14416", "P14555", "P14770",
                "P14867", "P15121", "P15144", "P15309", "P15502", "P16050", "P16083", "P16152", "P16220", "P16662",
                "P17787", "P18089", "P18146", "P18505", "P18507", "P18509", "P18825", "P19652", "P19793", "P19838",
                "P20309", "P20366", "P20594", "P20815", "P21397", "P21430", "P21554", "P21728", "P21731", "P21796",
                "P21817", "P21917", "P21918", "P21964", "P22301", "P22303", "P22309", "P23141", "P23219", "P23415",
                "P23975", "P24385", "P24462", "P24530", "P24557", "P24752", "P25021", "P25024", "P25100", "P25101",
                "P25103", "P25116", "P25963", "P26378", "P27338", "P27361", "P27815", "P28221", "P28222", "P28223",
                "P28335", "P28472", "P28482", "P28566", "P28702", "P29274", "P29275", "P29460", "P29466", "P29474",
                "P29475", "P29717", "P30049", "P30411", "P30532", "P30542", "P30793", "P30874", "P30926", "P30939",
                "P31151", "P31644", "P31645", "P31749", "P32121", "P32297", "P34903", "P34913", "P34969", "P34972",
                "P35228", "P35346", "P35348", "P35354", "P35367", "P35368", "P35372", "P35462", "P35498", "P35503",
                "P35869", "P36544", "P37231", "P37840", "P41143", "P41145", "P41146", "P41594", "P41595", "P42261",
                "P42330", "P42345", "P42574", "P42866", "P43681", "P46098", "P47712", "P47775", "P47869", "P47870",
                "P48039", "P48061", "P48145", "P48167", "P48169", "P48443", "P49286", "P49585", "P50052", "P50406",
                "P51580", "P51584", "P51608", "P51800", "P51812", "P54289", "P54577", "P54619", "P54646", "P56373",
                "P56817", "P60568", "P61073", "P62942", "P68400", "P78334", "P98066", "P98194", "P9WJI5", "Q00535",
                "Q00653", "Q00975", "Q01469", "Q01959", "Q02127", "Q02641", "Q03181", "Q04206", "Q04760", "Q04828",
                "Q05586", "Q06432", "Q07343", "Q07812", "Q07869", "Q08289", "Q08493", "Q08499", "Q09470", "Q12809",
                "Q12879", "Q13131", "Q13224", "Q13393", "Q13698", "Q13936", "Q14116", "Q14330", "Q14957", "Q14994",
                "Q15822", "Q15858", "Q16445", "Q16552", "Q16602", "Q16620", "Q16647", "Q2M3G0", "Q494W8", "Q495T6",
                "Q51911", "Q55813", "Q5T1J1", "Q693P7", "Q7Z2W7", "Q86T13", "Q8MU52", "Q8N1C3", "Q8N8N7", "Q8NER1",
                "Q8NET8", "Q8NFZ4", "Q8SPQ9", "Q8TCG2", "Q8TCT1", "Q8TCU5", "Q8TD43", "Q92945", "Q969S8", "Q96EB6",
                "Q96FT7", "Q99572", "Q99720", "Q99928", "Q9GZZ6", "Q9H244", "Q9H3N8", "Q9HBA0", "Q9NR96", "Q9NY46",
                "Q9NY47", "Q9NY72", "Q9NZK7", "Q9P0X4", "Q9UBN7", "Q9UBS5", "Q9UGI9", "Q9UGJ0", "Q9UGM1", "Q9UHC3",
                "Q9UI33", "Q9UN88", "Q9UNQ0", "Q9UPY5", "Q9Y2I1", "Q9Y2T6", "Q9Y478", "Q9Y5K3", "Q9Y5S1", "Q9Y5Y4",
                "Q9Y5Y9", "Q9Y6K9", "Q9Y6Y9", "U3JD03",
                "Q9JLT4", "Q16881"
            };

            string[] enzymes = new string[]
            {
                "D6RB81", "F1T0I5", "F5GY58", "O00519", "O60427", "O60656", "O95864", "P00390", "P00441", "P00813",
                "P04035", "P04040", "P04054", "P04798", "P05093", "P05164", "P05177", "P05181", "P06133", "P06276",
                "P07203", "P08263", "P08684", "P09211", "P09488", "P09917", "P0DMM9", "P10632", "P10635", "P11245",
                "P11509", "P11511", "P11712", "P14555", "P14902", "P16050", "P16435", "P16662", "P17405", "P17516",
                "P18440", "P19224", "P20711", "P20813", "P20815", "P21397", "P22309", "P23141", "P23219", "P24462",
                "P24752", "P27338", "P28329", "P28845", "P31513", "P33260", "P33261", "P35354", "P35503", "P35790",
                "P36537", "P42330", "P47712", "P48775", "P49888", "P50225", "P51589", "P52209", "P54687", "P54855",
                "P55263", "Q02083", "Q04828", "Q06520", "Q08257", "Q14097", "Q16678", "Q16696", "Q6IQ20", "Q8NE62",
                "Q9FAW5", "Q9HAW7", "Q9HAW8", "Q9HAW9", "Q9HB55", "Q9NYP7", "Q9Y259", "Q9Y6K0",
            };

            string[] transporters = new string[]
            {
                "O15244", "O15245", "O15438", "O15439", "O43525", "O43526", "O60669", "O75751", "O76082", "O94956",
                "O95255", "O95342", "P08183", "P33527", "P35499", "P43005", "P46721", "P53985", "P78348", "Q01650",
                "Q14973", "Q15311", "Q4U2R8", "Q53GD3", "Q63ZE4", "Q7Z2H8", "Q8IWA5", "Q8N4M1", "Q8TCC7", "Q8WWI5",
                "Q92887", "Q96J66", "Q96NT5", "Q99808", "Q9GZV3", "Q9H015", "Q9HAS3", "Q9NPD5", "Q9NSA0", "Q9NYB5",
                "Q9UNQ0", "Q9Y694", "Q9Y6L6",
            };

            string[] carriers = new string[]
            {
                "P02763", "P02766", "P02768", "P05543", "P19652", "P61769",
            };

            var uniprots = targets.Union(enzymes).Union(transporters).Union(carriers).Distinct().OrderBy(o => o).ToDictionary(o => o, o =>
            {
                var vals = new List<string>();
                if (targets.Contains(o))
                    vals.Add("Targets");
                if (enzymes.Contains(o))
                    vals.Add("Enzymes");
                if (transporters.Contains(o))
                    vals.Add("Transporters");
                if (carriers.Contains(o))
                    vals.Add("Carriers");
                return string.Join(' ', vals);
            });

            FetchDrugBankPdbs(uniprots, "MingsPR_Proteins");
        }

        private static void Fetch3CLPdbs()
        {
            string[] pdbs = new string[]
            {
                "5R7Y", "5R7Z", "5R80", "5R81", "5R82", "5R83", "5R84", "5R8T", "5RE4", "5RE5",
                "5RE6", "5RE7", "5RE8", "5RE9", "5REA", "5REB", "5REC", "5RED", "5REE", "5REF",
                "5REG", "5REH", "5REI", "5REJ", "5REK", "5REL", "5REM", "5REN", "5RER", "5RES",
                "5RET", "5REU", "5REV", "5REW", "5REX", "5REY", "5REZ", "5RF0", "5RF1", "5RF2",
                "5RF3", "5RF4", "5RF5", "5RF6", "5RF7", "5RF8", "5RF9", "5RFA", "5RFB", "5RFC",
                "5RFD", "5RFE", "5RFF", "5RFG", "5RFH", "5RFI", "5RFJ", "5RFK", "5RFL", "5RFM",
                "5RFN", "5RFO", "5RFP", "5RFQ", "5RFR", "5RFS", "5RFT", "5RFU", "5RFV", "5RFW",
                "5RFX", "5RFY", "5RFZ", "5RG0", "5RG1", "5RG2", "5RG3", "5RGG", "5RGH", "5RGI",
                "5RGJ", "5RGK", "5RGL", "5RGM", "5RGN", "5RGO", "5RGP", "5RGQ", "5RGR", "5RGS",
                "5RGT", "5RGV", "5RGW", "5RGX", "5RGY", "5RGZ", "5RH0", "5RH1", "5RH2", "5RH3",
                "5RH4", "5RH5", "5RH6", "5RH7", "5RH8", "5RH9", "5RHA", "5RHB", "5RHC", "5RHD",
                "5RHE", "5RHF", "5RL0", "5RL1", "5RL2", "5RL3", "5RL4", "5RL5", "5RG5", "6LU7",
                "6LZE", "6M03", "6M0K", "6M2N", "6M2Q", "6W63", "6WNP", "6WQF", "6WTJ", "6WTK",
                "6WTM", "6WTT", "6XA4", "6XB0", "6XB1", "6XB2", "6XBG", "6XBH", "6XBI", "6XCH",
                "6XFN", "6XHM", "6XHU", "6XKF", "6XKH", "6XMK", "6XOA", "6XQS", "6XQT", "6XQU",
                "6XR3", "6Y2E", "6Y2F", "6Y2G", "6Y84", "6YB7", "6YNQ", "6YVF", "6YZ6", "6Z2E",
                "6ZRT", "6ZRU", "7A1U", "7ABU", "7ADW", "7AF0", "7AGA", "7AHA", "7AK4", "7AKU",
                "7ALH", "7ALI", "7AMJ", "7ANS", "7AP6", "7APH", "7AQI", "7AQJ", "7AR5", "7AR6",
                "7ARF", "7AVD", "7AWR", "7AWS", "7AWU", "7AWW", "7AX6", "7AXM", "7AXO", "7AY7",
                "7B3E", "7B83", "7BQY", "7BRO", "7BRP", "7BUY", "7C6S", "7C6U", "7C7P", "7C8B",
                "7C8R", "7C8T", "7C8U", "7CBT", "7COM", "7CWB", "7CWC", "7CX9", "7D1M", "7D1O",
                "7JFQ", "7JKV", "7JOY", "7JP1", "7JPY", "7JPZ", "7JQ0", "7JQ1", "7JQ2", "7JQ3",
                "7JQ4", "7JQ5", "7JR3", "7JR4", "7JU7", "7JUN", "7JVZ", "7JYC", "7K3T", "7K40",
                "7K6D", "7K6E", "7KFI", "7KFJ", "7KHP", "7KPH", "7KS5", "7KVL", "7KVR", "7KX5",
                "7KYU", "7L5D", "7LDX",
            };

            var sw = Stopwatch.StartNew();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "3CL_Proteins");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            Parallel.ForEach(pdbs, pdb =>
            {
                if (pdb == null)
                    return;
                var b = new Dictionary<string, object>()
                    .SetPdbEntry(pdb)
                    .DownloadPdbOrCifAndSplit(outputDir, pdb, pdb);
            });

            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
        }

        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            Logger.Current = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            //FetchGpcrDbPdbs();
            //FetchAsdDbPdbs();
            //FetchMingzhePdbs();
            //FetchTinasPdbs();
            Fetch3CLPdbs();
        }
    }
}
