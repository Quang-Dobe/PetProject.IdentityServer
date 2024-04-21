using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Storage;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PetProject.Identity.Data.Identity
{
    public class MyIdentitySeeding
    {
        public static void EnsureSeedData<IDentityUser, IDentityRole, IDentityDbContext>(string connectionString, string migrationName)
            where IDentityUser : class
            where IDentityRole : class
            where IDentityDbContext : IdentityDbContext
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddDbContext<IDentityDbContext>(options => options.UseSqlServer(connectionString));

            services.AddIdentity<IDentityUser, IDentityRole>()
                .AddEntityFrameworkStores<IDentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddOperationalDbContext(options =>
            {
                options.ConfigureDbContext = config => config.UseSqlServer(connectionString,
                    opt => opt.MigrationsAssembly(migrationName));
                options.DefaultSchema = "idt_opr";
            });
            services.AddConfigurationDbContext(options =>
            {
                options.ConfigureDbContext = config => config.UseSqlServer(connectionString, 
                    opt => opt.MigrationsAssembly(migrationName));
                options.DefaultSchema = "idt_config";
            });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            using (var identityDbContext = scope.ServiceProvider.GetService<IDentityDbContext>())
            {
                identityDbContext.Database.Migrate();
            }

            using (var persistedDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>())
            {
                persistedDbContext.Database.Migrate();
            }

            using (var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>())
            {
                configurationDbContext.Database.Migrate();
                EnsureSeedData(configurationDbContext);
            }

            // We need to update this function - EnsureUsers() - onlyif we dont use IdentityUser for UserManager
            if (typeof(IDentityUser).GetInterface(typeof(IdentityUser).Name) != null)
            {
                EnsureUsers(scope);
            }
        }

        private static void EnsureUsers(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var angella = userMgr.FindByNameAsync("angella").Result;
            if (angella == null)
            {
                angella = new IdentityUser
                {
                    UserName = "angella",
                    Email = "angella.freeman@email.com",
                    EmailConfirmed = true
                };

                var result = userMgr.CreateAsync(angella, "Pass123$").Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(angella, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Angella Freeman"),
                    new Claim(JwtClaimTypes.GivenName, "Angella"),
                    new Claim(JwtClaimTypes.FamilyName, "Freeman"),
                    new Claim(JwtClaimTypes.WebSite, "http://angellafreeman.com"),
                    new Claim("location", "somewhere")
                }).Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in MyIdentitySeedingData.Clients.ToList())
                {
                    context.Clients.Add(client);
                }

                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in MyIdentitySeedingData.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource);
                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in MyIdentitySeedingData.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource);
                }

                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in MyIdentitySeedingData.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource);
                }

                context.SaveChanges();
            }
        }
    }

    public class MyIdentitySeedingData
    {
        public static IEnumerable<IdentityServer4.EntityFramework.Entities.IdentityResource> IdentityResources =>
            new[]
            {
                //new IdentityResources.OpenId(),
                new IdentityServer4.EntityFramework.Entities.IdentityResource
                {
                    Name = IdentityServerConstants.StandardScopes.OpenId,
                    UserClaims = new List<IdentityResourceClaim>()
                    {
                        new IdentityResourceClaim() { Type = JwtClaimTypes.Subject }
                    }
                },
                new IdentityServer4.EntityFramework.Entities.IdentityResource
                {
                    Name = IdentityServerConstants.StandardScopes.Profile,
                    UserClaims = new List<IdentityResourceClaim>()
                    {
                        new IdentityResourceClaim() { Type = IdentityServerConstants.StandardScopes.Profile }
                    }
                },
                new IdentityServer4.EntityFramework.Entities.IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<IdentityResourceClaim>()
                    {
                        new IdentityResourceClaim() { Type = "role" }
                    }
                }
            };

        public static IEnumerable<IdentityServer4.EntityFramework.Entities.ApiScope> ApiScopes =>
            new[] 
            { 
                new IdentityServer4.EntityFramework.Entities.ApiScope() { Name = "CoffeeAPI.read"}, 
                new IdentityServer4.EntityFramework.Entities.ApiScope() { Name = "CoffeeAPI.write" }, 
            };
        
        public static IEnumerable<IdentityServer4.EntityFramework.Entities.ApiResource> ApiResources =>
            new[]
            {
                new IdentityServer4.EntityFramework.Entities.ApiResource()
                {
                    Name = "CoffeeAPI",
                    Scopes = new List<ApiResourceScope>()
                    {
                        new ApiResourceScope() { Scope = "CoffeeAPI.read" },
                        new ApiResourceScope() { Scope = "CoffeeAPI.write" }
                    },
                    Secrets = new List<ApiResourceSecret>()
                    { 
                        new ApiResourceSecret() { Value = "ScopeSecret".Sha256() }
                    },
                    UserClaims = new List<ApiResourceClaim>()
                    {
                        new ApiResourceClaim() { Type = "role" }
                    }
                }
            };

        public static IEnumerable<IdentityServer4.EntityFramework.Entities.Client> Clients =>
            new[]
            {
                // m2m client credentials flow client
                new IdentityServer4.EntityFramework.Entities.Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client-Credentials Client",
                    ClientSecrets = new List<ClientSecret>()
                    {
                        new ClientSecret() { Value = "ClientSecret1".Sha256() },
                    },
                    AllowedScopes = new List<ClientScope>()
                    {
                        new ClientScope() { Scope = "CoffeeAPI.read" },
                        new ClientScope() { Scope = "CoffeeAPI.write" }
                    },
                    AllowedGrantTypes = GrantTypes.ClientCredentials.Select(x => new ClientGrantType() { GrantType = x }).ToList()
                },

                // interactive client using code flow + pkce
                new IdentityServer4.EntityFramework.Entities.Client
                {
                    ClientId = "interactive",
                    ClientName = "Resource-Credential Client",
                    ClientSecrets = new List<ClientSecret>()
                    { 
                        new ClientSecret() { Value = "ClientSecret1".Sha256() }
                    },
                    AllowedScopes = new List<ClientScope>()
                    {
                        new ClientScope() { Scope = "openid" },
                        new ClientScope() { Scope = "profile" },
                        new ClientScope() { Scope = "CoffeeAPI.read" },
                        new ClientScope() { Scope = "CoffeeAPI.write" }
                    },
                    AllowedGrantTypes = GrantTypes.Code.Select(x => new ClientGrantType() { GrantType = x }).ToList(),
                    AllowOfflineAccess = true,
                    RedirectUris = new List<ClientRedirectUri>()
                    {
                        new ClientRedirectUri() { RedirectUri = "https://localhost:5444/signin-oidc" }
                    },
                    PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>()
                    {
                        new ClientPostLogoutRedirectUri() { PostLogoutRedirectUri = "https://localhost:5444/signout-callback-oidc" }
                    },
                    FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                    RequirePkce = true,
                    RequireConsent = true,
                    AllowPlainTextPkce = false
                },
            };
    }
}
