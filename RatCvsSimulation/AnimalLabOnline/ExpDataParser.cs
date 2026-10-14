using AnimalLabOnline.DataModels;
using AnimalLabOnline.EnumHelpers;
using CommonTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimalLabOnline
{
    public class ExpDataParser
    {
        private int _rowNumber;
        private string _sheetName;

        /// <summary>
        /// Format an Excel cell name
        /// </summary>
        /// <param name="col">0 based</param>
        /// <returns></returns>
        private string GetCellName(int col, int rowOffset = 0)
        {
            return $"cell {(col + 1).ToCellRef(_rowNumber + rowOffset)} of sheet '{_sheetName}'";
        }

        /// <summary>
        /// Format an Excel row name
        /// </summary>
        /// <returns></returns>
        private string GetRowName(int rowOffset = 0)
        {
            return $"row {_rowNumber + rowOffset} of sheet '{_sheetName}'";
        }

        private IEnumerator<string[]> _il;

        private bool MoveNext()
        {
            bool r = _il.MoveNext();
            if (r)
                _rowNumber++;
            return r;
        }

        private IList<int> ParseHeaders()
        {
            string[] row = null;
            while (row.IsEmpty())
            {
                if (!MoveNext())
                    return null;

                // expect indicator header
                row = _il.Current;
            }

            if (row.Length < 4)
                throw new Exception($"Not enough headers discovered on {GetRowName()}.");

            // skip row[0]
            if (!string.IsNullOrEmpty(row[1]) && !row[1].FuzzyMatch("unit"))
                throw new Exception($"No unit header found at {GetCellName(1)}.");

            var widths = new List<int>();
            for (int i = 2, grpId = 1; i < row.Length; i++, grpId++)
            {
                // Group N column is required
                if (!row[i].FuzzyMatch($"group{grpId}"))
                    throw new Exception($"Expect group {grpId} header at {GetCellName(i)}.");

                // S.E. column is required
                if (!row[++i].FuzzyMatch("se"))
                    throw new Exception($"Expect standard deviation header at {GetCellName(i)}.");

                // P-Value column is optional
                if (i + 1 < row.Length && row[i + 1].FuzzyMatch("pvalue"))
                {
                    i++;
                    widths.Add(3);
                }
                else
                {
                    widths.Add(2);
                }
            }

            return widths;
        }

        private (IList<ExpIndicator> indicators, double?[,,] results, (int, double)[,][] pvalues) ParseIndicatorsAndResults(IDictionary<string, string> indicatorDict, IList<int> groupCols)
        {
            var indicators = new List<ExpIndicator>();
            var results = new List<double?[]>();
            var pvalues = new List<(int, double)[][]>();
            int colCount = 2 + groupCols.Sum();

            int n = 0;
            while (true)
            {
                if (!MoveNext())
                    throw new Exception("Unexpected end of file.");

                string[] row = _il.Current;

                if (row.IsEmpty()) // end of a section
                    break;

                if (row.Length > colCount)
                {
                    if (!row.Skip(colCount).All(o => string.IsNullOrWhiteSpace(o))) // some trailing cells are not blank
                        throw new Exception($"Too many fields on {GetRowName()}.");
                }

                row[0] = row[0].Trim();
                string name = row[0].TrimEnd('*').TrimEnd(), speciality = null;
                bool onFigure = row[0].EndsWith('*');

                {
                    var m = Regex.Match(name, @"^(.+)\((.+)\)\s*$");
                    if (m.Success)
                    {
                        name = m.Groups[1].Value.Trim().TrimEnd('*').TrimEnd();
                        onFigure |= m.Groups[1].Value.Trim().EndsWith('*');
                        speciality = m.Groups[2].Value.Trim();
                    }
                }

                if (indicators.Any(o => o.Name == name && o.Speciality == speciality))
                    throw new Exception($"Duplicate indicator '{row[0]}' on {GetRowName()}.");

                string unit = null, unitFull = null;
                if (row.Length > 1 && !string.IsNullOrWhiteSpace(row[1]))
                {
                    unit = row[1].Trim();
                }

                try
                {
                    var unitSegments = unit.ParseUnitFull();
                    unit = unitSegments.FormatUnit();
                    unitFull = unitSegments.FormatUnitFull();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + $" at {GetCellName(1)}.");
                }

                indicatorDict.TryGetValue(name.ToLower(), out string description);

                indicators.Add(new ExpIndicator
                {
                    Id = Guid.NewGuid(),
                    Index = ++n,
                    Name = name,
                    Description = description,
                    Speciality = speciality,
                    Unit = unit,
                    UnitFull = unitFull,
                    OnFigure = onFigure,
                });

                // A record is for a row (an indicator and all groups)
                double?[] resultRecord = new double?[groupCols.Count * 2];
                var pvalueRecord = new (int refnum, double pvalue)[groupCols.Count][];

                // i: zero-based column index, j: zero-based group number
                // [Indicator, Unit, Group1, Standard Deviation1, Group2, Standard Deviation2, ...]
                for (int i = 2, j = 0; i < row.Length && j < groupCols.Count; i += groupCols[j], j++)
                {
                    if (string.IsNullOrWhiteSpace(row[i])) // blank result value
                    {
                        if (i + 1 < row.Length && !string.IsNullOrWhiteSpace(row[i + 1])) // S.E. cell is not blank
                            throw new Exception($"Unexpected standard deviation at {GetCellName(i + 1)}.");
                        if (groupCols[j] > 2 && i + 2 < row.Length && !string.IsNullOrWhiteSpace(row[i + 2])) // P-Value cell is not blank
                            throw new Exception($"Unexpected p-value description at {GetCellName(i + 1)}.");
                        continue;
                    }

                    // non blank result value
                    if (!double.TryParse(row[i], out double value))
                        throw new Exception($"Not a number for result data at {GetCellName(i)}.");
                    resultRecord[j * 2] = value;

                    // S.E. cell contains something
                    if (i + 1 < row.Length && !string.IsNullOrWhiteSpace(row[i + 1]))
                    {
                        if (!double.TryParse(row[i + 1], out value))
                            throw new Exception($"Not a number for standard deviation at {GetCellName(i + 1)}.");
                        resultRecord[j * 2 + 1] = value;
                    }

                    // P-Value cell contains something
                    if (groupCols[j] > 2 && i + 2 < row.Length && !string.IsNullOrWhiteSpace(row[i + 2]))
                    {
                        string[] pvfields = row[i + 2].Split(';', StringSplitOptions.RemoveEmptyEntries);
                        pvalueRecord[j] = new (int, double)[pvfields.Length]; // At the length of pvfields

                        for (int k = 0; k < pvfields.Length; k++)
                        {
                            string pvfield = pvfields[k];
                            var m = Regex.Match(pvfield, @"^([gi]?)(\d+),([\d\.]+)$");
                            if (!m.Success)
                                throw new Exception($"Invalid P-Value format '{pvfield}' at {GetCellName(i + 2)}.");
                            if (!int.TryParse(m.Groups[2].Value, out int num))
                                throw new Exception($"Group or indicator number '{m.Groups[2].Value}' is not valid at {GetCellName(i + 2)}.");
                            if (!double.TryParse(m.Groups[3].Value, out double pv))
                                throw new Exception($"P-Value '{m.Groups[3].Value}' is not a number at {GetCellName(i + 2)}.");
                            if (num == 0)
                                throw new Exception($"P-Value group or indicator number cannot be 0 at {GetCellName(i + 2)}.");
                            if (m.Groups[1].Value == "i") // indicator reference
                            {
                                if (num >= n)
                                    throw new Exception($"P-Value must reference former defined indicators at {GetCellName(i + 2)}.");
                                num = -num;
                            }
                            else // group reference
                            {
                                if (num >= j + 1)
                                    throw new Exception($"P-Value must reference former defined groups at {GetCellName(i + 2)}.");
                            }
                            pvalueRecord[j][k] = (num, pv);
                        }
                    }
                }

                results.Add(resultRecord);
                pvalues.Add(pvalueRecord);
            }

            // Convert data structure
            double?[,,] results2 = new double?[indicators.Count, groupCols.Count, 2];
            var pvalues2 = new (int, double)[indicators.Count, groupCols.Count][];

            for (int i = 0; i < indicators.Count; i++)
                for (int j = 0; j < groupCols.Count; j++)
                {
                    results2[i, j, 0] = results[i][j * 2 + 0];
                    results2[i, j, 1] = results[i][j * 2 + 1];
                    pvalues2[i, j] = pvalues[i][j];
                }

            return (indicators, results2, pvalues2);
        }

        private (IList<ExpGroup>, IList<ExpDrug>) ParseGroupsAndExpDrugs(IDictionary<string, Drug> drugDict, int groupCount)
        {
            var expGroups = new List<ExpGroup>();
            var expDrugs = new List<ExpDrug>();

            bool started = false;
            for (int i = 1; i <= groupCount; i++)
            {
                if (!MoveNext())
                    throw new Exception($"Unexpected end of file.");

                string[] row = _il.Current;
                if (row.IsEmpty())
                {
                    if (started)
                        break;
                    else
                    {
                        i--;
                        continue;
                    }
                }
                started = true;

                string grpSpeciality = null;
                if (!row[0].FuzzyMatch($"group{i}"))
                {
                    var m = Regex.Match(row[0], @"^(.+)\((.+)\)\s*$");
                    if (!m.Success || !m.Groups[1].Value.FuzzyMatch($"group{i}"))
                        throw new Exception($"Expect group {i} header at {GetCellName(0)}.");

                    grpSpeciality = m.Groups[2].Value.Trim();
                }

                var group = new ExpGroup
                {
                    Id = Guid.NewGuid(),
                    Index = i,
                    Name = $"Group {i}",
                    Speciality = grpSpeciality,
                    Protocols = new List<Protocol>(),
                };

                // each group definition contains a series of drug treatment protocol
                // j: zero-based treatment protocol for a group
                // [Group No., Drug1, Admin1, Dose1, Unit1, Drug2, Admin2, Dose2, Unit2, ...]
                for (int j = 1; j < row.Length; j += 4)
                {
                    if (string.IsNullOrWhiteSpace(row[j])) // skip the 4 fields if the drug name field is blank
                    {
                        for (int k = 2; k < 4; k++)
                            if (j + k < row.Length && !string.IsNullOrWhiteSpace(row[j + k]))
                                throw new Exception($"Unexpected value '{row[j + k]}' at {GetCellName(j + k)}.");
                        continue;
                    }

                    /* Check the dose and unit for blank
                    for (int k = 2; k < 4; k++) // the last two fields should not be blank
                        if (j + k >= row.Length || string.IsNullOrWhiteSpace(row[j + k]))
                            throw new Exception($"Missing value at {GetCellName(j + k)}.");
                    //*/

                    // [0] Get the first field in the format of "name(speciality)"
                    string drugName = row[j].Trim();
                    string speciality = null;

                    var match = Regex.Match(drugName, @"^(.+)\s*\((.+)\)$");
                    if (match.Success)
                    {
                        drugName = match.Groups[1].Value;
                        speciality = match.Groups[2].Value;
                    }

                    // [1] Get the second field: admin
                    var adminType = (j + 1 < row.Length ? row[j + 1] : null).ParseAdminMethod();

                    // [2] Get the third and fourth field: dose, unit
                    float dose = -1;
                    var doseUnit = DoseUnit.Unknown;
                    if (j + 2 < row.Length && !string.IsNullOrWhiteSpace(row[j + 2])) // allow blank dose and unit but not dose without unit
                    {
                        if (!float.TryParse(row[j + 2], out dose))
                            throw new Exception($"Not an number at {GetCellName(j + 2)}.");
                        if (j + 3 >= row.Length || string.IsNullOrWhiteSpace(row[j + 3]))
                            throw new Exception($"Missing value at {GetCellName(j + 3)}.");
                        doseUnit = row[j + 3].ParseDoseUnit();
                    }

                    // [3] Look for the drug in dictionary
                    var treatmentType = drugName == null ? TreatmentType.Normal : drugName.ParseTreatmentType();
                    if (treatmentType == TreatmentType.Unknown)
                        treatmentType = TreatmentType.DrugTherapy;
                    string treatmentName = drugName;

                    if (!drugDict.TryGetValue(drugName.ToLower(), out var drug))
                    {
                        if (treatmentType == TreatmentType.DrugTherapy) // Could not be null for therapy treatment
                            throw new Exception($"Unknown drug name '{drugName}' at {GetCellName(j)}.");
                    }

                    // [4] Check if the drug exists in the experiment
                    ExpDrug expDrug = null;
                    if (drug != null) // Could be null for non-therapy treatment
                    {
                        expDrug = expDrugs.SingleOrDefault(o => o.Drug == drug);
                        if (expDrug == null)
                        {
                            expDrugs.Add(expDrug = new ExpDrug
                            {
                                Id = Guid.NewGuid(),
                                Index = expDrugs.Count + 1,
                                Drug = drug,
                                Protocols = new List<Protocol>(),
                            });
                        }
                    }

                    // [5] Create a protocol for the drug
                    var protocol = new Protocol
                    {
                        Id = Guid.NewGuid(),
                        Treatment = treatmentType,
                        TreatmentName = treatmentName,
                        Speciality = speciality,
                        AdminMethod = adminType,
                        Dose = dose > 0 ? dose : (float?)null,
                        DoseUnit = doseUnit,
                        ExpGroup = group,
                        ExpDrug = expDrug, // Could be null for non-therapy treatment
                    };

                    if (expDrug != null) // Could be null for non-therapy treatment
                        expDrug.Protocols.Add(protocol);
                    group.Protocols.Add(protocol);
                }

                // No treatment definition means Normal Control
                if (!group.Protocols.Any())
                    group.Protocols.Add(new Protocol
                    {
                        Id = Guid.NewGuid(),
                        Treatment = TreatmentType.Normal,
                        ExpGroup = group,
                    });

                expGroups.Add(group);
            }

            return (expGroups, expDrugs);
        }

        private Dictionary<string, string> ParseProperties()
        {
            var dict = new Dictionary<string, string>();

            bool started = false;
            while (true)
            {
                if (!MoveNext())
                    break;

                string[] row = _il.Current;
                if (row.IsEmpty())
                {
                    if (started)
                        break;
                    else
                        continue;
                }
                started = true;

                if (row.Length < 2)
                    throw new Exception($"Missing property value at {GetCellName(1)}.");

                if (row.Length > 2)
                    throw new Exception($"Trailing cell values on {GetRowName()}.");

                if (string.IsNullOrWhiteSpace(row[0]))
                    throw new Exception($"Property name cannot be blank at {GetCellName(0)}");

                dict[row[0].Trim().ToLower()] = row[1];
            }

            return dict;
        }

        public IEnumerable<Experiment> ConvertToExperiments(IEnumerable<string[]> data, IDictionary<string, Drug> drugDict, IDictionary<string, string> indicatorDict, string sheetName, int sheetIndex)
        {
            _rowNumber = 0;
            _sheetName = sheetName;

            var titles = new HashSet<string>();
            var urls = new HashSet<string>();

            _il = data.GetEnumerator();

            // allow leading blank lines
            var sheetProperties = ParseProperties();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = sheetProperties.GetValueSafe("title"),
                Desc = sheetProperties.GetValueSafe("intro"),
                Index = sheetIndex,
            };

            while (true)
            {
                // allow leading blank lines
                var groupCols = ParseHeaders();
                if (groupCols == null)
                    break;

                (var indicators, double?[,,] results, var pvalues) = ParseIndicatorsAndResults(indicatorDict, groupCols);

                // allow leading blank lines
                (var groups, var expDrugs) = ParseGroupsAndExpDrugs(drugDict, groupCols.Count);

                // allow leading blank lines
                var properties = ParseProperties();

                string title = properties.GetValueSafe("article title");
                if (titles.Contains(title))
                    throw new Exception($"Duplicate article title '{title}' on {GetRowName(-3)}.");
                titles.Add(title);

                string url = new Uri(properties.GetValueSafe("url")).ToString();
                if (urls.Contains(url))
                    throw new Exception($"Duplicate article url '{url}' on {GetRowName(-1)}.");
                urls.Add(url);

                string tag = AnalyseConclusion(groups, indicators, results, pvalues);

                yield return new Experiment
                {
                    Id = Guid.NewGuid(),
                    ExpIndicators = indicators,
                    ExpGroups = groups,
                    Results = results,
                    PValues = pvalues,
                    ArticleTitle = title,
                    Author = Regex.Replace(properties.GetValueSafe("author"), @"\s*,\s*", ", "),
                    Url = url,
                    ExpDrugs = expDrugs,
                    Subject = subject,
                    Tag = tag,
                };
            }
        }

        private int GetTreatmentUsage(TreatmentType type)
        {
            switch (type)
            {
                default:
                case TreatmentType.Unknown:
                    throw new Exception("Unknown treatment type discovered.");
                case TreatmentType.Normal:
                case TreatmentType.Vehicle:
                case TreatmentType.Sham:
                case TreatmentType.Placebo:
                    return 0;
                case TreatmentType.DrugInducedModeling:
                case TreatmentType.SurgicalModeling:
                case TreatmentType.StressModeling:
                case TreatmentType.GeneticModeling:
                case TreatmentType.OtherModeling:
                    return 1;
                case TreatmentType.DrugTherapy:
                case TreatmentType.CellularTherapy:
                    return 2;
            }
        }

        private string AnalyseConclusion(IList<ExpGroup> groups, IList<ExpIndicator> indicators, double?[,,] results, (int, double)[,][] pvalues)
        {
            int nInd = indicators.Count, nGrp = groups.Count;
            Debug.Assert(results.GetLength(0) == nInd);
            Debug.Assert(results.GetLength(1) == nGrp);
            Debug.Assert(results.GetLength(2) == 2);
            Debug.Assert(pvalues.GetLength(0) == nInd);
            Debug.Assert(pvalues.GetLength(1) == nGrp);

            var categories = groups.GroupBy(o => o.Protocols.Select(p => GetTreatmentUsage(p.Treatment)).Aggregate((a, b) => a | b));

            //// 1: Normal, 2: Modeling, 4: Therapy => 1, 2, 3, 4, 5, 6, 7[x]
            //if (categories.Any(o => o.Key == 3))
            //    Debug.Fail("Should not contain Normal+Modeling treatments");
            //if (categories.Any(o => o.Key == 5))
            //    Debug.Fail("Should not contain Normal+Therapy treatments");
            if (categories.Any(o => o.Key == 7))
                throw new Exception("Experiment should not contain Normal+Modeling+Therapy treatments.");

            //"1246": 39 => "0123"
            //"1356": 1 => "0123"
            //"1256": 2 => "0123"
            //"126": 31 => "013"
            //"136": 11 => "013"
            //"1236": 2 => "013"
            //"36": 4 => "13"
            //"16": 6 => "03"

            //"0123": 42 => normal, model, therapy, model+therapy => [1] compare model+therapy with model if significantly differ, [0] compare model+therapy with normal the closer the better
            //"013": 44 => normal, model, model+therapy => [1] compare model+therapy with model if significantly differ, [0] compare model+therapy with normal the closer the better
            //"13": 4 => model, model+therapy => [1] compare model+therapy with model if significantly differ
            //"03": 6 => normal, model+therapy => [0] compare model+therapy with normal the closer the better

            string category = string.Join("", categories.OrderBy(o => o.Key).Select(o => $"{o.Key}({o.Count()})").ToArray());

            var cat0 = categories.Where(o => o.Key == 0).Select(o => o.ToArray()).ToArray();
            var cat1 = categories.Where(o => o.Key == 1).Select(o => o.ToArray()).ToArray();
            var cat3 = categories.Where(o => o.Key == 3).Select(o => o.ToArray()).ToArray();

            // has 1 (model groups), compare 3 and 1
            if (cat1.Any())
            {
                // [1] compare model+therapy with model if significantly differ
                if (cat3.Length == 1 && cat1.Length == 1)
                {

                }
            }

            // has 0 (normal groups), compare 3 and 0
            if (cat0.Any())
            {
                // [0] compare model+therapy with normal the closer the better
            }

            return category;
        }

        public IDictionary<string, Drug> ConvertToDrugs(IEnumerable<string[]> data, OpenBabelOptions options, string sheetName)
        {
            _rowNumber = 1;
            _sheetName = sheetName;

            var drugDict = new Dictionary<string, Drug>();

            foreach (string[] row in data.Skip(1))
            {
                _rowNumber++;

                if (row.IsEmpty())
                    continue;

                if (row.Length < 2) // skip the incomplete record
                    continue;

                if (row.Length > 2)
                    throw new Exception($"Drug definition has trailing fields on {GetRowName()}.");

                var drug = new Drug
                {
                    Id = Guid.NewGuid(),
                    Name = row[0].Trim(),
                    Smiles = row[1].Trim(),
                    Depict2D = OpenBabelHelper.SmilesToSvg(row[1], false, options),
                    Fingerprint = OpenBabelHelper.ComputeFingerprint(row[1], "FP2"),
                };

                if (drug.Depict2D == null)
                    throw new Exception($"Cannot generate depiction for smiles '{row[1]}' for drug '{row[0]}'.");

                string lowerName = drug.Name.ToLower();
                if (drugDict.TryGetValue(lowerName, out var value))
                {
                    if (value.Smiles != drug.Smiles)
                        throw new Exception($"Duplicate drug name {drug.Name} at {GetCellName(0)}.");
                }
                else
                {
                    drugDict.Add(lowerName, drug);
                }
            }

            return drugDict;
        }

        public IDictionary<string, string> ConvertToIndicators(IEnumerable<string[]> data, string sheetName)
        {
            _rowNumber = 1;
            _sheetName = sheetName;

            var indDict = new Dictionary<string, string>();

            foreach (string[] row in data.Skip(1))
            {
                _rowNumber++;

                if (row.IsEmpty())
                    continue;

                if (row.Length < 2) // skip the incomplete record
                    continue;

                if (row.Length > 2)
                    throw new Exception($"Indicator definition has trailing fields on {GetRowName()}.");

                string name = row[0].Trim();
                string description = row[1].Trim();

                string lowerName = name.ToLower();
                if (indDict.TryGetValue(lowerName, out string value))
                {
                    if (value != description)
                        throw new Exception($"Duplicate indicator name {name} at {GetCellName(0)}.");
                }
                else
                {
                    indDict.Add(lowerName, description);
                }
            }

            return indDict;
        }
    }
}
