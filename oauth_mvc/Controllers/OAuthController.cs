using Newtonsoft.Json;
using oauth_mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace oauth_mvc.Controllers
{
    public class OAuthController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> CallBack(string code, string state)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://notify-bot.line.me/oauth/token");

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code"},
                    { "code", code },
                    { "redirect_uri", "https://localhost:44312/OAuth/CallBack"},
                    { "client_id", "r6UP0t7tf7biApeFBTMSuf"},
                    { "client_secret", "IQTmHYe5sOsL5TlJrMm2sfwiESqIZYSb0fy0yfKVdNR" }
                });

                var response = await client.PostAsync("", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var lineNotify = JsonConvert.DeserializeObject<LineNotify>(result);

                    var cookie = new HttpCookie("LineNotify");
                    cookie.Values.Add("access_token", lineNotify.Access_token);
                    Response.Cookies.Add(cookie);

                    return RedirectToAction("SendMessage");
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult SendMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string message)
        {
            var token = Request.Cookies["LineNotify"]?["access_token"];
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://notify-api.line.me/api/notify");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "message", message },
                });
                await client.PostAsync("", content);

                return RedirectToAction("SendMessage");
            }
        }
    }


}