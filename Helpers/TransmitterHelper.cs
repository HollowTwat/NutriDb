using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.NoCodeModels;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NutriDbService.Helpers
{

    public class TransmitterHelper
    {
        private readonly string BaseUrl = "http://quart-test.railway.internal";
        public railwayContext _nutriDbContext { get; set; }
        public TransmitterHelper(railwayContext nutriDbContext)
        {
            _nutriDbContext = nutriDbContext;
        }

        public async Task<int> CreateGPTRequest(CreateGPTRequest request)
        {
            var reqid = _nutriDbContext.Gptrequests.Last().Id;
            reqid++;
            await _nutriDbContext.Gptrequests.AddAsync(new Gptrequest { Id = reqid, Iserror = false, Done = false, UserTgid = request.UserTgId });
            await _nutriDbContext.SaveChangesAsync();

            var reqparams = new Dictionary<string, string>();
            switch (request.Type)
            {
                case "txt":
                    reqparams.Add("txt", request.Question);
                    reqparams.Add("id", request.UserTgId.ToString());
                    break;
                case "oga":
                case "imggg":
                    reqparams.Add("url", request.Question);
                    reqparams.Add("id", request.UserTgId.ToString());
                    break;
                case "edit_oga":
                    reqparams.Add("url", request.Question);
                    reqparams.Add("id", request.UserTgId.ToString());
                    reqparams.Add("oldmeal", request.Oldmeal);
                    break;
                case "edit_txt":
                    reqparams.Add("txt", request.Question);
                    reqparams.Add("id", request.UserTgId.ToString());
                    reqparams.Add("oldmeal", request.Oldmeal);
                    break;
                default:
                    throw new ArgumentNullException("Пустой type");
            }
            var url = $"{BaseUrl}/{request.Type}";
            Task.Run(() => { ExecuteRequest(reqparams, url, reqid); });
            return reqid;
        }

        public CheckGPTResponse CheckGPT(long requestId)
        {
            try
            {
                var gptreq = _nutriDbContext.Gptrequests.SingleOrDefault();
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
                    Response = Newtonsoft.Json.JsonConvert.DeserializeObject<GPTResponse>(gptreq.Answer)
                };
            }
            catch (Exception ex) { return new CheckGPTResponse { IsError = true, Done = true, Response = new GPTResponse { pretty = "Мы упали" } }; }
        }

        public async Task<string> SendRequest(Dictionary<string, string> reqparams, string reqUrl)
        {
            HttpClient client = new HttpClient();

            var content = new FormUrlEncodedContent(reqparams);

            var response = await client.PostAsync(reqUrl, content);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<bool> ExecuteRequest(Dictionary<string, string> reqparams, string reqUrl, int requstId)
        {
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

        public string Test(string input)
        {
            var par = new Dictionary<string, string> { { "test", input } };
            var url = $"{BaseUrl}/test";
            return SendRequest(par, url).GetAwaiter().GetResult();
        }
    }
}
