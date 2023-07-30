using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Excel.FinancialFunctions;
using Entities.Models.DTO.India;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Ubiety.Dns.Core;
using MySqlX.XDevAPI.Common;

namespace BusinessAccess.Services
{
    public class MutualFundsService : IMutualFundsService
    {
        private readonly IMutualFundsDataAccess _iMutualFundsDataAccess;
        private readonly ILogger<MutualFundsService> _logger;

        public MutualFundsService(ILogger<MutualFundsService> logger, IMutualFundsDataAccess iMutualFundsDataAccess)
        {
            _iMutualFundsDataAccess = iMutualFundsDataAccess;
            _logger = logger;
        }

        #region old implementation

        //public async Task<ServiceResponse<List<FundInvestment>>> GetFundTransactions(int portfolioID)
        //{
        //    try
        //    {
        //        var transactions = await _iMutualFundsDataAccess.GetFundTransactions(portfolioID);
        //        return MapTransactions(transactions);
        //    }
        //    catch(Exception ex)
        //    {
        //        _logger.LogError($"GetFundTransactions exception: {ex.Message}");
        //        return new ServiceResponse<List<FundInvestment>> (false, "Failed to Get Fund Transactions", null);
        //    }
        //}

        //private ServiceResponse<List<FundInvestment>> MapTransactions(ServiceResponse<DataTable> transactions)
        //{
        //    List<FundInvestment> response = null;

        //    if (transactions.Success)
        //    {
        //        response = (from t in transactions.ResponseObject.AsEnumerable()
        //                    where  t["TransactionType"].ToString() == "I" 
        //                    group t by new { SchemaCode = Convert.ToInt32(t["SchemaCode"].ToString()), SchemaName = t["SchemaName"].ToString() } into tg
        //                    select new FundInvestment()
        //                    {
        //                        SchemaCode = tg.Key.SchemaCode,
        //                        SchemaName = tg.Key.SchemaName,

        //                        Amount = tg.Sum(t => Convert.ToDecimal(t["Amount"])),
        //                        CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),

        //                        Units = tg.Sum(t => Convert.ToDecimal(t["Units"])),
        //                        NAV = tg.Average(t => Convert.ToDecimal(t["NAV"])),

        //                        InvestNAV = tg.Sum(t => Convert.ToDecimal(t["Amount"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
        //                        ActualNAV = tg.Sum(t => Convert.ToDecimal(t["Amount"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
        //                        DividendPerNAV = tg.Sum(t => Convert.ToDecimal(t["DividendPerNAV"])),
        //                        Dividend = tg.Sum(t => Convert.ToDecimal(t["Dividend"])),

        //                        //WithholdDays = GetWithHoldDays(tg),
        //                        XIRR = GetXIRR(tg)
        //                    }).ToList();

        //        //response.ForEach(t =>
        //        //{
        //        //    t.Profit = t.CurrentValue - t.Amount;
        //        //    t.ProfitPer = ((t.CurrentValue - t.Amount) / t.Amount) * 100;
        //        //});

        //        //fundTransactions.ForEach(t =>
        //        //{
        //        //    t.WithholdDays = CalculateWithholdDays(t.TransactionDate);
        //        //    t.XIRR = decimal.Round(Convert.ToDecimal(CalculateXirr(t.TransactionDate, double.Parse(t.Amount.ToString()), double.Parse(t.CurrentValue.ToString()))), 5);
        //        //    t.Profit = decimal.Round(t.CurrentValue - t.Amount, 5);
        //        //    t.ProfitPer = decimal.Round(((t.CurrentValue - t.Amount) * 100 / t.Amount), 5);
        //        //    t.FinancialYear = SetFinancialYear(t.TransactionDate);
        //        //});

        //        //response = MapFundTransactionsResponse(fundTransactions);

        //        //response.Consolidated = MapConsolidated(response);
        //    }
        //    return new ServiceResponse<List<FundInvestment>>(true, response == null ? "Error" : "Success", response);
        //}

        //private Consolidated MapConsolidated(Portfolio r)
        //{
        //    Consolidated result = new Consolidated()
        //    {
        //        NoOfFundHouses = (from t in r.Transactions
        //                          group t by new { t.FundHouse } into hg
        //                          select hg).Count(),
        //        NoOfFolios = (from t in r.Transactions
        //                      group t by new { t.FolioNumber } into fg
        //                      select fg).Count(),
        //        NoOfFunds = (from t in r.Transactions
        //                     group t by new { t.SchemaCode } into sg
        //                     select sg).Count(),
        //        Investment = r.Transactions.Sum(x => x.Amount),
        //        Profit = r.Transactions.Sum(x => x.Profit),
        //        Xirr = r.Transactions.Average(x => x.XIRR),
        //        WithHoldDays = Convert.ToInt32(r.Transactions.Average(x => x.WithholdDays)),
        //    };
        //    result.AbsoulteReturn = (result.Profit * 100) / result.Investment;
        //    return result;
        //}

        //private Portfolio MapFundTransactionsResponse(List<FundInvestment> fundTransactions)
        //{
        //    Portfolio result = null;

        //    result = (from t in fundTransactions
        //              group t by new { t.Portfolio, t.PortfolioID } into ts
        //              select new Portfolio()
        //              {
        //                  PortfolioID = ts.Key.PortfolioID,
        //                  PortfolioName = ts.Key.Portfolio,
        //                  Category = GetCategory(ts),
        //                  FinancialYear = GetFYTotals(ts),
        //                  Transactions = GetFundTransactions(ts),
        //                  // Consolidated = MapConsolidated(ts),
        //              }).FirstOrDefault();

        //    return result;
        //}

        //private List<FundInvestment> GetFundTransactions(IGrouping<object, FundInvestment> ts)
        //{
        //    return (from t in ts select t).ToList();
        //}

        //private List<Investment> GetFYTotals(IGrouping<object, FundInvestment> ts)
        //{
        //    List<Investment> result = null;

        //    result = (from t in ts
        //              group t by new { FinancialYear = t.FinancialYear } into fyGroup
        //              select new Investment()
        //              {
        //                  GroupKey = fyGroup.Key.FinancialYear.ToString(),
        //                  Amount = fyGroup.Sum(x => x.Amount),
        //                  Profit = decimal.Round(fyGroup.Sum(x => x.Profit), 5),
        //                  WithholdDays = Convert.ToInt32(fyGroup.Average(x => x.WithholdDays)),

        //              }).ToList();
        //    result.ForEach(r => r.AbsoulteReturn = decimal.Round((r.Profit * 100 / r.Amount), 5));
        //    return result;
        //}

        //private List<Investment> GetCategory(IGrouping<object, FundInvestment> ts)
        //{
        //    List<Investment> result = null;

        //    result = (from t in ts
        //              group t by new { category = t.Category } into categorygroup
        //              select new Investment()
        //              {
        //                 GroupKey = categorygroup.Key.category,
        //                 Amount = categorygroup.Sum(x => x.Amount),
        //                 Profit = decimal.Round(categorygroup.Sum(x => x.Profit), 5),
        //                 WithholdDays = Convert.ToInt32(categorygroup.Average(x => x.WithholdDays)),
        //              }).OrderByDescending(t => t.Amount).ToList();
        //    result.ForEach(r => r.AbsoulteReturn = decimal.Round((r.Profit * 100 / r.Amount), 5));
        //    return result;
        //}

        //private int SetFinancialYear(DateTime transactionDate)
        //{
        //    return transactionDate.Month < 4 ? transactionDate.Year - 1 : transactionDate.Year;
        //}

        //private int CalculateWithholdDays(DateTime investDate)
        //{
        //    return DateTime.Now.Date.Subtract(investDate.Date).Days;
        //}

        //private double CalculateXirr(DateTime investDate, double investment, double currentValue)
        //{
        //    // RETURN (power((@currentValue/@investValue), (365.00/DATEDIFF(day, @investDate,GETDATE())))-1)*100;
        //    int investedDays = DateTime.Now.Date.Subtract(investDate.Date).Days;
        //    return (Math.Pow((currentValue / investment), (365.0 / investedDays))-1)*100;
        //}

        //private List<FundTransaction> GetHistory(DataRow r)
        //{
        //    try
        //    {
        //        if (r != null && r["History"] != null)
        //            return JsonConvert.DeserializeObject<List<FundTransaction>>(r["History"].ToString());
        //    }
        //    catch(Exception ex)
        //    {
        //        _logger.LogError($"GetHistory exception: {ex.Message}");
        //    }
        //    return null;
        //}

