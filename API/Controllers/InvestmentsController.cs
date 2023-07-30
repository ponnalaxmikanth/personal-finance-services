using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Entities.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private IInvestmentsService _investmentsService;

        public InvestmentsController(IInvestmentsService investmentsService)
        {
            _investmentsService = investmentsService;
        }

        [HttpGet("GetFundsNAV")]
        public async Task<IActionResult> GetFundsNAV(int SchemaCode)
        {
            var nav = await _investmentsService.GetFundsNAVData(SchemaCode);
            return Ok(nav);
        }

        [HttpGet("DownloadFundsNAV")]
        public async Task<IActionResult> DownloadFundsNAV()
        {
            var result = await _investmentsService.DownloadFundsNAV();
            return Ok(result);
        }

        [HttpGet("GoalsInfo")]
        public async Task<IActionResult> GoalsInfo(int portfolioID = -1)
        {
            DateTime fromDate = new DateTime(2008, 01, 01);
            DateTime toDate = DateTime.Now.Date;
            var result = await _investmentsService.GoalsInfo(fromDate, toDate, portfolioID);
            return Ok(result);
        }

        [HttpGet("Goals")]
        public async Task<IActionResult> Goals()
        {
            var result = await _investmentsService.Goals();
            return Ok(result);
        }

        [HttpGet("GoalAllocations")]
        public async Task<IActionResult> GoalAllocations()
        {
            var result = await _investmentsService.GoalAllocations();
            return Ok(result);
        }

        [HttpGet("PortfolioData")]
        public async Task<IActionResult> PortfolioData(DateTime fromDate, DateTime toDate, int portfolioID = -1, bool groupTransactions = false)
        {
            var result = await _investmentsService.PortfolioData(fromDate, toDate, portfolioID, groupTransactions);
            return Ok(result);
        }

        [HttpGet("FundsDailyTrack")]
        public async Task<IActionResult> FundsDailyTrack(DateTime fromDate, DateTime toDate, int portfolioID = -1)
        {
            var result = await _investmentsService.FundsDailyTrack(fromDate, toDate, portfolioID);
            return Ok(result);
        }

        [HttpGet("CurrentMonthTracker")]
        public async Task<IActionResult> CurrentMonthTracker(DateTime date, int portfolioID = -1)
        {
            var result = await _investmentsService.CurrentMonthTracker(portfolioID, date);
            return Ok(result);
        }

        [HttpGet("NAVDownloadDetails")]
        public async Task<IActionResult> NAVDownloadDetails()
        {
            var result = await _investmentsService.NAVDownloadDetails();
            return Ok(result);
        }
    }
}
