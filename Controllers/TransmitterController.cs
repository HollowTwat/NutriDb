using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.NoCodeModels;
using NutriDbService.PythModels.Response;
using System;
using System.Threading;

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class TransmitterController : Controller
    {
        private readonly ILogger<TransmitterController> _logger;
        private railwayContext _context;
        private TransmitterHelper _transmitterHelper;
        public TransmitterController(railwayContext context, TransmitterHelper transmitterHelper, ILogger<TransmitterController> logger)
        {
            _context = context;
            _transmitterHelper = transmitterHelper;
            _logger = logger;
        }

        [HttpPost]
        public CreateGPTResponse CreateGPTRequset(CreateGPTRequest req)
        {
            try
            {
                var res = _transmitterHelper.CreateGPTRequest(req).GetAwaiter().GetResult();
                return new CreateGPTResponse(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new CreateGPTResponse() { isError = true };
            }
        }

        [HttpPost]
        public CheckGPTResponse CheckGPTStatus(CheckGPTRequest req)
        {
            var res = _transmitterHelper.CheckGPT(req.RequestId);
            return res;
        }

        [HttpPost]
        public string Test(string aa)
        {
            return _transmitterHelper.Test(aa);
        }
        public string Testinner(string aa)
        {
            return _transmitterHelper.TestInner(aa);

        }
    }
}
