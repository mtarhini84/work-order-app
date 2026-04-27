//using Newtonsoft.Json;
//using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;
//using System.Drawing;

//namespace WorkOrderApp.Helpers.Excel
//{
//    public class ExcelManager
//    {
//        private ExcelPackage excelPackage;

//        public ExcelManager(Stream fileStream)
//        {
//            excelPackage = new ExcelPackage(fileStream);
//        }
//        public ExcelManager()
//        {
//            excelPackage = new ExcelPackage();
//        }

//        public List<string> GetWorksheetNames()
//        {
//            List<string> worksheetNames = new List<string>();
//            foreach (var worksheet in excelPackage.Workbook.Worksheets)
//            {
//                worksheetNames.Add(worksheet.Name);
//            }
//            return worksheetNames;
//        }

//        public ExcelWorksheet GetWorksheet(string worksheetName)
//        {
//            return excelPackage.Workbook.Worksheets[worksheetName];
//        }

//        public Dictionary<string, object> ReadWorksheetAsDictionary(string worksheetName)
//        {
//            var worksheet = excelPackage.Workbook.Worksheets[worksheetName];
//            if (worksheet == null)
//            {
//                throw new KeyNotFoundException($"Worksheet {worksheetName} not found.");
//            }

//            if (worksheet.Dimension == null)
//            {
//                throw new KeyNotFoundException($"Worksheet {worksheetName} is empty.");
//            }

//            var headers = new List<string>();
//            var data = new Dictionary<string, object>();

//            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
//            {
//                headers.Add(worksheet.Cells[1, col].Text);
//            }

//            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
//            {
//                var rowData = new Dictionary<string, object>();
//                for (int col = 1; col <= headers.Count; col++)
//                {
//                    rowData[headers[col - 1]] = worksheet.Cells[row, col].Text;
//                }

//                data[worksheet.Cells[row, 1].Text] = rowData;
//            }

//            return data;
//        }

//        public IList<string> ReadColumnAsStringArray(string worksheetName, string columnName)
//        {
//            var worksheet = excelPackage.Workbook.Worksheets[worksheetName];
//            if (worksheet == null)
//            {
//                throw new KeyNotFoundException($"Worksheet {worksheetName} not found.");
//            }

//            int columnNumber = GetColumnNumber(worksheet, columnName);
//            if (columnNumber == -1)
//            {
//                throw new KeyNotFoundException($"Column {columnName} not found.");
//            }

//            List<string> columnData = new List<string>();

//            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
//            {
//                columnData.Add(worksheet.Cells[row, columnNumber].Text);
//            }

//            return columnData;
//        }

//        public MemoryStream ConvertXlsToXlsx(IFormFile file)
//        {
//            using var xlsStream = new MemoryStream();
//            file.CopyTo(xlsStream);
//            xlsStream.Position = 0;

//            var xlsWorkbook = new HSSFWorkbook(xlsStream);
//            var xlsxWorkbook = new XSSFWorkbook();

//            for (int i = 0; i < xlsWorkbook.NumberOfSheets; i++)
//            {
//                var oldSheet = xlsWorkbook.GetSheetAt(i);
//                var newSheet = xlsxWorkbook.CreateSheet(oldSheet.SheetName);

//                for (int row = 0; row <= oldSheet.LastRowNum; row++)
//                {
//                    var oldRow = oldSheet.GetRow(row);
//                    if (oldRow == null) continue;
//                    var newRow = newSheet.CreateRow(row);

//                    for (int col = 0; col < oldRow.LastCellNum; col++)
//                    {
//                        var oldCell = oldRow.GetCell(col);
//                        if (oldCell == null) continue;
//                        var newCell = newRow.CreateCell(col);
//                        newCell.SetCellValue(oldCell.ToString());
//                    }
//                }
//            }

//            var outputStream = new MemoryStream();
//            xlsWorkbook.Write(outputStream);
//            outputStream.Position = 0;

//            return outputStream;
//        }

//        private int GetColumnNumber(ExcelWorksheet worksheet, string columnName)
//        {
//            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
//            {
//                if (worksheet.Cells[1, col].Text.Equals(columnName, StringComparison.OrdinalIgnoreCase))
//                {
//                    return col;
//                }
//            }

//            return -1;
//        }

//        public byte[] ExportToExcel<T>(string sheetName, List<T> data)
//        {
//            var worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);

//            if (data == null || !data.Any())
//                return excelPackage.GetAsByteArray();

//            var firstItem = data.First();

//            if (firstItem is Dictionary<string, object> firstDict)
//            {
//                var headers = firstDict.Keys.ToArray();

//                for (int i = 0; i < headers.Length; i++)
//                {
//                    worksheet.Cells[1, i + 1].Value = headers[i];
//                }

