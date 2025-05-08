using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace WebService
{
    public class DeepSeek
    {
        public string? ApiUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public string ErrorMessage = "";
        public DeepSeekResult? ResultJson;

        public DeepSeek(string apikey = "")
        {
            ApiKey = apikey;
        }

        public void Chat(string[] user, string system = "你是一个问答助手。")
        {
            ApiUrl = "https://api.deepseek.com/chat/completions";
            Model = "deepseek-chat";
            var ws = new WebService();
            string[] headers = new string[]
            {
                "Content-Type:application/json",
                "Accept:application/json",
                "Authorization:Bearer " + ApiKey + "",
            };
            var messages = new[] {
                    new { role = "system", content = system}
                };
            foreach (var item in user)
            {
                messages = messages.Append(new { role = "user", content = item }).ToArray();
            }

            var ReadyData = new
            {
                model = Model,
                messages
            };
            string postData = JsonConvert.SerializeObject(ReadyData);

            ErrorMessage = "";
            ResultJson = new DeepSeekResult();
            string rs = ws.GetResponseResult(ApiUrl, Encoding.UTF8, "POST", postData, headers);
            ErrorMessage = ws.ErrorMessage;
            ResultJson = JsonConvert.DeserializeObject<DeepSeekResult>(rs);
        }

        public void TC_chat(string say)
        {
            ApiUrl = "https://api.lkeap.cloud.tencent.com/v1/chat/completions";
            Model = "deepseek-r1";
            var ws = new WebService();
            string[] headers = new string[]
            {
                "Content-Type:application/json",
                "Accept:application/json",
                "Authorization:Bearer " + ApiKey + "",
            };
            var ReadyData = new
            {
                model = Model,
                messages = new[]{
                    new {role="user",content=say}
                }
            };
            string postData = JsonConvert.SerializeObject(ReadyData);

            ErrorMessage = "";
            ResultJson = new DeepSeekResult();
            string rs = ws.GetResponseResult(ApiUrl, Encoding.UTF8, "POST", postData, headers);
            ErrorMessage = ws.ErrorMessage;
            ResultJson = JsonConvert.DeserializeObject<DeepSeekResult>(rs);
        }
    }
}
