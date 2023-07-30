using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Entities.Models.DTO.Funds;
using Microsoft.AspNetCore.Mvc;

namespace Home.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private IHomeService _iHomeService;
        public HomeController(IHomeService iHomeService)
        {
            _iHomeService = iHomeService;
        }

        [HttpPost("SaveTransactions")]
        public async Task<IActionResult> SaveTransactions(DateTime minDate)
        {
            return Ok(await _iHomeService.SaveTransactions(minDate));
        }

        [HttpGet("GetTransactions")]
        public async Task<IActionResult> GetTransactions(DateTime fromDate, DateTime toDate)
        {
            return Ok(await _iHomeService.GetTransactions(fromDate, toDate));
        }
    }
}

