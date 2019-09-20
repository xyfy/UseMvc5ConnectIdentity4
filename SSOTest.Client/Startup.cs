using System;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Helpers;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(SSOTest.Client.Startup))]

namespace SSOTest.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                CookieHttpOnly = true
            });
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "http://localhost:5000",
                ClientId = "admin_ui",
                ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".ToSha256(),
                RedirectUri = "http://localhost:5001/signin-oidc",
                PostLogoutRedirectUri = "http://localhost:5001/signout-oidc",
                ResponseType = "code id_token",
                Scope = "openid profile admin_api offline_access",
                RequireHttpsMetadata = false,
                SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                    {
                        var client = new HttpClient();
                        var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest()
                        {
                            Address = "http://localhost:5000",
                            Policy ={
                                RequireHttps = false
                            }
                        });
                        if (disco.IsError) throw new Exception(disco.Error);
                        // use the code to get the access and refresh token
                        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest()
                        {
                            Address = disco.TokenEndpoint,
                            ClientId = "mvc",
                            ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".ToSha256(),
                            Code = n.Code,
                            RedirectUri = n.RedirectUri
                        });

                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }
                        // use the access token to retrieve claims from userinfo
                        var userInfoResponse = await client.GetUserInfoAsync(new UserInfoRequest()
                        {
                            Address = disco.UserInfoEndpoint,
                            Token = tokenResponse.AccessToken
                        });

                        // create new identity
                        var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                        id.AddClaims(userInfoResponse.Claims);
                        id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                        id.AddClaim(new Claim("expires_at", DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToString()));
                        id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                        id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        id.AddClaim(new Claim("sid", n.AuthenticationTicket.Identity.FindFirst("sid").Value));

                        n.AuthenticationTicket = new AuthenticationTicket(
                                    new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, "name", "role"),
                                    n.AuthenticationTicket.Properties);
                        //TODO 本地USER同步
                        foreach (var item in userInfoResponse.Claims)
                        {
                            if (item.Type == "sub")
                            {
                            }
                        }
                    }
                }
            });
        }
    }
}
