using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DataAccess.Services
{
    public class MutualFundsDataAccess : IMutualFundsDataAccess
    {
        private readonly ILogger<MutualFundsDataAccess> _logger;
        private readonly ISQLHelper _sQLHelper;

        public MutualFundsDataAccess(ILogger<MutualFundsDataAccess> logger, ISQLHelper sQLHelper)
        {
            _logger = logger;
            _sQLHelper = sQLHelper;
        }

        #region old implementation

        public async Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID)
        {
            string statement = @"select n.FundHouse, n.Category, n.SchemaName, t.FinancialYear, t.SchemaCode, t.FolioNumber, t.Amount, t.Units,
                                n.NAV * t.Units CurrentValue, n.NAV, t.InvestNAV, t.ActualNAV , t.DividendPerNAV, t.Dividend, t.TransactionDate,
                                'I' TransactionType
	                                from Investments.FundTransactions t
	                                inner join Investments.FundsNAV n on n.SchemaCode = t.SchemaCode
	                        where t.Units > 0 " + GetPortfolioClause(portfolioID, "t")

                            + @" UNION ALL

                            select n.FundHouse, n.Category, n.SchemaName, r.FinancialYear, r.SchemaCode, r.FolioNumber, r.Amount, r.Units,
                            r.RedeemNAV * r.Units CurrentValue, n.NAV, r.InvestNAV, 0 ActualNAV , 0 DividendPerNAV, 0 Dividend, r.RedeemDate TransactionDate,
                            'R' TransactionType
	                            from Investments.FundRedeems r
	                            inner join Investments.FundsNAV n on n.SchemaCode = r.SchemaCode
                            where r.Units > 0 " + GetPortfolioClause(portfolioID, "r");

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID, int schemaCode)
        {

            string statement = @"select t.TransactionDate, t.FinancialYear, t.FolioNumber, t.TransactionType,
                                AVG(t.CurrentNAV) CurrentNAV, SUM(t.CurrentValue) CurrentValue, SUM(t.Investment) Investment, SUM(t.Units) Units
                                from (
                                    select n.NAV CurrentNAV,
                                    case when ga.Percent is null then 0 else t.Units * n.NAV * ga.Percent/ 100 END as CurrentValue,
                                    t.FinancialYear, t.TransactionDate TransactionDate, ga.FolioNumber, 
                                    case when ga.Percent is null then 0 else t.Amount * ga.Percent / 100 END as Investment,
                                    t.Units * ga.Percent / 100 Units, 'I' TransactionType
	                                    from  Goal g
	                                    left join GoalAllocation ga on g.GoalID = ga.GoalID and ga.IsActive = '1'
	                                    left join FundTransactions t on ga.FolioNumber = t.FolioNumber
	                                    left join FundsNAV n on t.SchemaCode = n.SchemaCode
	                                    left join Portfolio p on p.ID = g.PortfolioID
                                    where (t.Units is null or t.Units <> 0) and t.SchemaCode = " + schemaCode + GetPortfolioClause(portfolioID, "g") +
                                    " ) t group by t.TransactionDate, t.FinancialYear, t.FolioNumber, t.TransactionType";
            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetLatestNAVDate(DateTime date)
        {
            _logger.LogInformation($"GetLatestNAVDate: {date.Date}");
            string statement = @"select h.NAVDate, count(*) count
                                from (
                                    SELECT h.SchemaCode, h.NAVDate, h.NAV NAV , ROW_NUMBER() OVER(PARTITION BY h.SchemaCode ORDER BY h.NAVDate DESC) AS row_index
                                    FROM FundsNAVHistory h
                                    where h.NAVDate <= @date and h.NAVDate >= DATE_SUB(@date, INTERVAL 5 DAY)
                                ) h where h.row_index = 1
                                group by h.NAVDate
                                order by count(*) desc;";

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            parameters.Add(new MySqlParameter() { ParameterName = "@date", Value = date, DbType = DbType.Date });
            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> GoalsReview(int portfolioID)
        {
            string whereClause = GetPortfolioClause(portfolioID, "g", "PortfolioID");
            whereClause = whereClause.Contains(" and ") ? whereClause.Replace(" and ", "") : whereClause;
            // , t.Units* ga.Percent / 100 Units, t.InvestNAV, t.ActualNAV, t.STT + t.StampDuty, t.TDS Charges, h.NAV, h.NAVDate
            string statement = @"select p.Portfolio, g.PortfolioID, g.GoalID, g.GoalName, g.StartDate, g.EndDate, g.MonthlyInvestment, g.Amount goalAmount,
                                g.SIPDuration, g.TargetDuration, g.TargetAmount, g.ExpectedInflation
                                , q.Investment Investment, q.CurrentValue CurrentValue, q.ReviewDate
                                from goal g
                                inner join Investments.QuaterlyGoalReview q on q.PortfolioID = g.PortfolioID and g.GoalID = q.GoalID
                                inner join Investments.Portfolio p on q.PortfolioID = p.ID
                                where " + whereClause;

            //List<MySqlParameter> parameters = new List<MySqlParameter>();
            //parameters.Add(new MySqlParameter() { ParameterName = "@PortfolioID", Value = portfolioID, DbType = DbType.Int32 });

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);

            //if (cutOffDate > DateTime.Today.Date) cutOffDate = DateTime.Today.Date.AddDays(-1);

            //string statement = @"select p.Portfolio, g.PortfolioID, g.GoalID, g.GoalName, g.StartDate, g.EndDate, g.MonthlyInvestment, g.Amount goalAmount, g.SIPDuration
            //                        , g.TargetDuration, g.TargetAmount, g.ExpectedInflation
            //                        , ga.GoalAllocationID, ga.FolioNumber, ga.Percent
            //                        , t.SchemaCode, t.TransactionDate, t.FundOption
            //                        , t.Amount * ga.Percent / 100 Investment, t.Units * ga.Percent / 100 Units, t.InvestNAV, t.ActualNAV
            //                        , t.STT + t.StampDuty, t.TDS Charges
            //                        , h.NAV, h.NAVDate
            //                        , h.NAV * t.Units * ga.Percent / 100 CurrentValue
            //                        from goal g
            //                        inner join GoalAllocation ga on g.GoalID = ga.GoalID and ga.IsActive = b'1'
            //                        inner join FundTransactions t on ga.FolioNumber = t.FolioNumber and t.Units > 0 and t.TransactionDate <= @cutOffDate ";

            //statement = statement + (latest ? @" inner join FundsNAV h on  t.SchemaCode = h.SchemaCode " : @" inner join FundsNAVHistory h on  t.SchemaCode = h.SchemaCode and h.NAVDate = @cutOffDate ");

            //string whereClause = GetPortfolioClause(portfolioID, "g");
            //whereClause = whereClause.Contains(" and ") ? whereClause.Replace(" and ", "") : whereClause;
            
        }

        //public async Task<ServiceResponse<int>> SaveQuarterlyGoalReview(List<GoalReview> goalReviews)
        //{
        //    _logger.LogInformation($"SaveQuarterlyGoalReview total records: {goalReviews.Count}");
        //    try
        //    {
        //        List<MySqlParameter> parameters = new List<MySqlParameter>();
        //        parameters.Add(new MySqlParameter() { ParameterName = "goalReview", Value = JsonConvert.SerializeObject(goalReviews), DbType = DbType.String });

        //        var result = await _sQLHelper.ExecuteScalar("UpdateQuarterlyReview", CommandType.StoredProcedure, parameters);

        //        if (result.ResponseObject == goalReviews.Count)
        //            return new ServiceResponse<int>(true, "Successfully Updated", default);
        //        else
        //            return new ServiceResponse<int>(false, "Failed to Update", goalReviews.Count - result.ResponseObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation(ex, $"SaveQuarterlyGoalReview failed to Quarterly Goals Review: {ex.Message}");
        //    }
        //    return new ServiceResponse<int>(false, "Failed to Save", default);
        //}

        private string GetPortfolioClause(int portfolioID, string alias, string field = "PortfolioID")
        {
            switch (portfolioID)
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

        public async Task<ServiceResponse<int>> SaveFundsDailyTracker(Consolidated consolidated)
        {
            try
            {
                string statement = @"INSERT INTO `Investments`.`FundsDailyTracker` (`PortfolioID`, `TrackDate`, `NoOfFundHouses`, `NoOfFolios`, `NoOfFunds`, 
                            `Investment`, `Profit`, `AbsoluteReturn`, `XIRR`, `AvgWithholdDays`)
                            VALUES (@PortfolioID, @TrackDate, @NoOfFundHouses, @NoOfFolios, @NoOfFunds, 
                            @Investment, @Profit, @AbsoluteReturn, @XIRR, @AvgWithholdDays)
                                ON DUPLICATE KEY
                                    UPDATE NoOfFundHouses=@NoOfFundHouses, NoOfFolios=@NoOfFolios, NoOfFunds=@NoOfFunds,
                                        Investment=@Investment, Profit=@Profit, AbsoluteReturn=@AbsoluteReturn, XIRR=@XIRR, AvgWithholdDays=@AvgWithholdDays";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                parameters.Add(new MySqlParameter() { ParameterName = "@PortfolioID", Value = consolidated.PortfolioID, DbType = DbType.Int32 });
                parameters.Add(new MySqlParameter() { ParameterName = "@TrackDate", Value = DateTime.Now.Date, DbType = DbType.Date });
                parameters.Add(new MySqlParameter() { ParameterName = "@NoOfFundHouses", Value = consolidated.NoOfFundHouses, DbType = DbType.Int32 });
                parameters.Add(new MySqlParameter() { ParameterName = "@NoOfFolios", Value = consolidated.NoOfFolios, DbType = DbType.Int32 });
                parameters.Add(new MySqlParameter() { ParameterName = "@NoOfFunds", Value = consolidated.NoOfFunds, DbType = DbType.Int32 });

                parameters.Add(new MySqlParameter() { ParameterName = "@Investment", Value = consolidated.Investment, DbType = DbType.Decimal });
                parameters.Add(new MySqlParameter() { ParameterName = "@Profit", Value = consolidated.Profit, DbType = DbType.Decimal });
                parameters.Add(new MySqlParameter() { ParameterName = "@AbsoluteReturn", Value = consolidated.AbsoulteReturn, DbType = DbType.Decimal });
                parameters.Add(new MySqlParameter() { ParameterName = "@XIRR", Value = consolidated.Xirr, DbType = DbType.Decimal });
                parameters.Add(new MySqlParameter() { ParameterName = "@AvgWithholdDays", Value = consolidated.WithHoldDays, DbType = DbType.Int32 });

                var result = await _sQLHelper.ExecuteScalar(statement, CommandType.Text, parameters);

                return new ServiceResponse<int>(true, "Successfully Updated", default);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundsDailyTracker failed to Save: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save", default);
        }

        public async Task<ServiceResponse<DataTable>> GetSwitcInValuation(int portfolioID)
        {
            string statement = @"select s.InSchemaCode, n.SchemaName, s.SwitchInDate, s.InUnits, s.InNAV,
                                (s.InUnits * n.NAV) currentInValue, (s.OutUnits * h.NAV) currentOutValue
	                                from fundswitchin s
	                                inner join FundsNAV n on s.InSchemaCode = n.SchemaCode
	                                inner join FundsNAV h on s.OutSchemaCode = h.SchemaCode
                                where s.InUnits > 0 " + GetPortfolioClause(portfolioID, "S");

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetCurrentValues()
        {
            string statement = @"select n.Classification, n.Category, AVG(n.NAV) NAV, count(*) Count
                                        from FundsNAV n where n.FundOption = 'Growth' and n.FundType = 'Direct'
                                    group by n.Classification, n.Category";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetCurrentValues(string classification, string category)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@category", Value = category, DbType = DbType.String });

            string statement = @"select n.SchemaCode, n.SchemaName, n.Classification, n.Category, n.NAV NAV
	                                from FundsNAV n where n.FundOption = 'Growth' and n.FundType = 'Direct' and n.Category = @category";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> GetPerformance(int period)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@period", Value = period, DbType = DbType.Int32 });

            string statement = @"select n.Classification, n.Category, AVG(n.NAV) NAV
                                    from (	
	                                    SELECT n.Classification, n.Category, h.NAV NAV, ROW_NUMBER() OVER(PARTITION BY h.SchemaCode ORDER BY h.NAVDate DESC) AS row_index
	                                    FROM FundsNAVHistory h
	                                    inner join FundsNAV n on n.SchemaCode = h.SchemaCode and n.FundOption = 'Growth' and n.FundType = 'Direct'
	                                    where h.NAVDate > DATE_SUB(CURDATE(), INTERVAL @period MONTH)
                                            and h.NAVDate < DATE_ADD(DATE_SUB(CURDATE(), INTERVAL @period MONTH), INTERVAL 5 DAY)
                                    ) n
                                    where n.row_index = 1 group by n.Classification, n.Category";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> GetCategoryPerformance(int period, string classification, string category)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "@period", Value = period, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@category", Value = category, DbType = DbType.String });

            string statement = @"select n.SchemaCode, n.SchemaName, AVG(n.NAV) NAV
                                from (	
	                                SELECT h.SchemaCode, n.SchemaName, h.NAV NAV, ROW_NUMBER() OVER(PARTITION BY h.SchemaCode ORDER BY h.NAVDate DESC) AS row_index
	                                FROM FundsNAVHistory h
	                                inner join FundsNAV n on n.SchemaCode = h.SchemaCode and n.FundOption = 'Growth' and n.FundType = 'Direct' and n.Category = @category
	                                where h.NAVDate < DATE_SUB(CURDATE(), INTERVAL  (@period * 30)  DAY)
		                                and h.NAVDate > DATE_SUB(CURDATE(), INTERVAL ((@period * 30) + 5) DAY)
                                ) n
                                where n.row_index = 1 group by n.SchemaCode, n.SchemaName;";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        public async Task<ServiceResponse<DataTable>> GetPortfolios()
        {
            string statement = @"select ID, Portfolio, IsActive, Description, InvestorPAN from Portfolio where IsActive = '1';";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetFundFolios()
        {
            //string statement = @"select p.ID, p.Portfolio, p.InvestorPAN, p.IsActive, p.Description,
            //                    f.ID FolioID, f.FolioNumber, f.FundHouse, f.DateOpened, f.DefaultBank, f.DefaultAccountNumber, f.Email, f.MobileNumber, f.IsActive
            //                    from
            //                    Portfolio p
            //                    left join FundFolio f on p.ID = f.PortfolioID
            //                    where p.IsActive = b'1' and f.IsActive = b'1';";

            string statement = @"select p.ID, p.Portfolio, p.InvestorPAN, p.IsActive, p.Description,
                                    f.ID FolioID, f.FolioNumber, f.FundHouse, f.DateOpened, f.DefaultBank, f.DefaultAccountNumber, f.Email, f.MobileNumber, f.IsActive,
                                    ga.GoalID, ga.Percent,
                                    g.GoalName, g.StartDate, g.EndDate, g.MonthlyInvestment, g.Amount, g.SIPDuration, g.TargetDuration, g.TargetAmount, g.ExpectedInflation
                                    from Portfolio p
                                    left join FundFolio f on p.ID = f.PortfolioID
                                    left join GoalAllocation ga on f.FolioNumber = ga.FolioNumber and ga.IsActive = b'1'
                                    left join goal g on g.GoalID = ga.GoalID and g.IsActive = b'1'
                                    where p.IsActive = b'1' and f.IsActive = b'1';";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetFundNAV(string fundHouse, int schemaCode, DateTime date)
        {
            string statement = @"select n.FundHouse, n.SchemaCode, n.NAVDate, n.FundHouse, n.SchemaName, n.FundOption, n.Category, n.NAV
                                    from FundsNAV n
                                    where n.SchemaCode = @schemaCode and n.NAVDate = @navDate and n.FundHouse = @fundHouse
                                    UNION ALL
                                    select n.FundHouse, n.SchemaCode, h.NAVDate, n.FundHouse, n.SchemaName, n.FundOption, n.Category, h.NAV
                                    from FundsNAVHistory h
                                    inner join FundsNAV n on n.SchemaCode = h.SchemaCode and n.SchemaCode = @schemaCode and h.NAVDate = @navDate and n.FundHouse = @fundHouse;";

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            parameters.Add(new MySqlParameter() { ParameterName = "@schemaCode", Value = schemaCode, DbType = DbType.Int32 });
            parameters.Add(new MySqlParameter() { ParameterName = "@navDate", Value = date.Date, DbType = DbType.Date });
            parameters.Add(new MySqlParameter() { ParameterName = "@fundHouse", Value = fundHouse, DbType = DbType.String });

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters);
        }

        #endregion

        #region new region

        

        #endregion
    }
}
