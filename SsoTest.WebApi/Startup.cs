using System.IdentityModel.Tokens.Jwt;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Cors;
using IdentityModel;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;

[assembly: OwinStartup(typeof(SsoTest.WebApi.Startup))]

namespace SsoTest.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=316888
            ConfigureAuth(app);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var secret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".ToSha256();
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "http://localhost:5000",
                RequiredScopes = new string[] { "webapi" },
                ClientId = "Test",
                ClientSecret = secret,
                DelayLoadMetadata = true
            });
        }
    }
}
