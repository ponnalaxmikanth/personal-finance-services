using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Home;
using Entities.Models.DTO.India;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Services
{
    public class HomeDataAccess: IHomeDataAccess
    {
        private readonly ILogger<IHomeDataAccess> _logger;
        private readonly ISQLHelper _sQLHelper;

        public HomeDataAccess(ILogger<IHomeDataAccess> logger, ISQLHelper sQLHelper)
        {
            _logger = logger;
            _sQLHelper = sQLHelper;
        }

        public async Task<ServiceResponse<DataTable>> GetAccounts()
        {
            _logger.LogInformation($"HomeDataAccess--GetAccounts");
            try
            {
                string statement = @"select AccountID, AccountType, AccountName, ExcelMapping, IsActive from Home.Accounts";

                return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null, "Home");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"HomeDataAccess--GetAccounts: {ex.Message}");
            }
            return new ServiceResponse<DataTable>(false, "Get Accounts Failed", default);
        }

        public async Task<ServiceResponse<DataTable>> GetBudgets(DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation($"HomeDataAccess--GetBudgets");
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "@fromDate", Value = fromDate.Date, DbType = DbType.Date });
                parameters.Add(new MySqlParameter() { ParameterName = "@toDate", Value = toDate.Date, DbType = DbType.Date });
                string statement = @"select ID, FromDate, ToDate, `Group`, SubGroup, Amount, IsActive 
                                        from Home.Budget
                                    where @fromDate between FromDate and ToDate;";

                return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters, "Home");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"HomeDataAccess--GetBudgets: {ex.Message}");
            }
            return new ServiceResponse<DataTable>(false, "Get Budgets Failed", default);
        }

        public async Task<ServiceResponse<int>> SaveTransactions(List<Transaction> transactions)
        {
            //throw new NotImplementedException();
            _logger.LogInformation($"SaveTransactions total records: {transactions.Count}");
            try
            {
                int saveCount = 0;
                var batchSize = 1000;
                int batchCount = (int)Math.Ceiling((double)transactions.Count() / batchSize);
                for (int i = 0; i < batchCount; i++)
                {
                    List<MySqlParameter> parameters = new List<MySqlParameter>();
                    var trans = transactions.Skip(i * batchSize).Take(batchSize);

                    var strtrans = JsonConvert.SerializeObject(trans);
                    parameters.Add(new MySqlParameter() { ParameterName = "transactions", Value = JsonConvert.SerializeObject(trans), DbType = DbType.String });

                    var result = await _sQLHelper.ExecuteScalar("SaveTransactions", CommandType.StoredProcedure, parameters, "Home");
                    saveCount += result.ResponseObject;
                }

                if (saveCount == transactions.Count)
                    return new ServiceResponse<int>(true, "Successfully Updated", saveCount);
                else
                    return new ServiceResponse<int>(false, "Failed to Update", saveCount);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"failed to Update Transactions: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Save", 0);
        }

        public async Task<ServiceResponse<DataTable>> GetTransactions(DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation($"HomeDataAccess--GetTransactions");
            try
            {
                //string statement = @"select a.AccountType, a.AccountName,
                //        t.TransactionDate, t.Description, t.Debit, t.Credit, t.Total, 
                //        case when t.Group is null then 'Un-Category' else t.Group END as `Group`, 
                //        case when t.SubGroup is null then 'Un-Category' else t.SubGroup END as `SubGroup`, 
                //        t.TransactedBy, t.Store, t.Comments
                //            from Home.Transactions t
                //            inner join Home.Accounts a on a.AccountID = t.AccountID
                //        where t.TransactionDate between @fromDate and @toDate";
                string statement = @"select 
                    COALESCE(COALESCE(t.Group,b.Group),'Un-Category') as `Group`, IFNULL(IFNULL(t.SubGroup,b.SubGroup),'Un-Category') as `SubGroup`,
                    COALESCE(t.Debit, 0) Debit, COALESCE(t.Credit, 0) Credit, COALESCE(b.Amount, 0) as Budget,
                    t.*
                    from Home.Transactions t 
                    left outer join Home.Budget b on b.Group = t.Group and b.SubGroup = t.SubGroup
                        and b.FromDate >= @fromDate and b.ToDate <= @toDate
                    where t.TransactionDate between @fromDate and @toDate";

                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "@fromDate", Value = fromDate.Date, DbType = DbType.Date });
                parameters.Add(new MySqlParameter() { ParameterName = "@toDate", Value = toDate.Date, DbType = DbType.Date });

                return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, parameters, "Home");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"HomeDataAccess--GetTransactions: {ex.Message}");
            }
            return new ServiceResponse<DataTable>(false, "Get Transactions Failed", default);
        }

    }
}
