using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Entities.Models.DTO;
using Entities.Models.DTO.Home;
using Microsoft.Extensions.Logging;

namespace BusinessAccess.Services
{
    public class HomeService : IHomeService
    {
        private readonly ILogger<IHomeService> _logger;
        private readonly IHomeDataAccess _homeDataAccess;

        public HomeService(ILogger<IHomeService> logger, IHomeDataAccess homeDataAccess)
        {
            _logger = logger;
            _homeDataAccess = homeDataAccess;
        }

        public async Task<ServiceResponse<List<Account>>> Accounts()
        {
            var accounts = MapAccounts(await _homeDataAccess.GetAccounts());
            if (accounts == null) return new ServiceResponse<List<Account>>(false, "Failed to get Accounts", null);
            return new ServiceResponse<List<Account>>(true, "Success", accounts);
        }

        private List<Account> MapAccounts(ServiceResponse<DataTable> accounts)
        {
            if (accounts == null || !accounts.Success) return null;
            return (from a in accounts.ResponseObject.AsEnumerable()
                    select new Account()
                    {
                        ID = Conversions.ToInt(a["AccountID"], -1),
                        Name = Conversions.ToString(a["AccountName"], ""),
                        Type = Conversions.ToString(a["AccountType"], ""),
                        ExcelMapping = Conversions.ToString(a["ExcelMapping"], ""),
                        IsActive = Conversions.ToBoolean(a["IsActive"], false)
                    }).ToList();
        }

        public async Task<ServiceResponse<List<Budget>>> Budgets(DateTime fromDate, DateTime toDate)
        {
            var budgets = MapBudgets(await _homeDataAccess.GetBudgets(fromDate, toDate));
            if (budgets == null) return new ServiceResponse<List<Budget>>(false, "Failed to get Budgets", null);
            return new ServiceResponse<List<Budget>>(true, "Success", budgets);
        }

        private List<Budget> MapBudgets(ServiceResponse<DataTable> budgets)
        {
            if (budgets == null || !budgets.Success) return null;
            return (from b in budgets.ResponseObject.AsEnumerable()
                    select new Budget()
                    {
                        ID = Conversions.ToInt(b["ID"], -1),
                        FromDate = Conversions.ToDateTime(b["FromDate"], DateTime.MinValue),
                        ToDate = Conversions.ToDateTime(b["ToDate"], DateTime.MinValue),
                        Group = Conversions.ToString(b["Group"], ""),
                        SubGroup = Conversions.ToString(b["SubGroup"], ""),
                        Amount = Conversions.ToDecimal(b["Amount"], -1),
                        IsActive = Conversions.ToBoolean(b["IsActive"], false)
                    }).ToList();
        }

