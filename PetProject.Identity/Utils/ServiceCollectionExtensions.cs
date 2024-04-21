using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PetProject.Identity.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyIdentityDbContext<IDentityDbContext>(this IServiceCollection services, string connectionString, string migrationName)
            where IDentityDbContext : IdentityDbContext
        {
            services.AddDbContext<IDentityDbContext>(options => options.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationName)));

            return services;
        }

        public static IServiceCollection AddMyIdentity<IDentityUser, IDentityRole, IDentityDbContext>(this IServiceCollection services)
            where IDentityUser : class
            where IDentityRole : class
            where IDentityDbContext : IdentityDbContext
        {
            services.AddIdentity<IDentityUser, IDentityRole>().AddEntityFrameworkStores<IDentityDbContext>();

            return services;
        }

        public static IServiceCollection AddMyIdentityServer4<IDentityUser>(this IServiceCollection services, string connectionString, string migrationName) 
            where IDentityUser : class
        {
            services.AddIdentityServer()
                .AddAspNetIdentity<IDentityUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = config => config.UseSqlServer(connectionString,
                        opt => opt.MigrationsAssembly(migrationName));
                    options.DefaultSchema = "idt_config";
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = config => config.UseSqlServer(connectionString,
                        opt => opt.MigrationsAssembly(migrationName));
                    options.DefaultSchema = "idt_opr";
                })
                .AddDeveloperSigningCredential();

            return services;
        }
    }
}
