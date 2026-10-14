using ChemicalFunctions;
using CommonTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FetchPdbForViruses
{
    internal class Program
    {
        private static readonly Dictionary<string, (string virusSymbol, string proteinSymbol, string uniprotId)> Data
            = new Dictionary<string, (string, string, string)>
        {
            {"4D00", ("A-H10N7", "HA", "P12581")},
            {"4XQO", ("A-H10N8", "HA", "A0A059T4A1")},
            {"2CML", ("A-H11N6", "NA", "Q6XV27")},
            {"6HFY", ("A-H11N6", "NA", "Q6XV27")},
            {"6HG5", ("A-H11N6", "NA", "Q6XV27")},
            {"1A14", ("A-H11N9", "NA", "P03472")},
            {"1F8B", ("A-H11N9", "NA", "P03472")},
            {"6HEB", ("A-H11N9", "NA", "P03472")},
            {"3SAL", ("A-H12N5", "NA", "A1ILL9")},
            {"3SAN", ("A-H12N5", "NA", "A1ILL9")},
            {"3TI8", ("A-H12N5", "NA", "A1ILL9")},
            {"4KPQ", ("A-H13N6", "HA", "P13103")},
            {"1NMB", ("A-H13N9", "NA", "P05803")},
            {"3EYK", ("A-H14N6", "HA", "P26137")},
            {"5TG8", ("A-H15N9", "HA", "L0L3X3")},
            {"4I78", ("A-H17N10", "HA", "H6QM93")},
            {"4GDI", ("A-H17N10", "NA", "H6QM85")},
            {"4NFZ", ("A-H17N10", "PA", "H6QM92")},
            {"4K3X", ("A-H18N11", "HA", null)},
            {"4K3Y", ("A-H18N11", "NA", null)},
            {"3HTP", ("A-H1N1", "HA", "C7C6F1")},
            {"3HTT", ("A-H1N1", "HA", "C7C6F1")},
            {"6G01", ("A-H1N1", "NA", "C6KP13")},
            {"3RO5", ("A-H1N1", "NP", "Q1K9H2")},
            {"3TG6", ("A-H1N1", "NP", "Q1K9H2")},
            {"6J1U", ("A-H1N1", "NP", "Q1K9H2")},
            {"5W92", ("A-H1N1", "PA", "C3W5S0")},
            {"5WA6", ("A-H1N1", "PA", "C3W5S0")},
            {"5WE7", ("A-H1N1", "PA", "C3W5S0")},
            {"4HFU", ("A-H2N2", "HA", "P03451")},
            {"4HG4", ("A-H2N2", "HA", "P03451")},
            {"1IVC", ("A-H2N2", "NA", "P06820")},
            {"1IVD", ("A-H2N2", "NA", "P06820")},
            {"1IVE", ("A-H2N2", "NA", "P06820")},
            {"2YP3", ("A-H3N2", "HA", "K7N5L2")},
            {"2YP4", ("A-H3N2", "HA", "K7N5L2")},
            {"2YP5", ("A-H3N2", "HA", "K7N5L2")},
            {"2AEP", ("A-H3N2", "NA", "P84751")},
            {"2AEQ", ("A-H3N2", "NA", "P84751")},
            {"5F79", ("A-H3N2", "PB2", "Q30NP1")},
            {"5JUR", ("A-H3N2", "PB2", "Q30NP1")},
            {"5JUN", ("A-H3N2", "PB3", "Q30NP1")},
            {"5BUH", ("A-H3N2", "PB4", "Q30NP1")},
            {"4UNX", ("A-H3N8", "HA", "Q82847")},
            {"4UNY", ("A-H3N8", "HA", "Q82847")},
            {"4UNZ", ("A-H3N8", "HA", "Q82847")},
            {"4D8S", ("A-H3N8", "NA", "Q07599")},
            {"4MJU", ("A-H3N8", "NA", "Q07599")},
            {"4MJV", ("A-H3N8", "NA", "Q07599")},
            {"3ZNK", ("A-H5N1", "HA", "Q6DQ34")},
            {"3ZP0", ("A-H5N1", "HA", "Q6DQ34")},
            {"3ZP1", ("A-H5N1", "HA", "Q6DQ34")},
            {"6BKK", ("A-H5N1", "MX2", "Q9Q0L9")},
            {"5HUG", ("A-H5N1", "NA", "A0A0C5BL75")},
            {"2Q06", ("A-H5N1", "NP", "Q9PX50")},
            {"3HW3", ("A-H5N1", "PA", "Q9Q0U9")},
            {"3HW4", ("A-H5N1", "PA", "Q9Q0U9")},
            {"3HW5", ("A-H5N1", "PA", "Q9Q0U9")},
            {"4CB4", ("A-H5N1", "PB2", "Q2LG68")},
            {"4CB5", ("A-H5N1", "PB2", "Q2LG68")},
            {"4CB6", ("A-H5N1", "PB2", "Q2LG68")},
            {"1JSN", ("A-H5N3", "HA", "A5Z226")},
            {"1JSO", ("A-H5N3", "HA", "A5Z226")},
            {"4XKE", ("A-H6N1", "HA", "A0A0J9X268")},
            {"5BQY", ("A-H6N6", "HA", "A0A067YZ73")},
            {"3M5H", ("A-H7N2", "HA", "B7NY59")},
            {"3M5I", ("A-H7N2", "HA", "B7NY59")},
            {"3M5J", ("A-H7N2", "HA", "B7NY59")},
            {"4DJ7", ("A-H7N7", "HA", "Q6VMK1")},
            {"4DJ8", ("A-H7N7", "HA", "Q6VMK1")},
            {"4BSE", ("A-H7N9", "HA", "M4YV75")},
            {"5L15", ("A-H7N9", "NA", "R4NFR6")},
            {"5L17", ("A-H7N9", "NA", "R4NFR6")},
            {"5L18", ("A-H7N9", "NA", "R4NFR6")},
            {"3CL4", ("BCV", "HE", "P15776")},
            {"3CL5", ("BCV", "HE", "P15776")},
            {"5JQ7", ("EBOV", "GP", "Q05320")},
            {"5JQB", ("EBOV", "GP", "Q05320")},
            {"6NAE", ("EBOV", "GP", "Q05320")},
            {"4IBG", ("EBOV", "VP35", "Q05127")},
            {"4IBD", ("EBOV", "VP36", "Q05127")},
            {"4IBC", ("EBOV", "VP37", "Q05127")},
            {"4ZRO", ("FCoV", "PR", "Q98VG9")},
            {"3EWR", ("HCoV-229E", "NSP3", "P0C6U2")},
            {"2J97", ("HCoV-229E", "NSP9", "P0C6X1")},
            {"2J98", ("HCoV-229E", "NSP9", "P0C6X1")},
            {"1P9S", ("HCoV-229E", "PR", "P0C6U2")},
            {"2ZU2", ("HCoV-229E", "PR", "P0C6U2")},
            {"6ATK", ("HCoV-229E", "SPIKE", "P15423")},
            {"5N4K", ("HCoV-NL63", "NP", "Q6Q1R8")},
            {"3TLO", ("HCoV-NL63", "PR", "P0C6U6")},
            {"5GWY", ("HCoV-NL63", "PR", "P0C6U6")},
            {"3KBH", ("HCoV-NL63", "SPIKE", "Q6Q1S2")},
            {"5N11", ("HCoV-OC43", "HE", "Q4VID6")},
            {"4KXJ", ("HCoV-OC43", "NP", "P33469")},
            {"4LI4", ("HCoV-OC43", "NP", "P33469")},
            {"4LM7", ("HCoV-OC43", "NP", "P33469")},
            {"6NZK", ("HCoV-OC43", "SPIKE", "Q696P8")},
            {"2OBQ", ("HCV1A", "NS3", "P27958")},
            {"2OC0", ("HCV1A", "NS3", "P27958")},
            {"2OC1", ("HCV1A", "NS3", "P27958")},
            {"3HKW", ("HCV1A", "NS5B", "P26664")},
            {"3QGH", ("HCV1A", "NS5B", "P26664")},
            {"3QGI", ("HCV1A", "NS5B", "P26664")},
            {"2A4Q", ("HCV1B", "NS3", "Q91RS4")},
            {"2F9U", ("HCV1B", "NS3", "Q91RS4")},
            {"2OBO", ("HCV1B", "NS3", "Q91RS4")},
            {"2HAI", ("HCV1B", "NS5B", "P26663")},
            {"2HWH", ("HCV1B", "NS5B", "P26663")},
            {"2HWI", ("HCV1B", "NS5B", "P26663")},
            {"5QJ0", ("HCV2A", "NS5B", "Q99IB8")},
            {"5TWM", ("HCV2A", "NS5B", "Q99IB8")},
            {"5EQR", ("HCV3A", "NS3", "A0A0B4WYC6")},
            {"5EQS", ("HCV3A", "NS3", "A0A0B4WYC6")},
            {"5ESB", ("HCV3A", "NS3", "A0A0B4WYC6")},
            {"5F4L", ("HIV1", "GP160", "C6G099")},
            {"5F4P", ("HIV1", "GP160", "C6G099")},
            {"5F4R", ("HIV1", "GP160", "C6G099")},
            {"3LPT", ("HIV1", "IN", "P12497")},
            {"3LPU", ("HIV1", "IN", "P12497")},
            {"1AID", ("HIV1", "PR", "P03369")},
            {"1D4K", ("HIV1", "PR", "P03369")},
            {"2AID", ("HIV1", "PR", "P03369")},
            {"1DTQ", ("HIV1", "RT", "P04585")},
            {"1REV", ("HIV1", "RT", "P04585")},
            {"3LP1", ("HIV1", "RT", "P04585")},
            {"5CAY", ("HIV2", "GP160", "P20872")},
            {"5UPJ", ("HIV2", "PR", "P04584")},
            {"6UPJ", ("HIV2", "PR", "P04584")},
            {"3D23", ("HKU1", "PR", "P0C6U3")},
            {"6MEA", ("HKU4", "MD", "P0C6W3")},
            {"6MEB", ("HKU4", "MD", "P0C6W3")},
            {"6MEN", ("HKU4", "MD", "P0C6W3")},
            {"4YOG", ("HKU4", "PR", "P0C6W3")},
            {"4YOI", ("HKU4", "PR", "P0C6W3")},
            {"4YOJ", ("HKU4", "PR", "P0C6W3")},
            {"4QZV", ("HKU4", "SPIKE", "A3EX94")},
            {"3EWP", ("IBV", "NSP3", "P0C6V5")},
            {"2Q6F", ("IBV", "PR", "P0C6V5")},
            {"6CV0", ("IBV", "SPIKE", "F4MIW6")},
            {"4ZI0", ("INFA", "PA", "P03433")},
            {"4ZQQ", ("INFA", "PA", "P03433")},
            {"5FDD", ("INFA", "PA", "P03433")},
            {"2RFT", ("INFB", "HA", "P03462")},
            {"2RFU", ("INFB", "HA", "P03462")},
            {"1INF", ("INFB", "NA", "P03474")},
            {"1IVB", ("INFB", "NA", "P03474")},
            {"3TJ0", ("INFB", "NP", "C4LQ26")},
            {"3R66", ("INFB", "NSP1", "P03502")},
            {"3SDL", ("INFB", "NSP1", "P03502")},
            {"6FS8", ("INFB", "PA", "Q5V8Z9")},
            {"5EFA", ("INFB", "PB2", "Q9QLL6")},
            {"5E64", ("INFD", "HA", "K9LG83")},
            {"5N2U", ("INFD", "NP", "A0A0E3VZU8")},
            {"5DUS", ("MERs", "MD", "K9N638")},
            {"5HIH", ("MERs", "MD", "K9N638")},
            {"4RSP", ("MERs", "PR", "V9TU12")},
            {"4YLU", ("MERs", "PR", "V9TU12")},
            {"5W9H", ("MERs", "SPIKE", "K9N5Q8")},
            {"5W9I", ("MERs", "SPIKE", "K9N5Q8")},
            {"5YY5", ("MERs", "SPIKE", "K9N5Q8")},
            {"5JIF", ("MHV", "HE", "O92367")},
            {"4YPT", ("MHV", "NSP3", "P0C6X9")},
            {"3R4D", ("MHV", "SPIKE", "Q9J3E7")},
            {"6B7N", ("PDCoV", "SPIKE", "A0A075E3D7")},
            {"5HYO", ("PEDV", "PR", "U6BPB2")},
            {"1SSK", ("SARS", "NP", "P59595")},
            {"2HSX", ("SARS", "NSP1", "P0C6X7")},
            {"2H85", ("SARS", "NSP15", "P0C6X7")},
            {"3MJ5", ("SARS", "PLP", "P0C6U8")},
            {"4OVZ", ("SARS", "PLP", "P0C6U8")},
            {"4OW0", ("SARS", "PLP", "P0C6U8")},
            {"2AMD", ("SARS", "PR", "P0C6X7")},
            {"2D2D", ("SARS", "PR", "P0C6X7")},
            {"4TWY", ("SARS", "PR", "P0C6X7")},
            {"2DD8", ("SARS", "SPIKE", "P59594")},
            {"3D0H", ("SARS", "SPIKE", "P59594")},
            {"6LU7", ("SARS2", "PR", null)},
            {"6VSB", ("SARS2", "SPIKE", null)},
            {"3MP2", ("TGEV", "PR", "P0C6V2")},
        };

        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            Logger.Current = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Virus_Proteins_031120_transformed");

            var summary = new ConcurrentBag<string[]>();

            foreach (var (pdbId, (virusSymbol, proteinSymbol, uniprotId)) in Data)
            {
                var bag = new Dictionary<string, object>();

                var newProteinId = $"{proteinSymbol}_{virusSymbol}";

                string newTargetDir = Path.Combine(outputDir, newProteinId);
                string modelDir = Path.Combine(newTargetDir, pdbId);
                if (!Directory.Exists(newTargetDir))
                    Directory.CreateDirectory(newTargetDir);

                if (uniprotId == null)
                {
                    Logger.Current.LogWarning($"No uniprot Ids for PDB: {pdbId}");

                    //string oldModelDir = Path.Combine(outputDir, "Unknown", pdbId);
                    if (!File.Exists(Path.Combine(modelDir, "AminoAcids.pdb")))
                    {
                        Logger.Current.LogError($"Cannot find {Path.Combine(modelDir, "AminoAcids.pdb")}");
                        continue;
                    }
                    //Directory.Move(oldModelDir, modelDir);

                    //bag.DownloadPdbOrCifAndSplit(modelDir, proteinSymbol, pdbId, true);

                    //if (bag.GetPdbOrCifDownloadUri() == null)
                    //{
                    //    Logger.Current.LogError($"PDB {pdbId} failed to download");
                    //}

                    if (Directory.GetFiles(modelDir).IsEmpty() && Directory.GetDirectories(modelDir).IsEmpty())
                        Directory.Delete(modelDir);

                    // Properties output
                    Logger.Current.LogInformation($"1|{pdbId}: Unknown");

                    string propFileName = Path.Combine(newTargetDir, "Properties.txt");
                    string props = $"GeneSymbol={proteinSymbol}\nProteinSymbol={proteinSymbol}\nOrganismSymbol={virusSymbol}\nProteinName=\nOrganism=\nSynonyms=\nLocusType=\n" +
                        $"ChromosomalLocation=\nGeneFamily=\nHgncId=\nUniProtId=\nEnsemblId=\nGenBankId=\nNcbiId=\nChemblId=\n";

                    File.WriteAllText(propFileName, props);

                    string[] infoLines = new[] {
                            null, null, pdbId, null, proteinSymbol, null, null, virusSymbol, null, null, null, null, null, bag.GetPdbDownloadUri(), bag.GetRcsbDownloadUri()
                        }.Select(o => o ?? "").ToArray();

                    summary.Add(infoLines);
                    string txtFileName = Path.Combine(modelDir, "Properties.txt");
                    string props2 = $"PdbId={pdbId}\nUniProtId=\nProteinSymbol={proteinSymbol}\nOrganismSymbol={virusSymbol}\nBindingCavity=orthosteric\n";
                    File.WriteAllText(txtFileName, props2);

                    continue;
                }
                else
                {
                    bag.SetUniProtId(uniprotId)
                        .FetchUniProtHtml()
                        .ParseChemblId();

                    string geneName = bag.ParseHgncIdAndGeneName().GetGeneName();
                    string proteinName = bag.ParseProteinName().GetProteinName();
                    string uniprotSymbol = bag.GetUniProtSymbol();
                    string proteinName2 = bag.GetSubmittedProteinName();
                    string organismSymbol = bag.GetSpecies();
                    string proteinNameFull = bag.GetProteinNameFull();

                    if (uniprotSymbol.EndsWith("_HUMAN"))
                        continue;

                    if ((proteinName == uniprotId || proteinName == null) && geneName != null)
                        proteinName = geneName;
                    if (proteinName == null)
                        proteinName = proteinName2;
                    proteinName = new string(proteinName.Where(o => char.IsLetterOrDigit(o)).ToArray());

                    string[] organisms = bag.ParseOrganisms().GetOrganisms();
                    string organism = organisms.Length == 0 ? organismSymbol : organisms.Length == 1 ? organisms[0] : organisms[1];
                    organism = organism.Split('/', StringSplitOptions.RemoveEmptyEntries).Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToArray().First();

                    string proteinSymbol2 = proteinName.ToUpper();
                    if (proteinSymbol2.Contains(' '))
                    {
                        proteinSymbol2 = proteinSymbol2.Split(' ')[0];
                        if (proteinSymbol2.Length > 5)
                            proteinSymbol2 = proteinSymbol2.Substring(0, 5);
                    }

                    string oldProteinId = $"{proteinSymbol2}_{organismSymbol}";
                    string oldModelName = $"{pdbId}_{uniprotId}_[{organism.Replace(' ', '_')}]";

                    //string oldModelDir = Path.Combine(outputDir, oldProteinId, oldModelName);
                    if (!File.Exists(Path.Combine(modelDir, "AminoAcids.pdb")))
                    {
                        Logger.Current.LogError($"Cannot find {Path.Combine(modelDir, "AminoAcids.pdb")}");
                        continue;
                    }
                    //Directory.Move(oldModelDir, modelDir);

                    //bag.DownloadPdbOrCifAndSplit(modelDir, proteinName, pdbId, true);

                    //if (bag.GetPdbOrCifDownloadUri() == null)
                    //{
                    //    Logger.Current.LogError($"PDB {pdbId} failed to download");
                    //}
                    //else
                    {
                        string propFileName = Path.Combine(newTargetDir, "Properties.txt");
                        string props = $"GeneSymbol={proteinSymbol}\nProteinSymbol={proteinSymbol}\nOrganismSymbol={virusSymbol}\nProteinName={proteinNameFull}\nOrganism={string.Join(",", organisms)}\nSynonyms=\nLocusType=\n" +
                            $"ChromosomalLocation=\nGeneFamily=\nHgncId={bag.GetHgncId()}\nUniProtId={uniprotId}\nEnsemblId=\nGenBankId=\nNcbiId=\nChemblId={bag.GetChemblId()}\n";

                        File.WriteAllText(propFileName, props);

                        // Properties output
                        var pdb = bag.ParseUniProtStructures().GetUniProtPdbList().SingleOrDefault(o => o.PdbEntry == pdbId);
                        string[] infoLines;
                        if (pdb != null)
                        {
                            infoLines = new[] {
                                pdb.UniProtId, pdb.Method, pdb.PdbEntry, geneName, proteinSymbol,
                                proteinNameFull, organismSymbol, string.Join("|", organisms), bag.GetHgncId()?.ToString(), bag.GetChemblId(),
                                pdb.Resolution, pdb.Chain, pdb.Positions, bag.GetPdbDownloadUri(), bag.GetRcsbDownloadUri()
                            }.Select(o => o ?? "").ToArray();
                        }
                        else
                        {
                            bag.SetPdbEntry(pdbId);
                            infoLines = new[] {
                                uniprotId, null, pdbId, geneName, proteinSymbol,
                                proteinNameFull, organismSymbol, string.Join("|", organisms), bag.GetHgncId()?.ToString(), bag.GetChemblId(),
                                null, null, null, bag.GetPdbDownloadUri(), bag.GetRcsbDownloadUri()
                            }.Select(o => o ?? "").ToArray();
                        }

                        summary.Add(infoLines);
                        string txtFileName = Path.Combine(modelDir, "Properties.txt");
                        string props2 = $"PdbId={pdbId}\nUniProtId={uniprotId}\nProteinSymbol={proteinSymbol}\nOrganismSymbol={virusSymbol}\nBindingCavity=orthosteric\n";
                        File.WriteAllText(txtFileName, props2);
                    }

                    if (Directory.GetFiles(modelDir).IsEmpty() && Directory.GetDirectories(modelDir).IsEmpty())
                        Directory.Delete(modelDir);

                    Logger.Current.LogInformation($"{pdbId}: {geneName}, {organism}: {uniprotSymbol}, {proteinSymbol}, {uniprotId}");
                }
            }

            string[] headers = new[] { "UniProtId", "Method", "PDB Entry", "GeneName", "ProteinSymbol", "ProteinName", "OrganismSymbol", "Organisms", "HgncId", "ChemblId", "Resolution", "Chain", "Positions", "Download Url", "View PDB Url" };
            File.WriteAllLines(Path.Combine(outputDir, $"Virus-CKB.csv"), CsvHelper.FormatCsvRows(summary, headers));
        }
    }
}
