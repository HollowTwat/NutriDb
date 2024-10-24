﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Exceptions;
using NutriDbService.Helpers;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
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

        public TransmitterController(railwayContext context, TransmitterHelper transmitterHelper, ILogger<TransmitterController> logger, MealHelper mealHelper)
        {
            _context = context;
            _transmitterHelper = transmitterHelper;
            _logger = logger;
            _mealHelper = mealHelper;
        }

        [HttpPost]
        public async Task<CreateGPTResponse> CreateGPTRequset(CreateGPTNoCodeRequest req)
        {
            try
            {
                _logger.LogWarning($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                var res = await _transmitterHelper.CreateGPTRequest(req);
                if (res == 0)
                    return new CreateGPTResponse { isError = true, RequestId = 0 };
                else
                    return new CreateGPTResponse(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                ErrorHelper.SendErrorMess("CreateGPTRequset Error", ex);
                ErrorHelper.SendErrorMess($"Input:{req}");
                return new CreateGPTResponse() { isError = true };
            }
        }

        [HttpPost]
        public async Task<CreateGPTResponse> CreateGPTLongRateRequset(RateRequest rateReq)
        {
            try
            {
                _logger.LogWarning($"На вход пришло {Newtonsoft.Json.JsonConvert.SerializeObject(rateReq)}");

                var req = _transmitterHelper.CreateRateRequest(rateReq, _mealHelper);

                var res = await _transmitterHelper.CreateGPTRequest(req);
                if (res == 0)
                    return new CreateGPTResponse { isError = true, RequestId = 0 };
                return new CreateGPTResponse(res);
            }
            catch (EmptyMealException ex)
            {
                _logger.LogError("Попытка анализа пустой недели ");
                ErrorHelper.SendErrorMess($"Попытка анализа пустой недели :{rateReq}");
                return new CreateGPTResponse() { isError = true, Mess = "MealEmpty" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                ErrorHelper.SendErrorMess("CreateGPTLongRateRequset Error", ex);
                ErrorHelper.SendErrorMess($"Input:{rateReq}");
                return new CreateGPTResponse() { isError = true };
            }
        }

        [HttpPost]
        public CheckGPTResponse CheckGPTStatus(CheckGPTRequest req)
        {
            try
            {
                if (req.RequestId == 0)
                    return new CheckGPTResponse { Done = true, IsError = true };
                var res = _transmitterHelper.CheckGPT(req.RequestId);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                ErrorHelper.SendErrorMess("CheckGPTStatus Error", ex);
                ErrorHelper.SendErrorMess($"Input:{req}");
                return new CheckGPTResponse() { IsError = true };
            }
        }
    }
}
