using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Mgi.STP7000.Infrastructure.ApiProtocol;
using MGI.STP7000.WebHost.Filters;
using MGI.STP7000.WebHost.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.AspNetCore;

namespace MGI.STP7000.WebHost
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
            services.Configure<RequestLoggingOptions>(o =>
            {
                o.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress.MapToIPv4());
                };
            });
            #region JWT
            // configure strongly typed settings objects
            var section = Configuration.GetSection("Jwt");
            services.Configure<JwtSetting>(section);
            var jwtSetting = section.Get<JwtSetting>();
            var key = Encoding.ASCII.GetBytes(jwtSetting.Secret);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        var name = context.Principal.Identity.Name;
                        return Task.CompletedTask;
                    }
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateAudience = true,//是否验证Audience
                    ValidateLifetime = true,//是否验证失效时间
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    ValidAudience = jwtSetting.Audience,
                    ValidIssuer = jwtSetting.Issuer, //Issuer，这两项和签发jwt的设置一致
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
            });
            #endregion
            services.AddControllers(
                            options =>
                            {
                                options.EnableEndpointRouting = false;
                                //options.Filters.Add<ModelValidateFilter>();
                                options.Filters.Add<ApiExceptionFilter>();
                                options.Filters.Add<ApiInvokeLogFilter>();
                            })
                            .AddNewtonsoftJson()
                            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                            .AddControllersAsServices();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (ctx) =>
                {
                    var errors = ctx.ModelState
                                        .Where(e => e.Value.Errors.Count > 0)
                                        .Select(e => e.Value.Errors.First().ErrorMessage)
                                        .ToList();
                    var message = string.Join("|", errors);
                    var result = new ApiResponse(ResponseCode.ModelValidateError.Format(message));
                    return new OkObjectResult(result);
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("authenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
                options.AddPolicy("adminOnly", policy =>
                {
                    policy.RequireRole("admin");
                });
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Mgi.STP7000.Model.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Mgi.STP7000.Repository.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Mgi.STP7000.Service.xml"));
                //swagger中控制请求的时候发是否需要在url中增加token
                //c.OperationFilter<AddTokenHeaderOperationFilter>();
                c.ResolveConflictingActions(x => x.First());
                //c.DescribeAllEnumsAsStrings();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                         new OpenApiSecurityScheme
                         {
                           Reference = new OpenApiReference
                           {
                             Type = ReferenceType.SecurityScheme,
                             Id = "Bearer"
                           }
                          },
                          new string[] { }
                     }
               });
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //builder.RegisterType<MssqlConnectionManager>().As<IConnectionManager>().SingleInstance();
            //builder.RegisterType<ServiceInterceptor>();
            //builder.RegisterType<RepositoryInterceptor>();
            builder.RegisterAssemblyTypes(Assembly.Load("Mgi.STP7000.Repository"))
                .AsImplementedInterfaces()
                //.EnableInterfaceInterceptors()
                //.InterceptedBy(typeof(ServiceInterceptor))
                .PropertiesAutowired();
            builder.RegisterAssemblyTypes(Assembly.Load("Mgi.STP7000.Service"))
                .AsImplementedInterfaces()
                //.EnableInterfaceInterceptors()
                //.InterceptedBy(typeof(RepositoryInterceptor))
                .PropertiesAutowired();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "API V1");
                c.DefaultModelExpandDepth(5);
            });
            app.UseCors(x =>
            {
                x.AllowAnyHeader();
                x.AllowAnyMethod();
                x.AllowAnyOrigin();
            });

            // Write streamlined request completion events, instead of the more verbose ones from the framework.
            // To use the default framework request logging instead, remove this line and set the "Microsoft"
            // level in appsettings.json to "Information".
            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            //var options = new DefaultFilesOptions();
            //options.DefaultFileNames.Add("index.html");    //将index.html改为需要默认起始页的文件名.
            //app.UseDefaultFiles(options);
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    ServeUnknownFileTypes = true
            //});
            //var databaseUpdateService = app.ApplicationServices.GetRequiredService<IDatabaseUpdateService>();
            //lifetime.ApplicationStarted.Register(() => databaseUpdateService.UpdateDatabase());
            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}
