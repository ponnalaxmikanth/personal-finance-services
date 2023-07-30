using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Microsoft.Extensions.Logging;

namespace BusinessAccess.Interfaces
{
    public interface IInvestmentsService
    {
        public Task<ServiceResponse<int>> DownloadFundsNAV();

        public Task<ServiceResponse<FundsNav>> GetFundsNAVData(int schemaCode);

        public Task<ServiceResponse<int>> AddFundsTransaction(MutualFundTransaction request);
        public Task<ServiceResponse<int>> UpdateFundDividend(FundDividend request);
        public Task<ServiceResponse<int>> RedeemFund(MutualFundTransaction request);

        public Task<ServiceResponse<int>> GetHistory(HistoryRequest request);

        public Task<ServiceResponse<int>> InitializeFundTransactions(string path);
        public Task<ServiceResponse<GoalsInfo>> GoalsInfo(DateTime fromDate, DateTime toDate, int portfolioID);

        public Task<ServiceResponse<List<Goal>>> Goals();
        public Task<ServiceResponse<List<GoalAllocation>>> GoalAllocations();

        public Task<ServiceResponse<PortfolioData>> PortfolioData(DateTime fromDate, DateTime toDate, int portfolioID, bool groupTransactions);
        public Task<ServiceResponse<List<CurrentMonthTracker>>> CurrentMonthTracker(int portfolioID, DateTime date);
        public Task<ServiceResponse<List<FundsDailyTrack>>> FundsDailyTrack(DateTime fromDate, DateTime toDate, int portfolioID);

        public Task<ServiceResponse<NAVDownloadDetails>> NAVDownloadDetails();

    }
}
