using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Entities.Models.DTO;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Investments.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvestmentsController : ControllerBase
    {
        
        private IInvestmentsService _investmentsService;

        public InvestmentsController(IInvestmentsService fundsNAVService)
        {
            _investmentsService = fundsNAVService;
        }

        [HttpGet("DownloadFundsNAV")]
        public async Task<IActionResult> DownloadFundsNAV()
        {
            var result = await _investmentsService.DownloadFundsNAV();
            return Ok(new ServiceResponse<string>(true, "Success", "Success"));
        }
    }
}
