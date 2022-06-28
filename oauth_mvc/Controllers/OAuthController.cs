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
            ViewBag.UserID = TempData["UserID"];
            ViewBag.DisplayName = TempData["DisplayName"];

            return View();
        }

        public async Task<ActionResult> LineNotifyCallBack(string code, string state)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://notify-bot.line.me/oauth/token");

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code"},
                    { "code", code },
                    { "redirect_uri", "https://localhost:44312/OAuth/LineNotifyCallBack"},
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

                    return RedirectToAction("LineNotifySendMessage");
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult LineNotifySendMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LineNotifySendMessage(string message)
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

                return RedirectToAction("LineNotifySendMessage");
            }
        }

        public async Task<ActionResult> LineLogInCallBack(string code, string error)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.line.me/oauth2/v2.1/token");
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code"},
                    { "code", code },
                    { "redirect_uri", "https://localhost:44312/OAuth/LineLogInCallBack"},
                    { "client_id", "1657259162"},
                    { "client_secret", "4993340944b40619e44381305fb8f416" }
                });
                var response = await client.PostAsync("", content);
                var result = response.Content.ReadAsStringAsync().Result;
                var lineLogin = JsonConvert.DeserializeObject<LineLogin>(result);
                var proFile = GetProfile(lineLogin.Access_token);

                TempData["UserID"] = proFile.Result.UserID;
                TempData["DisplayName"] = proFile.Result.DisplayName;

                return RedirectToAction("Index");
            }
        }

        private async Task<LineLogin> GetProfile(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.GetAsync("https://api.line.me/v2/profile");
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var lineLogIn = JsonConvert.DeserializeObject<LineLogin>(result);

                    return lineLogIn;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}