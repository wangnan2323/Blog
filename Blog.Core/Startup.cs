using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Blog.Core.AOP;
using Blog.Core.Common.MemoryCache;
using Blog.Core.IServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Blog.Core.Common.Redis;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Blog.Core.AuthHelper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AutoMapper;
using log4net;
using log4net.Config;
using log4net.Repository;
using Blog.Core.Log;
using Blog.Core.Filter;

namespace Blog.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //log4net
            repository = LogManager.CreateRepository("");//需要获取日志的仓库名，也就是你的当然项目名

            //指定配置文件，如果这里你遇到问题，应该是使用了InProcess模式，请查看Blog.Core.csproj,并删之 
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));//配置文件
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //log日志注入
            services.AddSingleton<ILoggerHelper, LogHelper>();
            services.AddMvc(o=> {
                o.Filters.Add(typeof(GlobalExceptionsFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //注入AutoMapper
            services.AddAutoMapper(typeof(Startup));//这是AutoMapper的2.0新特性
            //注入缓存
            services.AddScoped<ICaching, MemoryCaching>();
            //注入Redis
            services.AddScoped<IRedisCacheManager, RedisCacheManager>();//这里说下，如果是自己的项目，个人更建议使用单例模式 
            #region Authorize权限设置三种情况

            //使用说明：
            //如果你只是简单的基于角色授权的，第一步：【1/2 简单角色授权】，第二步：配置【统一认证】，第三步：开启中间件app.UseMiddleware<JwtTokenAuth>()不能验证过期，或者 app.UseAuthentication();可以验证过期时间
            //如果你是用的复杂的策略授权，配置权限在数据库，第一步：【3复杂策略授权】，第二步：配置【统一认证】，第三步：开启中间件app.UseAuthentication();
            //综上所述，设置权限，必须要三步走，涉及授权策略 + 配置认证 + 开启授权中间件，只不过自定义的中间件不能验证过期时间，所以我都是用官方的。

            #region 【1/2、简单角色授权】
            #region 1、基于角色的API授权 

            // 1【授权】、这个很简单，其他什么都不用做，
            // 无需配置服务，只需要在API层的controller上边，增加特性即可，注意，只能是角色的:
            // [Authorize(Roles = "Admin")]

            // 2【认证】、然后在下边的configure里，配置中间件即可:app.UseMiddleware<JwtTokenAuth>();但是这个方法，无法验证过期时间，所以如果需要验证过期时间，还是需要下边的第三种方法，官方认证

            #endregion

            #region 2、基于策略的授权（简单版）

            // 1【授权】、这个和上边的异曲同工，好处就是不用在controller中，写多个 roles 。
            // 然后这么写 [Authorize(Policy = "Admin")]
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));
            });


            // 2【认证】、然后在下边的configure里，配置中间件即可:app.UseMiddleware<JwtTokenAuth>();但是这个方法，无法验证过期时间，所以如果需要验证过期时间，还是需要下边的第三种方法，官方认证
            #endregion 
            #endregion


            #region 【3、复杂策略授权】

            #region 参数
            //读取配置文件
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);


            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // 如果要数据库动态绑定，这里先留个空，后边处理器里动态赋值
            var permission = new List<PermissionItem>();

            // 角色与接口的权限要求参数
            var permissionRequirement = new PermissionRequirement(
                "/api/denied",// 拒绝授权的跳转地址（目前无用）
                permission,
                ClaimTypes.Role,//基于角色的授权
                audienceConfig["Issuer"],//发行人
                audienceConfig["Audience"],//听众
                signingCredentials,//签名凭据
                expiration: TimeSpan.FromSeconds(60 * 5)//接口的过期时间
                );
            #endregion

            //【授权】
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy(PermissionNames.Permission,
            //             policy => policy.Requirements.Add(permissionRequirement));
            //});


            #endregion


            #region 【统一认证】
            // 令牌验证参数
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Issuer"],//发行人
                ValidateAudience = true,
                ValidAudience = audienceConfig["Audience"],//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RequireExpirationTime = true,
            };

            //2.1【认证】、core自带官方JWT认证
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(o =>
             {
                 o.TokenValidationParameters = tokenValidationParameters;
                 o.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         // 如果过期，则把<是否过期>添加到，返回头信息中
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             context.Response.Headers.Add("Token-Expired", "true");
                         }
                         return Task.CompletedTask;
                     }
                 };
             });


            //2.2【认证】、IdentityServer4 认证 (暂时忽略)
            //services.AddAuthentication("Bearer")
            //  .AddIdentityServerAuthentication(options =>
            //  {
            //      options.Authority = "http://localhost:5002";
            //      options.RequireHttpsMetadata = false;
            //      options.ApiName = "blog.core.api";
            //  });
            // 注入权限处理器

            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton(permissionRequirement);
            #endregion

            #endregion
            #region 初始化DB
            services.AddScoped<Blog.Core.Model.Models.DBSeed>();
            services.AddScoped<Blog.Core.Model.Models.MyContext>();
            #endregion

            #region Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v0.1.0",
                    Title = "Blog.Core API",
                    Description = "框架说明文档",
                    TermsOfService = "None",
                    Contact = new Swashbuckle.AspNetCore.Swagger.Contact { Name = "Blog.Core", Email = "Blog.Core@xxx.com", Url = "https://www.jianshu.com/u/94102b59cc2a" }
                });

                var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Blog.Core.xml");//这个就是刚刚配置的xml文件名
                c.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                var xmlModelPath = Path.Combine(basePath, "Blog.Core.Model.xml");//这个就是Model层的xml文件名

                c.IncludeXmlComments(xmlModelPath);
            });

            #endregion
            #region AutoFac
            //实例化 AutoFac  容器   
            var builder = new ContainerBuilder();


            builder.RegisterType<BlogCacheAOP>();//可以直接替换其他拦截器！一定要把拦截器进行注册
            //注册要通过反射创建的组件
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
            var dllPath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;//获取项目路径
            var servicesDllFile = Path.Combine(dllPath, "Blog.Core.Services.dll");//获取注入项目绝对路径
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);//直接采用加载文件的方法
            //var assemblysServices = Assembly.Load("Blog.Core.Services");//要记得!!!这个注入的是实现类层，不是接口层！不是 IServices
            builder.RegisterAssemblyTypes(assemblysServices).AsImplementedInterfaces()//指定已扫描程序集中的类型注册为提供所有其实现的接口。
                                                            .InstancePerLifetimeScope()
                                                            .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;
                                                            .InterceptedBy(typeof(BlogCacheAOP));//可以直接替换拦截器
            var repositoryDllFile = Path.Combine(dllPath, "Blog.Core.Repository.dll");//获取注入项目绝对路径
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);//直接采用加载文件的方法
            //var assemblysRepository = Assembly.Load("Blog.Core.Repository");//要记得!!!这个注入的是实现类层，不是接口层！不是 IRepository
            builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();

            //将services填充到Autofac容器生成器中
            builder.Populate(services);



            //使用已进行的组件登记创建新容器
            var ApplicationContainer = builder.Build();

            #endregion

            #region CORS
            //跨域第二种方法，声明策略，记得下边app中配置
            services.AddCors(c =>
            {
                //↓↓↓↓↓↓↓注意正式环境不要使用这种全开放的处理↓↓↓↓↓↓↓↓↓↓
                c.AddPolicy("AllRequests", policy =>
                {
                    policy
                    .AllowAnyOrigin()//允许任何源
                    .AllowAnyMethod()//允许任何方式
                    .AllowAnyHeader()//允许任何头
                    .AllowCredentials();//允许cookie
                });
                //↑↑↑↑↑↑↑注意正式环境不要使用这种全开放的处理↑↑↑↑↑↑↑↑↑↑


                //一般采用这种方法
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    .WithOrigins("http://127.0.0.1:1818", "http://localhost:8080", "http://localhost:8021", "http://localhost:8081", "http://localhost:1818")//支持多个域名端口，注意端口号后不要带/斜杆：比如localhost:8000/，是错的
                    .AllowAnyHeader()//Ensures that the policy allows any header.
                    .AllowAnyMethod();
                });
            });
            #endregion
            return new AutofacServiceProvider(ApplicationContainer);//第三方IOC接管 core内置DI容器
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            #region CORS
            //跨域第二种方法，使用策略，详细策略信息在ConfigureService中
            app.UseCors("LimitRequests");//将 CORS 中间件添加到 web 应用程序管线中, 以允许跨域请求。
            #endregion
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseMvc();
            #region Authen

            //此授权认证方法已经放弃，请使用下边的官方验证方法。但是如果你还想传User的全局变量，还是可以继续使用中间件，第二种写法//app.UseMiddleware<JwtTokenAuth>(); 
            //app.UserJwtTokenAuth(); 

            //如果你想使用官方认证，必须在上边ConfigureService 中，配置JWT的认证服务 (.AddAuthentication 和 .AddJwtBearer 二者缺一不可)
            app.UseAuthentication();
            #endregion
            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
            });
            #endregion
        }

        /// <summary>
        /// log4net 仓储库
        /// </summary>
        public static ILoggerRepository repository { get; set; }
    }
}
