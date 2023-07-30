using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;

namespace DataAccess.Interfaces
{
    public interface IInitializeFundsDataAccess
    {
        Task<ServiceResponse<int>> SaveFundsTransactions(List<FundTransaction> fundTransactions);
        
        Task<ServiceResponse<int>> SaveFundRedeems(List<FundRedeems> fundRedeems);
        Task<ServiceResponse<int>> SaveFundSwitches(List<FundSwitchIn> fundSwitchIns);

        Task<ServiceResponse<int>> SaveFundsTransactionsHistory(List<Transaction> fundTransactions);
        Task<ServiceResponse<int>> SaveFundDividends(List<Transaction> Investments);
    }
}
