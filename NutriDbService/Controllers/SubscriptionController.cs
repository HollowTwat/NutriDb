using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PayModel;
using SixLabors.ImageSharp.Drawing;
using System;
using System.IO;
using System.Linq;
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
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    string bodyContent = await reader.ReadToEndAsync();
                    var ress2 = HttpUtility.UrlDecode(bodyContent);
                    _logger.LogWarning(ress2);
                    SuccessPayRequest cl = _subscriptionHelper.ConvertToPayRequestJSON(ress2);
                    await ErrorHelper.SendSystemMess($"Success:{Newtonsoft.Json.JsonConvert.SerializeObject(cl)}");

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
                        UserId = cl.CustomFields.First().Id,
                        IsActive = true,
                        DateCreate = DateTime.UtcNow.ToLocalTime().AddHours(3),
                        DateUpdate = DateTime.UtcNow.ToLocalTime().AddHours(3),
                        Extra = Newtonsoft.Json.JsonConvert.SerializeObject(cl)
                    });
                    await _context.SaveChangesAsync();
                }

                return new SubResponse { code = 0 };
            }
            catch (Exception ex) { ErrorHelper.SendErrorMess("Упали при оформлении подписки", ex); return new SubResponse { code = 0 }; }
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
                //await ErrorHelper.SendSystemMess(Newtonsoft.Json.JsonConvert.SerializeObject(cl));
                //var sub = await _context.Subscriptions.SingleOrDefaultAsync(x => x.SubscriptionId == cl.SubscriptionId);
                //if (sub != null)
                //{
                //    sub.IsActive = false;
                //    _context.Subscriptions.Update(sub);
                //    await _context.SaveChangesAsync();
                //}
                //else
                //    ErrorHelper.SendErrorMess("Ошибка падения оплаты");
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
