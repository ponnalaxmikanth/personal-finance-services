using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Entities.Models.DTO.Funds;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FundsController : ControllerBase
    {
        private IInvestmentsService _investmentsService;
        private IMutualFundsService _iMutualFundsService;

        public FundsController(IInvestmentsService investmentsService, IMutualFundsService iMutualFundsService)
        {
            _investmentsService = investmentsService;
            _iMutualFundsService = iMutualFundsService;
        }

        [HttpPost("AddFundsTransaction")]
        public async Task<IActionResult> AddFundsTransaction(MutualFundTransaction request) {
            return Ok(await _investmentsService.AddFundsTransaction(request));
        }

        [HttpPost("UpdateFundDividend")]
        public async Task<IActionResult> UpdateFundDividend(FundDividend request)
        {
            return Ok(await _investmentsService.UpdateFundDividend(request));
        }

        [HttpPost("RedeemFund")]
        public async Task<IActionResult> RedeemFund(MutualFundTransaction request)
        {
            return Ok(await _investmentsService.RedeemFund(request));
        }

        //[HttpPost("AddFundDividend")]
        //public async Task<IActionResult> AddFundDividend(FundDividend request) {
        //    return Ok(await _investmentsService.AddFundDividend(request));
        //}

        [HttpPost("GetHistory")]
        public async Task<IActionResult> GetHistory(HistoryRequest request) {
            return Ok(await _investmentsService.GetHistory(request));
        }

        //[HttpGet("GetFundTransactions")]
        //public async Task<IActionResult> GetFundTransactions(int portfolioID = -1) {
        //    return Ok(await _iMutualFundsService.GetFundTransactions(portfolioID));
        //}


        //[HttpGet("GetConsolidatedValuation")]
        //public async Task<IActionResult> GetConsolidatedValuation(int portfolioID = -1) {
        //    return Ok(await _iMutualFundsService.GetConsolidatedValuation(portfolioID));
        //}

        //[HttpGet("GetFinancialYearValuation")]
        //public async Task<IActionResult> GetFinancialYearValuation(int portfolioID = -1) {
        //    return Ok(await _iMutualFundsService.GetFinancialYearValuation(portfolioID));
        //}

        [HttpGet("GetCategoryValuation")]
        public async Task<IActionResult> GetCategoryValuation(int portfolioID = -1)
        {
            return Ok(await _iMutualFundsService.GetCategoryValuation(portfolioID));
        }

        [HttpGet("GetSwitcInValuation")]
        public async Task<IActionResult> GetSwitcInValuation(int portfolioID = -1) {
            return Ok(await _iMutualFundsService.GetSwitcInValuation(portfolioID));
        }

        [HttpGet("GetPerformance")]
        public async Task<IActionResult> GetPerformance() {
            return Ok(await _iMutualFundsService.GetPerformance());
        }

        [HttpGet("GetCategoryPerformance")]
        public async Task<IActionResult> GetCategoryPerformance(string classification, string category)
        {
            return Ok(await _iMutualFundsService.GetCategoryFundsPerformance(classification, category));
        }


        [HttpGet("GetPortfolios")]
        public async Task<IActionResult> GetPortfolios() {
            return Ok(await _iMutualFundsService.GetPortfolios());
        }

        [HttpGet("GetFundFolios")]
        public async Task<IActionResult> GetFundFolios() {
            return Ok(await _iMutualFundsService.GetFundFolios());
        }

        [HttpGet("GetFundNAV")]
        public async Task<IActionResult> GetFundNAV(string fundHouse, int schemaCode, DateTime date) {
            return Ok(await _iMutualFundsService.GetFundNAV(fundHouse, schemaCode, date));
        }

        [HttpGet("GetFundTransactions")]
        public async Task<IActionResult> GetFundTransactions(int portfolioID, int schemaCode)
        {
            return Ok(await _iMutualFundsService.GetFundTransactions(portfolioID, schemaCode));
        }

        [HttpGet("GoalsReview")]
        public async Task<IActionResult> GoalsReview(int portfolioID)
        {
            return Ok(await _iMutualFundsService.GoalsReview(portfolioID));
        }



        //[HttpGet("PortfolioData")]
        //public async Task<IActionResult> PortfolioData(int portfolioID = -1)
        //{
        //    var result = await _iMutualFundsService.PortfolioData(portfolioID);
        //    return Ok(result);
        //}

    }
}
