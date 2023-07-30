using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Interfaces;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Packaging;
using Entities;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;
using Excel.FinancialFunctions;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;

namespace BusinessAccess.Services
{

    public static class DateTimeExtensions
    {
        public static Int32 DiffMonths(this DateTime start, DateTime end)
        {
            Int32 months = 0;
            DateTime tmp = start;

            while (tmp < end)
            {
                months++;
                tmp = tmp.AddMonths(1);
            }

            return months;
        }
    }

    public class InvestmentsService : IInvestmentsService
    {
        private readonly ILogger<InvestmentsService> _logger;
        private readonly IInvestmentsDataAccess _investmentsDataAccess;
        private readonly IMutualFundsDataAccess _iMutualFundsDataAccess;

        public InvestmentsService(ILogger<InvestmentsService> logger, IInvestmentsDataAccess investmentsDataAccess, IMutualFundsDataAccess iMutualFundsDataAccess)
        {
            _logger = logger;
            _investmentsDataAccess = investmentsDataAccess;
            _iMutualFundsDataAccess = iMutualFundsDataAccess;
        }

        public async Task<ServiceResponse<int>> DownloadFundsNAV()
        {
            int count = 0;
            ServiceResponse<int> response = new ServiceResponse<int>(true, "Successfully Downloaded NAV Data", 0);

            _logger.LogInformation("DownloadFundsNAV");
            var openEnded = new DownloadUrls() { Url = "http://www.amfiindia.com/spages/NAVOpen.txt", Message = "Downloading Open Funds Data", Type = "Open Ended" };
            var closeEnded = new DownloadUrls() { Url = "http://www.amfiindia.com/spages/NAVClose.txt", Message = "Downloading Closed Funds Data", Type = "Close Ended" };
            var intervalFund = new DownloadUrls() { Url = "http://www.amfiindia.com/spages/NAVInterval.txt", Message = "Downloading Interval Funds Data", Type = "Interval Fund" };

            var backupNavData = await _investmentsDataAccess.BackupFundsNAV();

            var openEndedResponse = await DownloadNAVData(openEnded);
            var closeEndedResponse = await DownloadNAVData(closeEnded);
            var intervalFundResponse = await DownloadNAVData(intervalFund);

            if (!backupNavData.Success)
                response.SetFailure(backupNavData.Message);

            if (!openEndedResponse.Success)
                response.SetFailure(openEndedResponse.Message);
            count = openEndedResponse.ResponseObject;

            if (!closeEndedResponse.Success)
                response.SetFailure(closeEndedResponse.Message);
            count += closeEndedResponse.ResponseObject;

            if (!intervalFundResponse.Success)
                response.SetFailure(intervalFundResponse.Message);
            count += intervalFundResponse.ResponseObject;

            /*Task.WaitAll(openEndedResponse, closeEndedResponse, intervalFundResponse);

            if (!openEndedResponse.IsCompletedSuccessfully)
                response.SetFailure(openEndedResponse.Result.Message);

            if (!closeEndedResponse.IsCompletedSuccessfully)
                response.SetFailure(closeEndedResponse.Result.Message);

            if (!intervalFundResponse.IsCompletedSuccessfully)
                response.SetFailure(intervalFundResponse.Result.Message);
            */
            return response.Success ? new ServiceResponse<int>(true, "Successfully Downloaded NAV Data @ " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), count) : new ServiceResponse<int>(false, response.Error, count);
        }

        private async Task<ServiceResponse<int>> DownloadNAVData(DownloadUrls url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.Url);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // execute the request
                Stream resStream = response.GetResponseStream(); // we will read data via the response stream

