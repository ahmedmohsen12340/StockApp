using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;
using RepositoryContracts;
using Serilog;
using Services;
using ServicesContract;
using StockApp.ConfigurationOptions;
using StockApp.Core.Domain.Identity_Entities;

namespace StockApp.HelperExtentions
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddAServices(this IServiceCollection services,IConfiguration configuration)
        {
            //Services
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            services.AddHttpClient();
            services.AddScoped<IFinnhubGetDetailsService, FinnhubGetDetailsService>();
            services.AddScoped<IFinnhubGetCompaniesDataService,FinnhubGetCompaniesDataService>();
            services.AddScoped<IFinnhubSearchService, FinnhubSearchService>();
            services.AddScoped<IFinnhubRepository, FinnhubRepository>();
            services.AddScoped<IStocksCreateBuyOrderService, StocksCreateBuyOrderService>();
            services.AddScoped<IStocksCreateSellOrderService, StocksCreateSellOrderService>();
            services.AddScoped<IStocksGetBuyOrdersService, StocksGetBuyOrdersService>();
            services.AddScoped<IStocksGetSellOrdersService, StocksGetSellOrdersService>();
            services.AddScoped<IStocksRepository, StocksRepository>();
            services.Configure<TradingOption>(configuration.GetSection("TradingOptions"));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            //Enable Identity in the project
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredUniqueChars = 4;
            })
            //to determine which dbcontext we use
            .AddEntityFrameworkStores<ApplicationDbContext>()
            //to add token provider to Application to send OTP when sign in we may use this or not but we have to write this statment to make app work smoothly
            .AddDefaultTokenProviders()
            //We Don't need to make a custom repository layers by type this statments it made automatically
            //by this statement we have configured the repository layer that interacts with the DB context to manipulate the user's data
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            //by this statement we have configured the repository layer that interacts with the DB context to manipulate the Role's data
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                //make the actions unavilable except signIn

                options.AddPolicy("UnAuthorized", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return !context.User.Identity.IsAuthenticated;
                        //to make me access only if I am not sign in
                    });
                });
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/LogIn";
                //make the users who didn't signIn redirect to this path
            }); 

            services.AddHttpLogging(options =>
            {
                options.LoggingFields =
                HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponsePropertiesAndHeaders;
            });
            return services;
        }
    }
}
