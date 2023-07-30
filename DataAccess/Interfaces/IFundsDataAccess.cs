using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;

namespace DataAccess.Interfaces
{
    public interface IInvestmentsDataAccess
    {
        Task<ServiceResponse<int>> SaveFundsNAV(List<FundsNav> latestNavData);
        Task<ServiceResponse<int>> BackupFundsNAV();
        Task<ServiceResponse<int>> AddFundsTransaction(MutualFundTransaction request);
        Task<ServiceResponse<int>> AddFundsTransactionHistory(MutualFundTransaction request, decimal profit = 0);
        Task<ServiceResponse<int>> AddFundDividend(List<FundDividend> request);
        Task<ServiceResponse<int>> UpdateTransactions(List<Transaction> request);
        Task<ServiceResponse<int>> SaveFundsNAVHistory(List<FundHistory> latestNavData);
        Task<ServiceResponse<DataTable>> GetFundTransactions(int schemaCode, string folioNumber);
        Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID, int schemaCode, string folioNumber);


        Task<ServiceResponse<DataTable>> Goals();
        Task<ServiceResponse<DataTable>> GoalAllocations();

        Task<ServiceResponse<DataTable>> GoalsInfo(DateTime fromDate, DateTime toDate, int portfolioID);
        
        Task<ServiceResponse<DataTable>> CurrentMonthGoals(DateTime fromDate, DateTime toDate, int portfolioID);

        Task<ServiceResponse<DataTable>> GetDailyTrackHistory(DateTime fromDate, DateTime toDate, int portfolioID);

        Task<ServiceResponse<DataTable>> CurrentMonthTracker(int portfolioID, DateTime date);

        Task<ServiceResponse<DataTable>> NAVDownloadDetails();

        Task<ServiceResponse<int>> SaveQuarterlyReview(DateTime dtLastDay, List<Goal> goals);

    }
}
