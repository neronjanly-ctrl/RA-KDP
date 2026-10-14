using CommonTools;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnimalLabOnline
{
    public static class ExcelFileParser
    {
        private static uint GetColumnNumber(this Cell cell)
        {
            return ((string)cell.CellReference).ToColumnNumber(); // 1 based
        }

        private static string GetString(Cell cell, SharedStringTable strings)
        {
            return (cell.DataType != null && cell.DataType.HasValue && cell.DataType == CellValues.SharedString)
                ? strings.ChildElements[int.Parse(cell.InnerText)].InnerText
                : cell.InnerText;
        }

        public static IEnumerable<Dictionary<string, string>> GetAsDicts(string filepath)
        {
            using (var document = SpreadsheetDocument.Open(filepath, false))
            {
                var workbook = document.WorkbookPart.Workbook;
                var strings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                foreach (var sheet in workbook.Descendants<Sheet>())
                {
                    var part = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id);

                    var rows = part.Worksheet.Descendants<Row>().ToArray();

                    if (rows.Length <= 0)
                        throw new Exception($"Work sheet {sheet.Name} is blank.");

                    var headers = rows[0].Descendants<Cell>().ToArray();

                    for (int i = 1; i < rows.Length; i++)
                    {
                        var dict = new Dictionary<string, string>();
                        var cells = rows[i].Descendants<Cell>().ToArray();

                        for (int j = 0; j < Math.Min(headers.Length, cells.Length); j++)
                        {
                            string fieldName = GetString(headers[j], strings);
                            string fieldValue = GetString(cells[j], strings);

                            if (string.IsNullOrWhiteSpace(fieldValue))
                                throw new Exception($"Cell cannot be blank at row {i + 1}, column {fieldName} of work sheet {sheet.Name}.");

                            if (dict.ContainsKey(fieldName))
                                throw new Exception($"Field {fieldName} duplicate.");

                            dict.Add(fieldName, fieldValue);
                        }

                        yield return dict;
                    }
                }

                document.Close();
            }
        }

        private static IEnumerable<string[]> GetRawRows(Row[] rows, SharedStringTable strings)
        {
            int rowCount = (int)rows.Last().RowIndex.Value;
            for (int i = 1, ii = 0; i <= rowCount; i++)
            {
                if (rows[ii].RowIndex != i)
                {
                    yield return new string[0];
                    continue;
                }

                var cells = rows[ii++].Descendants<Cell>().ToArray();
                if (cells.Length == 0)
                {
                    yield return new string[0];
                    continue;
                }

                uint columnCount = cells.Last().GetColumnNumber();
                string[] result = new string[columnCount];
                int len = 0;

                for (int j = 0, jj = 0; j < columnCount; j++)
                {
                    if (cells[jj].GetColumnNumber() != j + 1)
                        result[j] = null;
                    else
                        result[j] = GetString(cells[jj++], strings);
                    if (!string.IsNullOrEmpty(result[j]))
                        len = j + 1;
                }

                if (len < columnCount)
                    result = result.Take(len).ToArray();

                yield return result;
            }
        }

        public static IEnumerable<(string name, IEnumerable<string[]> data)> GetRawData(string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var document = SpreadsheetDocument.Open(stream, false))
            {
                var workbook = document.WorkbookPart.Workbook;
                var strings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                foreach (var sheet in workbook.Descendants<Sheet>())
                {
                    var part = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id);

                    var rows = part.Worksheet.Descendants<Row>().ToArray();

                    if (rows.Length <= 0)
                        throw new Exception($"Work sheet {sheet.Name} is blank.");

                    yield return (sheet.Name, GetRawRows(rows, strings));
                }

                document.Close();
            }
        }

    }
}