        public async Task<ServiceResponse<List<CategoryValuation>>> GetCategoryValuation(int portfolioID)
        {
            try
            {
                var transactions = await _iMutualFundsDataAccess.GetFundTransactions(portfolioID);
                return MapCategoryValuation(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCategoryValuation exception: {ex.Message}");
                return new ServiceResponse<List<CategoryValuation>>(false, "Failed to Get Category Valuation", null);
            }
        }

        private ServiceResponse<List<CategoryValuation>> MapCategoryValuation(ServiceResponse<DataTable> transactions)
        {
            List<CategoryValuation> response = null;
            if (transactions.Success && transactions.ResponseObject.Rows.Count > 0) {
                response = (from t in transactions.ResponseObject.AsEnumerable()
                            group t by new { Category = t["Category"].ToString(), TransactionType = t["TransactionType"].ToString() } into tg
                            select new CategoryValuation()
                            {
                                Name = tg.Key.Category, // r["Category"].ToString(), // .Replace(" Sectoral/", "").Replace(" Cap", "").Trim()
                                TransactionType = tg.Key.TransactionType, // r["transactionType"].ToString()
                                Amount = tg.Sum(t => Convert.ToDecimal(t["Amount"])),
                                CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                                XIRR = GetXIRR(tg)
                            }).ToList().OrderByDescending(t => t.Amount).ToList();

                //response.ForEach(r => {
                //    r.Profit = r.CurrentValue - r.Amount;
                //    r.ProfitPer = ((r.CurrentValue - r.Amount) / r.Amount) * 100;
                //});
            }
            return new ServiceResponse<List<CategoryValuation>>(response == null ? false : true, response == null ? "Error" : "Success", response);
        }

        //public async Task<ServiceResponse<Consolidated>> GetConsolidatedValuation(int portfolioID)
        //{
        //    try
        //    {
        //        var transactions = await _iMutualFundsDataAccess.GetFundTransactions(portfolioID);
        //        var consolidated = MapConsolidated(portfolioID, transactions);
        //        await _iMutualFundsDataAccess.SaveFundsDailyTracker(consolidated.ResponseObject);
        //        return consolidated;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"GetConsolidatedValuation exception: {ex.Message}");
        //        return new ServiceResponse<Consolidated>(false, "Failed to Get Consolidated Result", null);
        //    }
        //}

        private ServiceResponse<Consolidated> MapConsolidated(int portfolioID, ServiceResponse<DataTable> transactions)
        {
            Consolidated response = null;
            if (transactions.Success)
            {
                var investTransacitons = transactions.ResponseObject.AsEnumerable().Where(t => t["TransactionType"].ToString() == "I");
                DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                response = new Consolidated()
                {
                    PortfolioID = portfolioID,
                    NoOfFundHouses = investTransacitons.Select(t => t["FundHouse"].ToString()).Distinct().Count(),
                    NoOfFolios = investTransacitons.Select(t => t["FolioNumber"].ToString()).Distinct().Count(),
                    NoOfFunds = investTransacitons.Select(t => t["SchemaCode"].ToString()).Distinct().Count(),

                    Investment = investTransacitons.Sum(t => Convert.ToDecimal(t["Amount"])),
                    CurrentValue = investTransacitons.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                    Xirr = GetXIRR(investTransacitons),
                    CurrentMonth = investTransacitons.Where(t => Convert.ToDateTime(t["TransactionDate"].ToString()) >= currentMonth).Sum(t => Convert.ToDecimal(t["Amount"])),

                    //WithHoldDays = Decimal.ToInt32(transactions.ResponseObject.AsEnumerable().Average(t => Convert.ToDecimal(t["WithHoldDays"]))),
                };
                //response.Profit = response.CurrentValue - response.Investment;
                //response.AbsoulteReturn = response.Investment > 0 ? (response.Profit * 100) / response.Investment : 0;

            }
            return new ServiceResponse<Consolidated>(response == null ? false : true, response == null ? "Error" : "Success", response);
        }

        private decimal GetXIRR(DataRow investTransaciton, string investProperty = "Amount", string transactDateProperty = "TransactionDate", string currentVallProperty = "CurrentValue")
        {
            var amount = Convert.ToDouble(investTransaciton[investProperty]) * -1;
            var transactionDate = Convert.ToDateTime(investTransaciton[transactDateProperty]);
            var currentValue = Convert.ToDouble(investTransaciton[currentVallProperty]);

            if (transactionDate.Date >= DateTime.Today.AddDays(-1)) return 0;

            List<double> valList = new List<double>() { amount }; ;
            List<DateTime> dtList = new List<DateTime>() { transactionDate };

            valList.Add(currentValue);
            dtList.Add(DateTime.Now.Date);

            return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
        }

        private decimal GetXIRR(EnumerableRowCollection<DataRow> investTransacitons)
        {
            var res = (from t in investTransacitons
                       select new
                       {
                           amount = Convert.ToDouble(t["Amount"]) * -1,
                           transactionDate = Convert.ToDateTime(t["TransactionDate"]),
                           CurrentValue = Convert.ToDouble(t["CurrentValue"])
                       }).Where(t => t.transactionDate.Date < DateTime.Now.Date).OrderBy(t => t.transactionDate);

            if (res == null || res.Count() < 1) return 0;

            List<double> valList = res.Select(t => t.amount).ToList();
            List<DateTime> dtList = res.Select(t => t.transactionDate).ToList();

            double currentValue = res.Sum(t => t.CurrentValue);

            valList.Add(currentValue);
            dtList.Add(DateTime.Now.Date);

            return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
        }

        //private decimal GetXIRR(DataTable transactions)
        //{

        //    var res = (from t in transactions.AsEnumerable()
        //               select new
        //               {
        //                   amount = Convert.ToDouble(t["Amount"]) * -1,
        //                   transactionDate = Convert.ToDateTime(t["TransactionDate"]),
        //                   CurrentValue = Convert.ToDouble(t["CurrentValue".ToString()])
        //               }).OrderBy(t => t.transactionDate);

        //    List<double> valList = res.Select(t => t.amount).ToList();
        //    List<DateTime> dtList = res.Select(t => t.transactionDate).ToList();

        //    double currentValue = res.Sum(t => t.CurrentValue);

        //    valList.Add(currentValue);
        //    dtList.Add(DateTime.Now.Date);

        //    return Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100;
        //}

        //private decimal GetXIRR(IGrouping<object, DataRow> tg)
        //{
        //    List<double> valList = (from t in tg select Convert.ToDouble(t["Amount"]) * -1).ToList();
        //    List<DateTime> dtList = (from t in tg select Convert.ToDateTime(t["TransactionDate"])).ToList();

        //    double currentValue = tg.Sum(t => Convert.ToDouble(t["CurrentValue"]));

        //    valList.Add(currentValue);
        //    dtList.Add(DateTime.Now.Date);

        //    return Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100;
        //}

        private decimal GetXIRR(IGrouping<object, DataRow> tg, string investProperty = "Amount", string transactDateProperty = "TransactionDate", string currentVallProperty = "CurrentValue")
        {
            try
            {
                List<double> valList = (from t in tg select Convert.ToDouble(t[investProperty]) * -1).ToList();
                List<DateTime> dtList = (from t in tg select Convert.ToDateTime(t[transactDateProperty])).ToList();

                double currentValue = tg.Sum(t => Convert.ToDouble(t[currentVallProperty]));

                valList.Add(currentValue);
                dtList.Add(DateTime.Now.Date);

                return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Calculate CAGR exception: {ex.Message}");
            }
            return 0;
        }

        private int GetWithHoldDays(IGrouping<object, DataRow> tg, string unitsProperty = "Units", string dateProperty = "TransactionDate")
        {
            try
            {
                var val = tg.Sum(t => Convert.ToDecimal(t[unitsProperty]) * Convert.ToDecimal(DateTime.Now.Date.Subtract(Convert.ToDateTime(t[dateProperty].ToString())).TotalDays));

                decimal totalUnits = tg.Sum(t => Convert.ToDecimal(t[unitsProperty]));
                return Convert.ToInt32(val / totalUnits);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Calculate With Holding Days exception: {ex.Message}");
            }
            return 0;
        }

        private int GetWithHoldDays(DataRow t, string DateProperty = "TransactionDate")
        {
            return Convert.ToInt32(DateTime.Now.Date.Subtract(Convert.ToDateTime(t[DateProperty].ToString())).TotalDays);
        }

        //private decimal CalculateXirr(DataTable transactions)
        //{
        //    // t.XIRR = decimal.Round(Convert.ToDecimal(CalculateXirr(t.TransactionDate, double.Parse(t.Amount.ToString()), double.Parse(t.CurrentValue.ToString()))), 5);

        //    //return Convert.ToDecimal((from t in transactions.AsEnumerable()
        //    //                        select CalculateXirr(
        //    //                                DateTime.Parse(t["TransactionDate"].ToString()),
        //    //                                Convert.ToDouble(t["Amount"].ToString()),
        //    //                                Convert.ToDouble(t["CurrentValue"].ToString())
        //    //                            ).Average()));

        //    return Convert.ToDecimal((from t in transactions.AsEnumerable()
        //                              select CalculateXirr(
        //                                          DateTime.Parse(t["TransactionDate"].ToString()),
        //                                          Convert.ToDouble(t["Amount"]),
        //                                          Convert.ToDouble(t["CurrentValue"])
        //                                      )
        //            ).Average());
        //}

        //private int CalculateAvgWithHoldDays(DataTable transactions)
        //{
        //    return Convert.ToInt32((from t in transactions.AsEnumerable()
        //               select CalculateWithholdDays(DateTime.Parse(t["TransactionDate"].ToString()))).Average());
        //}

        //private decimal GetTotalProfit(DataTable transactions)
        //{
        //    decimal CurrentValue = transactions.AsEnumerable().Sum(t => Convert.ToDecimal(t["CurrentValue"].ToString()));
        //    decimal Amount = transactions.AsEnumerable().Sum(t => Convert.ToDecimal(t["Amount"].ToString()));

        //    return decimal.Round(CurrentValue - Amount, 5);
        //}

        //private int GetDistinctFundsCount(DataTable transactions)
        //{
        //    return (from r in transactions.AsEnumerable()
        //            group r by new { SchemaCode = r["SchemaCode"].ToString() } into hg
        //            select hg).Count();
        //}

        //private int GetDistinctFoliosCount(DataTable transactions)
        //{
        //    return (from r in transactions.AsEnumerable()
        //            group r by new { FolioID = r["FolioNumber"].ToString() } into hg
        //            select hg).Count();
        //}

        //private int GetDistinctFundHouseCount(DataTable transactions)
        //{
        //    return (from r in transactions.AsEnumerable()
        //      group r by new { FundHouse = r["FundHouse"].ToString() } into hg
        //      select hg).Count();
        //}

        //public async Task<ServiceResponse<List<FinancialYearValuation>>> GetFinancialYearValuation(int portfolioID)
        //{
        //    try
        //    {
        //        var transactions = await _iMutualFundsDataAccess.GetFundTransactions(portfolioID);
        //        return MapFinancialYearValuation(transactions);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"GetFinancialYearValuation exception: {ex.Message}");
        //        return new ServiceResponse<List<FinancialYearValuation>>(false, "Failed to Get Financial Year Valuation", null);
        //    }
        //}

        private ServiceResponse<List<FinancialYearValuation>> MapFinancialYearValuation(ServiceResponse<DataTable> transactions)
        {
            List<FinancialYearValuation> response = null;
            if (transactions.Success && transactions.ResponseObject.Rows.Count > 0)
            {
                //response = (from r in transactions.ResponseObject.AsEnumerable()
                //            select new FinancialYearValuation()
                //            {
                //                FinancialYear = Convert.ToInt32(r["FinancialYear"].ToString()),
                //                Amount = decimal.Parse(r["Amount"].ToString()),
                //                CurrentValue = decimal.Parse(r["CurrentValue"].ToString()),
                //                TransactionType = r["transactionType"].ToString()
                //            }).ToList();

                //response.ForEach(r => r.Profit = r.CurrentValue - r.Amount);

                response = (from t in transactions.ResponseObject.AsEnumerable()
                            group t by new { FinancialYear = Convert.ToInt32(t["FinancialYear"].ToString()), TransactionType = t["TransactionType"].ToString() } into tg
                            select new FinancialYearValuation()
                            {
                                FinancialYear = tg.Key.FinancialYear,
                                TransactionType = tg.Key.TransactionType,
                                Amount = tg.Sum(t => Convert.ToDecimal(t["Amount"])),
                                CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                                XIRR = GetXIRR(tg)
                            }).OrderBy(t => t.FinancialYear).ToList();

                //response.ForEach(r => {
                //    r.Profit = r.CurrentValue - r.Amount;
                //    r.ProfitPer = ((r.CurrentValue - r.Amount) / r.Amount) * 100;
                //});
            }
            return new ServiceResponse<List<FinancialYearValuation>>(response == null ? false : true, response == null ? "Error" : "Success", response);
        }

        public async Task<ServiceResponse<List<FundSwitchInValuation>>> GetSwitcInValuation(int portfolioID)
        {
            try
            {
                var transactions = await _iMutualFundsDataAccess.GetSwitcInValuation(portfolioID);
                return MapSwitcInValuation(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetSwitcInValuation exception: {ex.Message}");
                return new ServiceResponse<List<FundSwitchInValuation>>(false, "Failed to Get Switch Transactions Valuation", null);
            }
        }

        private ServiceResponse<List<FundSwitchInValuation>> MapSwitcInValuation(ServiceResponse<DataTable> transactions)
        {
            List<FundSwitchInValuation> response = null;
            if (transactions.Success && transactions.ResponseObject.Rows.Count > 0)
            {
                //response = (from r in transactions.ResponseObject.AsEnumerable()
                //            select new FundSwitchIn()
                //            {
                //                SwitchInDate = DateTime.Parse(r["SwitchInDate"].ToString()),
                //                InSchemaCode = Convert.ToInt32(r["InSchemaCode"].ToString()),
                //                InUnits = decimal.Parse(r["InUnits"].ToString()),
                //                InNAV = decimal.Parse(r["InNAV"].ToString()),
                //                InCharges = decimal.Parse(r["InCharges"].ToString()),
                //                CurrentInNAV = decimal.Parse(r["CurrentInNAV"].ToString()),
                //                SchemaName = r["SchemaName"].ToString(),

                //                SwitchOutDate = DateTime.Parse(r["SwitchOutDate"].ToString()),
                //                OutSchemaCode = Convert.ToInt32(r["OutSchemaCode"].ToString()),
                //                OutUnits = decimal.Parse(r["OutUnits"].ToString()),
                //                OutNAV = decimal.Parse(r["OutNAV"].ToString()),
                //                OutCharges = decimal.Parse(r["OutCharges"].ToString()),
                //                CurrentOutNAV = decimal.Parse(r["CurrentOutNAV"].ToString()),
                //            }).ToList();



                //response = (from r in transactions.ResponseObject.AsEnumerable()
                //            select new FundSwitchInValuation()
                //            {
                //                SchemaName = r["SchemaName"].ToString(),
                //                CurrentInValue = Convert.ToDecimal(r["currentInValue"]),
                //                CurrentOutValue = Convert.ToDecimal(r["currentOutValue"]),
                //            }).ToList();

                //response.ForEach(
                //    r =>
                //    {
                //        r.Profit = r.CurrentInValue - r.CurrentOutValue;
                //        r.ProfitPer = ((r.CurrentInValue - r.CurrentOutValue) / r.CurrentOutValue) * 100;
                //    });

                response = (from t in transactions.ResponseObject.AsEnumerable()
                            group t by new { InSchemaCode = Convert.ToInt32(t["InSchemaCode"].ToString()), SchemaName = t["SchemaName"].ToString() } into tg
                            select new FundSwitchInValuation()
                            {
                                SchemaName = tg.Key.SchemaName,
                                CurrentInValue = tg.Sum(t => Convert.ToDecimal(t["currentInValue"])),
                                CurrentOutValue = tg.Sum(t => Convert.ToDecimal(t["currentOutValue"])),
                                XIRR = GetXIRR(tg, "currentOutValue", "SwitchInDate", "currentInValue"),
                                WithHoldDays = GetWithHoldDays(tg, "InUnits", "SwitchInDate")
                            }).ToList();

                //response.ForEach(r => {
                //    r.Profit = r.CurrentInValue - r.CurrentOutValue;
                //    r.ProfitPer = ((r.CurrentInValue - r.CurrentOutValue) / r.CurrentOutValue) * 100;
                //});
            }
            return new ServiceResponse<List<FundSwitchInValuation>>(response == null ? false : true, response == null ? "Error" : "Success", response);
        }

        public async Task<ServiceResponse<List<FundCagetoryPerformance>>> GetPerformance()
        {
            List<int> periods = new List<int>() { 1, 3, 6, 9, 12 };
            List<FundCagetoryPerformance> response = new List<FundCagetoryPerformance>();

            response = MapCurrentPerformance(await _iMutualFundsDataAccess.GetCurrentValues());

            if (response == null) return new ServiceResponse<List<FundCagetoryPerformance>>(false, "Failed to get Current Valuations", default);

            //var oneresult = _iMutualFundsDataAccess.GetPerformance(1);
            //var threeresult = _iMutualFundsDataAccess.GetPerformance(3);
            //var sixresult = _iMutualFundsDataAccess.GetPerformance(6);
            //var nineresult = _iMutualFundsDataAccess.GetPerformance(9);
            //var yrresult = _iMutualFundsDataAccess.GetPerformance(12);

            //await Task.WhenAll(oneresult, threeresult, sixresult, nineresult, yrresult);

            //response = MapPerformance(oneresult.Result, response, 1);
            //response = MapPerformance(oneresult.Result, response, 3);
            //response = MapPerformance(oneresult.Result, response, 6);
            //response = MapPerformance(oneresult.Result, response, 9);
            //response = MapPerformance(oneresult.Result, response, 12);

            foreach (var period in periods)
            {
                ServiceResponse<DataTable> result = await _iMutualFundsDataAccess.GetPerformance(period);
                response = MapPerformance(result, response, period);
            }
            //response.ForEach(r =>
            //{
            //    r.OneGrowth = r.One == 0 ? 0 : (r.Current - r.One) * 100 / r.One;
            //    r.ThreeGrowth = r.Three == 0 ? 0 : (r.Current - r.Three) * 100 / r.Three;
            //    r.SixGrowth = r.Six == 0 ? 0 : (r.Current - r.Six) * 100 / r.Six;
            //    r.NineGrowth = r.Nine == 0 ? 0 : (r.Current - r.Nine) * 100 / r.Nine;
            //    r.YearGrowth = r.Year == 0 ? 0 : (r.Current - r.Year) * 100 / r.Year;
            //});
            return new ServiceResponse<List<FundCagetoryPerformance>>(true, "Success", response);
        }

        private List<FundCagetoryPerformance> MapCurrentPerformance(ServiceResponse<DataTable> result)
        {
            if (result.Success && result.ResponseObject != null && result.ResponseObject.Rows.Count > 0) {
                return (from r in result.ResponseObject.AsEnumerable()
                        select new FundCagetoryPerformance()
                        {
                            Classification = r["Classification"].ToString(),
                            Category = r["Category"].ToString(),
                            Current = Convert.ToDecimal(r["NAV"]),
                            NoOfFunds = Convert.ToInt32(r["Count"].ToString())
                        }).ToList();
            }
            return null;
        }

        public async Task<ServiceResponse<List<CategoryFundsPerformance>>> GetCategoryFundsPerformance(string classification, string category)
        {
            List<int> periods = new List<int>() { 1, 3, 6, 9, 12 };
            List<CategoryFundsPerformance> response = new List<CategoryFundsPerformance>();

            response = MapCategoryFundsCurrentPerformance(await _iMutualFundsDataAccess.GetCurrentValues(classification, category));

            foreach (var period in periods)
            {
                ServiceResponse<DataTable> result = await _iMutualFundsDataAccess.GetCategoryPerformance(period, classification, category);
                MapCategoryFundsPerformance(result, period, response);
                //if (r1 != null && r1.Count() > 0)
                //    response.AddRange(r1);
            }

            return response.Count() > 0 ? new ServiceResponse<List<CategoryFundsPerformance>>(true, "Success", response) : new ServiceResponse<List<CategoryFundsPerformance>>(false, "Failed to get Category Funds Performance", null);
        }

        private List<CategoryFundsPerformance> MapCategoryFundsCurrentPerformance(ServiceResponse<DataTable> serviceResponse)
        {
            if(serviceResponse.Success && serviceResponse.ResponseObject != null && serviceResponse.ResponseObject.Rows.Count > 0)
            {
                return (from r in serviceResponse.ResponseObject.AsEnumerable()
                        select new CategoryFundsPerformance()
                        {
                            Current = Convert.ToDecimal(r["NAV"]),
                            SchemaCode = Conversions.ToInt(r["SchemaCode"], -1),
                            SchemaName = Conversions.ToString(r["SchemaName"], "SchemaName")
                        }).ToList();
            }
            return new List<CategoryFundsPerformance>();
        }

        private void MapCategoryFundsPerformance(ServiceResponse<DataTable> result, int period, List<CategoryFundsPerformance> response)
        {
            if (result.Success && result.ResponseObject != null && result.ResponseObject.Rows.Count > 0)
            {
                var r1 = (from r in result.ResponseObject.AsEnumerable()
                        select new CategoryFundsPerformance()
                        {
                            Current = Convert.ToDecimal(r["NAV"]),
                            SchemaCode = Conversions.ToInt(r["SchemaCode"], -1),
                            SchemaName = Conversions.ToString(r["SchemaName"], "SchemaName")
                        }).ToList();

                if (period == 1) response.ForEach(r => r.One = getValue(r.Classification, r.Category, r.SchemaCode, r1));
                else if (period == 3) response.ForEach(r => r.Three = getValue(r.Classification, r.Category, r.SchemaCode, r1));
                else if (period == 6) response.ForEach(r => r.Six = getValue(r.Classification, r.Category, r.SchemaCode, r1));
                else if (period == 9) response.ForEach(r => r.Nine = getValue(r.Classification, r.Category, r.SchemaCode, r1));
                else if (period == 12) response.ForEach(r => r.Year = getValue(r.Classification, r.Category, r.SchemaCode, r1));
            }
            //return null;
        }

        private List<FundCagetoryPerformance> MapPerformance(ServiceResponse<DataTable> result, List<FundCagetoryPerformance> response, int period)
        {
            if (result.Success && result.ResponseObject != null && result.ResponseObject.Rows.Count > 0)
            {
                var r1 = (from r in result.ResponseObject.AsEnumerable()
                          select new FundCagetoryPerformance()
                          {
                              Classification = r["Classification"].ToString(),
                              Category = r["Category"].ToString(),
                              Current = Convert.ToDecimal(r["NAV"])
                          }).ToList();

                if (period == 1) response.ForEach(r => r.One = getValue(r.Classification, r.Category, r1));
                else if (period == 3) response.ForEach(r => r.Three = getValue(r.Classification, r.Category, r1));
                else if (period == 6) response.ForEach(r => r.Six = getValue(r.Classification, r.Category, r1));
                else if (period == 9) response.ForEach(r => r.Nine = getValue(r.Classification, r.Category, r1));
                else if (period == 12) response.ForEach(r => r.Year = getValue(r.Classification, r.Category, r1));
            }
            return response;
        }

        private decimal getValue(string classification, string category, List<FundCagetoryPerformance> r1)
        {
            var filteredVal = r1.Where(r => r.Classification == classification && r.Category == category).FirstOrDefault();
            return filteredVal == null ? 0 : filteredVal.Current;
        }

        private decimal getValue(string classification, string category, int schemaCode, List<CategoryFundsPerformance> r1)
        {
            var filteredVal = r1.Where(r => r.SchemaCode == schemaCode).FirstOrDefault();
            return filteredVal == null ? 0 : filteredVal.Current;
        }


        public async Task<ServiceResponse<List<Portfolio>>> GetPortfolios()
        {
            return MapPortfolios(await _iMutualFundsDataAccess.GetPortfolios());
        }

        private ServiceResponse<List<Portfolio>> MapPortfolios(ServiceResponse<DataTable> portfolios)
        {
            if (portfolios.Success && portfolios.ResponseObject != null && portfolios.ResponseObject.Rows.Count > 0)
            {
                var result = (from r in portfolios.ResponseObject.AsEnumerable()
                              select new Portfolio()
                              {
                                  Description = r["Description"].ToString(),
                                  PortfolioName = r["Portfolio"].ToString(),
                                  IsActive = r["IsActive"].ToString() == "1" ? true : false,
                                  ID = Convert.ToInt32(r["ID"].ToString()),
                                  InvestorPAN = r["InvestorPAN"].ToString()
                              }).ToList();

                return new ServiceResponse<List<Portfolio>>(true, "Success", result);
            }
            return new ServiceResponse<List<Portfolio>>(false, "No Portfolios found", default);
        }

        public async Task<ServiceResponse<List<FundFolio>>> GetFundFolios()
        {
            return MapFundFolios(await _iMutualFundsDataAccess.GetFundFolios());
        }

        private ServiceResponse<List<FundFolio>> MapFundFolios(ServiceResponse<DataTable> portfolios)
        {
            if (portfolios.Success && portfolios.ResponseObject != null && portfolios.ResponseObject.Rows.Count > 0)
            {
                List<FundFolio> result = null;
                try
                {
                    result = (from r in portfolios.ResponseObject.AsEnumerable()
                                  group r by new {
                                      ID = Conversions.ToInt(r["ID"], -1),
                                      Portfolio = Conversions.ToString(r["Portfolio"], "portfolio"),
                                      
                                  } into rg
                                  select new FundFolio()
                                  {
                                      Portfolio = new Portfolio()
                                      {
                                          ID = rg.Key.ID,
                                          PortfolioName = rg.Key.Portfolio,
                                          InvestorPAN = rg.First()["InvestorPAN"].ToString()
                                      },
                                      Folios = (from f in rg.AsEnumerable()
                                                group f by new
                                                {
                                                    FolioNumber = Conversions.ToString(f["FolioNumber"], "FolioNumber"),
                                                    FolioID = Conversions.ToInt(f["FolioID"], -1)
                                                } into fg
                                                select new Entities.Models.DTO.India.Folio()
                                                {
                                                    //ID = Conversions.ConvertToInt(f["FolioID"], 0),
                                                    //FolioNumber = Conversions.ConvertToString(f["FolioNumber"], ""),
                                                    ID = fg.Key.FolioID,
                                                    FolioNumber = fg.Key.FolioNumber,
                                                    FundHouse = Conversions.ToString(fg.First()["FundHouse"], ""),
                                                    DateOpened = Conversions.ToDateTime(fg.First()["DateOpened"], DateTime.Now.AddYears(1)),
                                                    DefaultBank = Conversions.ToString(fg.First()["DefaultBank"], ""),
                                                    DefaultAccountNumber = Conversions.ToString(fg.First()["DefaultAccountNumber"], ""),
                                                    Email = Conversions.ToString(fg.First()["Email"], ""),
                                                    MobileNumber = Conversions.ToLong(fg.First()["MobileNumber"], 0),
                                                    IsActive = Conversions.ToBoolean(fg.First()["IsActive"], false),
                                                    Goal = String.Join(", ", fg.Select(g => Conversions.ToString(g["GoalName"], "goal")).Distinct()),
                                                }).ToList()
                                      
                                  }).ToList();
                }
                catch(Exception ex)
                {
                    _logger.LogError($"Failed to Map Fund Folios: {ex.Message}");
                    return new ServiceResponse<List<FundFolio>>(false, $"Exception: {ex.Message}", result);
                }
                return new ServiceResponse<List<FundFolio>>(true, "Success", result);
            }
            return new ServiceResponse<List<FundFolio>>(false, "No Folios found", default);
        }

        public async Task<ServiceResponse<FundNAV>> GetFundNAV(string fundHouse, int schemaCode, DateTime date)
        {
            return MapFundNAV(await _iMutualFundsDataAccess.GetFundNAV(fundHouse, schemaCode, date));
        }

        private ServiceResponse<FundNAV> MapFundNAV(ServiceResponse<DataTable> portfolios)
        {
            if (portfolios.Success && portfolios.ResponseObject != null && portfolios.ResponseObject.Rows.Count > 0)
            {
                var result = (from r in portfolios.ResponseObject.AsEnumerable()
                              select new FundNAV()
                              {
                                  SchemaCode = Convert.ToInt32(r["SchemaCode"].ToString()),
                                  date = Convert.ToDateTime(r["NAVDate"].ToString()),
                                  Name = r["SchemaName"].ToString(),
                                  NAV = Convert.ToDecimal(r["NAV"]),
                                  FundHouse = r["FundHouse"].ToString(),
                                  FundOption = r["FundOption"].ToString(),
                                  Category = r["Category"].ToString()
                              }).FirstOrDefault();

                return new ServiceResponse<FundNAV>(true, "Success", result);
            }
            return new ServiceResponse<FundNAV>(false, "Fund NAV not found", default);
        }

        public async Task<ServiceResponse<List<FundInvestment>>> GetFundTransactions(int portfolioID, int schemaCode)
        {
            return MapFundTransactions(await _iMutualFundsDataAccess.GetFundTransactions(portfolioID, schemaCode));
        }

        private ServiceResponse<List<FundInvestment>> MapFundTransactions(ServiceResponse<DataTable> transactions)
        {
            if (transactions == null || transactions.ResponseObject == null || !transactions.Success)
                return new ServiceResponse<List<FundInvestment>>(false, "Failed to Get Transactions", null);
            try
            {
                var transactionsLst = (from t in transactions.ResponseObject.AsEnumerable()
                                       select new FundInvestment()
                                       {
                                           NAV = Convert.ToDecimal(t["CurrentNAV"]),
                                           CurrentValue = Convert.ToDecimal(t["CurrentValue"]),
                                           FinancialYear = Convert.ToInt32(t["FinancialYear"]),
                                           TransactionDate = Convert.ToDateTime(t["TransactionDate"]),
                                           FolioNumber = t["FolioNumber"].ToString(),
                                           Amount = Convert.ToDecimal(t["Investment"]),
                                           Units = Convert.ToDecimal(t["Units"]),
                                           TransactionType = t["TransactionType"].ToString(),
                                           XIRR = GetXIRR(t, "Investment"),
                                           WithholdDays = GetWithHoldDays(t)
                                       }).ToList();

                return transactionsLst != null && transactionsLst.Count > 0 ? new ServiceResponse<List<FundInvestment>>(true, "Success", transactionsLst) : new ServiceResponse<List<FundInvestment>>(false, "Failed to map Transactions", null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Map Fund Transactions: {ex.Message}");
            }
            return new ServiceResponse<List<FundInvestment>>(false, "Failed to map Transactions", null);
        }

        public async Task<ServiceResponse<List<GoalReview>>> GoalsReview(int portfolioID)
        {
            try
            {
                List<GoalReview> result = MapGoalsReview(await _iMutualFundsDataAccess.GoalsReview(portfolioID));

                //List<DateTime> quarterDates = new List<DateTime>();
                //DateTime startDate = new DateTime(2022, 1, 1);
                ////DateTime today = DateTime.Now.Date;
                //DateTime dt = startDate.AddMonths(3).AddDays(-1);

                //DateTime datetime = DateTime.Now;
                //int currQuarter = (datetime.Month - 1) / 3 + 1;
                //DateTime dtLastDay = new DateTime(datetime.Year, 3 * currQuarter, 1).AddMonths(1).AddDays(-1);
                //while (dt <= dtLastDay)
                //{
                //    quarterDates.Add(dt > DateTime.Now.Date ? DateTime.Now.AddDays(-1).Date : dt);

                //    var date = GetDate(await _iMutualFundsDataAccess.GetLatestNAVDate(dt), dt);
                //    var res = MapGoalsReview(await _iMutualFundsDataAccess.GoalsReview(portfolioID, date, dt >= DateTime.Now.Date ? true : false), dtLastDay, dt >= DateTime.Now.Date ? true : false);
                //    if (res != null && res.Count() > 0) result.AddRange(res);

                //    dt = dt.AddMonths(3);
                //}

                //await _iMutualFundsDataAccess.SaveQuarterlyGoalReview(result);

                //var batchSize = 4;
                //int numberOfBatches = (int)Math.Ceiling((double)quarterDates.Count() / batchSize);

                //List<DateTime> navDates = new List<DateTime>();

                ////for (int i = 0; i < numberOfBatches; i++)
                ////{
                ////    quarterDates.Skip(i * batchSize).Take(batchSize).Select(async d =>
                ////    {
                ////        var latestDateTasks = await GetDate(_iMutualFundsDataAccess.GetLatestNAVDate(d), d);
                ////        var goalReviewTasks = MapGoalsReview(_iMutualFundsDataAccess.GoalsReview(d));

                ////        var r = await Task.WhenAll(goalReviewTasks);

                ////        result.AddRange(r.SelectMany(x => x).ToList());
                ////    });
                ////}

                //for (int i = 0; i < numberOfBatches; i++)
                //{
                //    var tasks = quarterDates.Skip(i * batchSize).Take(batchSize).Select(d => GetDate(_iMutualFundsDataAccess.GetLatestNAVDate(d), d));

                //    //result.AddRange(await Task.WhenAll(tasks));

                //    //var tasks = currentDates.Select(d => GoalsReview(d, result));
                //    //users.AddRange(await Task.WhenAll(tasks));

                //    //result.AddRange(await Task.WhenAll(tasks));

                //    navDates.AddRange(await Task.WhenAll(tasks));
                //}

                //for (int i = 0; i < numberOfBatches; i++)
                //{
                //    var gReviewTasks = navDates.Skip(i * batchSize).Take(batchSize).Select(d => MapGoalsReview(_iMutualFundsDataAccess.GoalsReview(d)));

                //    var r = await Task.WhenAll(gReviewTasks);

                //    result.AddRange(r.SelectMany(x => x).ToList());
                //}

                ////await Task.WhenAll(quarterDates.Select(d => Task.Run(() => GoalsReview(d, result))));

                return new ServiceResponse<List<GoalReview>>(result != null ? true : false, result != null ? "Success": "Failed to get Goals Review", result); // MapGoalsReview(await _iMutualFundsDataAccess.GoalsReview(new DateTime(2022, 9, 30)));
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to Get Fund GoalsReview: {ex.Message}");
            }
            return new ServiceResponse<List<GoalReview>>(false, "Failed to get Goals Review", null);
        }

        //private List<GoalReview> MapGoalsReview(ServiceResponse<DataTable> data)
        //{
        //    return MapGoalsReview(data);
        //}

        private async Task<DateTime> GetDate(Task<ServiceResponse<DataTable>> task, DateTime dt)
        {
            //throw new NotImplementedException();
            var serviceResponse = task.Result;
            if (serviceResponse == null || !serviceResponse.Success || serviceResponse.ResponseObject == null || serviceResponse.ResponseObject.Rows.Count == 0) return dt;
            else
            {
                return (from d in serviceResponse.ResponseObject.AsEnumerable()
                        select Conversions.ToDateTime(d["NAVDate"], dt)
                       ).FirstOrDefault();
            }
        }

        private async void GoalsReview(DateTime d, List<GoalReview> result)
        {
            //var date = GetDate(await _iMutualFundsDataAccess.GetLatestNAVDate(d), d);
            //var res = MapGoalsReview(await _iMutualFundsDataAccess.GoalsReview(date));
            //if (res != null && res.Count() > 0) result.AddRange(res);
        }

        private DateTime GetDate(ServiceResponse<DataTable> serviceResponse, DateTime dt)
        {
            if (serviceResponse == null || !serviceResponse.Success || serviceResponse.ResponseObject == null || serviceResponse.ResponseObject.Rows.Count == 0) return dt;
            else
            {
                return (from d in serviceResponse.ResponseObject.AsEnumerable()
                        select Conversions.ToDateTime(d["NAVDate"], dt)
                       ).FirstOrDefault();
            }
        }

        private List<GoalReview> MapGoalsReview(ServiceResponse<DataTable> transactions)
        {
            if (transactions == null || transactions.ResponseObject == null || !transactions.Success)
                return null;
            try
            {
                List<GoalReview> result = (from g in transactions.ResponseObject.AsEnumerable()
                                           //group t by new
                                           //{
                                           //    GoalName = Conversions.ToString(t["GoalName"], "GoalName"),
                                           //    GoalID = Conversions.ToInt(t["GoalID"], -1)
                                           //} into g
                                           select new GoalReview()
                                           {
                                               GoalID = Conversions.ToInt(g["GoalID"], -1),
                                               Goal = Conversions.ToString(g["GoalName"], "GoalName"),
                                               PortfolioID = Conversions.ToInt(g["PortfolioID"], -1),
                                               Portfolio = Conversions.ToString(g["Portfolio"], "Portfolio"),
                                               CurrentExpense = Conversions.ToDecimal(g["goalAmount"], -1),
                                               TargetExpense = Conversions.ToDecimal(g["TargetAmount"], -1),
                                               StartDate = Conversions.ToDateTime(g["StartDate"], DateTime.Now.AddYears(1)),
                                               EndDate = Conversions.ToDateTime(g["EndDate"], DateTime.Now.AddYears(1)),
                                               Date = Conversions.ToDateTime(g["ReviewDate"], DateTime.Now.AddYears(1)),
                                               Investment = Conversions.ToDecimal(g["Investment"], -1),
                                               CurrentValue = Conversions.ToDecimal(g["CurrentValue"], -1),

                                               SIP = Conversions.ToDecimal(g["MonthlyInvestment"], -1),
                                               //CompletedMonths = 3,

                                           }).OrderBy(g => g.PortfolioID).ThenBy(g => g.EndDate).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Map Fund GoalsReview: {ex.Message}");
            }
            return null;
        }


        //public async Task<ServiceResponse<PortfolioData>> PortfolioData(int portfolioID)
        //{
        //    _logger.LogInformation("PortfolioData");
        //    PortfolioData portfolio = new PortfolioData();
        //    try
        //    {

        //        var transactions = await _iMutualFundsDataAccess.GoalsInfo(portfolioID);
        //        var currentMonthTransactions = await _iMutualFundsDataAccess.CurrentMonthGoals(portfolioID);

        //        var goalsInfo = MapGoalsInfo(transactions);
        //        portfolio.Goals = goalsInfo == null ? null : goalsInfo.Goals;
        //        portfolio.ConsolidatedInfo = goalsInfo == null ? null : goalsInfo.ConsolidatedInfo;

        //        var currentMonth = MapGoalsInfo(currentMonthTransactions);
        //        portfolio.CurrentMonth = currentMonth == null || currentMonth.Goals == null ? null : currentMonth.Goals.OrderByDescending(g => g.Investment).ToList();

        //        var categoryValudation = MapPortfolioCategoryValuation(transactions);
        //        portfolio.CategoryValuation = categoryValudation == null ? null : categoryValudation;

        //        return goalsInfo == null ? new ServiceResponse<PortfolioData>(false, "No Goals Found", null) : new ServiceResponse<PortfolioData>(true, "Success", portfolio);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"PortfolioData exception: {ex.Message}");
        //    }
        //    return new ServiceResponse<PortfolioData>(false, "Failed to get Portfolio Data", portfolio);
        //}

        //private GoalsInfo MapGoalsInfo(ServiceResponse<DataTable> transactions)
        //{
        //    if (transactions.Success && transactions.ResponseObject != null && transactions.ResponseObject.Rows.Count > 0)
        //    {
        //        GoalsInfo goalsInfo = new GoalsInfo()
        //        {
        //            ConsolidatedInfo = new ConsolidatedGoal(),
        //            Goals = new List<Goal>()
        //        };
        //        var result = (from t in transactions.ResponseObject.AsEnumerable()
        //                      group t by new
        //                      {
        //                          PortfolioID = Convert.ToInt32(t["PortfolioID"]),
        //                          Portfolio = t["Portfolio"].ToString(),
        //                          GoalID = Convert.ToInt32(t["GoalID"]),
        //                          GoalName = t["GoalName"].ToString(),
        //                          StartDate = Convert.ToDateTime(t["StartDate"].ToString()),
        //                      } into tg
        //                      select new Goal()
        //                      {
        //                          PortfolioID = tg.Key.PortfolioID,
        //                          Portfolio = tg.Key.Portfolio,

        //                          GoalID = tg.Key.GoalID,
        //                          GoalName = tg.Key.GoalName,
        //                          StartDate = tg.Key.StartDate,

        //                          SIPDuration = tg.First()["SIPDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["SIPDuration"]),
        //                          TargetDuration = tg.First()["TargetDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["TargetDuration"]),

        //                          SIPAmount = tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["MonthlyInvestment"]),
        //                          PresentValue = tg.First()["Amount"] == DBNull.Value ? 0 : Convert.ToDouble(tg.First()["Amount"]),

        //                          Inflation = tg.First()["ExpectedInflation"] == DBNull.Value ? 0 : Convert.ToDouble(tg.First()["ExpectedInflation"]),
        //                          Investment = tg.Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),

        //                          CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentGoalValue"] == DBNull.Value ? 0 : t["CurrentGoalValue"])),

        //                      }).OrderBy(g => g.PortfolioID).ThenBy(g => g.EndDate).ToList();
        //        goalsInfo.ConsolidatedInfo = MapGoalsConsolidated(transactions);
        //        goalsInfo.Goals = result;

        //        return goalsInfo;
        //    }
        //    return null;
        //}

        //private ConsolidatedGoal MapGoalsConsolidated(ServiceResponse<DataTable> transactions)
        //{
        //    DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    var res = (from t in transactions.ResponseObject.AsEnumerable()
        //               group t by new
        //               {
        //                   PortfolioID = Convert.ToInt32(t["PortfolioID"]),
        //                   GoalName = t["GoalName"].ToString(),
        //                   GoalID = Convert.ToInt32(t["GoalID"]),
        //               } into tg
        //               select new ConsolidatedGoal()
        //               {
        //                   SIPAmount = tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["MonthlyInvestment"]),
        //                   PresentValue = tg.First()["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["Amount"]),
        //                   FutureValue = tg.First()["TargetAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["TargetAmount"]),
        //                   Target = tg.First()["TargetAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["TargetAmount"]),

        //                   CurrentInvestment = tg.Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),
        //                   CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"] == DBNull.Value ? 0 : t["CurrentValue"])),

        //                   StartDate = tg.First()["StartDate"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(tg.First()["StartDate"]),

        //                   EndDate = Convert.ToDateTime(tg.First()["StartDate"] == DBNull.Value ? DateTime.Now.Date : tg.First()["StartDate"]).AddYears(Convert.ToInt32(tg.First()["TargetDuration"] == DBNull.Value ? 0 : tg.First()["TargetDuration"])).AddDays(-1),

        //                   SIPDuration = tg.First()["SIPDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["SIPDuration"]),
        //                   TargetDuration = tg.First()["TargetDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["TargetDuration"]),

        //                   CurrentMonthInvestment = tg.Where(t => Convert.ToDateTime(t["TransactionDate"] == DBNull.Value ? DateTime.Now.Date : t["TransactionDate"]) >= currentMonth)
        //                   .Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),

        //                   CurrentMonthValue = tg.Where(t => Convert.ToDateTime(t["TransactionDate"] == DBNull.Value ? DateTime.Now.Date : t["TransactionDate"]) >= currentMonth)
        //                   .Sum(t => Convert.ToDecimal(t["CurrentValue"] == DBNull.Value ? 0 : t["CurrentValue"])),

        //                   TargetInvestment = Convert.ToDecimal(tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : tg.First()["MonthlyInvestment"]) * Convert.ToInt32(tg.First()["SIPDuration"] == DBNull.Value ? 0 : tg.First()["SIPDuration"]) * 12,

        //               });

        //    ConsolidatedGoal result = new ConsolidatedGoal()
        //    {
        //        SIPAmount = res.Sum(r => r.SIPAmount),
        //        PresentValue = res.Sum(r => r.PresentValue),
        //        FutureValue = res.Sum(r => r.FutureValue),
        //        Target = res.Sum(r => r.Target),
        //        CurrentInvestment = res.Sum(r => r.CurrentInvestment),
        //        CurrentValue = res.Sum(r => r.CurrentValue),
        //        CurrentMonthInvestment = res.Sum(r => r.CurrentMonthInvestment),
        //        CurrentMonthValue = res.Sum(r => r.CurrentMonthValue),

        //        StartDate = res.Min(r => r.StartDate),
        //        EndDate = res.Max(r => r.EndDate),

        //        TargetInvestment = res.Sum(r => r.TargetInvestment)
        //    };

        //    return result;
        //}

        //private List<CategoryValuation> MapPortfolioCategoryValuation(ServiceResponse<DataTable> transactions)
        //{
        //    if (transactions.Success && transactions.ResponseObject.Rows.Count > 0)
        //    {
        //        return (from t in transactions.ResponseObject.AsEnumerable()
        //                    where Convert.ToDecimal(t["Units"]) > 0
        //                    group t by new { Category = t["Category"].ToString(), TransactionType = t["TransactionType"].ToString() } into tg
        //                    select new CategoryValuation()
        //                    {
        //                        Name = tg.Key.Category,
        //                        TransactionType = tg.Key.TransactionType,
        //                        Amount = tg.Sum(t => Convert.ToDecimal(t["Investment"])),
        //                        CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
        //                        XIRR = GetXIRR(tg)
        //                    }).OrderByDescending(t => t.Amount).ToList();
        //    }
        //    return null;
        //}

        #endregion

        #region new implementation

        //public async Task<ServiceResponse<FundInvestments>> GetFundRedeems(int portfolioID)
        //{
        //    var redeemsData = await _iMutualFundsDataAccess.GetFundRedeems(portfolioID);

        //    List<Transaction> redeemInvests = MapRedeemInvests(redeemsData);
        //    List<Transaction> redeems = MapRedeems(redeemsData);

        //    var dividendData = await _iMutualFundsDataAccess.GetFundDividends(portfolioID);
        //    List<Transaction> dividends = MapDividends(dividendData);

        //    dividends = getDividends(redeems, dividends, FundTransactionTypes.Redeem);

        //    redeems.AddRange(redeemInvests);
        //    redeems.AddRange(dividends);

        //    redeems.ForEach(r => {
        //        r.Amount = getAmount(r);
        //    });

        //    FundInvestments response = new FundInvestments()
        //    {
        //        RedeemTransactions = redeems.OrderBy(r => r.TransactionDate).ToList()
        //    };

        //    var json = JsonConvert.SerializeObject(response);

        //    response.Redeems = GetOverallRedeemValuation(response.RedeemTransactions);

        //    return new ServiceResponse<FundInvestments>(true, "Success", response);
        //}

        //private List<Transaction> getDividends(List<Transaction> transactions, List<Transaction> dividends, FundTransactionTypes type)
        //{
        //    var r = transactions.Where(r => r.TransactionType == type.ToString()).Select(r => r.FundDetails.SchemaCode).Distinct().ToList();

        //    return dividends.Where(d => r.Contains(d.FundDetails.SchemaCode)).ToList();
        //}


        //private decimal getAmount(Transaction r)
        //{
        //    decimal returnValue = 0;
        //    if (r.TransactionType == FundTransactionTypes.Dividend.ToString())
        //        returnValue =(r.Dividend > 0 ? - 1 * r.Dividend : r.Dividend) - r.STT - r.StampDuty - r.TDS;
        //    else if (r.TransactionType == FundTransactionTypes.Investment.ToString())
        //        returnValue = r.PurchaseNAV * r.Units + (r.STT + r.StampDuty + r.TDS);
        //    else
        //        returnValue = r.PurchaseNAV * r.Units - (r.STT + r.StampDuty + r.TDS);

        //    return decimal.Round(returnValue, 4);
        //}

        //private FundStats GetOverallRedeemValuation(List<Transaction> redeemTransactions)
        //{
        //    FundStats response = new FundStats()
        //    {
        //        Investment = redeemTransactions.Where(r => r.TransactionType == FundTransactionTypes.RedeemInvest.ToString()).Sum(r => r.Amount),
        //        CurrentValue = redeemTransactions.Where(r => r.TransactionType == FundTransactionTypes.Redeem.ToString()).Sum(r => r.Amount),
        //        XIRR = GetXIRR(redeemTransactions, FundTransactionTypes.Redeem)
        //    };

        //    return response;
        //}

        //private FundStats GetOverallCurrentValuation(List<Transaction> curreentTransactions)
        //{
        //    FundStats response = new FundStats()
        //    {
        //        Investment = curreentTransactions.Sum(r => (r.PurchaseNAV * r.Units)),
        //        CurrentValue = curreentTransactions.Sum(r => r.CurrentNAV * r.Units),
        //        XIRR = GetXIRR(curreentTransactions, FundTransactionTypes.Investment)
        //    };

        //    return response;
        //}

        //private decimal GetXIRR(List<Transaction> transactions, FundTransactionTypes type)
        //{
        //    if (transactions == null || transactions.Count < 1) return 0;

        //    List<double> valList = null;
        //    List<DateTime> dtList = null;
        //    if (type == FundTransactionTypes.Redeem)
        //    {
        //        valList = transactions.Select(t => t.TransactionType == FundTransactionTypes.Redeem.ToString() ? -1 * Convert.ToDouble(t.Amount) : Convert.ToDouble(t.Amount)).ToList();
        //        dtList = transactions.Select(t => t.TransactionDate).ToList();
        //    }
        //    else if (type == FundTransactionTypes.Investment)
        //    {
        //        valList = transactions.Select(t => Convert.ToDouble(t.Amount)).ToList();
        //        dtList = transactions.Select(t => t.TransactionDate).ToList();


        //        double currentValue = Convert.ToDouble(transactions.Sum(t => t.CurrentNAV * t.Units));

        //        valList.Add(-currentValue);
        //        dtList.Add(DateTime.Now.Date);
        //    }

        //    return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
        //}

        //private decimal CalculateRedeemXIRR(DataRow t)
        //{
        //    DateTime investDate = Conversions.ConvertToDateTime(t["InvestDate"], DateTime.Now);
        //    double investValue = Conversions.ConvertToDouble(t["InvestNAV"], 0) * Conversions.ConvertToDouble(t["Units"], 0) - Conversions.ConvertToDouble(t["Charges"], 0);

        //    DateTime redeemDate = Conversions.ConvertToDateTime(t["RedeemDate"], DateTime.Now);
        //    double currentValue = Conversions.ConvertToDouble(t["RedeemNAV"], 0) * Conversions.ConvertToDouble(t["Units"], 0);

        //    List<double> valList = new List<double>() { investValue, (currentValue < 0 ? currentValue : -1 * currentValue) };
        //    List<DateTime> dtList = new List<DateTime>() { investDate, redeemDate };

        //    return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
        //}

        //private decimal CalculateInvestmenttXIRR(DataRow t)
        //{
        //    DateTime investDate = Conversions.ConvertToDateTime(t["TransactionDate"], DateTime.Now);
        //    double investValue = Conversions.ConvertToDouble(t["InvestNAV"], 0) * Conversions.ConvertToDouble(t["Units"], 0) - Conversions.ConvertToDouble(t["StampDuty"], 0) - Conversions.ConvertToDouble(t["STT"], 0);

        //    DateTime currentDate = DateTime.Now;
        //    double currentValue = Conversions.ConvertToDouble(t["NAV"], 0) * Conversions.ConvertToDouble(t["Units"], 0);

        //    List<double> valList = new List<double>() { investValue, (currentValue < 0 ? currentValue : -1 * currentValue) };
        //    List<DateTime> dtList = new List<DateTime>() { investDate, currentDate };

        //    return decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2);
        //}

        //private int getInvestmentWithHoldDays(DataRow t)
        //{
        //    return Conversions.ConvertToInt(DateTime.Now.Date.Subtract(Conversions.ConvertToDateTime(t["TransactionDate"], DateTime.Now)).TotalDays);
        //}

        //private int getRedeemWithHoldDays(DataRow t)
        //{
        //    return Conversions.ConvertToInt(Conversions.ConvertToDateTime(t["RedeemDate"], DateTime.Now).Date.Subtract(Conversions.ConvertToDateTime(t["InvestDate"], DateTime.Now)).TotalDays);
        //}

        //private FundDetails MapFundDetails(DataRow t)
        //{
        //    return new FundDetails()
        //    {
        //        House = Conversions.ConvertToString(t["FundHouse"], ""),
        //        FundType = Conversions.ConvertToString(t["FundType"], ""),
        //        Classification = Conversions.ConvertToString(t["Classification"], ""),
        //        Category = Conversions.ConvertToString(t["Category"], ""),
        //        SchemaCode = Conversions.ConvertToInt(t["SchemaCode"], 0),
        //        SchemaName = Conversions.ConvertToString(t["SchemaName"], ""),
        //    };
        //}

        //private List<Transaction> MapRedeemInvests(ServiceResponse<DataTable> redeemsData)
        //{
        //    try
        //    {
        //        return (from t in redeemsData.ResponseObject.AsEnumerable()
        //                select new Transaction()
        //                {
        //                    TransactionID = Conversions.ConvertToInt(t["TransactionID"], 0),
        //                    PortfolioID = Conversions.ConvertToInt(t["PortfolioID"], 0),

        //                    FolioNumber = Conversions.ConvertToString(t["FolioNumber"], ""),
        //                    //FinancialYear = Conversions.GetFinancialYear(Conversions.ConvertToDateTime(t["InvestDate"], DateTime.Now)),

        //                    //SchemaCode = Conversions.ConvertToInt(t["SchemaCode"], 0),
        //                    //SchemaName = Conversions.ConvertToString(t["SchemaName"], ""),

        //                    TransactionType = FundTransactionTypes.RedeemInvest.ToString(), // "RI",

        //                    TransactionDate = Conversions.ConvertToDateTime(t["InvestDate"], DateTime.Now),

        //                    Dividend = Conversions.ConvertToDecimal(t["Dividend"], 0),

        //                    PurchaseNAV = Conversions.ConvertToDecimal(t["InvestNAV"], 0),
        //                    DividendPerNAV = Conversions.ConvertToDecimal(t["DividendPerNAV"], 0),
        //                    Units = Conversions.ConvertToDecimal(t["Units"], 0),

        //                    WithHoldDays = getRedeemWithHoldDays(t),

        //                    FundDetails = MapFundDetails(t)

        //                }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Map Redeem Invests exception: {ex.Message}");
        //    }
        //    return null;
        //}

        //private List<Transaction> MapRedeems(ServiceResponse<DataTable> redeemsData)
        //{
        //    try
        //    {
        //        return(from t in redeemsData.ResponseObject.AsEnumerable()
        //                select new Transaction()
        //                {
        //                    TransactionID = Conversions.ConvertToInt(t["TransactionID"], 0),
        //                    PortfolioID = Conversions.ConvertToInt(t["PortfolioID"], 0),

        //                    FolioNumber = Conversions.ConvertToString(t["FolioNumber"], ""),
        //                    //FinancialYear = Conversions.GetFinancialYear(Conversions.ConvertToDateTime(t["RedeemDate"], DateTime.Now)),

        //                    TransactionType = FundTransactionTypes.Redeem.ToString(), // "R",

        //                    TransactionDate = Conversions.ConvertToDateTime(t["RedeemDate"], DateTime.Now),

        //                    Dividend = Conversions.ConvertToDecimal(t["Dividend"], 0),

        //                    PurchaseNAV = Conversions.ConvertToDecimal(t["RedeemNAV"], 0),
        //                    DividendPerNAV = Conversions.ConvertToDecimal(t["DividendPerNAV"], 0),
        //                    Units = Conversions.ConvertToDecimal(t["Units"], 0),

        //                    WithHoldDays = getRedeemWithHoldDays(t),

        //                    STT = Conversions.ConvertToDecimal(t["Charges"], 0),
        //                    TDS = 0,

        //                    FundDetails = MapFundDetails(t),
        //                    XIRR = CalculateRedeemXIRR(t),

        //                }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Map Redeems exception: {ex.Message}");
        //    }
        //    return null;
        //}

        //private List<Transaction> MapDividends(ServiceResponse<DataTable> dividendData)
        //{
        //    try
        //    {
        //        return (from t in dividendData.ResponseObject.AsEnumerable()
        //                select new Transaction()
        //                {
        //                    TransactionID = Conversions.ConvertToInt(t["ID"], 0),
        //                    PortfolioID = Conversions.ConvertToInt(t["PortfolioID"], 0),

        //                    FolioNumber = Conversions.ConvertToString(t["FolioNumber"], ""),
        //                    //FinancialYear = Conversions.GetFinancialYear(Conversions.ConvertToDateTime(t["DividendDate"], DateTime.Now)),

        //                    //SchemaCode = Conversions.ConvertToInt(t["SchemaCode"], 0),

        //                    TransactionType = FundTransactionTypes.Dividend.ToString(), // "D",

        //                    TransactionDate = Conversions.ConvertToDateTime(t["DividendDate"], DateTime.Now),

        //                    Dividend = Conversions.ConvertToDecimal(t["Amount"], 0),

        //                    PurchaseNAV = Conversions.ConvertToDecimal(t["NAV"], 0),
        //                    Units = Conversions.ConvertToDecimal(t["Units"], 0),

        //                    STT = Conversions.ConvertToDecimal(t["Charges"], 0),

        //                    TDS = 0

        //                }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Map Redeems exception: {ex.Message}");
        //    }
        //    return null;
        //}


        //public async Task<ServiceResponse<FundInvestments>> GetTransactions(int portfolioID)
        //{
        //    var investData = await _iMutualFundsDataAccess.GetFundinvestments(portfolioID);
        //    //var redeemsData = await _iMutualFundsDataAccess.GetFundRedeems(portfolioID);
        //    //var dividendData = await _iMutualFundsDataAccess.GetFundDividends(portfolioID);

        //    //List<Transaction> redeemInvests = MapRedeemInvests(redeemsData);
        //    //List<Transaction> redeems = MapRedeems(redeemsData);
        //    List<Transaction> investments = MapInvestments(investData);
        //    //List<Transaction> dividends = MapDividends(dividendData);

        //    //var Redeemdividends = getDividends(redeems, dividends, FundTransactionTypes.Redeem);

        //    //redeems.AddRange(redeemInvests);
        //    //redeems.AddRange(dividends);

        //    //redeems.ForEach(r => {
        //    //    r.Amount = getAmount(r);
        //    //});

        //    investments.ForEach(r => {
        //        r.Amount = getAmount(r);
        //    });

        //    FundInvestments response = new FundInvestments()
        //    {
        //        //RedeemTransactions = redeems.OrderBy(r => r.TransactionDate).ToList(),
        //        CurrentTransactions = investments.OrderBy(t => t.TransactionDate).ToList()
        //    };

        //    //var json = JsonConvert.SerializeObject(response);

        //    //response.Redeems = GetOverallRedeemValuation(response.RedeemTransactions);
        //    response.CurrentInvestment = GetOverallCurrentValuation(response.CurrentTransactions);

        //    return new ServiceResponse<FundInvestments>(true, "Success", response);
        //}

        //private List<Transaction> MapInvestments(ServiceResponse<DataTable> investments)
        //{
        //    try
        //    {
        //        if (investments == null || !investments.Success) return new List<Transaction>();

        //        var transactions = (from t in investments.ResponseObject.AsEnumerable()
        //                where Conversions.ConvertToDecimal(t["Units"], 0) > 0
        //                        && Conversions.ConvertToInt(t["SchemaCode"], 0) == 147662
        //                select new Transaction()
        //                {
        //                    TransactionID = Conversions.ConvertToInt(t["TransactionID"], 0),
        //                    PortfolioID = Conversions.ConvertToInt(t["PortfolioID"], 0),

        //                    FolioNumber = Conversions.ConvertToString(t["FolioNumber"], ""),
        //                    //FinancialYear = Conversions.GetFinancialYear(Conversions.ConvertToDateTime(t["TransactionDate"], DateTime.Now)),

        //                    //SchemaCode = Conversions.ConvertToInt(t["SchemaCode"], 0),
        //                    //SchemaName = Conversions.ConvertToString(t["SchemaName"], ""),

        //                    TransactionType = FundTransactionTypes.Investment.ToString(), // "R",

        //                    TransactionDate = Conversions.ConvertToDateTime(t["TransactionDate"], DateTime.Now),

        //                    Dividend = Conversions.ConvertToDecimal(t["Dividend"], 0),

        //                    PurchaseNAV = Conversions.ConvertToDecimal(t["InvestNAV"], 0),
        //                    DividendPerNAV = Conversions.ConvertToDecimal(t["DividendPerNAV"], 0),
        //                    CurrentNAV = Conversions.ConvertToDecimal(t["NAV"], 0),
        //                    Units = Conversions.ConvertToDecimal(t["Units"], 0),

        //                    WithHoldDays = getInvestmentWithHoldDays(t),

        //                    STT = Conversions.ConvertToDecimal(t["STT"], 0),
        //                    StampDuty = Conversions.ConvertToDecimal(t["StampDuty"], 0),
        //                    TDS = Conversions.ConvertToDecimal(t["TDS"], 0),

        //                    FundDetails = MapFundDetails(t),
        //                    XIRR = CalculateInvestmenttXIRR(t),

        //                }).ToList();

        //        var history = (from t in investments.ResponseObject.AsEnumerable()
        //                            where Conversions.ConvertToDecimal(t["Units"], 0) > 0
        //                                && t["History"] != DBNull.Value
        //                                && Conversions.ConvertToInt(t["SchemaCode"], 0) == 147662
        //                       select new {
        //                                history = JsonConvert.DeserializeObject<List<FundTransaction>>(t["History"].ToString())
        //                        }).ToList();

        //        foreach(var h in history)
        //        {
        //            foreach(var record in h.history)
        //            {
        //                if (record.TransactionType == "Reinvest" || record.TransactionType == "Payout")
        //                {
        //                    Transaction r = new Transaction()
        //                    {
        //                        PortfolioID = record.PortfolioID,
        //                        FolioNumber = record.FolioNumber,
        //                        //FinancialYear = record.FinancialYear,
        //                        //SchemaCode = record.SchemaCode,
        //                        TransactionType = record.TransactionType == "Reinvest" ? FundTransactionTypes.DividendReInvest.ToString() : FundTransactionTypes.Dividend.ToString(),
        //                        TransactionDate = record.TransactionDate,
        //                        Amount = record.Amount < 0 ? record.Amount : -1 * record.Amount,
        //                        Dividend = record.Dividend,
        //                        DividendPerNAV = record.DividendPerNAV,
        //                        Units = record.Units,
        //                        STT = record.STT,
        //                        StampDuty = record.StampDuty,
        //                        TDS = record.TDS,
        //                    };

        //                    transactions.Add(r);
        //                }
        //                else if (record.TransactionType == "Switch Out" || record.TransactionType == "Switch In")
        //                {
        //                    Transaction r = new Transaction()
        //                    {
        //                        PortfolioID = record.PortfolioID,
        //                        FolioNumber = record.FolioNumber,
        //                        //FinancialYear = record.FinancialYear,
        //                        //SchemaCode = record.SchemaCode,
        //                        TransactionType = record.TransactionType == "Switch Out" ? FundTransactionTypes.SwitchOut.ToString() : FundTransactionTypes.SwitchIn.ToString(),
        //                        TransactionDate = record.TransactionDate,
        //                        Amount = record.Amount < 0 ? record.Amount : -1 * record.Amount,
        //                        Dividend = record.Dividend,
        //                        DividendPerNAV = record.DividendPerNAV,
        //                        Units = record.Units,
        //                        STT = record.STT,
        //                        StampDuty = record.StampDuty,
        //                        TDS = record.TDS,
        //                    };

        //                    transactions.Add(r);
        //                }
        //            }
        //        }
        //        return transactions.OrderBy(t => t.TransactionDate).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Map Investments exception: {ex.Message}");
        //    }
        //    return null;
        //}

        #endregion
    }

}
