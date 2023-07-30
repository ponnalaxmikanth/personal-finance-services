using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DataAccess
{
    public class InvestmentsAccess : IInvestmentsDataAccess
    {
        private readonly ILogger<InvestmentsAccess> _logger;
        private readonly ISQLHelper _sQLHelper;
        public InvestmentsAccess(ILogger<InvestmentsAccess> logger, InvestmentContext investmentContext, ISQLHelper sQLHelper)
        {
            _logger = logger;
            _sQLHelper = sQLHelper;
        }

        public async Task<ServiceResponse<int>> SaveFundsNAV(List<FundsNav> latestNavData) {
            _logger.LogInformation($"SaveFundsNAV total records: {latestNavData.Count}");
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "nav", Value = JsonConvert.SerializeObject(latestNavData), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("UpdateFundsNAV", CommandType.StoredProcedure, parameters);

                if (result.ResponseObject == latestNavData.Count)
                    return new ServiceResponse<int>(true, "Successfully Updated", default);
                else
                    return new ServiceResponse<int>(false, "Failed to Update", latestNavData.Count - result.ResponseObject);
            }
            catch (Exception ex) {
                _logger.LogInformation(ex, $"SaveFundsNAV failed to Update NAV: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save", default);
        }

        public async Task<ServiceResponse<int>> BackupFundsNAV()
        {
            _logger.LogInformation($"BackupFundsNAV");
            try
            {
                return await _sQLHelper.ExecuteScalar("BackupFundsNAV", CommandType.StoredProcedure, null);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"BackupFundsNAV failed to Backup NAV Data: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Backup NAV Data", default);
        }

        public async Task<ServiceResponse<int>> AddFundsTransaction(MutualFundTransaction request)
        {
            _logger.LogInformation($"Add Funds Transaction: {JsonConvert.SerializeObject(request)}");

            string insertStatement = @"INSERT INTO `Investments`.`FundTransactions`
                                        (`PortfolioID`, `PANCard`, `FinancialYear`, `FolioNumber`, `FolioID`, `SchemaCode`, `TransactionDate`, FundOption, `Amount`, `Units`, `InvestNAV`, `ActualNAV`, `STT`, `GroupOrder`)
                                        VALUES (@PortfolioID, @PANCard, @FinancialYear, @FolioNumber, @FolioID, @SchemaCode, @TransactionDate, @FundOption, @Amount, @Units, @InvestNAV, @ActualNAV, @STT, @groupOrder); ";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@PortfolioID", Value = request.PortfolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@PANCard", Value = request.PANCard, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@FinancialYear", Value = request.FinancialYear, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@FolioNumber", Value = request.FolioNumber, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@FolioID", Value = request.FolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@SchemaCode", Value = request.SchemaCode, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@TransactionDate", Value = request.PurchaseDate.Date, DbType = DbType.Date });
            parameters.Add(new MySqlParameter() { ParameterName = "@FundOption", Value = request.FundOption, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@Amount", Value = request.Amount, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@Units", Value = request.Units, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@InvestNAV", Value = request.nav, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@ActualNAV", Value = request.nav, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@STT", Value = request.Charges, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@groupOrder", Value = request.GroupOrder, DbType = DbType.String });

            var result = await _sQLHelper.ExecuteNonQuery(insertStatement, CommandType.Text, parameters);

            return (result.Success ? new ServiceResponse<int>(true, "Saved Successfully", 1) : new ServiceResponse<int>(false, "Saving Transaction Failed", default));
        }

        public async Task<ServiceResponse<int>> AddFundsTransactionHistory(MutualFundTransaction request, decimal profit = 0)
        {
            _logger.LogInformation($"Add Funds Transaction History: {JsonConvert.SerializeObject(request)}");

            string insertStatement = @"INSERT INTO `Investments`.`FundTransactionsHistory`
                                        (PortfolioID, FolioNumber, FolioID, SchemaCode, TransactionDate, TransactionType, Amount, Units, InvestNAV, DividendPerNAV, STT, StampDuty, TDS, GroupOrder)
                                        VALUES (@PortfolioID, @FolioNumber, @FolioID, @SchemaCode, @TransactionDate, @FundOption, @Amount, @Units, @InvestNAV, @DividendPerNAV, @STT, @StampDuty, @TDS, @profit, @groupOrder); ";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@PortfolioID", Value = request.PortfolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@FolioNumber", Value = request.FolioNumber, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@FolioID", Value = request.FolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@SchemaCode", Value = request.SchemaCode, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@TransactionDate", Value = request.PurchaseDate.Date, DbType = DbType.Date });
            parameters.Add(new MySqlParameter() { ParameterName = "@FundOption", Value = (request.FundOption == "Purchase" ? "Investment" : request.FundOption), DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@Amount", Value = request.Amount, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@Units", Value = request.Units, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@InvestNAV", Value = request.nav, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@DividendPerNAV", Value = request.DividendPerNAV, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@STT", Value = request.STT, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@StampDuty", Value = request.StampDuty, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@TDS", Value = request.TDS, DbType = DbType.Decimal });
            parameters.Add(new MySqlParameter() { ParameterName = "@profit", Value = profit, DbType = DbType.Decimal }); 
            parameters.Add(new MySqlParameter() { ParameterName = "@groupOrder", Value = request.GroupOrder, DbType = DbType.String });

            var result = await _sQLHelper.ExecuteNonQuery(insertStatement, CommandType.Text, parameters);

            return (result.Success ? new ServiceResponse<int>(true, "Saved Successfully", 1) : new ServiceResponse<int>(false, "Saving Transaction Failed", default));
        }

        public async Task<ServiceResponse<DataTable>> GetFundTransactions(int schemaCode, string folioNumber)
        {
            string statement = @"select TransactionID, PortfolioID, FolioNumber, FolioID, ProductCode, SchemaCode, TransactionDate, FundOption, Amount, Units, InvestNAV, ActualNAV, History, IsActive, Comments, DividendPerNAV, Dividend, STT, StampDuty, TDS
                    from `Investments`.`FundTransactions`
              where FolioNumber = @folioNumber AND SchemaCode = @schemaCode and Units > 0";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            //parameters.Add(new MySqlParameter() { ParameterName = "@portfolioID", Value = portfolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@folioNumber", Value = folioNumber, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@schemaCode", Value = schemaCode, DbType = DbType.Int32 });

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID, int schemaCode, string folioNumber)
        {
            string statement = @"select TransactionID, PortfolioID, FolioNumber, FolioID, ProductCode, SchemaCode, TransactionDate, FundOption, Amount, Units, InvestNAV, ActualNAV, Profit, History, IsActive, Comments, DividendPerNAV, Dividend, STT, StampDuty, TDS
                    from `Investments`.`FundTransactions`
              where FolioNumber = @folioNumber AND SchemaCode = @schemaCode and Units > 0";

            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@portfolioID", Value = portfolioID, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@folioNumber", Value = folioNumber, DbType = DbType.String });
            parameters.Add(new MySqlParameter() { ParameterName = "@schemaCode", Value = schemaCode, DbType = DbType.Int32 });

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<int>> AddFundDividend(List<FundDividend> request)
        {
            _logger.LogInformation($"Add Fund Dividend request: {JsonConvert.SerializeObject(request)}");
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "dividend", Value = JsonConvert.SerializeObject(request), DbType = DbType.String });
            return await _sQLHelper.ExecuteScalar("AddFundDividend", CommandType.StoredProcedure, parameters);
        }


        public async Task<ServiceResponse<int>> UpdateTransactions(List<Transaction> request)
        {
            _logger.LogInformation($"Update Transactions port dividend: {JsonConvert.SerializeObject(request)}");
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "fundTransactions", Value = JsonConvert.SerializeObject(request), DbType = DbType.String });
            return await _sQLHelper.ExecuteScalar("UpdateTransactions", CommandType.StoredProcedure, parameters);
        }


        public async Task<ServiceResponse<int>> SaveFundsNAVHistory(List<FundHistory> latestNavData)
        {
            _logger.LogInformation($"SaveHistory total records: {latestNavData.Count}");
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "nav", Value = JsonConvert.SerializeObject(latestNavData), DbType = DbType.String });
                var jsonData = JsonConvert.SerializeObject(latestNavData);
                var result = await _sQLHelper.ExecuteScalar("SaveFundsNAVHistory", CommandType.StoredProcedure, parameters);

                if (result.ResponseObject == latestNavData.Count)
                    return new ServiceResponse<int>(true, "Successfully Updated", default);
                else
                    return new ServiceResponse<int>(false, "Failed to Update", latestNavData.Count - result.ResponseObject);
                
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"saveHistory failed to Save: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save", default);
        }

        public async Task<ServiceResponse<DataTable>> GoalsInfo(DateTime fromDate, DateTime toDate, int portfolioID)
        {
            string dateFilter = " and t.TransactionDate between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + "' ";

            string statement = @"select g.GoalID, g.PortfolioID, p.Portfolio, g.GoalName, g.StartDate, g.MonthlyInvestment, g.Amount, g.SIPDuration, g.TargetDuration, 
                                g.TargetAmount, g.ExpectedInflation, g.TypeOfInvestment, g.Description, g.IsActive GoalStatus,
                                ga.GoalAllocationID, 
                                ga.FolioNumber, 
                                case when ga.Percent is null then 100 else ga.Percent END as Percent,
                                case when ga.IsActive is null then 1 else ga.IsActive END as GoalAllocationStatus,
                                t.SchemaCode, n.SchemaName, 'I' TransactionType,
                                case when t.TransactionDate is null then DATE_SUB(CURDATE(), INTERVAL 1 MONTH) else t.TransactionDate END as TransactionDate,
                                case when t.Units is null then 0 else t.Units * ga.Percent / 100 END as Units,
                                case when t.InvestNAV is null then 0 else t.InvestNAV END as InvestNAV,
                                case when t.ActualNAV is null then 0 else t.ActualNAV END as ActualNAV,
                                case when n.NAV is null then 0 else n.NAV END as  CurrentNAV,
                                t.FinancialYear, n.FundHouse, n.Classification, n.Category,
                                case when ga.Percent is null then 0 else t.Amount * ga.Percent / 100 END as Investment,
                                case when ga.Percent is null then 0 else t.Units * n.NAV * ga.Percent/ 100 END as CurrentValue
	                                from  Goal g
	                                left join GoalAllocation ga on g.GoalID = ga.GoalID and ga.IsActive = '1'
	                                left join FundTransactions t on ga.FolioNumber = t.FolioNumber
	                                left join FundsNAV n on t.SchemaCode = n.SchemaCode
	                                left join Portfolio p on p.ID = g.PortfolioID
                                where (t.Units is null or t.Units <> 0) " + dateFilter + GetPortfolioClause(portfolioID, "g") +
                                @" UNION ALL
                                select -1 GoalID, t.PortfolioID, p.Portfolio, 'Un Mapped' GoalName, DATE_SUB(CURDATE(), INTERVAL 1 MONTH) StartDate,  0 MonthlyInvestment,
                                t.Units * n.NAV Amount, 0 SIPDuration, 0 TargetDuration,  t.Units * n.NAV TargetAmount,0 ExpectedInflation, 
                                g.TypeOfInvestment, g.Description, g.IsActive GoalStatus,
                                ga.GoalAllocationID, t.FolioNumber, 100 Percent, ga.IsActive GoalAllocationStatus,
                                t.SchemaCode, n.SchemaName, 'I' TransactionType, t.TransactionDate, t.Units,
                                t.InvestNAV, t.ActualNAV, n.NAV CurrentNAV, t.FinancialYear, n.FundHouse, n.Classification, n.Category, t.Amount Investment,
                                t.Units * n.NAV CurrentValue
	                                from FundTransactions t
	                                inner join Portfolio p on p.ID = t.PortfolioID
	                                inner join FundsNAV n on t.SchemaCode = n.SchemaCode
	                                left join GoalAllocation ga on ga.FolioNumber = t.FolioNumber and ga.IsActive = '1'
	                                left join Goal g on g.GoalID = ga.GoalID
                                where t.Units > 0 and g.GoalID is null" + dateFilter + GetPortfolioClause(portfolioID, "t");

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> CurrentMonthGoals(DateTime fromDate, DateTime toDate, int portfolioID)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            DateTime stratDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            parameters.Add(new MySqlParameter() { ParameterName = "@minTransactionDate", Value = stratDayOfMonth, DbType = DbType.Date });
            parameters.Add(new MySqlParameter() { ParameterName = "@maxTransactionDate", Value = stratDayOfMonth.AddMonths(1).AddDays(-1).Date, DbType = DbType.Date });

            string dateFilter = " and t.TransactionDate between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + "' ";

            string statement = @"select g.GoalID, g.PortfolioID, p.Portfolio, g.GoalName, g.StartDate, g.MonthlyInvestment, g.Amount, g.SIPDuration, g.TargetDuration, 
                                g.TargetAmount, g.ExpectedInflation, g.TypeOfInvestment, g.Description, g.IsActive GoalStatus,
                                case when ga.GoalAllocationID is null then 0 else ga.GoalAllocationID END as GoalAllocationID,
                                ga.FolioNumber, 
                                case when ga.Percent is null then 100 else ga.Percent END as Percent,
                                ga.IsActive as GoalAllocationStatus,
                                case when t.SchemaCode is null then 0 else t.SchemaCode END as SchemaCode,
                                case when t.TransactionDate is null then DATE_SUB(CURDATE(), INTERVAL 1 MONTH) else t.TransactionDate END as TransactionDate,
                                case when t.Units is null then 0 else t.Units END as Units,
                                case when t.ActualNAV is null then 0 else t.ActualNAV END as ActualNAV,
                                case when n.NAV is null then 0 else n.NAV END as  CurrentNAV,
                                case when t.FinancialYear is null then 0 else t.FinancialYear END as FinancialYear,
                                n.Classification, n.Category,
                                case when t.Units is null then 0 else t.Amount * ga.Percent / 100 END as Investment,
                                case when t.Units is null then 0 else t.Units * n.NAV * ga.Percent/ 100 END as CurrentValue,
                                case when t.Units is null then 0 else t.Units * n.NAV * ga.Percent / 100  END as CurrentGoalValue
	                                from  Goal g
	                                left join GoalAllocation ga on g.GoalID = ga.GoalID and ga.IsActive = '1'
	                                left join FundTransactions t on ga.FolioNumber = t.FolioNumber
                                            and t.TransactionDate > @minTransactionDate and t.TransactionDate <= @maxTransactionDate
	                                left join FundsNAV n on t.SchemaCode = n.SchemaCode
	                                left join Portfolio p on p.ID = g.PortfolioID
                                where (t.Units is null or t.Units <> 0) " + GetPortfolioClause(portfolioID, "g");

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> CurrentMonthTracker(int portfolioID, DateTime date)
            {
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            //DateTime stratDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            parameters.Add(new MySqlParameter() { ParameterName = "@minTransactionDate", Value = date, DbType = DbType.Date });
            parameters.Add(new MySqlParameter() { ParameterName = "@maxTransactionDate", Value = date.AddMonths(1).AddDays(-1).Date, DbType = DbType.Date });

            string filter = GetPortfolioClause(portfolioID, "g");
            //string statement = @"select distinct p.portfolio
            //                    ,g.GoalID, g.PortfolioID, g.GoalName, g.StartDate, g.EndDate, g.Amount goalAmount, g.TargetAmount goalTarget, g.MonthlyInvestment
            //                    ,ga.FolioNumber goalFolioNumber, ga.Percent, m.FolioNumber, m.SchemaCode, n.SchemaName, m.ISIN, m.Amount TargetAmount, t.Amount Investment
            //                    ,m.ISActive
            //                    from goal g
            //                    inner join Portfolio p on p.ID = g.PortfolioID
            //                    inner join GoalAllocation ga on g.GoalID = ga.GoalID and ga.IsActive = b'1'
            //                    inner join MonthlyInvestment m on ga.FolioNumber = m.FolioNumber and m.IsActive = b'1'
            //                    inner join FundsNAV n on m.SchemaCode = n.SchemaCode
            //                    left join FundTransactions t on m.FolioNumber = t.FolioNumber and m.SchemaCode = t.SchemaCode
            //                            and t.TransactionDate >= @minTransactionDate and t.TransactionDate <= @maxTransactionDate "
            string statement = @"select distinct p.portfolio, g.GoalID, g.PortfolioID, g.GoalName, g.StartDate goalStartDate, g.EndDate goalEndDate, g.Amount goalAmount
                                , g.TargetAmount goalTarget, g.MonthlyInvestment
                                , ga.Percent
                                , m.FolioNumber, m.SchemaCode, m.ISIN, m.Amount TargetAmount,m.ISActive, m.StartDate, m.EndDate
                                , n.SchemaName, n.Category, t.Amount Investment, t.TransactionDate
                                from goal g
                                inner join Portfolio p on p.ID = g.PortfolioID
                                inner join GoalAllocation ga on g.GoalID = ga.GoalID
                                inner join MonthlyInvestment m on ga.FolioNumber = m.FolioNumber
                                inner join FundsNAV n on m.SchemaCode = n.SchemaCode and m.ISIN = n.ISINGrowth
                                left join FundTransactions t on m.FolioNumber = t.FolioNumber and m.SchemaCode = t.SchemaCode"
                                + (!string.IsNullOrWhiteSpace(filter) ? filter.Replace(" and ", " where ") : "")
                                + " order by g.PortfolioID, g.StartDate, g.EndDate";
                                

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }


        public async Task<ServiceResponse<DataTable>> GetDailyTrackHistory(DateTime fromDate, DateTime toDate, int portfolioID)
        {
            string filter = GetPortfolioClause(portfolioID, "t");
            string dateFilter = " and t.TrackDate between '" + fromDate.ToString("yyyy-MM-dd") + "' and '" + toDate.ToString("yyyy-MM-dd") + "' ";
            string statement = @"select t.ID, t.PortfolioID, p.Portfolio, t.TrackDate, t.NoOfFundHouses, t.NoOfFolios, t.NoOfFunds,
                                t.Investment, t.Profit, t.AbsoluteReturn, t.XIRR 
                                from FundsDailyTracker t
                                inner join Portfolio p on p.ID = t.PortfolioID
                                 where t.Investment > 0 " + dateFilter + filter + " order by t.TrackDate desc, t.PortfolioID; ";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        

        public async Task<ServiceResponse<DataTable>> GoalAllocations()
        {
            string statement = @"select g.PortfolioID, p.Portfolio,
                                  g.GoalID, g.GoalName, g.MonthlyInvestment, g.IsActive goalActive, g.Description, g.StartDate, g.EndDate,
                                  g.Amount, g.TargetAmount, g.ExpectedInflation,
                                  ga.GoalAllocationID, ga.FolioNumber, ga.Percent, ga.IsActive
                                from Goal g
                                left join GoalAllocation ga on g.GoalID = ga.GoalID
                                left join Portfolio p on p.ID = g.PortfolioID
                                order by g.PortfolioID, g.GoalID, ga.GoalAllocationID;";


            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> Goals()
        {
            string statement = @"select g.GoalID, g.PortfolioID, p.Portfolio, g.GoalName, g.MonthlyInvestment, g.IsActive, g.Description,
                                    g.StartDate, g.EndDate, g.Amount, g.TargetAmount, g.ExpectedInflation
	                                from Goal g
                                    inner join Portfolio p on p.ID = g.PortfolioID
                                    order by g.PortfolioID, g.EndDate;";
                                    

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> NAVDownloadDetails()
        {
            string statement = @"select NAVDate, count(*) as count from FundsNAV group by NAVDate order by 2 desc limit 1;";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        private string GetPortfolioClause(int portfolioID, string alias, string field = "PortfolioID")
        {
            switch(portfolioID)
            {
                case -1:
                    return string.Empty;
                case 92:
                    return " and " + alias + "." + field + " in (1, 4)";
                case 91:
                    return " and " + alias + "." + field + " in (6, 7)";
                case 99:
                    return " and " + alias + "." + field + " in (1, 4, 6, 7)";
                default:
                    return " and " + alias + "." + field + " = " + portfolioID.ToString();
            }
        }

        public async Task<ServiceResponse<int>> SaveQuarterlyReview(DateTime dtLastDay, List<Goal> goals)
        {
            _logger.LogInformation($"SaveQuarterlyGoalReview total records: {goals.Count}");
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "goalReview", Value = JsonConvert.SerializeObject(goals), DbType = DbType.String });
                parameters.Add(new MySqlParameter() { ParameterName = "reviewDate", Value = dtLastDay, DbType = DbType.Date });

                var result = await _sQLHelper.ExecuteScalar("UpdateQuarterlyReview", CommandType.StoredProcedure, parameters);

                if (result.ResponseObject == goals.Count)
                    return new ServiceResponse<int>(true, "Successfully Updated", default);
                else
                    return new ServiceResponse<int>(false, "Failed to Update", goals.Count - result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveQuarterlyGoalReview failed to Quarterly Goals Review: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save", default);
        }

    }
}
