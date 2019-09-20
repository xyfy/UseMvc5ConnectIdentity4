using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SSOTest.Client.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public async System.Threading.Tasks.Task<ActionResult> LogOutAsync()
        {
            var cp = (ClaimsPrincipal)User;
            var idtokenHint = cp.FindFirst("id_token");
            if (idtokenHint != null)
            {
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest()
                {
                    Address = "http://localhost:5000",
                    Policy = { RequireHttps = false }
                });
                var ru = new IdentityModel.Client.RequestUrl(disco.EndSessionEndpoint);
                var endsession = ru.CreateEndSessionUrl(idtokenHint.Value, postLogoutRedirectUri: "http://localhost:5001/signout-oidc");
                return Redirect(endsession);
            }
            return RedirectToAction("SsoSignOutCallBack", "Home");
        }


        /// <summary>
        /// sso退出回调路径
        /// </summary>
        /// <returns></returns>
        [Route("signout-oidc")]
        public ActionResult SsoSignOutCallBack()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Abandon();
            for (int i = 0; i < HttpContext.Request.Cookies.Count; i++)
            {
                var cookie = HttpContext.Request.Cookies[i];
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Request.Cookies.Remove(cookie.Name);
                HttpContext.Response.Cookies.Add(cookie);
            }
            return Redirect("/");
        }
    }
}