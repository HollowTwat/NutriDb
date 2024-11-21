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

                    await _context.Subscriptions.AddAsync(new Subscription
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
                        UserTgId = cl.CustomFields.First().ID,
                        IsActive = true,
                        DateCreate = DateTime.UtcNow.ToLocalTime().AddHours(3),
                        DateUpdate = DateTime.UtcNow.ToLocalTime().AddHours(3),
                        Extra = Newtonsoft.Json.JsonConvert.SerializeObject(cl)
                    });
                    var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == cl.CustomFields.First().ID);
                    if (user != null)
                    {
                        user.IsActive = true;
                        _context.Users.Update(user);
                    }
                    else
                    {
                        await ErrorHelper.SendSystemMess($"Пришел платеж без пользователя {Newtonsoft.Json.JsonConvert.SerializeObject(cl)}");
                    }
                    await _context.SaveChangesAsync();
                    var noti = await _subscriptionHelper.SendPayNoti(user.TgId);
                    if (!noti)
                        await ErrorHelper.SendSystemMess($"Не смогли отправить пользователю {user.TgId} ссылку на бота после оплаты");
                }

                return new SubResponse { code = 0 };
            }
            catch (Exception ex) { await ErrorHelper.SendErrorMess("Упали при оформлении подписки", ex); return new SubResponse { code = 500 }; }
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
                RecurrentRequest cl = _subscriptionHelper.ConvertToReqRequestJSON(ress2);
                await ErrorHelper.SendSystemMess($"Recurent:{Newtonsoft.Json.JsonConvert.SerializeObject(cl)}");
                var sub = _context.Subscriptions.Where(x => x.SubscriptionId == cl.Id);
                if (sub?.Count() == 1)
                    await ErrorHelper.SendSystemMess($"Отменить подписку пользователя:{sub.First().UserTgId}");
                if (sub?.Count() > 1)
                    await ErrorHelper.SendSystemMess($"Больше 1 подписки пользователя:{sub.First().UserTgId}");
                if (sub?.Count() == 0)
                    await ErrorHelper.SendSystemMess($"Отмена несуществующей подписки пользователя:{sub.First().UserTgId}");

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
