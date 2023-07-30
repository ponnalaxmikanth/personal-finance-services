using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Interfaces;
using DataAccess.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DownloadData.Interfaces;
using Entities.Models.DTO.Funds;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DownloadData
{
    public class InitializeFundInvestments : IInitializeFundInvestments
    {
        public readonly IInitializeFundsDataAccess _initializeFundsDataAccess;
        private readonly ILogger<InitializeFundInvestments> _logger;
        private List<FundRedeems> lstRedeems;

        public InitializeFundInvestments(ILogger<InitializeFundInvestments> logger, IInitializeFundsDataAccess initializeFundsDataAccess)
        {
            _initializeFundsDataAccess = initializeFundsDataAccess;
            _logger = logger;
            lstRedeems = new List<FundRedeems>();
        }

        #region old implementation

        

        //private List<FundSwitchIn> GetSwitchiInTransactions(List<FundTransaction> transactions)
        //{
        //    List<FundSwitchIn> result = new List<FundSwitchIn>();

        //    result = (from t in transactions
        //              where t.TransactionType == "Switch Out"
        //              select new FundSwitchIn()
        //              {
        //                  PortfolioID = t.SwitchInTransaction.PortfolioID,
        //                  FolioNumber = t.SwitchInTransaction.FolioNumber,

        //                  SwitchInDate = t.SwitchInTransaction.TransactionDate,
        //                  InSchemaCode = t.SwitchInTransaction.SchemaCode,
        //                  InUnits = t.SwitchInTransaction.Units,
        //                  InNAV = t.SwitchInTransaction.Price,
        //                  InCharges = t.SwitchInTransaction.Charges.STT + t.SwitchInTransaction.Charges.StampDuty,

        //                  SwitchOutDate = t.TransactionDate,
        //                  OutSchemaCode = t.SchemaCode,
        //                  OutUnits = t.Units < 0 ? t.Units * -1 : t.Units,
        //                  OutNAV = t.ActualNAV,

        //                  OutCharges = t.STT + t.StampDuty,
        //              }).ToList();

        //    return result;
        //}

        //private SwitchInTransaction GetSwitchInTransactionDetails(WorkbookPart workbookPart, Row r)
        //{
        //    var folioNumber = GetCellValue("V", workbookPart, r.RowIndex, r);
        //    if (string.IsNullOrWhiteSpace(folioNumber)) return null;

        //    return new SwitchInTransaction()
        //    {
        //        FolioNumber = folioNumber,
        //        FolioID = StringToInt(GetCellValue("W", workbookPart, r.RowIndex, r)),
        //        PortfolioID = StringToInt(GetCellValue("X", workbookPart, r.RowIndex, r)),
        //        SchemaCode = StringToInt(GetCellValue("Z", workbookPart, r.RowIndex, r)),
        //        TransactionDate = GetDate("AA", workbookPart, r),
        //        TransactionType = GetCellValue("AB   ", workbookPart, r.RowIndex, r),
        //        Amount = StringToDecimal(GetCellValue("AD", workbookPart, r.RowIndex, r)),
        //        Units = StringToDecimal(GetCellValue("AE", workbookPart, r.RowIndex, r)),
        //        Price = StringToDecimal(GetCellValue("AF", workbookPart, r.RowIndex, r)),
        //        Charges = new Charges()
        //        {
        //            STT = StringToDecimal(GetCellValue("AG", workbookPart, r.RowIndex, r)),
        //            StampDuty = StringToDecimal(GetCellValue("AH", workbookPart, r.RowIndex, r)),
        //            TDS = StringToDecimal(GetCellValue("AI", workbookPart, r.RowIndex, r)),
        //        }
        //    };
        //}

        //private List<InvestmentTracker> GetInvestmentTransaction(List<FundTransaction> res)
        //{
        //    foreach (var eachTransaction in res)
        //    {
        //        switch (eachTransaction.TransactionType)
        //        {
        //            case "Purchase":
        //            case "Switch In":
        //                result.Add(new InvestmentTracker() {
        //                    PortfolioID = eachTransaction.PortfolioID,
        //                    TransactionDate = eachTransaction.TransactionDate,
        //                    Amount = eachTransaction.TransactionType == "Purchase" ?  eachTransaction.Amount : 0,
        //                    Charges = eachTransaction.STT + eachTransaction.TDS + eachTransaction.StampDuty,
        //                    TransaactionType = eachTransaction.TransactionType
        //                });
        //                break;

        //            case "Reinvest":
        //            case "Payout":
        //                result.Add(new InvestmentTracker()
        //                {
        //                    PortfolioID = eachTransaction.PortfolioID,
        //                    TransactionDate = eachTransaction.TransactionDate,
        //                    Amount = 0,
        //                    Charges = eachTransaction.STT + eachTransaction.TDS + eachTransaction.StampDuty,
        //                    TransaactionType = eachTransaction.TransactionType
        //                });
        //                break;

        //            case "Redemption":
        //            case "Switch Out":
        //                result.Add(new InvestmentTracker()
        //                {
        //                    PortfolioID = eachTransaction.PortfolioID,
        //                    TransactionDate = eachTransaction.TransactionDate,
        //                    Amount = eachTransaction.TransactionType == "Redemption" ? eachTransaction.Amount : 0,
        //                    Charges = eachTransaction.STT + eachTransaction.TDS + eachTransaction.StampDuty,
        //                    TransaactionType = eachTransaction.TransactionType
        //                });
        //                break;
        //        }
        //    }
        //    return result.Where(t => t.Amount > 0 || t.Charges > 0).OrderBy(t => t.TransactionDate).ToList();
        //}

        private DateTime GetDate(string cellReference, WorkbookPart workbookPart, Row r)
        {
            try
            {
                var cellValue = GetCellValue(cellReference, workbookPart, r.RowIndex, r);
                return string.IsNullOrWhiteSpace(cellValue) ? new DateTime(2000, 1, 1) : DateTime.Parse(cellValue);
            }
            catch(Exception ex)
            {
                _logger.LogInformation(ex, $"Initialize failed to Initialize Fund Transactions: {ex.Message}, {r.RowIndex}");
            }
            return new DateTime(2000, 1, 1);
        }

        //private string GetCellValue(Cell cell, WorkbookPart wbPart)
        //{
        //    string value = cell.InnerText;
        //    if (cell.DataType != null)
        //    {
        //        switch (cell.DataType.Value)
        //        {
        //            case CellValues.SharedString:
        //                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
        //                if (stringTable != null)
        //                {
        //                    value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
        //                }
        //                break;
        //            case CellValues.Boolean:
        //                switch (value)
        //                {
        //                    case "0":
        //                        value = "FALSE";
        //                        break;
        //                    default:
        //                        value = "TRUE";
        //                        break;
        //                }
        //                break;
        //        }
        //    }
        //    return value;
        //}

        private string GetCellValue(string cellAddress, WorkbookPart wbPart, uint rowIndex, Row r) {
            string value = string.Empty;
            Cell cell = r.Elements<Cell>().Where(c => c.CellReference == cellAddress + rowIndex.ToString()).FirstOrDefault();
            if (cell == null) return value;
             value = cell.InnerText;
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        if (stringTable != null)
                            value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                        break;
                    case CellValues.Boolean:
                        switch (value)
                        {
                            case "0":
                                value = "FALSE";
                                break;
                            default:
                                value = "TRUE";
                                break;
                        }
                        break;
                }
            }
            return value;
        }

        private decimal StringToDecimal(string val) {
            try
            {
                if (string.IsNullOrWhiteSpace(val)) return 0;
                else return decimal.Round(decimal.Parse(val, System.Globalization.NumberStyles.Float), 5);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        private int StringToInt(string val)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(val)) return 0;
                else return int.Parse(val);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        private List<FundTransaction> GenerateHistory(List<FundTransaction> res, List<FundDividend> lstDividends, List<FundRedeems> lstRedeems)
        {
            List<FundTransaction> result = new List<FundTransaction>();

            foreach (var eachTransaction in res) {
                switch (eachTransaction.TransactionType) {
                    case "Purchase":
                    //case "Switch In":
                        result.Add(eachTransaction);
                        break;

                    case "Reinvest":
                        UpdateDividend(result, eachTransaction);
                        AddDividend(lstDividends, eachTransaction);
                        result.Add(eachTransaction);
                        break;

                    case "Payout":
                        UpdateDividend(result, eachTransaction);
                        AddDividend(lstDividends, eachTransaction);
                        break;

                    case "Redemption":
                    case "Switch Out":
                        RedeemFunds(result, eachTransaction, lstRedeems);
                        break;
                }
            }
            return result;
        }

        private void AddDividend(List<FundDividend> lstDividends, FundTransaction transaction)
        {
            if (transaction.DividendHistory == null) transaction.DividendHistory = new List<FundDividend>();

            var dividendTransaction = new FundDividend()
            {
                PortfolioID = transaction.PortfolioID,
                FolioNumber = transaction.FolioNumber,
                SchemaCode = transaction.SchemaCode,
                DividendDate = transaction.TransactionDate,
                Units = transaction.Units,
                NAV = transaction.ActualNAV,
                DividendType = transaction.TransactionType,
                Amount = transaction.Amount > 0 ? transaction.Amount * -1 : transaction.Amount,
                DividendNAV = transaction.DividendPerNAV,
                Charges = transaction.STT + transaction.StampDuty,
                TDS = transaction.TDS
            };

            lstDividends.Add(dividendTransaction);
            transaction.DividendHistory.Add(dividendTransaction);
        }

        private void RedeemFunds(List<FundTransaction> transactions, FundTransaction redeemTransaction, List<FundRedeems> lstRedeems)
        {
            var filteredTransactions = transactions.Where(r => r.Units > 0 && r.PortfolioID == redeemTransaction.PortfolioID && r.FolioNumber == redeemTransaction.FolioNumber && r.SchemaCode == redeemTransaction.SchemaCode).ToList();

            decimal redeemUnits = redeemTransaction.Units < 0 ? redeemTransaction.Units * -1 : redeemTransaction.Units;
            decimal remainingUnits = redeemUnits;
            decimal soldUnits = redeemUnits;

            decimal tds = redeemTransaction.TDS;
            decimal stt = redeemTransaction.STT;
            decimal stampDuty = redeemTransaction.StampDuty;

            foreach (var transaction in filteredTransactions)
            {
                if (transaction.History == null) transaction.History = new List<FundTransaction>();

                if (redeemUnits < transaction.Units)
                {
                    lstRedeems.Add(new FundRedeems()
                    {
                        PortfolioID = transaction.PortfolioID,
                        FolioNumber = transaction.FolioNumber,
                        FolioID = transaction.FolioID,
                        InvestDate = transaction.TransactionDate,
                        InvestNAV = transaction.ActualNAV,
                        Amount = redeemUnits * transaction.ActualNAV,
                        
                        SchemaCode = transaction.SchemaCode,
                        RedeemDate = redeemTransaction.TransactionDate,
                        RedeemNAV = redeemTransaction.InvestNAV,

                        Profit = redeemUnits * (redeemTransaction.ActualNAV - transaction.ActualNAV),
                        Units = redeemUnits,
                        Charges = new Charges()
                        {
                            TDS = RoundDecimal(transaction.TDS + ((redeemUnits / soldUnits) * tds)),
                            STT = RoundDecimal(transaction.STT + ((redeemUnits / soldUnits) * stt)),
                            StampDuty = RoundDecimal(transaction.StampDuty + ((redeemUnits / soldUnits) * stampDuty))
                        },
                        FinancialYear = transaction.FinancialYear,
                        FundOption = "Redemption",
                        PANCard = transaction.PanCard,
                        ProductCode = transaction.ProductCode,
                        DividendPerNAV = transaction.DividendPerNAV,
                        Dividend = redeemUnits * transaction.DividendPerNAV,
                    });

                    transaction.Units = transaction.Units - redeemUnits;
                    transaction.Amount = transaction.Units * transaction.ActualNAV;
                    transaction.TDS = transaction.TDS + RoundDecimal((remainingUnits/ soldUnits) * tds);
                    transaction.STT = transaction.STT + RoundDecimal((remainingUnits / soldUnits) * stt);
                    transaction.StampDuty = transaction.StampDuty + RoundDecimal((remainingUnits / soldUnits) * stampDuty);

                    //transaction.FundOption = redeemTransaction.FundOption;
                    //transaction.TransactionType = redeemTransaction.TransactionType;

                    transaction.History.Add(new FundTransaction()
                    {
                        PortfolioID = transaction.PortfolioID,
                        FolioNumber = transaction.FolioNumber,
                        FolioID = transaction.FolioID,
                        ProductCode = transaction.ProductCode,
                        SchemaCode = transaction.SchemaCode,
                        TransactionDate = redeemTransaction.TransactionDate,
                        FundOption = redeemTransaction.FundOption,
                        TransactionType = redeemTransaction.TransactionType,
                        Units = -redeemUnits,

                        Amount = redeemUnits * redeemTransaction.InvestNAV,
                        DividendPerNAV = transaction.DividendPerNAV,
                        Dividend = redeemUnits * transaction.DividendPerNAV,
                        InvestNAV = transaction.InvestNAV,
                        ActualNAV = transaction.ActualNAV,
                        RedeemNAV = redeemTransaction.InvestNAV,
                        TDS = transaction.TDS + RoundDecimal((remainingUnits / soldUnits) * tds),
                        STT = transaction.STT + RoundDecimal((remainingUnits / soldUnits) * stt),
                        StampDuty = transaction.StampDuty + RoundDecimal((remainingUnits / soldUnits) * stampDuty),
                    });
                    break;
                }
                else
                {
                    decimal transUnits = transaction.Units;
                    if (redeemTransaction.TransactionType == "Switch Out")
                    {
                        decimal tranpercent = GetSwitchInPercent(transaction, redeemTransaction);

                        transaction.SchemaCode = redeemTransaction.SwitchInTransaction.SchemaCode;
                        transaction.InvestNAV = redeemTransaction.SwitchInTransaction.Price;
                        
                        transaction.TransactionType = "Switch";
                        transaction.FundOption = "Switch";
                        redeemUnits = redeemUnits - transaction.Units;

                        transaction.Units = RoundDecimal(redeemTransaction.SwitchInTransaction.Units * tranpercent);
                        transaction.ActualNAV = transaction.Amount / transaction.Units;
                        transaction.STT = RoundDecimal(transaction.STT + RoundDecimal(transaction.STT * tranpercent));
                        transaction.StampDuty = transaction.StampDuty + RoundDecimal(transaction.StampDuty * tranpercent);
                        transaction.TDS = transaction.TDS + RoundDecimal(transaction.TDS * tranpercent);
                    }
                    else
                    {
                        lstRedeems.Add(new FundRedeems()
                        {
                            PortfolioID = transaction.PortfolioID,
                            FolioNumber = transaction.FolioNumber,
                            FolioID = transaction.FolioID,
                            SchemaCode = transaction.SchemaCode,
                            
                            InvestDate = transaction.TransactionDate,
                            InvestNAV = transaction.ActualNAV,
                            Amount = transaction.Amount,

                            RedeemDate = redeemTransaction.TransactionDate,
                            RedeemNAV = redeemTransaction.ActualNAV,
                            Profit = transaction.Units * (redeemTransaction.ActualNAV - transaction.ActualNAV),
                            Units = transUnits,
                            Charges = new Charges()
                            {
                                TDS = RoundDecimal(transaction.TDS + ((transUnits / soldUnits) * tds)),
                                STT = RoundDecimal(transaction.STT + ((transUnits / soldUnits) * stt)),
                                StampDuty = RoundDecimal(transaction.StampDuty + ((transUnits / soldUnits) * stampDuty))
                            },
                            FinancialYear = transaction.FinancialYear,
                            FundOption = "Redemption",
                            PANCard = transaction.PanCard,
                            ProductCode = transaction.ProductCode,
                            DividendPerNAV = transaction.DividendPerNAV,
                            Dividend = transUnits * transaction.DividendPerNAV,
                        });
                        redeemUnits = redeemUnits - transaction.Units;
                        transaction.Units = 0;
                        transaction.FundOption = redeemTransaction.FundOption;
                        transaction.TransactionType = redeemTransaction.TransactionType;

                        transaction.TDS += RoundDecimal(transaction.TDS + ((transUnits / soldUnits) * tds));
                        transaction.STT += RoundDecimal(transaction.STT + ((transUnits / soldUnits) * stt));
                        transaction.StampDuty += RoundDecimal(transaction.TDS + ((transUnits / soldUnits) * stampDuty));
                    }

                    transaction.History.Add(new FundTransaction()
                    {
                        PortfolioID = transaction.PortfolioID,
                        FolioNumber = transaction.FolioNumber,
                        FolioID = transaction.FolioID,
                        ProductCode = transaction.ProductCode,
                        SchemaCode = transaction.SchemaCode,
                        TransactionDate = redeemTransaction.TransactionDate,
                        FundOption = redeemTransaction.FundOption,
                        TransactionType = redeemTransaction.TransactionType,
                        Units = -transUnits,

                        Amount = transUnits * redeemTransaction.InvestNAV,
                        DividendPerNAV = transaction.DividendPerNAV,
                        Dividend = transUnits * transaction.DividendPerNAV,
                        InvestNAV = transaction.InvestNAV,
                        ActualNAV = transaction.ActualNAV,
                        RedeemNAV = redeemTransaction.InvestNAV,
                        TDS = transaction.TDS + RoundDecimal((transUnits / soldUnits) * tds),
                        STT = transaction.STT + RoundDecimal((transUnits / soldUnits) * stt),
                        StampDuty = transaction.StampDuty + RoundDecimal((transUnits / soldUnits) * stampDuty),
                    });
                }
                if (redeemUnits <= 0) break;
            }
        }

        private decimal GetSwitchInPercent(FundTransaction transaction, FundTransaction redeemTransaction)
        {
            decimal result = transaction.Units / redeemTransaction.Units;
            return result < 0 ? result * -1 : result;
        }

        private void UpdateDividend(List<FundTransaction> transactions, FundTransaction dividendTransaction)
        {
            var filtered = transactions.Where(r => r.Units > 0 && r.PortfolioID == dividendTransaction.PortfolioID && r.FolioNumber == dividendTransaction.FolioNumber && r.SchemaCode == dividendTransaction.SchemaCode).ToList();
            decimal totalUnits = filtered.Sum(r => r.Units);

            decimal tds = dividendTransaction.TDS;
            decimal stt = dividendTransaction.STT;
            decimal stampDuty = dividendTransaction.StampDuty;

            foreach (var transaction in filtered)
            {
                if (transaction.History == null)
                    transaction.History = new List<FundTransaction>();

                transaction.ActualNAV = transaction.ActualNAV - dividendTransaction.DividendPerNAV;
                transaction.Amount = transaction.Amount - (transaction.Units * dividendTransaction.DividendPerNAV);
                transaction.Dividend = transaction.Dividend + (transaction.Units * dividendTransaction.DividendPerNAV);
                transaction.DividendPerNAV = transaction.DividendPerNAV + dividendTransaction.DividendPerNAV;

                transaction.TDS = transaction.TDS + RoundDecimal((transaction.Units / totalUnits) * tds);
                transaction.STT = transaction.STT + RoundDecimal((transaction.Units / totalUnits) * stt);
                transaction.StampDuty = transaction.StampDuty + RoundDecimal((transaction.Units / totalUnits) * stampDuty);

                transaction.History.Add(new FundTransaction()
                {
                    PortfolioID = transaction.PortfolioID,
                    FolioNumber = transaction.FolioNumber,
                    FolioID = transaction.FolioID,
                    ProductCode = transaction.ProductCode,
                    SchemaCode = transaction.SchemaCode,
                    TransactionDate = dividendTransaction.TransactionDate,
                    FundOption = dividendTransaction.FundOption,
                    TransactionType = dividendTransaction.TransactionType,
                    Amount = transaction.Units * dividendTransaction.DividendPerNAV,
                    DividendPerNAV = dividendTransaction.DividendPerNAV,
                    Dividend = transaction.Units * dividendTransaction.DividendPerNAV,

                    TDS = transaction.TDS + RoundDecimal((transaction.Units / totalUnits) * tds),
                    STT = transaction.STT + RoundDecimal((transaction.Units / totalUnits) * stt),
                    StampDuty = transaction.StampDuty + RoundDecimal((transaction.Units / totalUnits) * stampDuty),
                });
            }
        }

        private decimal RoundDecimal(decimal value)
        {
            return decimal.Round(value, 4);
        }

        #endregion

        #region new implementation

        public bool Initialize(string path)
        {
            try
            {
                _logger.LogInformation("Processing started: " + path);
                var fileName = string.Format(path);
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    int maxRows = sheetData.Elements<Row>().Count();
                    //var res = (from r in sheetData.Elements<Row>()
                    //           where r.RowIndex.Value > 1
                    //           select new FundTransaction
                    //           {
                    //               RowIndex = r.RowIndex.Value,
                    //               FolioNumber = GetCellValue("A", workbookPart, r.RowIndex, r),
                    //               FolioID = StringToInt(GetCellValue("B", workbookPart, r.RowIndex, r)),
                    //               PortfolioID = StringToInt(GetCellValue("C", workbookPart, r.RowIndex, r)),
                    //               SchemaCode = StringToInt(GetCellValue("E", workbookPart, r.RowIndex, r)),
                    //               TransactionDate = GetDate("G", workbookPart, r),
                    //               TransactionType = GetCellValue("H", workbookPart, r.RowIndex, r),
                    //               DividendPerNAV = StringToDecimal(GetCellValue("I", workbookPart, r.RowIndex, r)),
                    //               Amount = StringToDecimal(GetCellValue("J", workbookPart, r.RowIndex, r)),
                    //               Units = StringToDecimal(GetCellValue("K", workbookPart, r.RowIndex, r)),
                    //               InvestNAV = StringToDecimal(GetCellValue("L", workbookPart, r.RowIndex, r)),
                    //               STT = StringToDecimal(GetCellValue("M", workbookPart, r.RowIndex, r)),
                    //               StampDuty = StringToDecimal(GetCellValue("N", workbookPart, r.RowIndex, r)),
                    //               TDS = StringToDecimal(GetCellValue("O", workbookPart, r.RowIndex, r)),
                    //               PanCard = GetCellValue("R", workbookPart, r.RowIndex, r),
                    //               ProductCode = GetCellValue("S", workbookPart, r.RowIndex, r),
                    //               SwitchInTransaction = GetSwitchInTransactionDetails(workbookPart, r)
                    //           }).Where(r => r.TransactionType != "Invalid Redemption" && r.TransactionDate != null)
                    //             .Where(r => r.TransactionType != "Cancelled")
                    //             //.Where(r => r.FolioNumber == "12532488" && r.SchemaCode == 100915 && r.PortfolioID == 1)
                    //             //.Where(r => r.PortfolioID == 1 && (r.SchemaCode == 135800 || r.SchemaCode == 135805) &&
                    //             //           (
                    //             //               r.TransactionDate == new DateTime(2017, 08, 22) ||
                    //             //               r.TransactionDate == new DateTime(2017, 09, 07) ||
                    //             //               r.TransactionDate == new DateTime(2017, 09, 11) ||
                    //             //               r.TransactionDate == new DateTime(2018, 01, 10) ||
                    //             //               r.TransactionDate == new DateTime(2018, 10, 03)
                    //             //           )
                    //             //      )
                    //             .OrderBy(t => t.TransactionDate)
                    //             .ToList();

                    //res.ForEach(r =>
                    //{
                    //    r.ActualNAV = r.InvestNAV;
                    //    r.FundOption = r.TransactionType;
                    //    r.FinancialYear = r.TransactionDate.Month < 4 ? r.TransactionDate.Year - 1 : r.TransactionDate.Year;
                    //});

                    //List<InvestmentTracker> lstInvestmentTracker = new List<InvestmentTracker>();
                    //List<FundDividend> lstDividends = new List<FundDividend>();
                    //List<FundRedeems> lstRedeems = new List<FundRedeems>();
                    //List<FundSwitchIn> lstSwitchIns = new List<FundSwitchIn>();
                    //var result = GenerateHistory(res, lstDividends, lstRedeems);

                    //lstSwitchIns = GetSwitchiInTransactions(res);

                    //string res1 = JsonConvert.SerializeObject(lstSwitchIns);
                    //int count = lstRedeems.Where(r => r.PortfolioID == 1 && r.FolioNumber == "6390028/26" && r.SchemaCode == 120622).Count();
                    //string res1 = JsonConvert.SerializeObject(lstRedeems.Where(r => r.FolioNumber == "6390028/26" && r.SchemaCode == 120622 && r.PortfolioID == 1));
                    //string trans = JsonConvert.SerializeObject(res.Where(r => r.FolioNumber == "6390028/26" && r.SchemaCode == 120622 && r.PortfolioID == 1));
                    //_initializeFundsDataAccess.SaveFundsTransactions(result);

                    //_initializeFundsDataAccess.SaveFundDividends(lstDividends);

                    //_initializeFundsDataAccess.SaveFundRedeems(lstRedeems);
                    //_initializeFundsDataAccess.SaveFundSwitches(lstSwitchIns.OrderBy(t => t.SwitchOutDate).ToList());


                    saveFundTransactions(workbookPart, sheetData.Elements<Row>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Initialize failed to Initialize Fund Transactions: {ex.Message}");
            }
            _logger.LogInformation("Processing compelted");
            return true;
        }

        private void saveFundTransactions(WorkbookPart workbookPart, IEnumerable<Row> transactions)
        {
            try
            {
                var res = (from r in transactions
                           where r.RowIndex.Value > 1
                           select new Transaction
                           {
                               FolioNumber = GetCellValue("A", workbookPart, r.RowIndex, r),
                               PortfolioID = StringToInt(GetCellValue("B", workbookPart, r.RowIndex, r)),
                               FolioID = StringToInt(GetCellValue("C", workbookPart, r.RowIndex, r)),
                               TransactionDate = GetDate("G", workbookPart, r),
                               TransactionType = MapTransactionType(GetCellValue("H", workbookPart, r.RowIndex, r)),
                               Amount = StringToDecimal(GetCellValue("I", workbookPart, r.RowIndex, r)),
                               PurchaseNAV = StringToDecimal(GetCellValue("J", workbookPart, r.RowIndex, r)),
                               Units = StringToDecimal(GetCellValue("K", workbookPart, r.RowIndex, r)),
                               StampDuty = StringToDecimal(GetCellValue("L", workbookPart, r.RowIndex, r)),
                               STT = StringToDecimal(GetCellValue("M", workbookPart, r.RowIndex, r)),
                               TDS = StringToDecimal(GetCellValue("N", workbookPart, r.RowIndex, r)),

                               DividendPerNAV = StringToDecimal(GetCellValue("O", workbookPart, r.RowIndex, r)),

                               FundDetails = new FundDetails()
                               {
                                   SchemaCode = StringToInt(GetCellValue("E", workbookPart, r.RowIndex, r))
                               }
                           }).Where(r => r.TransactionType != "Invalid Redemption" && r.TransactionType != "Cancelled" && r.TransactionDate != null)
                             .Where(t => t.FundDetails.SchemaCode == 100915)
                             .OrderBy(t => t.TransactionDate)
                             .ToList();

                //_initializeFundsDataAccess.SaveFundsTransactionsHistory(res);

                //string[] divTypes = new string[] { FundTransactionTypes.ReInvest.ToString(), FundTransactionTypes.Payout.ToString() };
                //_initializeFundsDataAccess.SaveFundDividends(res.Where(t => divTypes.Contains(t.TransactionType)).ToList());

                List<Transaction> currentTransactions = generateTransactionHistory(res);

                var jsonData = JsonConvert.SerializeObject(currentTransactions);

                //List<Transaction> redeemedTransactions = getRedeemedTransactions(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Initialize Fund Transactions: {ex.Message}");
            }
        }

        private string MapTransactionType(string transactionType)
        {
            if (transactionType == "Reinvest")
                return FundTransactionTypes.ReInvest.ToString();
            else if (transactionType == "Payout")
                return FundTransactionTypes.Payout.ToString();
            else if (transactionType == "Purchase")
                return FundTransactionTypes.Investment.ToString();
            else if (transactionType == "Redemption")
                return FundTransactionTypes.Redeem.ToString();
            else if (transactionType == "Switch In")
                return FundTransactionTypes.SwitchIn.ToString();
            else if (transactionType == "Switch Out")
                return FundTransactionTypes.SwitchOut.ToString();
            return transactionType;
        }

        private List<Transaction> generateTransactionHistory(List<Transaction> res)
        {
            List<Transaction> result = new List<Transaction>();

            foreach (var eachTransaction in res)
            {
                var jsonData = JsonConvert.SerializeObject(eachTransaction);
                if (eachTransaction.TransactionType == FundTransactionTypes.Investment.ToString())
                {
                    if (eachTransaction.History == null) eachTransaction.History = new List<Transaction>();

                    eachTransaction.History.Add(JsonConvert.DeserializeObject<Transaction>(jsonData));
                    result.Add(eachTransaction);
                }

                else if (eachTransaction.TransactionType == FundTransactionTypes.Payout.ToString()
                            || eachTransaction.TransactionType == FundTransactionTypes.ReInvest.ToString())
                {
                    UpdateDividend(result, eachTransaction);
                    //AddDividend(lstDividends, eachTransaction);
                    //result.Add(eachTransaction);
                }

                else if (eachTransaction.TransactionType == FundTransactionTypes.Redeem.ToString())
                {
                    RedeemFunds(result, eachTransaction);
                }

                //case "Reinvest":
                //    UpdateDividend(result, eachTransaction);
                //    AddDividend(lstDividends, eachTransaction);
                //    result.Add(eachTransaction);
                //    break;

                //case "Payout":
                //    UpdateDividend(result, eachTransaction);
                //    AddDividend(lstDividends, eachTransaction);
                //    break;

                //case "Redemption":
                //case "Switch Out":
                //    RedeemFunds(result, eachTransaction, lstRedeems);
                //    break;
            }
            return result;
        }

        private void UpdateDividend(List<Transaction> transactions, Transaction dividendTransaction)
        {
            var filtered = transactions
                .Where(r => r.Units > 0
                        && r.FolioNumber == dividendTransaction.FolioNumber
                        && r.FundDetails.SchemaCode == dividendTransaction.FundDetails.SchemaCode
                        && r.TransactionDate <= dividendTransaction.TransactionDate)
                .ToList();
            decimal totalUnits = filtered.Sum(r => r.Units);

            decimal tds = dividendTransaction.TDS;
            decimal stt = dividendTransaction.STT;
            decimal stampDuty = dividendTransaction.StampDuty;

            foreach (var transaction in filtered)
            {
                if (transaction.History == null)
                    transaction.History = new List<Transaction>();

                var trans = JsonConvert.DeserializeObject<Transaction>(JsonConvert.SerializeObject(dividendTransaction));

                transaction.Amount = transaction.Amount - (transaction.Units * dividendTransaction.DividendPerNAV);
                transaction.DividendPerNAV = transaction.DividendPerNAV + dividendTransaction.DividendPerNAV;

                transaction.TDS = transaction.TDS + RoundDecimal((transaction.Units / totalUnits) * tds);
                transaction.STT = transaction.STT + RoundDecimal((transaction.Units / totalUnits) * stt);
                transaction.StampDuty = transaction.StampDuty + RoundDecimal((transaction.Units / totalUnits) * stampDuty);


                trans.Amount = (transaction.Units * dividendTransaction.DividendPerNAV);
                trans.DividendPerNAV = dividendTransaction.DividendPerNAV;

                trans.TDS = RoundDecimal((transaction.Units / totalUnits) * tds);
                trans.STT = RoundDecimal((transaction.Units / totalUnits) * stt);
                trans.StampDuty = RoundDecimal((transaction.Units / totalUnits) * stampDuty);
                trans.Units = dividendTransaction.TransactionType == FundTransactionTypes.Payout.ToString() ? transaction.Units :
                                                RoundDecimal((transaction.Units / totalUnits) * trans.Units);

                transaction.History.Add(trans);
            }

            if (dividendTransaction.TransactionType == FundTransactionTypes.ReInvest.ToString())
            {
                transactions.Add(dividendTransaction);
            }
        }

        private void RedeemFunds(List<Transaction> transactions, Transaction redeemTransaction)
        {
            var filteredTransactions = transactions
                .Where(r => r.Units > 0
                        && r.PortfolioID == redeemTransaction.PortfolioID
                        && r.FolioNumber == redeemTransaction.FolioNumber
                        && r.TransactionDate < redeemTransaction.TransactionDate
                        && r.FundDetails.SchemaCode == redeemTransaction.FundDetails.SchemaCode).ToList();

            decimal redeemUnits = redeemTransaction.Units < 0 ? redeemTransaction.Units * -1 : redeemTransaction.Units;
            decimal remainingUnits = redeemUnits;
            decimal soldUnits = redeemUnits;

            decimal tds = redeemTransaction.TDS;
            decimal stt = redeemTransaction.STT;
            decimal stampDuty = redeemTransaction.StampDuty;

            foreach (var transaction in filteredTransactions)
            {
                if (transaction.History == null) transaction.History = new List<Transaction>();

                var trans = JsonConvert.DeserializeObject<Transaction>(JsonConvert.SerializeObject(redeemTransaction));

                // 282.2640   113.7230
                if (redeemUnits < transaction.Units)
                {
                    transaction.TDS = transaction.TDS + RoundDecimal((transaction.Units / soldUnits) * tds);
                    transaction.STT = transaction.STT + RoundDecimal((transaction.Units / soldUnits) * stt);
                    transaction.StampDuty = transaction.StampDuty + RoundDecimal((transaction.Units / soldUnits) * stampDuty);
                    transaction.Units = transaction.Units - redeemUnits;
                    trans.Units = redeemUnits > 0 ? redeemUnits * -1 : redeemUnits;

                    break;
                }
                else
                {
                    trans.TDS = RoundDecimal((transaction.Units / soldUnits) * tds);
                    trans.STT = RoundDecimal((transaction.Units / soldUnits) * stt);
                    trans.StampDuty = RoundDecimal((transaction.Units / soldUnits) * stampDuty);
                    trans.Units = transaction.Units > 0 ? transaction.Units * -1 : transaction.Units;

                    transaction.TransactionType = redeemTransaction.TransactionType;
                    redeemUnits = redeemUnits - transaction.Units;
                    
                    transaction.Units = 0;
                }

                transaction.History.Add(trans);

                if (redeemUnits <= 0) break;
            }
        }

        #endregion
    }
}
