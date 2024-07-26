using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NutriDbService.DbModels;
using NutriDbService.NoCodeModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NutriDbService.Helpers
{

    public class TransmitterHelper
    {
        private readonly string BaseUrl = "https://quart-test-production-9039.up.railway.app";
        public railwayContext _nutriDbContext { get; set; }
        private readonly IServiceProvider _serviceProvider;
        public TransmitterHelper(railwayContext nutriDbContext, IServiceProvider serviceProvider)
        {
            _nutriDbContext = nutriDbContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<int> CreateGPTRequest(CreateGPTRequest request)
        {
            var req = new Gptrequest { Iserror = false, Done = false, UserTgid = request.UserTgId };

            await _nutriDbContext.Gptrequests.AddAsync(req);
            await _nutriDbContext.SaveChangesAsync();


            CreateGPTPythRequest reqparams = new CreateGPTPythRequest();
            switch (request.Type)
            {

                case "txt":
                    reqparams.txt = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    break;
                case "oga":
                case "imggg":

                    reqparams.url = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    break;
                case "edit_oga":
                    reqparams.url = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.oldmeal = request.Oldmeal.ToString();
                    break;
                case "edit_txt":
                    reqparams.txt = request.Question.ToString();
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.oldmeal = request.Oldmeal.ToString();
                    break;
                case "day1/yapp_oga":
                case "day1/yapp":
                    reqparams.id = request.UserTgId.ToString();
                    reqparams.txt = request.Question.ToString();
                    break;
                default:
                    throw new ArgumentNullException("Пустой type");
            }
            var url = $"{BaseUrl}/{request.Type}";
            Task.Run(() => { ExecuteRequest(reqparams, url, req.Id); });
            return req.Id;
        }

        public CheckGPTResponse CheckGPT(int requestId)
        {
            try
            {
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
                    Response = String.IsNullOrEmpty(gptreq.Answer) ? null : Regex.Escape(gptreq.Answer)//Newtonsoft.Json.JsonConvert.DeserializeObject<GPTResponse>(gptreq.Answer)
                };
            }
            catch (Exception ex) { return new CheckGPTResponse { IsError = true, Done = true, Response = "Мы упали" }; }//new GPTResponse { pretty = "Мы упали" } }; }
        }

        public async Task<string> SendRequest(CreateGPTPythRequest reqparams, string reqUrl)
        {
            HttpClient client = new HttpClient();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(reqUrl, content);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<bool> ExecuteRequest(CreateGPTPythRequest reqparams, string reqUrl, int requstId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _nutriDbContext = scope.ServiceProvider.GetRequiredService<railwayContext>();
                string responseString = string.Empty;
                try
                {
                    responseString = await SendRequest(reqparams, reqUrl);
                }
                catch (Exception ex)
                {
                    var dbreq = _nutriDbContext.Gptrequests.SingleOrDefault(x => x.Id == requstId);
                    if (dbreq == null)
                        throw new NullReferenceException($"В бд нет реквеста с id={requstId}");
                    dbreq.Done = true;
                    dbreq.Answer = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
                    dbreq.Iserror = true;
                    _nutriDbContext.Update(dbreq);
                    await _nutriDbContext.SaveChangesAsync();
                }
                try
                {
                    var dbreq = _nutriDbContext.Gptrequests.SingleOrDefault(x => x.Id == requstId);
                    if (dbreq == null)
                        throw new NullReferenceException($"В бд нет реквеста с id={requstId}");
                    dbreq.Done = true;
                    dbreq.Answer = responseString;
                    dbreq.Iserror = false;
                    _nutriDbContext.Update(dbreq);
                    await _nutriDbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Упали при попытке создать запрос к ГПТ", ex);
                }
            }
        }

        public string Test(string input)
        {
            var par = new CreateGPTPythRequest();
            par.txt = input;
            var url = $"{BaseUrl}/test";
            return SendRequest(par, url).GetAwaiter().GetResult();
        }
        public string TestInner(string input)
        {
            var par = new CreateGPTPythRequest();
            par.txt = input;
            var url = $"http://quart-test.railway.internal:7610/test";
            return SendRequest(par, url).GetAwaiter().GetResult();
        }
    }
}
