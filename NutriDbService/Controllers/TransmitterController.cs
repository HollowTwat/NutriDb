using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Exceptions;
using NutriDbService.Helpers;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class TransmitterController : Controller
    {
        private readonly ILogger<TransmitterController> _logger;
        private railwayContext _context;
        private TransmitterHelper _transmitterHelper;
        private SubscriptionHelper _subscriptionHelper;
        private MealHelper _mealHelper;
        private static ConcurrentDictionary<long, bool> _userStatus = new ConcurrentDictionary<long, bool>();
        private static readonly object locker = new object();
        //private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly int _errorTimeout = 1000;
        public TransmitterController(railwayContext context, TransmitterHelper transmitterHelper, SubscriptionHelper subscriptionHelper, ILogger<TransmitterController> logger, MealHelper mealHelper)
        {
            _context = context;
            _transmitterHelper = transmitterHelper;
            _subscriptionHelper = subscriptionHelper;
            _logger = logger;
            _mealHelper = mealHelper;
        }
        //public void StartMethod(long userId)
        //{
        //    lock (locker)
        //    {
        //        if (_userStatus.TryGetValue(userId, out bool value) && value)
        //        {
        //            ErrorHelper.SendErrorMess($"Doublicate").GetAwaiter().GetResult();
        //            throw new DoubleUserException();
        //        }

        //        // Удостоверяемся, что установка статуса происходит внутри блокировки
        //        _userStatus[userId] = true;
        //        ErrorHelper.SendErrorMess($"user{userId} Start").GetAwaiter().GetResult();
        //    }
        //}

        private bool StartMethod(long userId)
        {
            lock (locker)
            {
                System.Threading.Thread.Sleep(250);
                if (!_userStatus.ContainsKey(userId))
                    _userStatus[userId] = false;

                var isget = _userStatus.TryGetValue(userId, out bool isUserActive);
                if (isget && isUserActive)
                {
                    return false; // Установка провала
                }
                // Синхронно выполняем окончательную постобменную миссию на дальнейшую побуждение
                _userStatus[userId] = true;

                return true; // Занавес со сброс эмоции
            }
        }
        private void FinishMethod(long userId)
        {
            lock (locker)
            {
                if (_userStatus.ContainsKey(userId))
                {
                    if (_userStatus.TryGetValue(userId, out bool isActive) && isActive)
                    {
                        _userStatus[userId] = false;
                    }
                    else
                        ErrorHelper.SendErrorMess($"На финише в словаре false для: {userId}").GetAwaiter().GetResult();
                }
                else
                {
                    ErrorHelper.SendErrorMess($"На финише в словаре нет {userId}").GetAwaiter().GetResult();
                }
            }
        }
        //public void FinishMethod(long userId)
        //{
        //    ErrorHelper.SendErrorMess($"user{userId} Finish").GetAwaiter().GetResult();
        //    _userStatus[userId] = false;
        //}

        [HttpPost]
        public async Task<CreateGPTResponse> CreateGPTRequset(CreateGPTNoCodeRequest req)
        {
            try
            {
                _logger.LogInformation($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                bool sub = await _subscriptionHelper.CheckSub(req.UserTgId);
                if (!sub)
                    throw new SubscriptionException();
                if (!StartMethod(req.UserTgId))
                    throw new DoubleUserException();
                var res = await _transmitterHelper.CreateGPTRequest(req);
                System.Threading.Thread.Sleep(100);
                FinishMethod(req.UserTgId);
                if (res == 0)
                {
                    await ErrorHelper.SendErrorMess($"DbEmpty for user:{req.UserTgId}");
                    await Task.Delay(_errorTimeout);
                    return new CreateGPTResponse { isError = true, RequestId = 0, Mess = "DbEmpty" };
                }
                else
                    return new CreateGPTResponse(res);
            }
            catch (DoubleUserException e)
            {
                await ErrorHelper.SendErrorMess($"Double for user:{req.UserTgId}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "Double" };
            }
            catch (SubscriptionException e)
            {
                await ErrorHelper.SendErrorMess($"Нет подписки у пользователя:{req.UserTgId}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "SubsFail" };
            }
            catch (Exception ex)
            {
                FinishMethod(req.UserTgId);
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("CreateGPTRequset Error", ex);
                await ErrorHelper.SendErrorMess($"Input:{Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "Other" };
            }
        }

        [HttpPost]
        public async Task<CreateGPTResponse> CreateGPTLongRateRequset(RateRequest rateReq)
        {
            try
            {
                _logger.LogInformation($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
                bool sub = await _subscriptionHelper.CheckSub(rateReq.UserTgId);
                if (!sub)
                    throw new SubscriptionException();
                if (!StartMethod(rateReq.UserTgId))
                    throw new DoubleUserException();
                var req = await _transmitterHelper.CreateRateRequest(rateReq, _mealHelper);
                var res = await _transmitterHelper.CreateGPTRequest(req);
                System.Threading.Thread.Sleep(100);
                FinishMethod(rateReq.UserTgId);
                if (res == 0)
                {
                    await ErrorHelper.SendErrorMess($"DbEmpty for user:{rateReq.UserTgId}");
                    await Task.Delay(_errorTimeout);
                    return new CreateGPTResponse { isError = true, RequestId = 0, Mess = "DbEmpty" };
                }
                return new CreateGPTResponse(res);
            }
            catch (EmptyMealException ex)
            {
                FinishMethod(rateReq.UserTgId);
                _logger.LogError("Попытка анализа пустой недели ");
                await ErrorHelper.SendErrorMess($"Попытка анализа пустой недели :{Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "MealEmpty" };
            }
            catch (ExtraEmptyException ex)
            {
                FinishMethod(rateReq.UserTgId);
                _logger.LogError("Попытка анализа с пустой анкетой");
                await ErrorHelper.SendErrorMess($"Попытка анализа с пустой анкетой :{Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "ExtraEmpty" };
            }
            catch (SubscriptionException e)
            {
                await ErrorHelper.SendErrorMess($"Нет подписки у пользователя:{rateReq.UserTgId}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "SubsFail" };
            }
            catch (DoubleUserException e)
            {
                await ErrorHelper.SendErrorMess($"Double for user:{rateReq.UserTgId}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "Double" };
            }
            catch (Exception ex)
            {
                FinishMethod(rateReq.UserTgId);
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("CreateGPTLongRateRequset Error", ex);
                await ErrorHelper.SendErrorMess($"Input:{rateReq}");
                await Task.Delay(_errorTimeout);
                return new CreateGPTResponse() { isError = true, RequestId = 0, Mess = "Other" };
            }
        }

        [HttpPost]
        public async Task<CheckGPTResponse> CheckGPTStatus(CheckGPTRequest req)
        {
            try
            {
                if (req.RequestId == 0)
                    return new CheckGPTResponse { Done = true, IsError = true };
                var res = await _transmitterHelper.CheckGPT(req.RequestId);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("CheckGPTStatus Error", ex);
                await ErrorHelper.SendErrorMess($"Input:{req}");
                return new CheckGPTResponse() { IsError = true };
            }
        }
    }
}
