using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsApiController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("pipeline")]
        public async Task<IActionResult> GetPipelineReport()
        {
            var data = await _reportService.GetPipelineReportAsync();
            return Ok(data);
        }
    }
}

