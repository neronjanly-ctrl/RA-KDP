using CommonTools;
using CommonTools.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ParseDrugBank
{
    [XmlType("drug")]
    public class Drug
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("calculated-properties")]
        [XmlArrayItem("property")]
        public List<Property> CalculatedProperties { get; set; }

        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public List<GroupType> Groups { get; set; }
    }

    [XmlType("group-type")]
    public enum GroupType
    {
        [XmlEnum("approved")]
        Approved,

        [XmlEnum("illicit")]
        Illicit,

        [XmlEnum("experimental")]
        Experimental,

        [XmlEnum("withdrawn")]
        Withdrawn,

        [XmlEnum("nutraceutical")]
        Nutraceutical,

        [XmlEnum("investigational")]
        Investigational,

        [XmlEnum("vet_approved")]
        VetApproved,
    }

    [XmlType("kind")]
    public enum Kind
    {
        [XmlEnum("logP")]
        LogP,

        [XmlEnum("logS")]
        LogS,

        [XmlEnum("Water Solubility")]
        WaterSolubility,

        [XmlEnum("IUPAC Name")]
        IUPACName,

        [XmlEnum("Traditional IUPAC Name")]
        TraditionalIUPACName,

        [XmlEnum("Molecular Weight")]
        MolecularWeight,

        [XmlEnum("Monoisotopic Weight")]
        MonoisotopicWeight,

        [XmlEnum("SMILES")]
        SMILES,

        [XmlEnum("Molecular Formula")]
        MolecularFormula,

        [XmlEnum("InChI")]
        InChI,

        [XmlEnum("InChIKey")]
        InChIKey,

        [XmlEnum("Polar Surface Area (PSA)")]
        PolarSurfaceAreaPSA,

        [XmlEnum("Refractivity")]
        Refractivity,

        [XmlEnum("Polarizability")]
        Polarizability,

        [XmlEnum("Rotatable Bond Count")]
        RotatableBondCount,

        [XmlEnum("H Bond Acceptor Count")]
        HBondAcceptorCount,

        [XmlEnum("H Bond Donor Count")]
        HBondDonorCount,

        [XmlEnum("pKa (strongest acidic)")]
        pKaStrongestAcidic,

        [XmlEnum("pKa (strongest basic)")]
        pKaStrongestBasic,

        [XmlEnum("Physiological Charge")]
        PhysiologicalCharge,

        [XmlEnum("Number of Rings")]
        NumberOfRings,

        [XmlEnum("Bioavailability")]
        Bioavailability,

        [XmlEnum("Rule of Five")]
        RuleOfFive,

        [XmlEnum("Ghose Filter")]
        GhoseFilter,

        [XmlEnum("MDDR-Like Rule")]
        MDDRLikeRule,

        [XmlEnum("Veber's Rule")]
        VeberRule,
    }

    [XmlType("source")]
    public enum Source
    {
        [XmlEnum("ALOGPS")]
        ALOGPS,

        [XmlEnum("ChemAxon")]
        ChemAxon
    }

    [XmlType("property")]
    public class Property
    {
        [XmlElement("kind")]
        public Kind Kind { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("source")]
        public Source Source { get; set; }
    }

    [XmlRoot("drugbank", Namespace = "http://www.drugbank.ca")]
    public class DrugBank
    {
        [XmlElement("drug")]
        public List<Drug> Drugs { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            string workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "2019-nCoV");
            Console.WriteLine(workspace);

            // step 1: extracts smiles from drugbank database and turn them into pdbqts
            //ConvertDrugBankDbToPdbqts(workspace);

            // step 2: protonates pdbqts with propka and docks against the target with jdock

            // step 3: parse jdock results, computes the similarity for each result conformation in each drug, then find the most similar one in each drug
            ComputeSimilarities(workspace);
        }

        private static void ComputeSimilarities(string workspace)
        {
            string resultDir = Path.Combine(workspace, "Result_Approved");
            string bestMatchDir = Path.Combine(workspace, "BestMatch_Approved");
            const int maxTop = 100;

            IVectorDistanceMeasure dist = new CosineSimilarityMeasure();

            string[] postfixes = new[] {
                "_gauss",
                "_gauss1",
                "_gauss2",
                "_hbonding",
                "_hydrophobic",
                "_nonsteric",
                "_repulsion",
                "_steric",
                "_total",
            };

            foreach (string postfix in postfixes)
            {
                string corona = Path.Combine(workspace, "2019-nCoV_NoUniProtId_[2019-nCoV]", "out", $"ligand{postfix}.csv");

                var dict = File.ReadAllLines(corona).SkipLast(5).ParseCsvRows("Residue sequence", "Conf 1(Input)").ToDictionary(o => o[0], o => double.TryParse(o[1], out double val) ? val : 0.0);
                var nCoV = new MapVector<string>(dict, "2019-nCoV");

                var outputLines = new List<(string drug, string bestConf, double similarity, string[][] csv)>();
                foreach (string csvfile in Directory.GetFiles(resultDir, $"*{postfix}.csv", SearchOption.AllDirectories))
                {
                    string[] lines = File.ReadAllLines(csvfile);
                    string[] confs = lines[0].SplitCsvFields().Skip(3).ToArray();

                    string[][] data = lines.Skip(1).SkipLast(5).Select(o => o.SplitCsvFields().Skip(2).ToArray()).ToArray();
                    string[] resSeq = data.Select(o => o[0]).ToArray();

                    var vecs = Enumerable.Range(0, confs.Length)
                        .Select // column vectors, no specific order required since they're being reordered while clustering
                        (
                            i => (IVector<string>)new MapVector<string>
                            (
                                Enumerable.Range(0, resSeq.Length)
                                    .ToDictionary(
                                        j => resSeq[j], // row key
                                        j => double.TryParse(data[j][i + 1], out double val) ? val : 0.0 // score value
                                    ),
                                confs[i] // column key
                            )
                        ).ToArray();

                    var best = vecs.Select(o => new { vector = o, simimarity = dist.Measure(o, nCoV) }).OrderByDescending(o => o.simimarity).First();
                    string confName = (best.vector as MapVector<string>).Name;

                    string[][] newCsv = lines.ParseCsvRows("Chain ID", "Residue name", "Residue sequence", confName).ToArray();

                    outputLines.Add((new string(Path.GetFileNameWithoutExtension(csvfile).SkipLast(postfix.Length).ToArray()), confName, best.simimarity, newCsv));
                    Console.WriteLine(outputLines.Last());
                }

                string postfixDir = Path.Combine(bestMatchDir, postfix.TrimStart('_'));
                string targetDir = Path.Combine(postfixDir, "2019-nCoV_[2019-nCov]");
                Directory.CreateDirectory(targetDir);

                var topBestMatches = outputLines.OrderByDescending(o => o.similarity).Take(maxTop);

                int id = 1;
                foreach (var (drug, bestConf, similarity, csv) in topBestMatches)
                {
                    int confId = int.Parse(Regex.Match(bestConf, @"(\d+)").Groups[1].Value);
                    string outDir = Path.Combine(targetDir, $"D{id++}C{confId}_NoUniProtId_[{drug}]", "out");
                    Directory.CreateDirectory(outDir);

                    File.Copy(Path.Combine(resultDir, $"{drug}.pdbqt"), Path.Combine(outDir, "ligand.pdbqt"));
                    File.WriteAllLines(Path.Combine(outDir, $"ligand{postfix}.csv"), csv.FormatCsvRows(new[] { "Chain ID", "Residue name", "Residue sequence", "Conf 1" }));
                }

                string outputFile = Path.Combine(bestMatchDir, $"BestMatchReport{postfix}.csv");
                File.WriteAllLines(outputFile, outputLines
                    .Select(o => new[] { o.drug, o.bestConf, o.similarity.ToString() })
                    .FormatCsvRows(new[] { "Drug", "Best Conf", "Cosine Similarity" }));
            }
        }

        private static void ConvertDrugBankDbToPdbqts(string workspace)
        {
            string xmlPath = Path.Combine(workspace, "full database.xml");
            string csvPath = Path.Combine(workspace, "full database.csv");
            string pdbqtDir = Path.Combine(workspace, "DrugbankDrugs");

            var dict = new ConcurrentDictionary<string, string>();

            using (var istream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stringReader = new StreamReader(istream))
            {
                var serializer = new XmlSerializer(typeof(DrugBank));
                var db = (DrugBank)serializer.Deserialize(stringReader);

                using var ostream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var stringWriter = new StreamWriter(ostream);
                stringWriter.WriteLine("Drug,SMILES,Groups");

                foreach (var drug in db.Drugs)
                {
                    if (drug.Groups.Any(o => o == GroupType.Illicit || o == GroupType.Withdrawn || o == GroupType.VetApproved) || !drug.CalculatedProperties.Any(o => o.Kind == Kind.SMILES))
                        continue;

                    stringWriter.Write('"');
                    stringWriter.Write(drug.Name);
                    stringWriter.Write('"');
                    stringWriter.Write(',');

                    string smiles = null;
                    foreach (var prop in drug.CalculatedProperties)
                    {
                        if (prop.Kind == Kind.SMILES)
                        {
                            stringWriter.Write(prop.Value);
                            smiles = prop.Value;
                        }
                    }

                    if (dict.ContainsKey(drug.Name))
                    {
                        Console.WriteLine($"duplicate {drug.Name}");
                    }
                    dict[drug.Name] = smiles;

                    stringWriter.Write(',');
                    stringWriter.Write(string.Join(';', drug.Groups));
                    stringWriter.WriteLine();
                }
            }

            if (!Directory.Exists(pdbqtDir))
                Directory.CreateDirectory(pdbqtDir);

            Parallel.ForEach(dict, o =>
            {
                (string name, string smiles) = o;
                string pdbqtFileName = Path.Combine(pdbqtDir, Encode(name) + ".pdbqt");

                string pdbqt;
                if (!File.Exists(pdbqtFileName))
                {
                    pdbqt = smiles.SmilesToPdbqtVega();

                    if (string.IsNullOrEmpty(pdbqt))
                    {
                        Console.WriteLine($"Failed on {name}");
                        return;
                    }

                    File.WriteAllText(pdbqtFileName, pdbqt);
                }
                else
                {
                    pdbqt = File.ReadAllText(pdbqtFileName);
                }

                if (Regex.IsMatch(pdbqt, @"\d{4}"))
                {
                    Console.WriteLine($"Deleting bad pdbqt {name}");
                    File.Delete(pdbqtFileName);
                    return;
                }

                if (!File.Exists(Path.ChangeExtension(pdbqtFileName, ".pka")))
                {
                    if (!pdbqtFileName.CalcPka())
                    {
                        Console.WriteLine($"Pka failed on {name}");
                        return;
                    }
                }
            });
        }

        private static string Encode(string name)
        {
            var sb = new StringBuilder();
            const string illegal = @"\/:*?""<>|";
            foreach (char c in name)
            {
                if (illegal.Contains(c))
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
