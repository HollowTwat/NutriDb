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

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class TransmitterController : Controller
    {
        private readonly ILogger<TransmitterController> _logger;
        private railwayContext _context;
        private TransmitterHelper _transmitterHelper;
        private MealHelper _mealHelper;
        private readonly ConcurrentDictionary<long, bool> _userStatus = new ConcurrentDictionary<long, bool>();
        private static readonly object locker = new object();
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public TransmitterController(railwayContext context, TransmitterHelper transmitterHelper, ILogger<TransmitterController> logger, MealHelper mealHelper)
        {
            _context = context;
            _transmitterHelper = transmitterHelper;
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
        public async Task StartMethod(long userId)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (_userStatus.TryGetValue(userId, out bool value) && value)
                {
                    await ErrorHelper.SendErrorMess($"Doublicate");
                    throw new DoubleUserException();
                }

                _userStatus[userId] = true;
                await ErrorHelper.SendErrorMess($"user{userId} Start");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task FinishMethod(long userId)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (_userStatus.ContainsKey(userId))
                {
                    _userStatus[userId] = false;
                }

                await ErrorHelper.SendErrorMess($"user{userId} Finish");
            }
            finally
            {
                semaphoreSlim.Release();
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
                _logger.LogWarning($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                await StartMethod(req.UserTgId);
                var res = await _transmitterHelper.CreateGPTRequest(req);
                if (res == 0)
                    return new CreateGPTResponse { isError = true, RequestId = 0 };
                else
                    return new CreateGPTResponse(res);
            }
            catch (DoubleUserException e)
            {
                await ErrorHelper.SendErrorMess($"Double for user:{req.UserTgId}");
                return new CreateGPTResponse() { isError = true, Mess = "Double" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("CreateGPTRequset Error", ex);
                await ErrorHelper.SendErrorMess($"Input:{Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                return new CreateGPTResponse() { isError = true };
            }
            finally
            {
                await FinishMethod(req.UserTgId);
            }
        }

        [HttpPost]
        public async Task<CreateGPTResponse> CreateGPTLongRateRequset(RateRequest rateReq)
        {
            try
            {
                _logger.LogWarning($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
             await   StartMethod(rateReq.UserTgId);
                var req = await _transmitterHelper.CreateRateRequest(rateReq, _mealHelper);

                var res = await _transmitterHelper.CreateGPTRequest(req);
                if (res == 0)
                    return new CreateGPTResponse { isError = true, RequestId = 0 };
                return new CreateGPTResponse(res);
            }
            catch (EmptyMealException ex)
            {
                _logger.LogError("Попытка анализа пустой недели ");
                await ErrorHelper.SendErrorMess($"Попытка анализа пустой недели :{Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
                return new CreateGPTResponse() { isError = true, Mess = "MealEmpty" };
            }
            catch (ExtraEmptyException ex)
            {
                _logger.LogError("Попытка анализа с пустой анкетой");
                await ErrorHelper.SendErrorMess($"Попытка анализа с пустой анкетой :{Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");
                return new CreateGPTResponse() { isError = true, Mess = "ExtraEmpty" };
            }
            catch (DoubleUserException e)
            {
                await ErrorHelper.SendErrorMess($"Double for user:{rateReq.UserTgId}");
                return new CreateGPTResponse() { isError = true, Mess = "Double" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("CreateGPTLongRateRequset Error", ex);
                await ErrorHelper.SendErrorMess($"Input:{rateReq}");
                return new CreateGPTResponse() { isError = true };
            }
            finally
            {
            await    FinishMethod(rateReq.UserTgId);
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
