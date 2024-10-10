using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NutriDbService.DbModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using SixLabors.Fonts.Unicode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NutriDbService.Helpers
{

    public class TransmitterHelper
    {
        private readonly string BaseUrl = "https://quart-test-production-9039.up.railway.app";
        public railwayContext _nutriDbContext { get; set; }
        private readonly IServiceScopeFactory _serviceProviderFactory;
        private readonly ILogger _logger;
        public TransmitterHelper(railwayContext nutriDbContext, IServiceScopeFactory serviceProviderFactory)
        {
            _nutriDbContext = nutriDbContext;
            _serviceProviderFactory = serviceProviderFactory;

            _logger = _serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<TransmitterHelper>>();
        }

        public async Task<int> CreateGPTRequest(CreateGPTNoCodeRequest request)
        {

            var req = new Gptrequest { Iserror = false, Request = JsonConvert.SerializeObject(request), Done = false, UserTgid = request.UserTgId, CreationDate = DateTime.UtcNow.ToLocalTime().AddHours(3), ReqType = string.IsNullOrEmpty(request.Type) ? "empty" : request.Type };
            bool send = true;

            var usrId = _nutriDbContext.Users.SingleOrDefault(x => x.TgId == request.UserTgId).Id;
            var isEmptyExtra = _nutriDbContext.Userinfos.Any(x => x.UserId == usrId && string.IsNullOrEmpty(x.Extra));

            CreateGPTPythRequest reqparams = new CreateGPTPythRequest();
            switch (request.Type)
            {

                case "txt":
                case "txt2":
                    reqparams.txt = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.outputtype = String.IsNullOrEmpty(request.OutputType) == true ? "0" : request.OutputType;
                    break;
                case "oga":
                case "imggg":
                case "oga2":
                case "imggg2":
                    var lastimgreq = _nutriDbContext.Gptrequests.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UserTgid == request.UserTgId);
                    if (lastimgreq != null)
                        if ((DateTime.UtcNow.ToLocalTime().AddHours(3) - lastimgreq.CreationDate).Seconds < 3)
                        {
                            send = false;
                            req.Answer = "Duplicate";
                            req.Done = true;
                            req.Iserror = true;
                        }

                    reqparams.url = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.outputtype = String.IsNullOrEmpty(request.OutputType) == true ? "0" : request.OutputType;
                    break;
                case "etik":
                    reqparams.url = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.extra = request.Extra.ToString();
                    break;
                case "edit_oga":
                case "recipe_oga":
                    reqparams.url = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.extra = request.Extra.ToString();
                    reqparams.outputtype = String.IsNullOrEmpty(request.OutputType) == true ? "0" : request.OutputType;
                    break;
                case "edit_txt":
                case "recipe_txt":
                    reqparams.txt = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.extra = request.Extra.ToString();
                    reqparams.outputtype = String.IsNullOrEmpty(request.OutputType) == true ? "0" : request.OutputType;
                    break;
                case "day1/yapp_oga":
                case "day1/yapp":
                    if (isEmptyExtra)
                        return 0;
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.txt = request.Question.ToString();
                    break;
                case "rate_day":
                    if (isEmptyExtra)
                        return 0;
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.txt = request.Question.ToString();
                    break;
                case "rate_any"://оценка
                    if (isEmptyExtra)
                        return 0;
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.txt = request.Question.ToString();
                    reqparams.assistanttype = request.AssistantType;//week,smol,mid,big
                    var lastreq = _nutriDbContext.Gptrequests.OrderByDescending(x => x.Id).FirstOrDefault(x => x.Request == req.Request);
                    if (lastreq != null && lastreq?.Iserror != true)
                    {
                        send = false;
                        req.Answer = lastreq.Answer;
                        req.Done = true;
                        req.Iserror = false;
                    }
                    break;
                default:
                    throw new ArgumentNullException("Пустой type");
            }
            await _nutriDbContext.Gptrequests.AddAsync(req);
            var url = $"{BaseUrl}/{request.Type}";
            await _nutriDbContext.SaveChangesAsync();
            if (send)
                Task.Run(() => { ExecuteRequest(reqparams, url, req.Id); });
            return req.Id;
        }

        public CheckGPTResponse CheckGPT(int requestId)
        {
            try
            {
                _logger.LogWarning($"Check GPT Status № {requestId}");
                var gptreq = _nutriDbContext.Gptrequests.SingleOrDefault(x => x.Id == requestId);
                if (gptreq == null)
                    return new CheckGPTResponse
                    {
                        Done = false,
                        IsError = true
                    };

                return new CheckGPTResponse
                {
                    Done = gptreq.Done,
                    IsError = gptreq?.Iserror ?? false,
                    Response = String.IsNullOrEmpty(gptreq.Answer) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<GPTResponse>(gptreq.Answer)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "упали при попытке получить статус");
                return new CheckGPTResponse { IsError = true, Done = true, Response = new GPTResponse { pretty = "Мы упали" } };
            }
        }

        public CreateGPTNoCodeRequest CreateRateRequest(RateRequest ratereq, MealHelper mealHelper)
        {
            var mealresp = new List<MealResponse>();
            switch (ratereq.AssistantType)
            {
                case "week":
                    mealresp = mealHelper.GetMeals(new GetUserMealsRequest { userTgId = ratereq.UserTgId, period = PythModels.Periods.mathweek });
                    break;
                case "twone":
                    mealresp = mealHelper.GetMeals(new GetUserMealsRequest { userTgId = ratereq.UserTgId, period = PythModels.Periods.math3weeks });
                    break;
                default:
                    throw new ArgumentNullException("Пустой AssistantType");
            }
            var useri = _nutriDbContext.Userinfos.Single(x => x.UserId == _nutriDbContext.Users.Single(x => x.TgId == ratereq.UserTgId).Id);
            var ext = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(useri.Extra);
            RateQuestion rateQuestion = new RateQuestion
            {
                food = Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponseNoPretty { Meals = mealresp }),
                user_info = new QuesUserInfo
                {
                    age = useri.Age,
                    target_calories = useri.Goalkk,
                    bmi = string.IsNullOrEmpty(ext["user_info_bmi"]) ? null : decimal.Parse(ext["user_info_bmi"]),
                    bmr = string.IsNullOrEmpty(ext["bmr"]) ? null : decimal.Parse(ext["bmr"]),
                    allergies = string.IsNullOrEmpty(ext["user_info_meals_ban"]) ? null : ext["user_info_meals_ban"],
                    gender = useri.Gender,
                    goal = useri.Goal,
                }
            };
            var gptRequest = new CreateGPTNoCodeRequest { AssistantType = ratereq.AssistantType, Type = "rate_any", UserTgId = ratereq.UserTgId, Question = Newtonsoft.Json.JsonConvert.SerializeObject(rateQuestion) };

            return gptRequest;
        }

        public async Task<string> SendRequest(CreateGPTPythRequest reqparams, string reqUrl)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(90);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(reqUrl, content);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<bool> ExecuteRequest(CreateGPTPythRequest reqparams, string reqUrl, int requstId)
        {

            string responseString = string.Empty;
            try
            {
                _logger.LogWarning($"GPT Req № {requstId}=\n{JsonConvert.SerializeObject(reqparams)}");
                responseString = await SendRequest(reqparams, reqUrl);
                _logger.LogWarning($"GPT Resp № {requstId}=\n {responseString}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Упали при создании реквеста");
                ErrorHelper.SendErrorMess("Упали при создании реквеста", ex);
                using (var scope = _serviceProviderFactory.CreateScope().ServiceProvider.CreateScope())
                {
                    var _nutriDbContext = scope.ServiceProvider.GetRequiredService<railwayContext>();
                    var dbreq = _nutriDbContext.Gptrequests.SingleOrDefault(x => x.Id == requstId);
                    if (dbreq == null)
                        throw new NullReferenceException($"В бд нет реквеста с id={requstId}");
                    dbreq.FinishDate = DateTime.UtcNow.ToLocalTime().AddHours(3);
                    dbreq.Done = true;
                    dbreq.Answer = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
                    dbreq.Iserror = true;
                    _nutriDbContext.Update(dbreq);
                    await _nutriDbContext.SaveChangesAsync();
                }
            }
            try
            {
                using (var scope = _serviceProviderFactory.CreateScope().ServiceProvider.CreateScope())
                {
                    var _nutriDbContext = scope.ServiceProvider.GetRequiredService<railwayContext>();
                    var dbreq = _nutriDbContext.Gptrequests.SingleOrDefault(x => x.Id == requstId);
                    if (dbreq == null)
                        throw new NullReferenceException($"В бд нет реквеста с id={requstId}");
                    dbreq.FinishDate = DateTime.UtcNow.ToLocalTime().AddHours(3);
                    if (string.IsNullOrEmpty(responseString))
                    {
                        dbreq.Done = true;
                        dbreq.Answer = "Response Is Empty";
                        dbreq.Iserror = true;
                    }
                    else
                    {
                        dbreq.Done = true;
                        dbreq.Answer = responseString;
                        dbreq.Iserror = false;
                    }
                    _nutriDbContext.Update(dbreq);
                    await _nutriDbContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестное падение при попытке создать запрос к ГПТ");
                throw new Exception("Упали при попытке создать запрос к ГПТ", ex);
            }
        }


        public string Test(CreateGPTNoCodeRequest req)
        {
            var par = new CreateGPTPythRequest();
            par.txt = req.Question;
            par.id = req.UserTgId.ToString();
            var url = $"{BaseUrl}/test";
            return SendRequest(par, url).GetAwaiter().GetResult();
        }
        public string TestInner(CreateGPTNoCodeRequest req)
        {
            var par = new CreateGPTPythRequest();
            par.txt = req.Question;
            par.id = req.UserTgId.ToString();
            var url = $"http://quart-test.railway.internal:7610/test";
            return SendRequest(par, url).GetAwaiter().GetResult();
        }
    }
}
