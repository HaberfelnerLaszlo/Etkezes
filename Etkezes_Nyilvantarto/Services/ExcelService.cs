using Etkezes_Models;
using Etkezes_Models.ViewModels;
using NPOI.SS.Converter;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.Util.ArrayExtensions;
using NPOI.XSSF.UserModel;
using System.Data;

namespace Etkezes_Nyilvantarto.Services
{
    public class ExcelService
    {
        public event EventHandler<OnMessageEventArgs>? OnMessage;
        //private FileStream OpenFile(string filePath)
        //{
        //    using (FileStream stream = File.OpenRead(filePath))
        //    {
        //        return stream;
        //    } ;
        //}
        public List<User> ReadUsers(MemoryStream stream)
        {
            try
            {
                int[] positions = [-1, -1, -1, -1];   //vezetéknév, ketersztnév, azonosító, osztály
                ISheet sheet;
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                sheet = xssWorkbook.GetSheetAt(0);
                IRow headerRow = sheet.GetRow(0);
                int cellCount = headerRow.LastCellNum;
                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = headerRow.GetCell(j);
                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                    {
                        switch (cell.StringCellValue)
                        {
                            case string x when x.Contains("vezetéknév",StringComparison.CurrentCultureIgnoreCase):
                                positions[0] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("utónév",StringComparison.CurrentCultureIgnoreCase):
                                positions[1] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("azonosító",StringComparison.CurrentCultureIgnoreCase):
                                positions[2] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("osztály",StringComparison.CurrentCultureIgnoreCase):
                                positions[3] = cell.ColumnIndex;
                                break;
                           default:
                                break;
                        }
                    }
                }
                if (positions.Any(p => p == -1))
                {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Nem található a szükséges oszlopok egyike a fájlban. Kérem ellenőrizze a fájl formátumát.", 200));
                    return new List<User>();
                }
                List<User> users = new List<User>();
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                    if (row.GetCell(positions[3]).StringCellValue.Length > 5) continue; //ez egy gyors megoldás arra, hogy kiszűrjük a nem osztályokat tartalmazó sorokat, mivel az osztályok nevei 5 karakternél rövidebbek
                    users.Add(new User() {
                        Name=string.Join(' ', row.GetCell(positions[0]).StringCellValue, row.GetCell(positions[1]).StringCellValue),
                        Id=long.Parse(row.GetCell(positions[2]).StringCellValue),
                        Osztaly = row.GetCell(positions[3]).StringCellValue,
                     });
                }
                return users;
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Hiba történt a felhasználók beolvasása során: {ex.Message}", 201));
                return [];
            }
        }
        public List<EtkezokFromExcel> ReadMaiEtkezesek(MemoryStream stream)
        {
            try
            {
                int[] positions = [-1, -1, -1, -1, -1, -1];   //név, azonosító,osztaly,étkezés típusa, hónap, adott daysInMonth
                ISheet sheet;
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                sheet = xssWorkbook.GetSheetAt(0);
                int rowNumber = 0;
                while (string.IsNullOrEmpty(sheet.GetRow(rowNumber)?.GetCell(0)?.StringCellValue) || !sheet.GetRow(rowNumber).GetCell(0).StringCellValue.ToLower().Contains("név"))
                {
                    rowNumber++;
                }
                IRow headerRow = sheet.GetRow(rowNumber);
                int cellCount = headerRow.LastCellNum;
                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = headerRow.GetCell(j);
                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                    {
                        string cellValue = string.Empty;
                        if (cell.CellType != CellType.String)
                        {
                            cellValue = cell.NumericCellValue.ToString();
                        }
                        else
                        {
                            cellValue = cell.StringCellValue;
                        }
                        switch (cellValue)
                        {
                            case string x when x.Contains("név", StringComparison.CurrentCultureIgnoreCase):
                                positions[0] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("azonosító", StringComparison.CurrentCultureIgnoreCase):
                                positions[1] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("étk.cs.", StringComparison.CurrentCultureIgnoreCase):
                                positions[2] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("étk.t.", StringComparison.CurrentCultureIgnoreCase):
                                positions[3] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("hó", StringComparison.CurrentCultureIgnoreCase):
                                positions[4] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains(DateTime.Today.Day.ToString(), StringComparison.CurrentCultureIgnoreCase):
                                positions[5] = cell.ColumnIndex;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (positions.Any(p => p == -1))
                {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Nem található a szükséges oszlopok egyike a fájlban. Kérem ellenőrizze a fájl formátumát.", 200));
                    return [];
                }
                List<EtkezokFromExcel> etkezesek = new List<EtkezokFromExcel>();
                string[] osztaly = ["5a", "5b", "5c","6a", "6b", "6c", "7a", "7b", "7c", "8a", "8b", "8c","9a", "9b", "10a", "10b", "10c","11a","11b","12a","12b"];
                for (int i = (rowNumber + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                    if (row.GetCell(positions[0]).StringCellValue.ToLower().Contains("név", StringComparison.CurrentCultureIgnoreCase)) continue;
                    if (row.GetCell(positions[4]).NumericCellValue != DateTime.Today.Month)  
                    {
                        OnMessage?.Invoke(this, new OnMessageEventArgs("A fájlban szereplő hónap nem egyezik a mai hónappal. Kérem ellenőrizze a fájl tartalmát.", 200));
                        return etkezesek;
                    }
                    if(row.GetCell(positions[5]).ToString() == "1")
                    {
                        if(row.GetCell(positions[3]).StringCellValue.ToLower().Contains("ebéd") || row.GetCell(positions[3]).StringCellValue.ToLower().Contains("menza"))
                        {
                            if(osztaly.Any(s => row.GetCell(positions[2]).StringCellValue.Contains(s, StringComparison.OrdinalIgnoreCase)))
                            {
                                etkezesek.Add(new EtkezokFromExcel(
                                    row.GetCell(positions[0]).StringCellValue,
                                    row.GetCell(positions[1]).ToString()?? string.Empty,
                                    row.GetCell(positions[2]).StringCellValue,
                                    row.GetCell(positions[3]).StringCellValue,
                                    row.GetCell(positions[4]).ToString()?? string.Empty
                                ));
                            }
                        }
                    }
                }
                return etkezesek;
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Hiba történt az étkezések beolvasása során: {ex.Message}", 201));
                return [];

            }
        }
         public List<EtkezokMonthFromExcel> ReadMonthEtkezesek(MemoryStream stream)
        {
            try
            {
                int[] positions = [-1, -1, -1, -1, -1];   //név, azonosító,osztaly,étkezés típusa, hónap
                int[] etkezesAll= new int[32];
                ISheet sheet;
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                sheet = xssWorkbook.GetSheetAt(0);
                int rowNumber = 0;
                while (string.IsNullOrEmpty(sheet.GetRow(rowNumber)?.GetCell(0)?.StringCellValue) || !sheet.GetRow(rowNumber).GetCell(0).StringCellValue.ToLower().Contains("név"))
                {
                    rowNumber++;
                }
                IRow headerRow = sheet.GetRow(rowNumber);
                int cellCount = headerRow.LastCellNum;
                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = headerRow.GetCell(j);
                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                    {
                        string cellValue = string.Empty;
                        if (cell.CellType != CellType.String)
                        {
                            cellValue = cell.NumericCellValue.ToString();
                        }
                        else
                        {
                            cellValue = cell.StringCellValue;
                        }
                        switch (cellValue)
                        {
                            case string x when x.Contains("név", StringComparison.CurrentCultureIgnoreCase):
                                positions[0] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("azonosító", StringComparison.CurrentCultureIgnoreCase):
                                positions[1] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("étk.cs.", StringComparison.CurrentCultureIgnoreCase):
                                positions[2] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("étk.t.", StringComparison.CurrentCultureIgnoreCase):
                                positions[3] = cell.ColumnIndex;
                                break;
                            case string x when x.Contains("hó", StringComparison.CurrentCultureIgnoreCase):
                                positions[4] = cell.ColumnIndex;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (positions.Any(p => p == -1))
                {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Nem található a szükséges oszlopok egyike a fájlban. Kérem ellenőrizze a fájl formátumát.", 200));
                    return [];
                }
                List<EtkezokMonthFromExcel> etkezesek = new List<EtkezokMonthFromExcel>();
                string[] osztaly = ["5a", "5b", "5c","6a", "6b", "6c", "7a", "7b", "7c", "8a", "8b", "8c","9a", "9b", "10a", "10b", "10c","11a","11b","12a","12b"];
                int daysInMonth= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                int day= 1;
                if ( (int)sheet.GetRow(rowNumber + 1).GetCell(positions[4]).NumericCellValue == DateTime.Now.Month)
                {
                      day= DateTime.Now.Day;
                }
                for (int i = (rowNumber + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                    if (row.GetCell(positions[0]).StringCellValue.ToLower().Contains("név", StringComparison.CurrentCultureIgnoreCase)) continue;
                    if(row.GetCell(positions[3]).StringCellValue.ToLower().Contains("ebéd") || row.GetCell(positions[3]).StringCellValue.ToLower().Contains("menza"))
                    {
                        if(osztaly.Any(s => row.GetCell(positions[2]).StringCellValue.Contains(s, StringComparison.OrdinalIgnoreCase)))
                        {
                            for (int k = day; k <= daysInMonth; k++)
                            {
                                var cell = row.GetCell(positions[4] + k).ToString();
                                if (row.GetCell(positions[4] + k).ToString() != null && row.GetCell(positions[4] + k).ToString() == "1")
                                {
                                    etkezesAll[k] = 1;
                                }
                            }
                            etkezesAll[0] = (int)row.GetCell(positions[4]+32).NumericCellValue;
                            etkezesek.Add(new EtkezokMonthFromExcel(
                                row.GetCell(positions[0]).StringCellValue,
                                row.GetCell(positions[1]).ToString()?? string.Empty,
                                row.GetCell(positions[2]).StringCellValue,
                                row.GetCell(positions[3]).StringCellValue,
                                row.GetCell(positions[4]).ToString()?? string.Empty,
                                etkezesAll
                            ));
                        }
                    }
                    Array.ForEach(etkezesAll, d => d = 0);
                }
                return etkezesek;
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Hiba történt az étkezések beolvasása során: {ex.Message}", 201));
                return [];
            }
        }
        public string CreateEtkezokExcel(List<EtkezesView> etkezesek) 
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"etkezesek_{DateTime.Now:yyyy_MMMM}.xlsx");
                string[] osztalyok = etkezesek.Select(e => e.Osztaly).Distinct().ToArray();
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    //fs.Position = 0;
                    XSSFWorkbook workbook = new XSSFWorkbook();
                    var sheetName = $"{etkezesek[0].Datum:MM_dd}";
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.SetColumnWidth(0, 20 * 256);
                    sheet.SetColumnWidth(1, 1 * 256);
                    sheet.SetColumnWidth(2, 20 * 256);
                    sheet.SetColumnWidth(3, 1 * 256);
                    sheet.SetColumnWidth(4, 20 * 256);
                    sheet.SetColumnWidth(5, 1 * 256);
                    sheet.SetColumnWidth(6, 20 * 256);
                    sheet.Footer.Center = $"A mai étkezések: {DateTime.Now:yyyy-MM-dd}";
                    int rowIndex = 0;
                    int cellIndex = 0;
                    IRow row = sheet.CreateRow(rowIndex);
                    foreach (var o in osztalyok)
                    {
                        int maxRow = etkezesek.Where(e => e.Osztaly == o).Count() + 1;
                        var names = etkezesek.Where(e => e.Osztaly == o).Select(e => e.Name).ToArray();
                        row = sheet.GetRow(rowIndex);
                        if (row == null)
                        {
                            row = sheet.CreateRow(rowIndex);
                        }
                        row.CreateCell(cellIndex).SetCellValue($"{o}");
                        rowIndex++;
                        for (int i = 0; i < names.Length; i++)
                        {
                            if (rowIndex % 48 == 0 && rowIndex != 0)
                            {
                                rowIndex = 0;
                                cellIndex += 2;
                            }
                            //if (cellIndex > 0)
                            //{
                                row = sheet.GetRow(rowIndex);
                                if(row == null)
                                {
                                    row = sheet.CreateRow(rowIndex);
                                }
                                rowIndex++;
                            //}
                            //else
                            //{
                            //    row = sheet.CreateRow(rowIndex);
                            //    rowIndex++;
                            //}
                            row.CreateCell(cellIndex).SetCellValue(names[i]);
                        }
                    }
                    workbook.Write(fs);
                    fs.Close();
                }
                return filePath;
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Hiba történt az étkezők Excel fájl létrehozása során: {ex.Message}", 202));
                return string.Empty;
            }
        }
   }
}
