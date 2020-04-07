using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeduCoreApp.Data;
using TeduCoreApp.Models;
using TeduCoreApp.Services;
using TeduCoreApp.Data.EF;
using TeduCoreApp.Data.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using TeduCoreApp.Application.Dapper.Implementation;
using TeduCoreApp.Application.Dapper.Interfaces;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Data.EF.Repositories;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Application.Implementation;
using TeduCoreApp.Authorization;
using TeduCoreApp.Extensions;
using TeduCoreApp.Helpers;
using TeduCoreApp.Infrastructure.Interfaces;

namespace TeduCoreApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                o=>o.MigrationsAssembly("TeduCoreApp.Data.EF")));

            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddMemoryCache();

            // Configure Identity
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;

                // User settings
                options.User.RequireUniqueEmail = true;
            });
            services.AddImageResizer();
            services.AddDistributedMemoryCache();
            services.AddMinResponse();
            services.AddAuthentication()
                .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            }).AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });  
                

            services.AddSession(option =>
            {
                option.IdleTimeout = TimeSpan.FromHours(2);
                option.Cookie.HttpOnly = true;
            });

            services.AddRecaptcha(new RecaptchaOptions
            {
                SiteKey = Configuration["Recaptcha:SiteKey"],
                SecretKey = Configuration["Recaptcha:SecretKey"],
                ValidationMessage = "Are you a robot?"
            });
            services.AddAutoMapper();
            // Add application services.
            services.AddScoped<UserManager<AppUser>, UserManager<AppUser>>();

            services.AddScoped<RoleManager<AppRole>, RoleManager<AppRole>>();

            services.AddSingleton(Mapper.Configuration);

            services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService));

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddTransient<DbInitializer>();

            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomClaimPrincipalFactory>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportCulture = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),

                    new CultureInfo("vi-VN")
                };

                opts.DefaultRequestCulture = new RequestCulture("en-US");
                //formating number, date, ..
                opts.SupportedCultures = supportCulture;
                //formating string
                opts.SupportedUICultures = supportCulture;
            });

            services.AddTransient(typeof(IUnitOfWork), typeof(EFUnitOfWork));
            services.AddTransient(typeof(IRepository<,>), typeof(EFRepository<,>));

            //Repository
            services.AddTransient<IProductCategoryRepository,ProductCategoryRepository>();
            services.AddTransient<IFunctionRepository, FunctionRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<IProductTagRepository, ProductTagRepository>();
            services.AddTransient<IPermissionRepository, PermissionRepository>();
            services.AddTransient<IBillRepository, BillRepository>();
            services.AddTransient<IBillDetailRepository, BillDetailRepository>();
            services.AddTransient<IColorRepository, ColorRepository>();
            services.AddTransient<ISizeRepository, SizeRepository>();
            services.AddTransient<IProductQuantityRepository, ProductQuantityRepository>();
            services.AddTransient<IProductImageRepository, ProductImageRepository>();
            services.AddTransient<IWholePriceRepository, WholePriveRepository>();
            services.AddTransient<IContactRepository, ContactRepository>();
            services.AddTransient<IFeedbackRepository, FeedbackRepository>();
            services.AddTransient<IBlogRepository, BlogRepository>();
            services.AddTransient<IBlogTagRepository, BlogTagRepository>();
            services.AddTransient<ISystemConfigRepository, SystemConfigRepository>();
            services.AddTransient<IFooterRepository, FooterRepository>();
            services.AddTransient<ISlideRepository, SlideRepository>();
            services.AddTransient<IPageRepository, PageRepository>();

            //Service
            services.AddTransient<IProductCategoryService, ProductCategoryService>();
            services.AddTransient<IFunctionService, FunctionService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IBillService, BillService>();
            services.AddTransient<IBlogService, BlogService>();
            services.AddTransient<ICommonService, CommonService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IFeedbackService, FeedbackService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<IPageService, PageService>();
            services.AddTransient<IViewRenderService, ViewRenderService>();
            services.AddTransient<IAuthorizationHandler, BaseResourceAuthorizationHandler>();

            services.AddMvc().AddJsonOptions(option => option.SerializerSettings.ContractResolver = new DefaultContractResolver());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("Logs/tedu-{Date}.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseImageResizer();

            app.UseMinResponse();

            app.UseStaticFiles();

            app.UseSession();

            var option = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(option.Value);

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller=Login}/{action=Index}/{id?}");
            });
        }
    }
}
