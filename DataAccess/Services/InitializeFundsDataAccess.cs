using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DataAccess.Services
{
    public class InitializeFundsDataAccess : IInitializeFundsDataAccess
    {
        private readonly ISQLHelper _sQLHelper;
        private readonly ILogger<InitializeFundsDataAccess> _logger;

        public InitializeFundsDataAccess(ILogger<InitializeFundsDataAccess> logger, ISQLHelper sQLHelper)
        {
            _sQLHelper = sQLHelper;
            _logger = logger;
        }

        public async Task<ServiceResponse<int>> SaveFundsTransactions(List<FundTransaction> fundTransactions)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "fundTransactions", Value = JsonConvert.SerializeObject(fundTransactions), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("InsertFundTransactions", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == fundTransactions.Count)
                    return new ServiceResponse<int>(true, "Success while uploading data", fundTransactions.Count);
                else
                    new ServiceResponse<int>(false, "Error while uploading data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundsTransactions failed to Initialize Fund Transactions: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading data", default);
        }


        

        

        public async Task<ServiceResponse<int>> SaveFundRedeems(List<FundRedeems> fundRedeems)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "redeems", Value = JsonConvert.SerializeObject(fundRedeems), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("AddFundRedeems", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == fundRedeems.Count)
                    return new ServiceResponse<int>(true, "Redeems data saved successfully", fundRedeems.Count);
                else
                    new ServiceResponse<int>(false, "Error while saving Redeem data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundRedeems failed to uploading Redeem data: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading Redeem data", default);
        }

        public async Task<ServiceResponse<int>> SaveFundSwitches(List<FundSwitchIn> fundSwitchIns)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "transactions", Value = JsonConvert.SerializeObject(fundSwitchIns), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("AddFundSwitches", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == fundSwitchIns.Count)
                    return new ServiceResponse<int>(true, "switches data saved successfully", fundSwitchIns.Count);
                else
                    new ServiceResponse<int>(false, "Error while saving switches data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundSwitches failed to uploading switches data: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading switches data", default);
        }



        public async Task<ServiceResponse<int>> SaveFundsTransactionsHistory(List<Transaction> fundTransactions)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "fundTransactions", Value = JsonConvert.SerializeObject(fundTransactions), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("InsertFundTransactionsHistory", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == fundTransactions.Count)
                    return new ServiceResponse<int>(true, "Success while uploading data", fundTransactions.Count);
                else
                    new ServiceResponse<int>(false, "Error while uploading data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundsTransactions failed to Initialize Fund Transactions History: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading data", default);
        }

        public async Task<ServiceResponse<int>> SaveFundDividends(List<Transaction> Investments)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "dividend", Value = JsonConvert.SerializeObject(Investments), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("AddFundDividend", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == Investments.Count)
                    return new ServiceResponse<int>(true, "Dividend data saved successfully", Investments.Count);
                else
                    new ServiceResponse<int>(false, "Error while saving dividend data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveFundDividends failed to uploading dividend data: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading dividend data", default);
        }
    }
}
