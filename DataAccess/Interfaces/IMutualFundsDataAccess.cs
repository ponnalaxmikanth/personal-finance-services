using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;

namespace DataAccess.Interfaces
{
    public interface IMutualFundsDataAccess
    {
        #region old implementation

        Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID);
        Task<ServiceResponse<DataTable>> GetFundTransactions(int portfolioID, int schemaCode);

        //Task<ServiceResponse<DataTable>> GetCategoryValuation(int portfolioID);

        //Task<ServiceResponse<DataTable>> GetFinancialYearValuation(int portfolioID);

        //Task<ServiceResponse<DataTable>> GetConsolidatedValuation(int portfolioID);
        Task<ServiceResponse<DataTable>> GetSwitcInValuation(int portfolioID);

        Task<ServiceResponse<int>> SaveFundsDailyTracker(Consolidated consolidated);
        Task<ServiceResponse<DataTable>> GetCurrentValues();
        Task<ServiceResponse<DataTable>> GetCurrentValues(string classification, string category);

        Task<ServiceResponse<DataTable>> GetPerformance(int period);
        Task<ServiceResponse<DataTable>> GetCategoryPerformance(int period, string classification, string category);

        Task<ServiceResponse<DataTable>> GetPortfolios();
        Task<ServiceResponse<DataTable>> GetFundFolios();

        Task<ServiceResponse<DataTable>> GetFundNAV(string fundHouse, int schemaCode, DateTime date);

        Task<ServiceResponse<DataTable>> GetLatestNAVDate(DateTime date);
        Task<ServiceResponse<DataTable>> GoalsReview(int portfolioID);

        //Task<ServiceResponse<int>> SaveQuarterlyGoalReview(List<GoalReview> goalReviews);

        //Task<ServiceResponse<DataTable>> GoalsInfo(int portfolioID);
        //Task<ServiceResponse<DataTable>> CurrentMonthGoals(int portfolioID);

        #endregion

        #region new region

        //Task<ServiceResponse<DataTable>> GetFundRedeems(int portfolioID);
        //Task<ServiceResponse<DataTable>> GetFundDividends(int portfolioID);
        //Task<ServiceResponse<DataTable>> GetFundinvestments(int portfolioID);

        #endregion
    }
}
