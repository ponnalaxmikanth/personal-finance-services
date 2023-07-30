using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("BSE")]
    public class BSESensexController : ControllerBase
    {
        private IBSEIndicesService _bseSensexService;

        public BSESensexController(IBSEIndicesService bseSensexService)
        {
            _bseSensexService = bseSensexService;
        }

        [HttpGet("DownloadIndices")]
        public async Task<IActionResult> DownloadIndicesInfo()
        {
            var response = await _bseSensexService.DownloadIndicesInfo();
            return Ok(response);
        }

        [HttpGet("DownloadIndicesData")]
        public async Task<IActionResult> DownloadIndicesData(DateTime fromDate, DateTime toDate)
        {
            var response = await _bseSensexService.DownloadIndicesData(fromDate, toDate);
            return Ok(response);
        }

        [HttpGet("GetPerformance")]
        public async Task<IActionResult> GetPerformance() {
            return Ok(await _bseSensexService.GetPerformance());
        }
    }
}
