using System;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StocksController : ControllerBase
    {
        private IStocksService _iStocksService;

        public StocksController(IStocksService iStocksService)
        {
            _iStocksService = iStocksService;
        }

        [HttpGet("CurrentStocks")]
        public async Task<IActionResult> CurrentStocks(int portfolioID = -1)
        {
            var result = await _iStocksService.GetCurrentStocks(portfolioID);
            return Ok(result);
        }
    }
}
