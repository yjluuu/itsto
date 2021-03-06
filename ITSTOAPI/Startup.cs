using Common.Tool;
using ITSTOAPI.Controllers;
using ITSTOAPI.Attribute;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Routine.Models.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace ITSTOAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //数据库链接
            //DBContext.conStr = Configuration.GetConnectionString("connStr");
            services.AddDbContext<DBContext>(options => { options.UseMySql(Configuration.GetConnectionString("connStr")); }, ServiceLifetime.Scoped);
            //redis链接
            RedisClient.redisClient.InitConnect(Configuration);
            services.AddControllers(options => { options.Filters.Add(typeof(CustomResourceAttribute)); });
            //添加全局的Authorization过滤器
            //services.AddControllers(options => { options.Filters.Add(typeof(CustomAuthorization)); });
            //添加全局的Action过滤器
            services.AddControllers(options => { options.Filters.Add(typeof(CustomActionFilterAttribute)); });
            //添加全局的Exception过滤器
            services.AddControllers(options => { options.Filters.Add(typeof(CustomExceptionFilterAttribute)); });
            //AutoMapper
            services.AddAutoMapper(typeof(AutoMapperConfigures));
            //添加httpcontext
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                ////日期类型默认格式化处理 方式1
                //options.SerializerSettings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy/MM/dd HH:mm:ss" });
                ////日期类型默认格式化处理 方式2
                //options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                //options.SerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
                //空值处理
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            #region 注入
            services.AddTransient<Bo.Interface.IBusiness.IInterfaceLogsService, Bo.Business.InterfaceLogsService>();
            services.AddTransient<Bo.Interface.IRepository.IRepositoryFactory, Bo.Repository.RepositoryFactory>();
            services.AddTransient<Bo.Interface.IBusiness.ITestService, Bo.Business.TestService>();
            services.AddTransient<Bo.Interface.IBusiness.IStoreService, Bo.Business.StoreService>();
            services.AddTransient<Bo.Interface.IBusiness.IInterfaceUserService, Bo.Business.InterfaceUserService>();
            services.AddTransient<Bo.Interface.IBusiness.IInterfaceMappingService, Bo.Business.InterfaceMappingService>();
            services.AddTransient<Bo.Interface.IBusiness.IDishCategoryService, Bo.Business.DishCategoryService>();
            services.AddTransient<Bo.Interface.IBusiness.IDishService, Bo.Business.DishService>();
            services.AddTransient<Bo.Interface.IBusiness.IAppSettingService, Bo.Business.AppSettingService>();
            services.AddTransient<Bo.Interface.IBusiness.IOrdersService, Bo.Business.OrdersService>();
            services.AddTransient<Bo.Interface.IBusiness.IChannelService, Bo.Business.ChannelService>();
            services.AddTransient<Bo.Interface.IBusiness.ICustomerService, Bo.Business.CustomerService>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                return next();
            });


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "api/{controller}/{action}");
            });

        }
    }
}