                using (var reader = new StreamReader(resStream))
                {
                    return await UpdateNAVData(reader.ReadToEnd(), url.Type);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadNAVData {url.Type}, exception: {ex.Message}");
                return new ServiceResponse<int>(false, "Failed to Download NAV Data", 1);
            }
        }

        private async Task<ServiceResponse<int>> UpdateNAVData(string data, string type)
        {
            ServiceResponse<int> response = new ServiceResponse<int>(true, "Successfully Downloaded NAV Data", default);
            string[] navdata = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<FundsNav> latestNavData = new List<FundsNav>();
            decimal nav;
            string fundsHouse = string.Empty;
            string fundType = string.Empty;
            for (int i = 0; i < navdata.Length; i++)
            {
                string[] result = navdata[i].Split(';');
                if (result.Length == 6 && result[0].Trim().ToLower() != "Scheme Code".ToLower() && decimal.TryParse(result[4].Trim(), out nav)
                    && Convert.ToDateTime(result[5].Trim()) > DateTime.Now.AddDays(-30).Date
                    )
                {
                    try
                    {
                        FundsNav funddata = new FundsNav()
                        {
                            FundHouse = fundsHouse,
                            SchemaCode = Convert.ToInt32(result[0].Trim()),
                            ISINGrowth = GetISINValue(result[1]),
                            ISINDivReinvestment = GetISINValue(result[2]),
                            SchemaName = GetSchemaName(result[3]),
                            FundOption = result[3].ToUpper().Contains("GROWTH") ? "Growth" : "Dividend",
                            FundType = result[3].ToUpper().Contains("DIRECT") ? "Direct" : "Regular",
                            Category = fundType,
                            NAV = nav,
                            NAVDate = Convert.ToDateTime(result[5].Trim())
                        };
                        funddata.Classification = GetFundClassification(funddata.Category);
                        funddata.Category = GetFundCategory(funddata);
                        latestNavData.Add(funddata);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"UpdateNAVData {type}, exception: {ex.Message}");
                        response.SetFailure($"Error occurred while processing line: {i}");
                    }
                }

                else if (navdata[i].Trim().Length > 0 && navdata[i].ToLower() != "Open Ended Schemes(Balanced)".ToLower() && result[0].Trim().ToLower() != "Scheme Code".ToLower() && !navdata[i].Contains(';'))
                {
                    fundsHouse = GetFundHouseName(navdata[i]);
                }

                if (navdata[i].ToLower().Contains("open ended schemes") || navdata[i].ToLower().Contains("close ended schemes")
                    || navdata[i].ToLower().Contains("interval fund schemes")) {
                    fundType = navdata[i].Replace("Open Ended Schemes", "", StringComparison.InvariantCultureIgnoreCase)
                        .Replace("interval fund schemes", "", StringComparison.InvariantCultureIgnoreCase)
                        .Replace("close ended schemes", "", StringComparison.InvariantCultureIgnoreCase)
                        .Replace("Scheme", "", StringComparison.InvariantCultureIgnoreCase)
                        .Replace(" Fund", "", StringComparison.InvariantCultureIgnoreCase)
                        .Replace("  ", " ").Replace("(", "").Replace(")", "").Replace(" and ", " & ").Trim();
                }
            }

            // Update Database
            var saveResponse = await _investmentsDataAccess.SaveFundsNAV(latestNavData);
            if (!saveResponse.Success)
                response.SetFailure(saveResponse.Message);

            return response.Success ? new ServiceResponse<int>(true, "Successfully Downloaded NAV Data", latestNavData.Count) : new ServiceResponse<int>(false, response.Error, saveResponse.ResponseObject);
        }

        private string GetFundCategory(FundsNav nav)
        {
            string category = nav.Category;
            if (!string.IsNullOrWhiteSpace(category))
            {
                string val = category.Split('-').LastOrDefault();
                switch (val.Trim()) {
                    case "Sectoral/ Thematic":

                        if (nav.SchemaName.Contains("Healthcare", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Pharma", StringComparison.CurrentCultureIgnoreCase))
                            return "Pharma";

                        if (nav.SchemaName.Contains("Technology", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Digital", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Gennext", StringComparison.CurrentCultureIgnoreCase))
                            return "IT";

                        if (nav.SchemaName.Contains("FMCG", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Consumption", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Consumer", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Opportunities", StringComparison.CurrentCultureIgnoreCase))
                            return "FMCG";

                        if (nav.SchemaName.Contains("Banking", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Financial", StringComparison.CurrentCultureIgnoreCase))
                            return "Banking";

                        if (nav.SchemaName.Contains("Infra", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Transpotation", StringComparison.CurrentCultureIgnoreCase) ||
                            nav.SchemaName.Contains("Logistics", StringComparison.CurrentCultureIgnoreCase))
                            return "Infra";

                        if (nav.SchemaName.Contains("ESG", StringComparison.CurrentCultureIgnoreCase))
                            return "ESG";

                        if (nav.SchemaName.Contains("QUANT", StringComparison.CurrentCultureIgnoreCase))
                            return "QUANT";

                        else return "Thematic";

                    case "FoF Overseas":
                        return "Overseas";

                    case "FoF Domestic":
                        return "Domestic";

                    case "Multi Asset Allocation":
                        return "Multi Asset";

                    default:
                        return val.Trim();
                }
            }
            return category.Trim();
        }

        private string GetFundClassification(string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                string retValue = category.Split('-')[0].Trim();
                return !string.IsNullOrWhiteSpace(retValue) ? retValue.Replace(" Sectoral/ Thematic", "Thematic") : null;
            }
            return null;
        }

        private string? GetISINValue(string isin)
        {
            return (string.IsNullOrWhiteSpace(isin) || isin == "-") ? null : isin.Trim();
        }

        private string GetSchemaName(string name)
        {
            name = name
                                .Replace("DIRECT PLAN", "Direct", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("(Direct)", "Direct", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("REGULAR PLAN", "Regular", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("(REGULAR)", "Regular", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Growth Plan", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Growth Option", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("GROWTH", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Dividend Option", "(D)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("DIVIDEND", "(D)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("AND", "&", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("PLAN", "", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("(G) Direct", " Direct(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("PAYOUT", "Payout", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Fixed Term Income", "FTI", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Income Distribution cum Capital Withdrawal", "IDCW", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Income Distribution Capital Withdrawal", "IDCW", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Mutual Fund", "", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("FUND", "", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Option", "", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Aditya Birla Sun Life ", "ABSL ", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Mahindra Manulife", "Mahindra", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("ICICI Prudential ", "ICICI ", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("NIPPON INDIA", "Nippon ", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("BANK OF INDIA", "BOI ", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Franklin India", "Franklin", StringComparison.InvariantCultureIgnoreCase)
                                .Replace("-", "")
                                .Replace(") (", ")(")
                                .Replace("Fixed Maturity", "FMP", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("  (G) - (G)", " (G)")
                                .Replace("(G) (G)", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace(" (G)  (G)", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("(G)(G)", "(G)", StringComparison.CurrentCultureIgnoreCase)
                                
                                .Replace("IDCW (IDCW)", "IDCW", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("(G) Direct", "Direct (G)", StringComparison.CurrentCultureIgnoreCase)
                                .Replace("Direct(G)", "Direct (G)")
                                .Replace("((", "(")
                                .Replace("))", ")")
                                .Replace(" (G) Regular", " Regular (G)")
                                .Trim();

            name = Regex.Replace(name, @"\s+", " ");

            return name;
        }

        private string GetFundHouseName(string name)
        {
            name = name.Replace(" Mutual Fund", "");
            if (name.Contains("Aditya Birla Sun Life", StringComparison.InvariantCultureIgnoreCase))
                name = "ABSL";
            if (name.Contains("ICICI PRUDENTIAL", StringComparison.InvariantCultureIgnoreCase))
                name = "ICICI";
            if (name.Contains("Mahindra Manulife", StringComparison.InvariantCultureIgnoreCase))
                name = "Mahindra";
            if (name.Contains("Nippon India", StringComparison.InvariantCultureIgnoreCase))
                name = "Nippon";
            if (name.Contains("BANK OF INDIA", StringComparison.InvariantCultureIgnoreCase))
                name = "BOI";
            if (name.Contains("Franklin Templeton", StringComparison.InvariantCultureIgnoreCase))
                name = "Franklin";
            return name;
        }

        

        public async Task<ServiceResponse<FundsNav>> GetFundsNAVData(int schemaCode)
        {
            _logger.LogInformation($"get funds nav for schema: {schemaCode}");
            var navData = new FundsNav();
            return new ServiceResponse<FundsNav>(true, "", navData);
        }


        public async Task<ServiceResponse<int>> AddFundsTransaction(MutualFundTransaction request)
        {
            var hisresponse = await _investmentsDataAccess.AddFundsTransactionHistory(request);
            var response = await _investmentsDataAccess.AddFundsTransaction(request);

            if (hisresponse.Success && response.Success)
                return new ServiceResponse<int>(true, "History: " + hisresponse.ResponseObject.ToString() + ", Transaction:" + response.ResponseObject, hisresponse.ResponseObject + response.ResponseObject);

            return new ServiceResponse<int>(false, "Failed to Add Transaction", default);
        }


        public async Task<ServiceResponse<int>> RedeemFund(MutualFundTransaction request)
        {
            //return await _investmentsDataAccess.RedeemFund(request);
            ServiceResponse<DataTable> resp = await _investmentsDataAccess.GetFundTransactions(request.PortfolioID, request.SchemaCode, request.FolioNumber);
            List<Transaction> fundTransactions = MapTransactions(resp);

            return await UpdateRedeem(request, fundTransactions);
        }

        private async Task<ServiceResponse<int>> UpdateRedeem(MutualFundTransaction request, List<Transaction> fundTransactions)
        {
            try
            {
                if (fundTransactions == null || fundTransactions.Count() <= 0) return new ServiceResponse<int>(false, "Transactions not found", default);

                string[] transTypes = new string[] { "Purchase", "Switch", "Reinvest" };
                var trans = fundTransactions.Where(t => t.Units > 0 && t.TransactionDate <= request.PurchaseDate && transTypes.Contains(t.TransactionType)).ToList();

                if (trans.Sum(t => t.Units) < request.Units) return new ServiceResponse<int>(false, "Request Units: " + request.Units + " Available Units: " + trans.Sum(t => t.Units), default);

                decimal updatedUnits = 0;
                decimal percent = 0;
                decimal profit = 0;
                decimal transProfit = 0;
                for (int i = 0; i < trans.Count(); i++)
                {
                    transProfit = 0;
                    if (trans[i].History == null) trans[i].History = new List<Transaction>();

                    trans[i].History.Add(getRedeemTransactionHistory(request, updatedUnits, trans[i]));
                    if (trans[i].Units > request.Units + updatedUnits)
                    {
                        percent = (request.Units - updatedUnits) / request.Units;

                        transProfit = (request.nav - (trans[i].Units - (request.Units - updatedUnits))) - ((trans[i].Units - (request.Units - updatedUnits)) * trans[i].ActualNAV) - (request.STT * percent) - (request.StampDuty * percent) - (request.TDS * percent);
                        profit = profit + transProfit;

                        trans[i].STT = trans[i].STT + (request.STT * percent);
                        trans[i].StampDuty = trans[i].StampDuty + (request.StampDuty * percent);
                        trans[i].TDS = trans[i].TDS + (request.TDS * percent);
                        trans[i].Profit = trans[i].Profit + transProfit;
                        trans[i].Units = trans[i].Units - (request.Units - updatedUnits);

                        updatedUnits = request.Units;
                        trans[i].Amount = trans[i].Units * trans[i].ActualNAV;
                    }
                    else // all units redeemed
                    {
                        percent = trans[i].Units / request.Units;
                        trans[i].TransactionType = FundTransactionTypes.Redeem.ToString();
                        updatedUnits = updatedUnits + trans[i].Units;

                        transProfit = (request.nav * trans[i].Units) - (trans[i].Units * trans[i].ActualNAV) - (request.STT * percent) - (request.StampDuty * percent) - (request.TDS * percent);
                        profit = profit + transProfit;

                        trans[i].Units = 0;
                        trans[i].Profit = trans[i].Profit + transProfit;
                        trans[i].STT = trans[i].STT + (request.STT * percent);
                        trans[i].StampDuty = trans[i].StampDuty + (request.StampDuty * percent);
                        trans[i].TDS = trans[i].TDS + (request.TDS * percent);

                        trans[i].Amount = 0;
                    }
                    if (updatedUnits >= request.Units) break;
                }
                string fundTrans = JsonConvert.SerializeObject(fundTransactions);

                var hisresponse = await _investmentsDataAccess.AddFundsTransactionHistory(request, profit);
                var response = await _investmentsDataAccess.UpdateTransactions(fundTransactions);
                if (hisresponse.Success && response.Success)
                    return new ServiceResponse<int>(true, "History: " + hisresponse.ResponseObject.ToString() + ", Redeem:" + response.ResponseObject, hisresponse.ResponseObject + response.ResponseObject);
            }
            catch(Exception ex)
            {
                _logger.LogError($"UpdateRedeem exception: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to update redeem", default);
        }

        private Transaction getRedeemTransactionHistory(MutualFundTransaction request, decimal updatedUnits, Transaction transaction)
        {
            Transaction t = JsonConvert.DeserializeObject<Transaction>(JsonConvert.SerializeObject(transaction));
            t.History = null;
            t.TransactionType = FundTransactionTypes.Redeem.ToString();
            t.Amount = t.Units * request.nav;
            t.Units = t.Units > request.Units + updatedUnits ? request.Units - updatedUnits : t.Units;
            t.TransactionDate = request.PurchaseDate;
            t.RedeemNAV = request.nav;

            decimal percent = 0;
            if (transaction.Units > request.Units + updatedUnits)
            {
                percent = (request.Units - updatedUnits) / request.Units;
                transaction.STT = transaction.STT + (request.STT * percent);
                transaction.StampDuty = transaction.StampDuty + (request.StampDuty * percent);
                transaction.TDS = transaction.TDS + (request.TDS * percent);

                t.Units = request.Units - updatedUnits;
            } else
            {
                transaction.STT = transaction.STT + (request.STT * percent);
                transaction.StampDuty = transaction.StampDuty + (request.StampDuty * percent);
                transaction.TDS = transaction.TDS + (request.TDS * percent);
                t.Units = t.Units;
            }

            return t;
        }

        public async Task<ServiceResponse<int>> UpdateFundDividend(FundDividend request)
        {
            ServiceResponse<DataTable> resp = await _investmentsDataAccess.GetFundTransactions(request.SchemaCode, request.FolioNumber);
            List<Transaction> fundTransactions = MapTransactions(resp);

            //string fundTrans_old = JsonConvert.SerializeObject(fundTransactions);

            UpdateDividend(request, fundTransactions);
            
            if(request.DividendType == "Reinvest" || request.DividendType == "ReInvest" || request.DividendType == "Re Invest")
            {
                fundTransactions.Add(new Transaction()
                {
                    PortfolioID = request.PortfolioID,
                    FolioID = request.FolioID,
                    FolioNumber = request.FolioNumber,
                    TransactionType = request.DividendType,
                    TransactionDate = request.DividendDate,
                    Amount = request.Amount,
                    PurchaseNAV= request.NAV,
                    Units = request.Units,

                    FundDetails = new FundDetails()
                    {
                        SchemaCode = request.SchemaCode,
                        FundOption = request.DividendType
                    }
                });
            }

            await _investmentsDataAccess.AddFundDividend(new List<FundDividend>() { request });
            var hisresponse = await _investmentsDataAccess.AddFundsTransactionHistory(new MutualFundTransaction() {
                PortfolioID = request.PortfolioID,
                FolioNumber = request.FolioNumber,
                FolioID = request.FolioID,
                SchemaCode = request.SchemaCode,
                PurchaseDate = request.DividendDate,
                FundOption = request.DividendType,
                Amount = request.Amount,
                Units = request.Units,
                nav = request.NAV,
                DividendPerNAV = request.DividendNAV,
                STT = request.STT,
                StampDuty = request.StampDuty,
                TDS = request.TDS
            });
            var response = await _investmentsDataAccess.UpdateTransactions(fundTransactions);

            //string fundTrans = JsonConvert.SerializeObject(fundTransactions);
            return response; //  new ServiceResponse<int>(response.Success, response.Message, response.ResponseObject);
        }

        private void UpdateDividend(FundDividend request, List<Transaction> fundTransactions)
        {
            string[] transTypes = new string[] { "Purchase", "Switch", "Reinvest", "Re Invest" };
            var trans = fundTransactions.Where(t => t.Units > 0 && t.TransactionDate <= request.DividendDate && transTypes.Contains(t.TransactionType)).ToList();
            trans.ForEach(t =>
            {
                t.DividendPerNAV = t.DividendPerNAV + request.DividendNAV;
                t.History.Add(new Transaction()
                {
                    PortfolioID = t.PortfolioID,
                    FolioNumber = t.FolioNumber,
                    FolioID = t.FolioID,
                    TransactionDate = request.DividendDate,
                    Amount = t.Units * request.DividendNAV,
                    PurchaseNAV = request.NAV,
                    DividendPerNAV = request.DividendNAV,
                    TransactionType = request.DividendType,
                    Units = t.Units
                    // Units = (t.Units * request.DividendNAV) / request.NAV
                });
            });
        }

        private List<Transaction> MapTransactions(ServiceResponse<DataTable> resp)
        {
            if (resp.Success && resp.ResponseObject.Rows.Count > 0)
            {
                return resp.ResponseObject.AsEnumerable().Select(
                    r => new Transaction()
                    {
                        TransactionID = int.Parse(r["TransactionID"].ToString()),
                        PortfolioID = int.Parse(r["PortfolioID"].ToString()),

                        FolioNumber = r["FolioNumber"].ToString(),
                        FolioID = int.Parse(r["FolioID"].ToString()),

                        TransactionType = r["FundOption"].ToString(),
                        TransactionDate = DateTime.Parse(r["TransactionDate"].ToString()),
                        Amount = decimal.Parse(r["Amount"].ToString()),
                        Units = decimal.Parse(r["Units"].ToString()),
                        PurchaseNAV = decimal.Parse(r["InvestNAV"].ToString()),
                        Profit = r["Profit"] == DBNull.Value ? 0 :decimal.Parse(r["Profit"].ToString()),
                        DividendPerNAV = decimal.Parse(r["DividendPerNAV"].ToString()),
                        STT = decimal.Parse(r["STT"].ToString()),
                        StampDuty = decimal.Parse(r["StampDuty"].ToString()),
                        TDS = decimal.Parse(r["TDS"].ToString()),

                        History = r["History"] != DBNull.Value ? JsonConvert.DeserializeObject<List<Transaction>>(r["History"].ToString()) : null,

                        FundDetails = new FundDetails()
                        {
                            SchemaCode = int.Parse(r["SchemaCode"].ToString()),
                            FundOption = r["FundOption"].ToString(),
                        }
                    }).OrderBy(t => t.TransactionDate).ToList();
            }
            return null;
        }

        public async Task<ServiceResponse<int>> GetHistory(HistoryRequest request)
        {
            // return await _fundsNAVDataAccess.getHistory(request);
            ServiceResponse<int> response = new ServiceResponse<int>(true, "", 0);
            int noOfRecords = 0;
            for (DateTime date = request.ToDate; date >= request.FromDate; date = date.AddDays(-1)) {
                _logger.LogInformation("Download NAV History for " + date.ToString("dd-MMM-yyyy"));
                string url = "https://portal.amfiindia.com/DownloadNAVHistoryReport_Po.aspx?frmdt=" + date.ToString("dd-MMM-yyyy") + "&todt=" + date.ToString("dd-MMM-yyyy");
                var resp = await DownloadNAVHistory(new DownloadUrls() { Url = url });
                if (!resp.Success) response.SetFailure(date.ToString("dd-MMM-yyyy") + ", ");
                noOfRecords = noOfRecords + resp.ResponseObject;
            }

            return response.Success ? new ServiceResponse<int>(true, "Successfully Downloaded NAV Data", noOfRecords) : new ServiceResponse<int>(false, response.Error, noOfRecords);
        }

        private async Task<ServiceResponse<int>> DownloadNAVHistory(DownloadUrls url)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.Url);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // execute the request
                Stream resStream = response.GetResponseStream(); // we will read data via the response stream

                using (var reader = new StreamReader(resStream))
                {
                    return await UpdateNAVHistory(reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadNAVHistory {url.Type}, exception: {ex.Message}");
                return new ServiceResponse<int>(false, "Failed to Download NAV History", 1);
            }
        }

        private async Task<ServiceResponse<int>> UpdateNAVHistory(string data)
        {
            ServiceResponse<int> response = new ServiceResponse<int>(true, "Successfully Downloaded NAV Data", default);
            string[] navdata = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<FundHistory> navData = new List<FundHistory>();
            decimal nav;
            
            for (int i = 0; i < navdata.Length; i++)
            {
                string[] result = navdata[i].Split(';');
                if (result.Length == 8 && result[0].Trim().ToLower() != "Scheme Code".ToLower() && decimal.TryParse(result[4].Trim(), out nav))
                {
                    try
                    {
                        navData.Add(new FundHistory()
                        {
                            SchemaCode = Convert.ToInt32(result[0].Trim()),
                            NAV = nav,
                            NAVDate = Convert.ToDateTime(result[7].Trim())
                        });
                    }
                    catch (Exception ex)
                    {
                        response.SetFailure($"Error occurred while processing line: {i}");
                    }
                }
            }

            // Update Database
            var saveResponse = await _investmentsDataAccess.SaveFundsNAVHistory(navData);
            if (!saveResponse.Success)
                response.SetFailure(saveResponse.Message);

            return response.Success ? new ServiceResponse<int>(true, "Successfully Downloaded NAV History", navData.Count) : new ServiceResponse<int>(false, response.Error, saveResponse.ResponseObject);
        }

        public async Task<ServiceResponse<int>> InitializeFundTransactions(string path)
        {
            ServiceResponse<int> response = new ServiceResponse<int>(true, "Success", default);
            var fileName = string.Format(path);

            // Open the document for editing.

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
            }

            return response;
        }

        

        

        //private void SetGoalName(List<Goal> lstGoals)
        //{
        //    //foreach(var goal in lstGoals)
        //    //{
        //    //    if (goal.Percent > 100)
        //    //    {
        //    //        goal.GoalName = "Over Mapped";
        //    //    }
        //    //}
        //}

        public async Task<ServiceResponse<List<Goal>>> Goals()
        {
            _logger.LogInformation("Goals");
            try
            {
                var goals = await _investmentsDataAccess.Goals();
                return MapGoals(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Goals exception: {ex.Message}");
            }
            return new ServiceResponse<List<Goal>>(false, "Failed to get Goals", default);
        }

        private ServiceResponse<List<Goal>> MapGoals(ServiceResponse<DataTable> goals)
        {
            if (goals.Success && goals.ResponseObject != null && goals.ResponseObject.Rows.Count > 0)
            {
                var result = (from g in goals.ResponseObject.AsEnumerable()
                              select new Goal()
                              {
                                  GoalID = Convert.ToInt32(g["GoalID"].ToString()),
                                  Portfolio = g["Portfolio"].ToString(),
                                  PortfolioID = Convert.ToInt32(g["PortfolioID"].ToString()),
                                  GoalName = g["GoalName"].ToString(),
                                  //MonthlyInvestment = Convert.ToDecimal(g["MonthlyInvestment"]),
                                  IsActive = g["IsActive"].ToString() == "1" ? true : false,
                                  Description = g["Description"].ToString(),
                                  StartDate = Convert.ToDateTime(g["StartDate"].ToString()),
                                  //EndDate = Convert.ToDateTime(g["EndDate"].ToString()),
                                  PresentValue = Convert.ToDouble(g["Amount"]),

                                  //FutureValue = Convert.ToDouble(g["TargetAmount"]),
                                  Inflation = Convert.ToDouble(g["ExpectedInflation"]),


                              }).ToList();
                return new ServiceResponse<List<Goal>>(true, "Success", result);
            }
            return new ServiceResponse<List<Goal>>(false, "No Goal Information available", default);
        }


        public async Task<ServiceResponse<List<GoalAllocation>>> GoalAllocations()
        {
            _logger.LogInformation("GoalAllocations");
            try
            {
                var transactions = await _investmentsDataAccess.GoalAllocations();
                return MapGoalAllocations(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GoalAllocations exception: {ex.Message}");
            }
            return new ServiceResponse<List<GoalAllocation>>(false, "Failed to get Goal Allocations", default);
        }

        private ServiceResponse<List<GoalAllocation>> MapGoalAllocations(ServiceResponse<DataTable> goals)
        {
            if (goals.Success && goals.ResponseObject != null && goals.ResponseObject.Rows.Count > 0)
            {
                var result = (from g in goals.ResponseObject.AsEnumerable()
                              group g by new
                              {
                                  PortfolioID = Convert.ToInt32(g["PortfolioID"].ToString()),
                                  Portfolio = g["Portfolio"].ToString(),
                                  GoalID = Convert.ToInt32(g["GoalID"].ToString()),
                                  GoalName = g["GoalName"].ToString(),
                              } into tg
                              select new GoalAllocation()
                              {
                                  GoalInfo = new Goal()
                                  {
                                      GoalID = tg.Key.GoalID,
                                      GoalName = tg.Key.GoalName,
                                      PortfolioID = tg.Key.PortfolioID,
                                      Portfolio = tg.Key.Portfolio,
                                      
                                      StartDate = Convert.ToDateTime(tg.First()["StartDate"].ToString()),

                                      SIPAmount = Convert.ToDecimal(tg.First()["MonthlyInvestment"]),
                                      PresentValue = Convert.ToDouble(tg.First()["Amount"]),

                                      Inflation = Convert.ToDouble(tg.First()["ExpectedInflation"]),
                                      Description = tg.First()["Description"].ToString(),
                                      IsActive = tg.First()["goalActive"].ToString() == "1" ? true : false,
                                  },
                                  FolioDetails = (from gf in tg
                                                  select new GoalFolioDetails()
                                                  {
                                                      GoalAllocationID = gf["GoalAllocationID"] == DBNull.Value ? -1 : Convert.ToInt32(gf["GoalAllocationID"].ToString()),
                                                      FolioNumber = gf["GoalAllocationID"] == DBNull.Value ? "-1" : gf["FolioNumber"].ToString(),
                                                      Percent = gf["GoalAllocationID"] == DBNull.Value ? 100 : Convert.ToDecimal(gf["Percent"]),
                                                      IsActive = (gf["GoalAllocationID"] == DBNull.Value || gf["GoalAllocationID"].ToString() == "0") ? false : true,
                                                  }).ToList()
                                  //FolioDetails = (from gf in tg
                                  //                select new GoalFolioDetails()
                                  //                {
                                  //                    GoalAllocationID = gf["GoalAllocationID"] == null ? -1 : Convert.ToInt32(gf["GoalAllocationID"].ToString()),
                                  //                    //FolioNumber = gf["GoalAllocationID"] == null ? string.Empty : gf["FolioNumber"].ToString(),
                                  //                    //Percent = gf["GoalAllocationID"] == null ? -1 : Convert.ToDecimal(gf["Percent"]),
                                  //                    //IsActive = gf["GoalAllocationID"] == null ? false : gf["IsActive"].ToString() == "1" ? true : false,
                                  //                }).ToList()
                              }
                              //select new GoalAllocation()
                              //{
                              //    GoalAllocationID = Convert.ToInt32(g["GoalAllocationID"].ToString()),
                              //    GoalID = Convert.ToInt32(g["GoalID"].ToString()),
                              //    FolioNumber = g["FolioNumber"].ToString(),
                              //    IsActive = g["IsActive"].ToString() == "1" ? true : false,
                              //    Percent = Convert.ToDecimal(g["Percent"]),
                              //    GoalInfo = new Goal()
                              //    {
                              //        GoalID = Convert.ToInt32(g["GoalID"].ToString()),
                              //        PortfolioID = Convert.ToInt32(g["PortfolioID"].ToString()),
                              //        Portfolio = g["Portfolio"].ToString(),
                              //        GoalName = g["GoalName"].ToString(),
                              //        MonthlyInvestment = Convert.ToDecimal(g["MonthlyInvestment"]),
                              //        IsActive = g["goalActive"].ToString() == "1" ? true : false,
                              //        Description = g["Description"].ToString(),
                              //        StartDate = Convert.ToDateTime(g["StartDate"].ToString()),
                              //        EndDate = Convert.ToDateTime(g["EndDate"].ToString()),
                              //        TargetAmount = Convert.ToDecimal(g["Amount"]),
                              //    }
                              //}
                              ).ToList();
                return new ServiceResponse<List<GoalAllocation>>(true, "Success", result);
            }
            return new ServiceResponse<List<GoalAllocation>>(false, "No Goal Allocations available", default);
        }


        public async Task<ServiceResponse<GoalsInfo>> GoalsInfo(DateTime fromDate, DateTime toDate, int portfolioID)
        {
            _logger.LogInformation("GoalsInfo");

            try
            {
                var goalsInfo = MapGoalsInfo(await _investmentsDataAccess.GoalsInfo(fromDate, toDate, portfolioID));

                return goalsInfo == null ? new ServiceResponse<GoalsInfo>(false, "Failed to Get Goals Info", null) : new ServiceResponse<GoalsInfo>(true, "Success", goalsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetFundTransactions exception: {ex.Message}");
            }
            return new ServiceResponse<GoalsInfo>(false, "Failed to get Goals Information", default);
        }

        private GoalsInfo MapGoalsInfo(ServiceResponse<DataTable> transactions)
        {
            if (transactions.Success && transactions.ResponseObject != null && transactions.ResponseObject.Rows.Count > 0)
            {
                GoalsInfo goalsInfo = new GoalsInfo()
                {
                    ConsolidatedInfo = new ConsolidatedGoal(),
                    Goals = new List<Goal>()
                };
                var result = (from t in transactions.ResponseObject.AsEnumerable()
                              group t by new
                              {
                                  PortfolioID = Convert.ToInt32(t["PortfolioID"]),
                                  Portfolio = t["Portfolio"].ToString(),
                                  GoalID = Convert.ToInt32(t["GoalID"]),
                                  GoalName = t["GoalName"].ToString(),
                                  StartDate = Convert.ToDateTime(t["StartDate"].ToString()),
                              } into tg
                              select new Goal()
                              {
                                  PortfolioID = tg.Key.PortfolioID,
                                  Portfolio = tg.Key.Portfolio,

                                  GoalID = tg.Key.GoalID,
                                  GoalName = tg.Key.GoalName,
                                  StartDate = tg.Key.StartDate,

                                  SIPDuration = tg.First()["SIPDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["SIPDuration"]),
                                  TargetDuration = tg.First()["TargetDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["TargetDuration"]),

                                  SIPAmount = tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["MonthlyInvestment"]),
                                  PresentValue = tg.First()["Amount"] == DBNull.Value ? 0 : Convert.ToDouble(tg.First()["Amount"]),

                                  Inflation = tg.First()["ExpectedInflation"] == DBNull.Value ? 0 : Convert.ToDouble(tg.First()["ExpectedInflation"]),
                                  Investment = tg.Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),

                                  CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"] == DBNull.Value ? 0 : t["CurrentValue"])),

                              }).OrderBy(g => g.PortfolioID).ThenBy(g =>g.RemainingGoal).ToList();
                goalsInfo.ConsolidatedInfo = MapGoalsConsolidated(transactions);
                goalsInfo.Goals = result;

                return goalsInfo;
            }
            return null;
        }

        private ConsolidatedGoal MapGoalsConsolidated(ServiceResponse<DataTable> transactions)
        {
            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var res = (from t in transactions.ResponseObject.AsEnumerable()
                       group t by new
                       {
                           PortfolioID = Convert.ToInt32(t["PortfolioID"]),
                           GoalName = t["GoalName"].ToString(),
                           GoalID = Convert.ToInt32(t["GoalID"]),
                       } into tg
                       select new ConsolidatedGoal()
                       {
                           SIPAmount = tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["MonthlyInvestment"]),
                           PresentValue = tg.First()["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["Amount"]),
                           FutureValue = tg.First()["TargetAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["TargetAmount"]),
                           Target = tg.First()["TargetAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(tg.First()["TargetAmount"]),

                           CurrentInvestment = tg.Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),
                           CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"] == DBNull.Value ? 0 : t["CurrentValue"])),

                           ExpectedInvestment = Conversions.ToDecimal(tg.First()["SIPDuration"], 0) * 12 * Conversions.ToDecimal(tg.First()["MonthlyInvestment"], 0),

                           StartDate = tg.First()["StartDate"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(tg.First()["StartDate"]),

                           EndDate = Convert.ToDateTime(tg.First()["StartDate"] == DBNull.Value ? DateTime.Now.Date : tg.First()["StartDate"]).AddYears(Convert.ToInt32(tg.First()["TargetDuration"] == DBNull.Value ? 0 : tg.First()["TargetDuration"])).AddDays(-1),

                           SIPDuration = tg.First()["SIPDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["SIPDuration"]),
                           TargetDuration = tg.First()["TargetDuration"] == DBNull.Value ? 0 : Convert.ToInt32(tg.First()["TargetDuration"]),

                           CurrentMonthInvestment = tg.Where(t => Convert.ToDateTime(t["TransactionDate"] == DBNull.Value ? DateTime.Now.Date : t["TransactionDate"]) >= currentMonth)
                           .Sum(t => Convert.ToDecimal(t["Investment"] == DBNull.Value ? 0 : t["Investment"])),

                           CurrentMonthValue = tg.Where(t => Convert.ToDateTime(t["TransactionDate"] == DBNull.Value ? DateTime.Now.Date : t["TransactionDate"]) >= currentMonth)
                           .Sum(t => Convert.ToDecimal(t["CurrentValue"] == DBNull.Value ? 0 : t["CurrentValue"])),

                           TargetInvestment = Convert.ToDecimal(tg.First()["MonthlyInvestment"] == DBNull.Value ? 0 : tg.First()["MonthlyInvestment"]) * Convert.ToInt32(tg.First()["SIPDuration"] == DBNull.Value ? 0 : tg.First()["SIPDuration"]) * 12,

                       });

            ConsolidatedGoal result = new ConsolidatedGoal()
            {
                SIPAmount = res.Sum(r => r.SIPAmount),
                PresentValue = res.Sum(r => r.PresentValue),
                FutureValue = res.Sum(r => r.FutureValue),
                Target = res.Sum(r => r.Target),
                CurrentInvestment = res.Sum(r => r.CurrentInvestment),
                CurrentValue = res.Sum(r => r.CurrentValue),
                CurrentMonthInvestment = res.Sum(r => r.CurrentMonthInvestment),
                CurrentMonthValue = res.Sum(r => r.CurrentMonthValue),

                StartDate = res.Min(r => r.StartDate),
                EndDate = res.Max(r => r.EndDate),

                TargetInvestment = res.Sum(r => r.TargetInvestment),
                ExpectedInvestment = res.Sum(r => r.ExpectedInvestment)
            };

            return result;
        }

        //private decimal calculateExpectedInvestment(IGrouping<object, DataRow> tg)
        //{
        //    //tg.Sum(t => Conversions.ConvertToDecimal(t["SIPDuration"], 0) * 12 * Conversions.ConvertToDecimal(t["MonthlyInvestment"], 0))
        //    decimal result = 0;

        //    result = tg.Select(t => Conversions.ConvertToDecimal(t["SIPDuration"], 0) * 12 * Conversions.ConvertToDecimal(t["MonthlyInvestment"], 0)).First();

        //    return result;
        //}

        public async Task<ServiceResponse<PortfolioData>> PortfolioData(DateTime fromDate, DateTime toDate, int portfolioID, bool groupTransactions)
        {
            _logger.LogInformation("PortfolioData");

            //var name = GetSchemaName("Nippon India Pharma Fund - Direct Plan Growth Plan - Growth Option");
            PortfolioData portfolio = new PortfolioData();
            try
            {
                var transactions = await _investmentsDataAccess.GoalsInfo(fromDate, toDate, portfolioID);
                //var currentMonthTransactions = await _investmentsDataAccess.CurrentMonthGoals(fromDate, toDate, portfolioID);

                
                

                if (transactions != null && transactions.Success && transactions.ResponseObject.Rows.Count > 0)
                {
                    var goalsInfo = MapGoalsInfo(transactions);
                    portfolio.Goals = goalsInfo == null ? null : goalsInfo.Goals;
                    portfolio.ConsolidatedInfo = goalsInfo == null ? null : goalsInfo.ConsolidatedInfo;

                    //var currentMonth = MapGoalsInfo(currentMonthTransactions);
                    //portfolio.CurrentMonth = currentMonth == null || currentMonth.Goals == null ? null :
                        //currentMonth.Goals.OrderByDescending(g => (g.SIPAmount - g.Investment) / g.SIPAmount).ToList();

                    var categoryValudation = MapCategoryValuation(transactions);
                    portfolio.CategoryValuation = categoryValudation == null ? null : categoryValudation;

                    var consolidated = MapConsolidatedInvestment(portfolioID, transactions);

                    if (consolidated != null)
                    {
                        var json = JsonConvert.SerializeObject(consolidated);
                        portfolio.Consolidated = consolidated;
                        await _iMutualFundsDataAccess.SaveFundsDailyTracker(consolidated);

                        await saveQuarterlyReview(portfolio.Goals);
                    }

                    MapFinancialYearValuation(portfolioID, transactions.ResponseObject, portfolio);
                    MapTransactions(transactions.ResponseObject, portfolio, groupTransactions);
                    //MapDailyTracker(dailyTransHistory, portfolio);
                    MapMonthlyTracker(transactions.ResponseObject, portfolio);
                    MapMonthlyGoalTracker(transactions.ResponseObject, portfolio);
                    MapYearOldInvestReview(portfolio.Transactions, portfolio);
                    
                }
                
                return portfolio.Goals == null ? new ServiceResponse<PortfolioData>(false, "No Goals Found", null) : new ServiceResponse<PortfolioData>(true, "Success", portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PortfolioData exception: {ex.Message}");
            }
            return new ServiceResponse<PortfolioData>(false, "Failed to get Portfolio Data", portfolio);
        }

        private void MapYearOldInvestReview(List<FundInvestment> transactions, PortfolioData portfolio)
        {
            List<DateTime> dtList = new List<DateTime>();
            List<double> valList = new List<double>();
            try
            {
                var yrOldTransactions = (from trans in transactions
                                         where trans.TransactionDate <= DateTime.Now.AddYears(-1)
                                         select new
                                         {
                                             date = trans.TransactionDate,
                                             Investment = Convert.ToDouble(trans.Amount),
                                             currentValue = Convert.ToDouble(trans.CurrentValue)
                                         }).OrderBy(t => t.date);

                if (yrOldTransactions.Count() <= 0) return;

                dtList = yrOldTransactions.Select(t => t.date).ToList();
                valList = yrOldTransactions.Select(t => t.Investment).ToList();

                double currentValue = yrOldTransactions.Sum(t => t.currentValue);

                valList.Add(currentValue * -1);
                dtList.Add(DateTime.Now.Date);

                portfolio.YearOldInvestReview = new YearOldInvestReview()
                {
                    Investment = yrOldTransactions.Sum(t => t.Investment),
                    CurrentValue = currentValue,
                    XIRR = decimal.Round(Convert.ToDecimal(Financial.XIrr(valList, dtList)) * 100, 2)
                };
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"MapYearOldInvestReview dates: {JsonConvert.SerializeObject(dtList)}");
                _logger.LogInformation($"MapYearOldInvestReview valList: {JsonConvert.SerializeObject(valList)}");
                _logger.LogError($"MapYearOldInvestReview exception: {ex.Message}");
            }
            
        }

        private async Task<int> saveQuarterlyReview(List<Goal> goals)
        {
            DateTime datetime = DateTime.Now;
            int currQuarter = (datetime.Month - 1) / 3 + 1;
            DateTime dtLastDay = new DateTime(datetime.Year, 3 * currQuarter, 1).AddMonths(1).AddDays(-1);

            var result = await _investmentsDataAccess.SaveQuarterlyReview(dtLastDay, goals);

            return goals.Count;
        }

        public async Task<ServiceResponse<List<CurrentMonthTracker>>> CurrentMonthTracker(int portfolioID, DateTime date)
        {
            var currentMonthTracker = await _investmentsDataAccess.CurrentMonthTracker(portfolioID, date);
            var response = MapCurrentMonthTracker(currentMonthTracker, date);
            return response == null ? new ServiceResponse<List<CurrentMonthTracker>>(false, "Failed", null) : new ServiceResponse<List<CurrentMonthTracker>>(true, "Success", response);
        }

        private List<CurrentMonthTracker> MapCurrentMonthTracker(ServiceResponse<DataTable> currentMonthTracker, DateTime startDate)
        {
            try
            {

                MonthlySchemaTracker obj = new MonthlySchemaTracker();
                DateTime dt = obj.StartDate;

                if (currentMonthTracker != null && currentMonthTracker.Success)
                {
                    //DateTime stratDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                    DateTime endDate = startDate.AddMonths(1).AddDays(-1).Date;

                    return (from t in currentMonthTracker.ResponseObject.AsEnumerable()
                                                     group t by new
                                                     {
                                                         Portfolio = Conversions.ToString(t["portfolio"], ""),
                                                         GoalName = Conversions.ToString(t["GoalName"], ""),
                                                     } into tg
                                                     select new CurrentMonthTracker()
                                                     {
                                                         Portfolio = tg.Key.Portfolio,
                                                         Goal = tg.Key.GoalName,
                                                         CurrentGoal = Conversions.ToDouble(tg.First()["goalAmount"], -1),
                                                         TargetGoal = Conversions.ToDouble(tg.First()["goalTarget"], -1),
                                                         StartDate = Conversions.ToDateTime(tg.First()["StartDate"], DateTime.Now.AddYears(1)),
                                                         EndDate = Conversions.ToDateTime(tg.First()["EndDate"], DateTime.Now.AddYears(1)),
                                                         MonthlyTarget = Conversions.ToDecimal(tg.First()["MonthlyInvestment"], 0),

                                                         MonthlySchemaTracker = (from t in tg
                                                                                 group t by new
                                                                                 {
                                                                                     FolioNumber = Conversions.ToString(t["FolioNumber"], "Folio"),
                                                                                     SchemaCode = Conversions.ToInt(t["SchemaCode"], -1),
                                                                                     StartDate = Conversions.ToDateTime(t["StartDate"], DateTime.Now.AddYears(1)),
                                                                                     EndDate = Conversions.ToDateTime(t["EndDate"], DateTime.Now.AddYears(1))
                                                                                 } into m
                                                                                 select new MonthlySchemaTracker()
                                                                                 {
                                                                                     FolioNumber = m.Key.FolioNumber,
                                                                                     SchemaCode = m.Key.SchemaCode,
                                                                                     StartDate = m.Key.StartDate,
                                                                                     EndDate = m.Key.EndDate,
                                                                                     SchemaName = Conversions.ToString(m.First()["SchemaName"], "Schema"),
                                                                                     Category = Conversions.ToString(m.First()["Category"], "Category"),
                                                                                     ISIN = Conversions.ToString(m.First()["ISIN"], "ISIN"),
                                                                                     IsActive = Conversions.ToBoolean(m.First()["ISActive"], false),

                                                                                     MonthlyTargetAmount = Conversions.ToDecimal(m.First()["TargetAmount"], 0) * Conversions.ToDecimal(m.First()["Percent"], 0) / 100,

                                                                                     OverallInvestment = m.Sum(t => Conversions.ToDecimal(t["Investment"], 0)) * Conversions.ToDecimal(m.First()["Percent"], 0) / 100,

                                                                                     CurrentMonthInvestment = m.Where(t => Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddYears(1)) >= startDate && Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddYears(1)) <= endDate).Sum(t => Conversions.ToDecimal(t["Investment"], 0) * Conversions.ToDecimal(m.First()["Percent"], 0) / 100)

                                                                                 }).ToList()
                                                     }).OrderByDescending(t => t.MonthRemaining).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"MapCurrentMonthTracker exception: {ex.Message}");
            }
            return null;
        }

        private void MapMonthlyTracker(DataTable transactions, PortfolioData portfolio)
        {
            portfolio.MonthlyTracker = (from t in transactions.AsEnumerable()
                                        where t["TransactionType"].ToString() == "I" && Convert.ToDecimal(t["Units"]) > 0
                                        group t by new { Year = Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date).Year, Month = Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date).Month } into tg
                                        select new MonthView()
                                        {
                                            Date = new DateTime(tg.Key.Year, tg.Key.Month, 1),
                                            Investment = tg.Sum(t => Conversions.ToDecimal(t["Investment"], 0)),
                                            CurrentValue = tg.Sum(t => Conversions.ToDecimal(t["CurrentValue"], 0)),
                                            XIRR = GetXIRR(tg)
                                        }).OrderBy(t => t.Date).ToList();
        }

        private void MapMonthlyGoalTracker(DataTable transactions, PortfolioData portfolio)
        {
            portfolio.MonthlyGoalView = (from t in transactions.AsEnumerable()
                                        where t["TransactionType"].ToString() == "I" && Convert.ToDecimal(t["Units"]) > 0
                                        group t by new {
                                            Year = Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date).Year,
                                            Month = Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date).Month,
                                            PortfolioID = Conversions.ToInt(t["PortfolioID"], -1),
                                            Portfolio = Conversions.ToString(t["Portfolio"], "Default"),
                                            GoalID = Conversions.ToInt(t["GoalID"], -1),
                                            GoalName = Conversions.ToString(t["GoalName"], "Default")
                                        } into tg
                                        select new MonthlyGoalView()
                                        {
                                            Portfolio = tg.Key.Portfolio,
                                            PortfolioID = tg.Key.PortfolioID,
                                            GoalID = tg.Key.GoalID,
                                            GoalName = tg.Key.GoalName,
                                            Date = new DateTime(tg.Key.Year, tg.Key.Month, 1).AddMonths(1).AddDays(-1),
                                            Investment = tg.Sum(t => Conversions.ToDecimal(t["Investment"], 0)),
                                            CurrentValue = tg.Sum(t => Conversions.ToDecimal(t["CurrentValue"], 0)),
                                            SIP = Conversions.ToDecimal(tg.First()["MonthlyInvestment"], -1),
                                            XIRR = GetXIRR(tg)
                                        }).OrderBy(t => t.Date).ToList();
        }


        public async Task<ServiceResponse<List<FundsDailyTrack>>> FundsDailyTrack(DateTime fromDate, DateTime toDate, int portfolioID)
        {
            _logger.LogInformation("FundsDailyTrack");
            var dailyTransHistory = await _investmentsDataAccess.GetDailyTrackHistory(fromDate, toDate, portfolioID);
            var res = MapDailyTracker(dailyTransHistory);

            return res == null ? new ServiceResponse<List<FundsDailyTrack>>(false, "Getting Daily History Failed", null) : new ServiceResponse<List<FundsDailyTrack>>(true, "Success", res);
        }

        private List<FundsDailyTrack> MapDailyTracker(ServiceResponse<DataTable> dailyTransHistory)
        {
            if (dailyTransHistory != null && dailyTransHistory.Success)
            {
                return (from t in dailyTransHistory.ResponseObject.AsEnumerable()
                                             select new FundsDailyTrack()
                                             {
                                                 PortfolioID = Convert.ToInt32(t["PortfolioID"]),
                                                 Portfolio = Convert.ToString(t["Portfolio"]),
                                                 TrackDate = Convert.ToDateTime(t["TrackDate"]),
                                                 NoOfFundHouses = Convert.ToInt32(t["NoOfFundHouses"]),
                                                 NoOfFolios = Convert.ToInt32(t["NoOfFolios"]),
                                                 NoOfFunds = Convert.ToInt32(t["NoOfFunds"]),
                                                 Investment = Convert.ToDecimal(t["Investment"]),
                                                 Profit = Convert.ToDecimal(t["Profit"]),
                                                 AbsoluteReturun = Convert.ToDecimal(t["AbsoluteReturn"]),
                                                 XIRR = Convert.ToDecimal(t["XIRR"]),
                                             }).ToList();
            }
            return null;
        }

        private void MapTransactions(DataTable transactions, PortfolioData portfolio, bool groupTransactions)
        {
            if (groupTransactions)
            {
                portfolio.Transactions = (from t in transactions.AsEnumerable()
                                          where t["TransactionType"].ToString() == "I" && Convert.ToDecimal(t["Units"]) > 0
                                          group t by new { SchemaCode = Convert.ToInt32(t["SchemaCode"].ToString()), SchemaName = t["SchemaName"].ToString() } into tg
                                          select new FundInvestment()
                                          {
                                              SchemaCode = tg.Key.SchemaCode,
                                              SchemaName = tg.Key.SchemaName,
                                              Portfolio = Conversions.ToString(tg.First()["Portfolio"], ""),
                                              PortfolioID = Conversions.ToInt(tg.First()["PortfolioID"], -1),
                                              Category = tg.First()["Category"] == DBNull.Value ? string.Empty : tg.First()["Category"].ToString(),

                                              Amount = tg.Sum(t => Convert.ToDecimal(t["Investment"])),
                                              CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),

                                              Units = tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              NAV = tg.Average(t => Convert.ToDecimal(t["CurrentNAV"])),

                                              InvestNAV = tg.Sum(t => Convert.ToDecimal(t["Investment"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              ActualNAV = tg.Sum(t => Convert.ToDecimal(t["Investment"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              //DividendPerNAV = tg.Sum(t => Convert.ToDecimal(t["DividendPerNAV"])),
                                              //Dividend = tg.Sum(t => Convert.ToDecimal(t["Dividend"])),

                                              WithholdDays = GetWithHoldDays(tg),
                                              XIRR = GetXIRR(tg)
                                          }).ToList();
            }
            else
            {
                portfolio.Transactions = (from t in transactions.AsEnumerable()
                                          where t["TransactionType"].ToString() == "I" && Convert.ToDecimal(t["Units"]) > 0
                                          group t by new {
                                              SchemaCode = Convert.ToInt32(t["SchemaCode"].ToString()),
                                              TransactionDate = Conversions.ToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date),
                                              FolioNumber = Conversions.ToString(t["FolioNumber"], string.Empty),
                                              Goal = Conversions.ToString(t["GoalName"], string.Empty),
                                          } into tg
                                          select new FundInvestment()
                                          {
                                              SchemaCode = tg.Key.SchemaCode,
                                              Portfolio = Conversions.ToString(tg.First()["Portfolio"], ""),
                                              PortfolioID = Conversions.ToInt(tg.First()["PortfolioID"], -1),
                                              SchemaName = tg.First()["SchemaName"] == DBNull.Value ? string.Empty : tg.First()["SchemaName"].ToString(),
                                              Category = tg.First()["Category"] == DBNull.Value ? string.Empty : tg.First()["Category"].ToString(),

                                              Amount = tg.Sum(t => Convert.ToDecimal(t["Investment"])),
                                              CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),

                                              Units = tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              NAV = tg.Average(t => Convert.ToDecimal(t["CurrentNAV"])),

                                              InvestNAV = tg.Sum(t => Convert.ToDecimal(t["Investment"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              ActualNAV = tg.Sum(t => Convert.ToDecimal(t["Investment"])) / tg.Sum(t => Convert.ToDecimal(t["Units"])),
                                              FolioNumber = tg.Key.FolioNumber,

                                              TransactionDate = tg.Key.TransactionDate,
                                              Goal = tg.Key.Goal, // Conversions.ToString(tg.First()["GoalName"], string.Empty),
                                              //DividendPerNAV = tg.Sum(t => Convert.ToDecimal(t["DividendPerNAV"])),
                                              //Dividend = tg.Sum(t => Convert.ToDecimal(t["Dividend"])),

                                              WithholdDays = GetWithHoldDays(tg),
                                              XIRR = GetXIRR(tg)
                                          }).ToList();
                //where Convert.ToDecimal(t["Units"]) > 0
                //select new FundInvestment()
                //{
                //    SchemaCode = Conversions.ConvertToInt(t["SchemaCode"], 0),
                //    SchemaName = Conversions.ConvertToString(t["SchemaName"], string.Empty),
                //    Category = Conversions.ConvertToString(t["Category"], string.Empty),

                //    Amount = Conversions.ConvertToDecimal(t["Investment"], 0),
                //    CurrentValue = Conversions.ConvertToDecimal(t["CurrentValue"], 0),

                //    Units = Conversions.ConvertToDecimal(t["Units"], 0),
                //    NAV = Conversions.ConvertToDecimal(t["CurrentNAV"], 0),

                //    InvestNAV = Conversions.ConvertToDecimal(t["InvestNAV"], 0),
                //    ActualNAV = Conversions.ConvertToDecimal(t["ActualNAV"], 0),

                //    TransactionDate = Conversions.ConvertToDateTime(t["TransactionDate"], DateTime.Now.AddDays(30).Date),
                //    FolioNumber = Conversions.ConvertToString(t["FolioNumber"], string.Empty),
                //    TransactionType = Conversions.ConvertToString(t["TransactionType"], string.Empty),
                //    CurrentNAV = Conversions.ConvertToDecimal(t["CurrentNAV"], 0),

                //    //CurrentValue = Conversions.ConvertToDecimal(t["CurrentNAV"], 0) * Conversions.ConvertToDecimal(t["Units"], 0)
                //    WithholdDays = GetWithHoldDays(t),
                //    XIRR = GetXIRR(t, "Investment", "TransactionDate", "CurrentValue")
                //}).ToList();
            }
            
        }

        private void MapFinancialYearValuation(int portfolioID, DataTable transactions, PortfolioData portfolio)
        {
            List<FinancialYearValuation> fyValuation = null;

            fyValuation = (from t in transactions.AsEnumerable()
                           where Convert.ToDecimal(t["Units"]) > 0
                           group t by new { FinancialYear = Convert.ToInt32(t["FinancialYear"].ToString()), TransactionType = t["TransactionType"].ToString() } into tg
                        select new FinancialYearValuation()
                        {
                            FinancialYear = tg.Key.FinancialYear,
                            TransactionType = tg.Key.TransactionType,
                            Amount = tg.Sum(t => Convert.ToDecimal(t["Investment"])),
                            CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                            XIRR = GetXIRR(tg)
                        }).OrderBy(t => t.FinancialYear).ToList();
            portfolio.FinancialYearValuation = fyValuation;
        }

        private Consolidated MapConsolidatedInvestment(int portfolioID, ServiceResponse<DataTable> transactions)
        {
            Consolidated response = new Consolidated();

            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var investTransacitons = transactions.ResponseObject.AsEnumerable()
                                        .Where(t => t["TransactionType"].ToString() == "I" && Convert.ToDecimal(t["Units"]) > 0);

            response = new Consolidated()
            {
                PortfolioID = portfolioID,
                NoOfFundHouses = investTransacitons.Select(t => t["FundHouse"].ToString()).Distinct().Count(),
                NoOfFolios = investTransacitons.Select(t => t["FolioNumber"].ToString()).Distinct().Count(),
                NoOfFunds = investTransacitons.Select(t => t["SchemaCode"].ToString()).Distinct().Count(),

                Investment = investTransacitons.Sum(t => Convert.ToDecimal(t["Investment"])),
                CurrentValue = investTransacitons.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                Xirr = GetXIRR(investTransacitons),
                CurrentMonth = investTransacitons.Where(t => Convert.ToDateTime(t["TransactionDate"].ToString()) >= currentMonth).Sum(t => Convert.ToDecimal(t["Investment"])),
            };

            return response;
        }

        private List<CategoryValuation> MapCategoryValuation(ServiceResponse<DataTable> transactions)
        {
            List<CategoryValuation> response = null;
            if (transactions.Success && transactions.ResponseObject.Rows.Count > 0)
            {
                response = (from t in transactions.ResponseObject.AsEnumerable()
                            where Convert.ToDecimal(t["Units"]) > 0
                            group t by new { Category = t["Category"].ToString(), TransactionType = t["TransactionType"].ToString() } into tg
                            select new CategoryValuation()
                            {
                                Name = tg.Key.Category,
                                TransactionType = tg.Key.TransactionType,
                                Amount = tg.Sum(t => Convert.ToDecimal(t["Investment"])),
                                CurrentValue = tg.Sum(t => Convert.ToDecimal(t["CurrentValue"])),
                                XIRR = GetXIRR(tg)
                            }).OrderByDescending(t => t.Amount).ToList();
            }
            return response;
        }

        private decimal GetXIRR(IGrouping<object, DataRow> tg, string investProperty = "Investment", string transactDateProperty = "TransactionDate", string currentVallProperty = "CurrentValue")
        {
            try
            {
                var list = (from t in tg
                            where Convert.ToDecimal(t["Units"]) > 0
                            select new
                            {
                                investment = Convert.ToDouble(t[investProperty]) * -1,
                                date = Convert.ToDateTime(t[transactDateProperty]),
                                currentValue = Convert.ToDouble(t[currentVallProperty])
                            }).Where(t => t.date.Date < DateTime.Now.Date).OrderBy(t => t.date).ToList();

                if (list == null || list.Count() < 1) return 0;

                List<DateTime> dtList = list.Select(t => t.date).ToList();
                List<double> valList = list.Select(t => t.investment).ToList();

                double currentValue = list.Sum(t => t.currentValue);

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

        private decimal GetXIRR(EnumerableRowCollection<DataRow> investTransacitons)
        {
            try {
                var res = (from t in investTransacitons
                           select new
                           {
                               amount = Convert.ToDouble(t["Investment"]) * -1,
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
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Calculate CAGR exception: {ex.Message}");
            }
            return 0;
        }

        private decimal GetXIRR(DataRow investTransaciton, string investProperty = "Amount", string transactDateProperty = "TransactionDate", string currentVallProperty = "CurrentValue")
        {
            try
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
            catch(Exception ex)
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

        public async Task<ServiceResponse<NAVDownloadDetails>> NAVDownloadDetails()
        {
            var navDetails = await _investmentsDataAccess.NAVDownloadDetails();
            NAVDownloadDetails response = MapNAVDownloadDetails(navDetails);
            return response == null ? new ServiceResponse<NAVDownloadDetails>(false, "Failed", null) : new ServiceResponse<NAVDownloadDetails>(true, "Success", response);
        }

        private NAVDownloadDetails MapNAVDownloadDetails(ServiceResponse<DataTable> navDetails)
        {
            if (navDetails != null && navDetails.Success)
            {
                return (from t in navDetails.ResponseObject.AsEnumerable()
                        select new NAVDownloadDetails()
                        {
                            NAVDate = Conversions.ToDateTime(t["NAVDate"], DateTime.Now.AddYears(1)),
                            Count = Conversions.ToInt(t["count"], -1)
                        }).FirstOrDefault();
            }
            return null;
        }

    }
}
