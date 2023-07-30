using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.Funds;
using Entities.Models.DTO.India;

namespace BusinessAccess.Interfaces
{
    public interface IMutualFundsService
    {
        #region old implementation
        //public Task<ServiceResponse<List<FundInvestment>>> GetFundTransactions(int portfolioID);

        //public Task<ServiceResponse<Consolidated>> GetConsolidatedValuation(int portfolioID);

        public Task<ServiceResponse<List<CategoryValuation>>> GetCategoryValuation(int portfolioID);

        //public Task<ServiceResponse<List<FinancialYearValuation>>> GetFinancialYearValuation(int portfolioID);

        public Task<ServiceResponse<List<FundSwitchInValuation>>> GetSwitcInValuation(int portfolioID);

        public Task<ServiceResponse<List<FundCagetoryPerformance>>> GetPerformance();

        public Task<ServiceResponse<List<CategoryFundsPerformance>>> GetCategoryFundsPerformance(string classification, string category);

        public Task<ServiceResponse<List<Portfolio>>> GetPortfolios();
        public Task<ServiceResponse<List<FundFolio>>> GetFundFolios();

        public Task<ServiceResponse<FundNAV>> GetFundNAV(string fundHouse, int schemaCode, DateTime date);
        public Task<ServiceResponse<List<FundInvestment>>> GetFundTransactions(int portfolioID, int schemaCode);

        //public Task<ServiceResponse<PortfolioData>> PortfolioData(int portfolioID);

        public Task<ServiceResponse<List<GoalReview>>> GoalsReview(int portfolioID);
        #endregion

        #region new implementation

        //public Task<ServiceResponse<FundInvestments>> GetFundRedeems(int portfolioID);

        //public Task<ServiceResponse<FundInvestments>> GetTransactions(int portfolioID);

        #endregion
    }
}
