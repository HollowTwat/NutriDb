﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PayModel;
using SixLabors.ImageSharp.Drawing;
using System;
using System.IO;
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
                SuccessPayRequest cl = _subscriptionHelper.ConvertToPayRequestJSON(ress2);
                await ErrorHelper.SendSystemMess($"Success:{Newtonsoft.Json.JsonConvert.SerializeObject(cl)}");
                int userId = -1;
                if (cl.Data["id"] != null)
                {
                    var userIdStr = cl.Data["id"].ToString();
                    int.TryParse(userIdStr, out userId);
                }
                _context.Subscriptions.Add(new Subscription
                {
                    TransactionId = cl.TransactionId,
                    Amount = cl.Amount,
                    DateTime = cl.DateTime,
                    Status = cl.Status,
                    InvoiceId = cl.InvoiceId,
                    AccountId = cl.AccountId,
                    SubscriptionId = cl.SubscriptionId,
                    Email = cl.Email,
                    Rrn = cl.Rrn,
                    UserId = userId,
                    IsActive = true,
                    Extra = Newtonsoft.Json.JsonConvert.SerializeObject(cl)
                });
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
                FailPayRequest cl = _subscriptionHelper.ConvertToFailRequestJSON(ress2);
                await ErrorHelper.SendSystemMess($"Fail:{Newtonsoft.Json.JsonConvert.SerializeObject(cl)}");
                await ErrorHelper.SendSystemMess(Newtonsoft.Json.JsonConvert.SerializeObject(cl));
                int userId = -1;
                if (cl.Data["id"] != null)
                {
                    var userIdStr = cl.Data["id"].ToString();
                    int.TryParse(userIdStr, out userId);
                }
                _context.Subscriptions.Add(new Subscription
                {
                    TransactionId = cl.TransactionId,
                    Amount = cl.Amount,
                    DateTime = cl.DateTime,
                    InvoiceId = cl.InvoiceId,
                    AccountId = cl.AccountId,
                    SubscriptionId = cl.SubscriptionId,
                    Email = cl.Email,
                    Rrn = cl.Rrn,
                    UserId = userId,
                    IsActive = true,
                    Extra = Newtonsoft.Json.JsonConvert.SerializeObject(cl)
                });
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