using System;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FundsController : ControllerBase
    {
        private IMutualFundsService _iMutualFundsService;

        public FundsController(IMutualFundsService iMutualFundsService)
        {
            _iMutualFundsService = iMutualFundsService;
        }

        //[HttpGet("Redeems")]
        //public async Task<IActionResult> GetRedeems(int portfolioID = -1)
        //{
        //    var result = await _iMutualFundsService.GetFundRedeems(portfolioID);
        //    return Ok(result);
        //}

        //[HttpGet("Transactions")]
        //public async Task<IActionResult> GetTransactions(int portfolioID = -1)
        //{
        //    var result = await _iMutualFundsService.GetTransactions(portfolioID);
        //    return Ok(result);
        //}
    }
}
