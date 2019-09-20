---
title: Asp.Net MVC接入Identity Server4 全记录
date: 2019-09-20 10:40
tags: Asp.Net MVC  Identity Server
---
- [Asp.Net MVC接入Identity Server4 全记录](#aspnet-mvc%e6%8e%a5%e5%85%a5identity-server4-%e5%85%a8%e8%ae%b0%e5%bd%95)
  - [当前环境](#%e5%bd%93%e5%89%8d%e7%8e%af%e5%a2%83)
  - [新增空的解决方案](#%e6%96%b0%e5%a2%9e%e7%a9%ba%e7%9a%84%e8%a7%a3%e5%86%b3%e6%96%b9%e6%a1%88)
  - [新增Identity Server4 服务端【本文不讨论服务端配置问题】](#%e6%96%b0%e5%a2%9eidentity-server4-%e6%9c%8d%e5%8a%a1%e7%ab%af%e6%9c%ac%e6%96%87%e4%b8%8d%e8%ae%a8%e8%ae%ba%e6%9c%8d%e5%8a%a1%e7%ab%af%e9%85%8d%e7%bd%ae%e9%97%ae%e9%a2%98)
    - [新增项目并添加到解决方案里](#%e6%96%b0%e5%a2%9e%e9%a1%b9%e7%9b%ae%e5%b9%b6%e6%b7%bb%e5%8a%a0%e5%88%b0%e8%a7%a3%e5%86%b3%e6%96%b9%e6%a1%88%e9%87%8c)
    - [新增一个空的MVC5项目](#%e6%96%b0%e5%a2%9e%e4%b8%80%e4%b8%aa%e7%a9%ba%e7%9a%84mvc5%e9%a1%b9%e7%9b%ae)
    - [配置MVC5接入Identity Server](#%e9%85%8d%e7%bd%aemvc5%e6%8e%a5%e5%85%a5identity-server)
  - [源码下载](#%e6%ba%90%e7%a0%81%e4%b8%8b%e8%bd%bd)
# Asp.Net MVC接入Identity Server4 全记录

## 当前环境

1. Net Core 2.2+ //建议使用Net Core 3.0

2. Asp.Net Framework 4.6.2+

3. Visual Studio 2019//如果使用Net Core 3.0，你可能需要预览版

## 新增空的解决方案

1. 打开命令行。执行`dotnet new sln -n SsoTest` 建立空白解决方案。

## 新增Identity Server4 服务端【本文不讨论服务端配置问题】

### 新增项目并添加到解决方案里

1. 打开命令行或者powershell

    ``` powershell
    # 新增IdentityServer4模板
    dotnet new -i IdentityServer4.Templates
    # 新建项目
    dotnet new is4empty -n IdentityServer
    # 添加到解决方案
    dotnet sln add .\IdentityServer\IdentityServer.csproj
    # 进入项目目录
    cd IdentityServer
    # 添加UI
    dotnet new is4ui
    ```

2. 修改Config.cs文件

    ```Csharp
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Test;
    using System.Collections.Generic;
    using System.Security.Claims;

    namespace IdentityServer
    {
        public static class Config
        {
            public static IEnumerable<IdentityResource> GetIdentityResources()
            {
                return new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email(),
                    new IdentityResources.Phone(),
                };
            }
            public static List<TestUser> GetUsers()
            {
                return new List<TestUser> {
                    new TestUser {
                        SubjectId = "1",
                        Username = "alice",
                        Password = "password",
                        Claims = new []
                        {
                            new Claim("name", "Alice"),
                            new Claim("website", "https://alice.com")
                        }
                    },
                    new TestUser {
                        SubjectId = "2",
                        Username = "bob",
                        Password = "password",
                        Claims = new []
                        {
                            new Claim("name", "Bob"),
                            new Claim("website", "https://bob.com")
                        }
                    }
                };
            }
            public static IEnumerable<ApiResource> GetApis()
            {
                return new ApiResource[] {
                    new ApiResource ("api1", "My API")
                };
            }

            public static IEnumerable<Client> GetClients()
            {
                var secret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256();
                return new Client[] {
                    new Client {
                        ClientId = "mvc",
                        ClientName = "MVC Client",
                        ClientSecrets = {
                            new Secret (secret)
                        },
                        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                        // where to redirect to after login
                        RedirectUris = { "http://localhost:5001/signin-oidc" },
                        // where to redirect to after logout
                        PostLogoutRedirectUris = { "http://localhost:5001/signout-callback-oidc" },
                        AllowedScopes = new List<string> {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Email,
                            IdentityServerConstants.StandardScopes.Phone,
                            IdentityServerConstants.StandardScopes.Profile,
                            "api1"
                        }
                    }};
            }
        }
    }
    ```

3. 修改Startup.cs,取消注释

    ``` CSharp
        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to support static files
            app.UseStaticFiles();

            app.UseIdentityServer();
            // uncomment, if you want to add an MVC-based UI
            app.UseMvcWithDefaultRoute();
        }
    ```

4. 新开一个命令行或者powershell窗口，运行服务端

    ``` powershell

    # 还原nuget,编译
    dotnet restore
    dotnet build
    # 运行
    dotnet run
    ```

### 新增一个空的MVC5项目

1. 打开解决方案SsoTest.sln
2. 在解决方案上右键->添加->新建项目，创建MVC5项目,名为SSOTest.Client
   ![](/images/20190920-1.jpg)
   ![](/images/20190920-2.jpg)
   ![](/images/20190920-3.jpg)
   ![](/images/20190920-4.jpg)

### 配置MVC5接入Identity Server

1. 修改Client项目属性，指定web端口为5001
![](/images/20190920-5.png)
2. 打开包控制台，安装nuget包
![](/images/20190920-7.jpg)
    ``` cmd
    Install-Package IdentityModel -Version 3.10.10
    Install-Package Microsoft.Owin.Security.Cookies
    Install-Package Microsoft.Owin.Security.OpenIdConnect
    ```
3. 新增OWIN的Startup.cs文件
   ![](/images/20190920-6.jpg)
4. 修改为Startup.cs文件.

    ```Csharp
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
                var secret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".ToSha256();
                app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Authority = "http://localhost:5000",
                    ClientId = "mvc",
                    ClientSecret = secret,
                    RedirectUri = "http://localhost:5001/signin-oidc",
                    PostLogoutRedirectUri = "http://localhost:5001/signout-oidc",
                    ResponseType = "code id_token",
                    Scope = "openid profile",
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
                                ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0",
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
                            //id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
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
    ```

5. 打开Controllers/HomeController.cs
   在`About`这个Action上加特性`[Authorize]`
6. 运行Client项目，访问 [http://localhost:5001/Home/About](http://localhost:5001/Home/About)

## 源码下载

[on the github](https://github.com/xyfy/UseMvc5ConnectIdentity4.git)
