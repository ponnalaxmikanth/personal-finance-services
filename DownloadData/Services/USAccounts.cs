using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DownloadData.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Home;
using Microsoft.Extensions.Logging;

namespace DownloadData.Services
{
    public class USAccounts: IUSAccounts
    {
        private readonly ILogger<USAccounts> _logger;
        List<Account> lstAccounts;

        public USAccounts(ILogger<USAccounts> logger)
        {
            _logger = logger;
            lstAccounts = new List<Account>();
            lstAccounts.Add(new Account() { ID = 1, Type = "Checking", Name = "BofA", ExcelMapping = "BofA" });
            //lstAccounts.Add(new Accounts() { AccountID = 2, AccountType = "Checking", AccountName = "Dis-Checking", ExcepMapping = "Dis-Checking" });
            //lstAccounts.Add(new Accounts() { AccountID = 3, AccountType = "Checking", AccountName = "DCU-Checking", ExcepMapping = "DCU-Checking" });
            
        }

        public bool SaveAccountTransactions(string path)
        {
            try
            {
                _logger.LogInformation("Processing started: " + path);
                var fileName = string.Format(path);

                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    var worksheetParts = workbookPart.WorksheetParts;
                    var sheetData = worksheetParts.First().Worksheet.Elements<SheetData>();

                    var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();

                    int noOfSheets = workbookPart.Workbook.Sheets.Count();

                    for (int i = 0; i < noOfSheets; i++)
                    {
                        _logger.LogInformation($"Processing Sheet: {sheets.ElementAt(i).Name}");
                        var account = lstAccounts.Where(a => a.ExcelMapping == sheets.ElementAt(i).Name).FirstOrDefault();

                        if (account == null)
                            _logger.LogInformation($"Ignoring Sheet: {sheets.ElementAt(i).Name}");

                    }
                    //foreach (var eachSheet in sheetData)
                    //{
                    //    //Sheet sheet = GetSheetFromWorkSheet(workbookPart, myWorksheetPart);
                    //    //string sheetName = sheet.Name;
                    //    //_logger.LogInformation(String.Format("RelationshipId:{0}\n SheetName:{1}\n SheetId:{2}" , eachSheet..Id.Value, eachSheet.Name.Value, eachSheet.SheetId.Value)));
                    //}
                }
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while processing: {ex.Message}");
            }
            return false;
        }

        private Worksheet GetWorkSheetFromSheet(WorkbookPart workbookPart, Sheet sheet)
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            return worksheetPart.Worksheet;
        }
    }
}
