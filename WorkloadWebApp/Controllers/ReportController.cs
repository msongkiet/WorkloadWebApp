using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WorkloadWebApp.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Data.Entity;
using System.Web.Routing;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WorkloadWebApp.Controllers
{
    [Authorize(Roles = "Administrator, Supervisor")]
    public class ReportController : Controller
    { 
        private WorkloadDBEntities db = new WorkloadDBEntities();      
        string templatePath = "D:\\cccl_songkiem\\project\\web_development\\WorkloadWebApp\\WorkloadWebApp\\Content";
        string path = "D:\\cccl_songkiem\\project\\web_development\\WorkloadWebApp\\WorkloadWebApp\\Content";

        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExportReport()
        {
            DateTime today = DateTime.Today;
            string fileName = "SummaryReport_" + today.ToString("dd_MM_yyyy") + ".xlsx";
            string templateFile = templatePath + "\\Template.xlsx";
            string destination = path + fileName;
            try
            {
                System.IO.File.Copy(templateFile, destination, true);

                using (SpreadsheetDocument myWorkBook = SpreadsheetDocument.Open(destination, true))
                {
                    WorkbookPart wbPart = myWorkBook.WorkbookPart;
                    Sheet sheetSummary = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "Summary").FirstOrDefault();

                    db = new WorkloadDBEntities();
                    List<UnitTB> CostCenterList = new List<UnitTB>();
                    CostCenterList = (from list in db.UnitTBs
                                  orderby list.CostCenter ascending
                                  select list).ToList();

                    List<EmployeeTB> EmployeeList = new List<EmployeeTB>();
                    EmployeeList = (from employDB in db.EmployeeTBs
                                    join unit in db.UnitTBs
                                    on employDB.UnitID equals unit.UnitID
                                    orderby unit.CostCenter ascending, employDB.EmployeeID ascending
                                    where employDB.EmployeeActive == true
                                    select employDB).ToList();

                    List<ProjectTB> ProjectList = new List<ProjectTB>();
                    ProjectList = (from projectDB in db.ProjectTBs
                                   orderby projectDB.ProjectType descending, projectDB.ProjectName ascending
                                   where projectDB.ProjectActive == true
                                   select projectDB).ToList();

                    #region GENERATE COLUMN INDEX
                    string[] Columns = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                               "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
                    List<string> ProjectCol = new List<string>();
                    for (var i = 0; i < ProjectList.Count(); i++)
                    {
                        string ColumnIndex = "";
                        int index = i*2 + 8;
                        int index0 = 0;
                        int index1 = 0;

                        if (index / 26 == 0)
                        {
                            ColumnIndex = "";
                            index1 = index % 26;
                            ColumnIndex = Columns[index1];
                            
                        }
                        else
                        {
                            ColumnIndex = "";
                            index0 = index / 26 - 1;
                            index1 = index % 26;
                            ColumnIndex = Columns[index0] + Columns[index1];
                        }
                        ProjectCol.Add(ColumnIndex);
                    }
                    #endregion

                    if (sheetSummary != null)
                    {

                        updateSummarySheet(CostCenterList, EmployeeList, sheetSummary, wbPart, ProjectList, ProjectCol);
                        updateProjectName(CostCenterList, EmployeeList, sheetSummary, wbPart, ProjectList, ProjectCol);

                    }

                }
                return File(destination, "application/msexcel", fileName);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    message = "There is something wrong, please try again.",
                    errorStack = e.StackTrace
                },
                        JsonRequestBehavior.AllowGet);
            }
        }

        public void updateSummarySheet(List<UnitTB> CostCenterList, List<EmployeeTB> EmployeeList,  Sheet sheetSummary, WorkbookPart wbPart, List<ProjectTB> ProjectList, List<string> ProjectCol )
        {
            // get worksheetpart by sheet id
            WorksheetPart worksheetPart = wbPart.GetPartById(sheetSummary.Id.Value) as WorksheetPart;

            var rowShift = 11;         
            for(var i = 0; i< CostCenterList.Count(); i++)
            {
                int row = i + rowShift;
                string cellIndex1 = "D" + row.ToString();
                string cellIndex2 = "E" + row.ToString();
                UpdateValue("Summary", cellIndex1, CostCenterList[i].CostCenter, 0, false, wbPart);
                UpdateValue("Summary", cellIndex2, CostCenterList[i].Unit, 0, true, wbPart);
            }

            var k = 0;
            for (var i = 0; i < EmployeeList.Count(); i++)
            {
                int row = k + CostCenterList.Count() + rowShift;
                int no = k + 1;
                string CostCenter = null;
                for (var m = 0; m < CostCenterList.Count(); m++)
                {
                    if (EmployeeList[i].UnitID == CostCenterList[m].UnitID)
                    {
                        CostCenter = CostCenterList[m].CostCenter.ToString();
                    }
                }

                string cellIndex0 = "C" + row.ToString();
                string cellIndex1 = "D" + row.ToString();
                string cellIndex2 = "E" + row.ToString();
                string cellIndex3 = "G" + row.ToString();

                UpdateValue("Summary", cellIndex0, no.ToString(), 0, false, wbPart);
                UpdateValue("Summary", cellIndex1, EmployeeList[i].EmployeeID, 0, true, wbPart);
                UpdateValue("Summary", cellIndex2, EmployeeList[i].Name + " " + EmployeeList[i].Surname, 0, true, wbPart);
                UpdateValue("Summary", cellIndex3, CostCenter, 0, false, wbPart);
                k++;
            }

            //for (var i = 0; i < ProjectList.Count(); i++)
            //{
            //    string cellIndex = ProjectCol[i] + "8";
            //    UpdateValue("Summary", cellIndex, ProjectList[i].ProjectName, 0, true, wbPart);
            //}

            wbPart.Workbook.Save();
        }
        public void updateProjectName(List<UnitTB> CostCenterList, List<EmployeeTB> EmployeeList, Sheet sheetSummary, WorkbookPart wbPart, List<ProjectTB> ProjectList, List<string> ProjectCol)
        {
            WorksheetPart worksheetPart = wbPart.GetPartById(sheetSummary.Id.Value) as WorksheetPart;
            for (var i = 0; i < ProjectList.Count(); i++)
            {
                string cellIndex = ProjectCol[i] + "11";
                //int m = i + 15;
                //string cellIndex = "A" + m.ToString();
                UpdateValue("Summary", cellIndex, ProjectList[i].ProjectName, 0, true, wbPart);
            }
            wbPart.Workbook.Save();
        }


        public bool UpdateValue(string sheetName, string addressName, string value,
                        UInt32Value styleIndex, bool isString, WorkbookPart wbPart)
        {
            // Assume failure.
            bool updated = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(
                (s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.

                    //int stringIndex = InsertSharedStringItem(wbPart, value);

                    //cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                ws.Save();
                updated = true;
            }

            return updated;
        }

        // Given the main workbook part, and a text value, insert the text into 
        // the shared string table. Create the table if necessary. If the value 
        // already exists, return its index. If it doesn't exist, insert it and 
        // return its new index.
        private int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart
                .GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        // Given a Worksheet and an address (like "AZ254"), either return a 
        // cell reference, or create the cell reference and return it.
        private Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        // Return the row at the specified rowIndex located within
        // the sheet data passed in via wsData. If the row does not
        // exist, create it.
        private Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
            Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }

        // This method is used to force a recalculation of cells containing formulas. The
        // CellValue has a cached value of the evaluated formula. This
        // prevents Excel from recalculating the cell even if 
        // calculation is set to automatic.
        private bool RemoveCellValue(string sheetName, string addressName, WorkbookPart wbPart)
        {
            bool returnValue = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().
                Where(s => s.Name == sheetName).FirstOrDefault();
            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                // If there is a cell value, remove it to force a recalculation
                // on this cell.
                if (cell.CellValue != null)
                {
                    cell.CellValue.Remove();
                }

                // Save the worksheet.
                ws.Save();
                returnValue = true;
            }

            return returnValue;
        }
    }
}