//                for (int i = 0; i < data.Count; i++)
//                {
//                    if (data[i] is Dictionary<string, object> rowDict)
//                    {
//                        for (int j = 0; j < headers.Length; j++)
//                        {
//                            var header = headers[j];
//                            var value = rowDict.ContainsKey(header) ? rowDict[header] : "";
//                            worksheet.Cells[i + 2, j + 1].Value = value;
//                        }
//                    }
//                }
//            }
//            else
//            {
//                var properties = typeof(T).GetProperties();

//                for (int i = 0; i < properties.Length; i++)
//                {
//                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
//                }

//                for (int i = 0; i < data.Count; i++)
//                {
//                    var rowData = data[i];
//                    for (int j = 0; j < properties.Length; j++)
//                    {
//                        var property = properties[j];
//                        try
//                        {
//                            var value = property.GetValue(rowData);

//                            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
//                            {
//                                var cell = worksheet.Cells[i + 2, j + 1];
//                                cell.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
//                                cell.Value = value;
//                            }
//                            else if (value is IEnumerable<object> list && !(value is string))
//                            {
//                                string jsonValue = JsonConvert.SerializeObject(value);
//                                worksheet.Cells[i + 2, j + 1].Value = jsonValue;
//                            }
//                            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && value != null)
//                            {
//                                var nestedProperties = property.PropertyType.GetProperties();
//                                string flattenedValue = string.Join(", ", nestedProperties.Select(np => np.GetValue(value)?.ToString() ?? ""));
//                                worksheet.Cells[i + 2, j + 1].Value = flattenedValue;
//                            }
//                            else
//                            {
//                                worksheet.Cells[i + 2, j + 1].Value = value;
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine($"Error getting property {property.Name}: {ex.Message}");
//                            worksheet.Cells[i + 2, j + 1].Value = "";
//                        }
//                    }
//                }
//            }

//            return excelPackage.GetAsByteArray();
//        }

//        public byte[] ExportMultiSheetExcel(Dictionary<string, object> sheets, bool highlightHeaders = true)
//        {
//            using var package = new ExcelPackage();

//            foreach (var sheet in sheets)
//            {
//                var sheetName = sheet.Key;
//                var data = sheet.Value;

//                var worksheet = package.Workbook.Worksheets.Add(sheetName);

//                if (data is IEnumerable<object> enumerable)
//                {
//                    var dataList = enumerable.ToList();
//                    if (dataList.Any())
//                    {
//                        var firstItem = dataList.First();

//                        if (firstItem is Dictionary<string, object> firstDict)
//                        {
//                            var headers = firstDict.Keys.ToArray();

//                            for (int i = 0; i < headers.Length; i++)
//                            {
//                                worksheet.Cells[1, i + 1].Value = headers[i];
//                            }

//                            if (highlightHeaders)
//                            {
//                                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
//                                {
//                                    range.Style.Font.Bold = true;
//                                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
//                                    range.Style.Font.Color.SetColor(Color.DarkBlue);
//                                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
//                                }
//                            }

//                            for (int i = 0; i < dataList.Count; i++)
//                            {
//                                if (dataList[i] is Dictionary<string, object> rowDict)
//                                {
//                                    for (int j = 0; j < headers.Length; j++)
//                                    {
//                                        var header = headers[j];
//                                        var value = rowDict.ContainsKey(header) ? rowDict[header] : "";
//                                        worksheet.Cells[i + 2, j + 1].Value = value;
//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {
//                            var properties = firstItem.GetType().GetProperties();

//                            for (int i = 0; i < properties.Length; i++)
//                            {
//                                worksheet.Cells[1, i + 1].Value = properties[i].Name;
//                            }

//                            if (highlightHeaders)
//                            {
//                                using (var range = worksheet.Cells[1, 1, 1, properties.Length])
//                                {
//                                    range.Style.Font.Bold = true;
//                                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
//                                    range.Style.Font.Color.SetColor(Color.DarkBlue);
//                                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
//                                }
//                            }

//                            for (int i = 0; i < dataList.Count; i++)
//                            {
//                                var rowData = dataList[i];
//                                for (int j = 0; j < properties.Length; j++)
//                                {
//                                    var property = properties[j];
//                                    try
//                                    {
//                                        var value = property.GetValue(rowData);
//                                        worksheet.Cells[i + 2, j + 1].Value = value;
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        Console.WriteLine($"Error getting property {property.Name}: {ex.Message}");
//                                        worksheet.Cells[i + 2, j + 1].Value = "";
//                                    }
//                                }
//                            }
//                        }

//                        if (worksheet.Dimension != null)
//                        {
//                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
//                        }
//                    }
//                }
//            }

//            return package.GetAsByteArray();
//        }
//    }
//}