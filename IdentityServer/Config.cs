// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
                },
                new Client
                {
                      ClientId = "Test",
                    ClientName = "Test",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenLifetime = 60*60,
                    AccessTokenType = AccessTokenType.Jwt,
                    RedirectUris =
                    {
                        "https://localhost:5002/signin-callback.html",
                        "https://localhost:5002/silent-callback.html"
                    },
                    PostLogoutRedirectUris = { "https://localhost:5002" },
                    AllowedCorsOrigins = { "https://localhost:5002" },
                    RequireConsent = false,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "webapi"
                    }
                }
            };
        }
    }
}