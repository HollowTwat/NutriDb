using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PayModel;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SubscriptionController : Controller
    {
        private readonly ILogger<SubscriptionController> _logger;
        private railwayContext _context;
        private SubscriptionHelper _subscriptionHelper;

        public SubscriptionController(railwayContext context, ILogger<SubscriptionController> logger, SubscriptionHelper subscriptionHelper)
        {
            _context = context;
            _logger = logger;
            _subscriptionHelper = subscriptionHelper;
        }

        [HttpPost]
        public async Task<SubResponse> SuccessPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Success {ress2}");
                var cl = _subscriptionHelper.ConvertToPayRequestJSON(ress2);
                await ErrorHelper.SendSystemMess(Newtonsoft.Json.JsonConvert.SerializeObject(cl));

            }

            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> FailPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Fail: {ress2}");

            }

            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> CancelPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Cancel: {ress2}");

            }

            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> SuccessPay2(SuccessPayRequest input)
        {
            var ress = Newtonsoft.Json.JsonConvert.SerializeObject(input);
            _logger.LogWarning(ress);
            await ErrorHelper.SendSystemMess(ress);
            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> SuccessPay3(string input)
        {

            _logger.LogWarning(input);
            await ErrorHelper.SendSystemMess(input);
            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> RecurrentPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Recurent: {ress2}");

            }
            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> ConfirmPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Confirm: {ress2}");

            }
            return new SubResponse { code = 0 };
        }

        [HttpPost]
        public async Task<SubResponse> RefundPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string bodyContent = await reader.ReadToEndAsync();
                var ress2 = HttpUtility.UrlDecode(bodyContent);
                _logger.LogWarning(ress2);
                await ErrorHelper.SendSystemMess($"Refund: {ress2}");

            }

            return new SubResponse { code = 0 };
        }
    }
}