        public async Task<ServiceResponse<int>> SaveTransactions(DateTime minDate) {
            _logger.LogInformation("SaveTransactions");
            int totalRecords = 0;
            int savedRecords = 0;
            try
            {
                DataTable dt = new DataTable();
                var accounts = MapAccounts(await _homeDataAccess.GetAccounts());
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open("/users/laxmikanthponna/Downloads/US-Accounts.xlsx", false))
                {
                    WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                    IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    //for (int i = 0; i < noOfSheets; i++)
                    
                    foreach (Sheet sheet in sheets)
                    {
                        List<Transaction> transactions = new List<Transaction>();
                        var account = accounts.Where(a => a.ExcelMapping == sheet.Name).FirstOrDefault();
                        if (account == null || !account.IsActive)
                        {
                            _logger.LogInformation($"Ignoring Sheet: {sheet.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Processing Sheet: {sheet.Name}");
                        string relationshipId = sheet.Id.Value;
                        WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                        Worksheet workSheet = worksheetPart.Worksheet;
                        SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                        IEnumerable<Row> rows = sheetData.Descendants<Row>();

                        for (int i = 0; i < rows.Count(); i++)
                        {
                            if (i == 0) continue;

                            Transaction _transaction = ParseTransaction(spreadSheetDocument, sheet.Name, rows.ElementAt(i), i, account);
                            if (_transaction != null)
                                transactions.Add(_transaction);
                        }

                        if (transactions != null && transactions.Count > 0)
                        {
                            totalRecords += transactions.Count;
                            var saveResponse = await _homeDataAccess.SaveTransactions(transactions);
                            savedRecords += saveResponse.ResponseObject;
                        }
                           
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SaveTransactions Exception: " + ex.Message);
            }
            _logger.LogInformation("Processing Completed");
            if (totalRecords == savedRecords) return new ServiceResponse<int>(true, "Success", savedRecords);
            else return new ServiceResponse<int>(false, $"Total Records: {totalRecords}, Updated: {savedRecords}", savedRecords);
            
        }

        private Transaction ParseTransaction(SpreadsheetDocument spreadSheetDocument, StringValue sheetName, Row row, int rowNumber, Account account)
        {
            Transaction _transaction = null;
            int cellCount = 0;
            string firstCellValue = "";

            try
            {
                if (rowNumber == 0)
                {
                }
                else
                {
                    var cellValues = from cell in row.Descendants<Cell>()
                                     select cell;
                    cellCount = cellValues.Count();

                    //DateTime dtPostedDate = DateTime.Now.AddYears(1);
                    firstCellValue = GetCellValue(spreadSheetDocument, cellValues, "A", rowNumber + 1);
                    DateTime date;
                    if (!string.IsNullOrWhiteSpace(firstCellValue) && ParseDateValue(firstCellValue) <= DateTime.Now.AddMonths(1))
                    {
                        date = ParseDateValue(firstCellValue);
                        try
                        {
                            _transaction = new Transaction()
                            {
                                RowIndex = rowNumber + 1,
                                AccountID = account.ID,
                                Date = date,
                                Description = GetCellValue(spreadSheetDocument, cellValues, "B", rowNumber + 1),
                                Debit = Conversions.ToDecimal(GetCellValue(spreadSheetDocument, cellValues, "C", rowNumber + 1), 0),
                                Credit = Conversions.ToDecimal(GetCellValue(spreadSheetDocument, cellValues, "D", rowNumber + 1), 0),
                                Total = Conversions.ToDecimal(GetCellValue(spreadSheetDocument, cellValues, "E", rowNumber + 1), 0),
                                TransactedBy = cellCount > 4 ? GetCellValue(spreadSheetDocument, cellValues, "F", rowNumber + 1) : "Kanth",
                                Group = cellCount > 5 ? GetCellValue(spreadSheetDocument, cellValues, "G", rowNumber + 1) : null,
                                SubGroup = cellCount > 6 ? GetCellValue(spreadSheetDocument, cellValues, "H", rowNumber + 1) : null,
                                Comments = cellCount > 7 ? GetCellValue(spreadSheetDocument, cellValues, "I", rowNumber + 1) : null
                            };
                            _transaction.TransactedBy = String.IsNullOrWhiteSpace(_transaction.TransactedBy) ? "Kanth" : _transaction.TransactedBy;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Exception while processing sheet:" + sheetName + " row number: " + (rowNumber + 1) + " Cell count: " + cellCount + " first cell: " + date.ToString() + " Exception: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ParseTransaction Exception: " + ex.Message);
            }
            return _transaction;
        }

        private string GetCellValue(SpreadsheetDocument document, IEnumerable<Cell> cellValues, string v1, int v2)
        {
            string reusltStr = string.Empty;
            try
            {
                Cell cell = cellValues.Where(c => c.CellReference.Value == (v1 + v2.ToString())).FirstOrDefault();
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                string value = (cell == null || cell.CellValue == null) ? null : cell.CellValue.InnerXml;

                if (cell != null && cell.DataType != null && cell.DataType.Value == CellValues.SharedString) {
                    reusltStr = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
                else {
                    reusltStr = value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCellValue cell reference: " + v1 + v2 + " Exception: " + ex.Message);
            }
            return String.IsNullOrWhiteSpace(reusltStr) ? null : reusltStr;
        }

        private DateTime ParseDateValue(string val)
        {
            DateTime retVal = DateTime.Now.AddYears(1);
            try
            {
                if (!string.IsNullOrWhiteSpace(val))
                {
                    retVal = DateTime.FromOADate(double.Parse(val));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception ParseDateValue: " + ex.Message);
            }
            return retVal;
        }

        public async Task<ServiceResponse<HomeTransactionsResponse>> GetTransactions(DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation($"HomeService--GetTransactions");
            try
            {
                var budgets = MapBudgets(await _homeDataAccess.GetBudgets(fromDate, toDate));
                List<HomeExpensesTracker> trans = MapExpenses(await _homeDataAccess.GetTransactions(fromDate, toDate), budgets);
                if (trans != null) return new ServiceResponse<HomeTransactionsResponse>(true, "Success", new HomeTransactionsResponse() { transactions = trans });
            }
            catch(Exception ex)
            {
                _logger.LogError("HomeService--GetTransactions Exception: " + ex.Message);
            }
            return new ServiceResponse<HomeTransactionsResponse>(false, "Failed to get transactions", null);
        }

        private List<HomeExpensesTracker> MapExpenses(ServiceResponse<DataTable> trans, List<Budget> budgets)
        {
            if (trans == null || !trans.Success) return null;

            return (from t in trans.ResponseObject.AsEnumerable()
                    //where ((Conversions.ToString(t["Group"], "") != "Credit Card" && Conversions.ToString(t["SubGroup"], "") != "Payment")
                    //|| ((Conversions.ToString(t["Group"], "") != "Account" && Conversions.ToString(t["SubGroup"], "") != "Transfer"))) //Convert.ToDecimal(t["Units"]) > 0
                    group t by new { Group = Conversions.ToString(t["Group"], "Group"), SubGroup = Conversions.ToString(t["SubGroup"], "SubGroup") } into tg
                    select new HomeExpensesTracker()
                    {
                        Group = tg.Key.Group,
                        SubGroup = tg.Key.SubGroup,
                        Debit = tg.Sum(t => Conversions.ToDecimal(t["Debit"], 0)),
                        Credit = tg.Sum(t => Conversions.ToDecimal(t["Credit"], 0)),
                        Budget = GetExpenseBudget(budgets, tg.Key.Group, tg.Key.SubGroup),
                    })
                    .Where(t => t.Group != "Credit Card" && t.SubGroup != "Payment")
                    .Where(t => t.Group != "Account" && t.SubGroup != "Transfer")
                    .ToList();
        }

        private decimal GetExpenseBudget(List<Budget> budgets, string group, string subGroup)
        {
            var budget = budgets.Where(b => b.Group == group && b.SubGroup == subGroup).FirstOrDefault();
            return budget == null ? 0 : budget.Amount;
        }
    }
}
