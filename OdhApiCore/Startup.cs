using AspNetCore.CacheOutput.InMemory.Extensions;
using AspNetCoreRateLimit;
using Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OdhApiCore.Controllers;
using OdhApiCore.Factories;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using SqlKata.Compilers;
using SqlKata.Execution;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OdhApiCore
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDefaultIdentity<IdentityUser>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddHttpClient("mss", client =>
            {
                //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
            }).ConfigureHttpMessageHandlerBuilder(config => new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            services.AddHttpClient("lcs"); // TODO: put LCS config here            

            //Adding Cache Service in Memory
            services.AddInMemoryCacheOutput();
            services.AddSingleton<CustomCacheKeyGenerator>();

            //Adding Quota Service in Memory https://github.com/stefanprodan/AspNetCoreRateLimit
            services.AddMemoryCache();

            //ClientRateLimit
            services.Configure<ClientRateLimitOptions>(options =>
            {
                // Enable "global" limit, not per endpoint
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.ClientIdHeader = "Referer";
                //NOT Overwritten by ClientRateLimitPolicies
                //options.GeneralRules = new List<RateLimitRule>
                //{
                //    new RateLimitRule()
                //    {
                //        Endpoint = "get:/v1",
                //                Period = "1m",
                //                Limit = 10
                //    }
                //};
            });
            //ClientRateLimitPolicies
            services.Configure<ClientRateLimitPolicies>(options =>
            {
                options.ClientRules = new List<ClientRateLimitPolicy>
                {
                    new ClientRateLimitPolicy()
                    {
                        ClientId = "Anonymous",
                        Rules = new List<RateLimitRule>()
                        {
                            new RateLimitRule()
                            {
                                Endpoint = "get:/v1",
                                Period = "1m",
                                Limit = 30
                            }
                        }
                    },
                    new ClientRateLimitPolicy()
                    {
                        ClientId = "Authenticated",
                        Rules = {
                            new RateLimitRule()
                            {
                                Endpoint = "get:/v1",
                                Period = "1m",
                                Limit = 60
                            }
                        }
                    }
                };
            });


            services.Configure<IpRateLimitOptions>(options =>
            {
                // Enable "global" limit, not per endpoint
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.ClientIdHeader = "Referer";
                options.ClientWhitelist = new List<string> { "Anonymous", "Authenticated" };
                //General Rule from 
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule()
                    {
                        Endpoint = "get:/v1",
                                Period = "1m",
                                Limit = 10
                    }
                };
            });

            ////IpRateLimitPolicies
            //services.Configure<IpRateLimitPolicies>(options =>
            //{
            //    options.IpRules = new List<IpRateLimitPolicy>
            //    {
            //        new IpRateLimitPolicy()
            //        {
            //            Ip = "127.0.0.1",
            //            Rules = new List<RateLimitRule>()
            //            {
            //                new RateLimitRule()
            //                {
            //                    Endpoint = "get:/v1",
            //                    Period = "5m",
            //                    Limit = 10
            //                }
            //            }
            //        }
            //        //new IpRateLimitPolicy()
            //        //{
            //        //    Ip = "Authenticated",
            //        //    Rules = {
            //        //        new RateLimitRule()
            //        //        {
            //        //            Endpoint = "get:/v1",
            //        //            Period = "10m",
            //        //            Limit = 10
            //        //        }
            //        //    }
            //        //}
            //    };
            //});


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            //services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();


            services.AddInMemoryRateLimiting();

            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();                        
            services.AddSingleton<IRateLimitConfiguration, Middleware.OdhRateLimitConfiguration>();            

            services.AddLogging(options =>
            {
                options.ClearProviders();

                var levelSwitch = new LoggingLevelSwitch
                {
                    MinimumLevel =
                        CurrentEnvironment.IsDevelopment() ?
                            LogEventLevel.Debug :
                            LogEventLevel.Warning
                };
                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "{Message}{NewLine}")
                    .WriteTo.Debug()
                    //.WriteTo.Elasticsearch(
                    //    new ElasticsearchSinkOptions() {
                    //        AutoRegisterTemplate = true,
                    //        IndexFormat = "odh-tourism-{0:yyyy.MM}",
                    //        //ModifyConnectionSettings = (c) => c.GlobalHeaders(new NameValueCollection { { "Authorization", "Basic " + loggerconfig.elkbasicauthtoken } }),
                    //        FailureCallback = e => System.Console.Error.WriteLine("Unable to submit event " + e.MessageTemplate),
                    //        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                    //                           EmitEventFailureHandling.WriteToFailureSink |
                    //                           EmitEventFailureHandling.RaiseCallback,
                    //        //FailureSink = new FileSink(loggerconfig.filepathfailures, new JsonFormatter(), null),
                    //        MinimumLogEventLevel = LogEventLevel.Information
                    //    }
                    //)
                    .CreateLogger();
                options.AddSerilog(loggerConfiguration, dispose: true);

                // Configure Serilogs own configuration to use
                // the configured logger configuration.
                // This allows to Log via Serilog's Log and ILogger.
                Log.Logger = loggerConfiguration;
            });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression( options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddCors(o =>
            {
                o.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .SetIsOriginAllowed(hostName => true);
                });
            });
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddRazorPages();

            services.AddSingleton<ISettings, Settings>();
            services.AddScoped<QueryFactory, PostgresQueryFactory>();
      
            //Initialize JWT Authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.Authority = Configuration.GetSection("OauthServerConfig").GetValue<string>("Authority");                    
                    //jwtBearerOptions.Audience = "account";                
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidIssuer = Configuration.GetSection("OauthServerConfig").GetValue<string>("Authority"),
                        ValidateIssuer = true
                    };
                    jwtBearerOptions.Events = new JwtBearerEvents()
                    {     
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();

                            c.Response.StatusCode = 401;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync("");
                        },                        
                    };
                });

            services.AddMvc(options =>
                {
                    options.OutputFormatters.Add(new Formatters.CsvOutputFormatter());
                    options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");

                    options.OutputFormatters.Add(new Formatters.JsonLdOutputFormatter());
                    options.FormatterMappings.SetMediaTypeMappingForFormat("json-ld", "application/ldjson");

                    options.OutputFormatters.Add(new Formatters.RawdataOutputFormatter());
                    options.FormatterMappings.SetMediaTypeMappingForFormat("rawdata", "application/rawdata");
                });
                //.AddJsonOptions(options =>
                //{
                //    options.JsonSerializerOptions.PropertyNameCaseInsensitive = new DefaultContractResolver();
                //});
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "OdhApi Tourism .Net Core", 
                    Version = "v1",
                    Description = "ODH Tourism Api based on .Net Core with PostgreSQL",
                    TermsOfService = new System.Uri("https://opendatahub.readthedocs.io/en/latest/"),
                    Contact = new OpenApiContact
                    {
                        Name = "Open Data Hub Team",
                        Email = "help@opendatahub.bz.it",
                        Url = new System.Uri("https://opendatahub.bz.it/"),
                    },
                });                               
                c.MapType<LegacyBool>(() => new OpenApiSchema
                {
                    Type = "boolean"
                });
                c.MapType<PageSize>(() => new OpenApiSchema
                {
                    Type = "integer"
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);                

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                           TokenUrl = new Uri(Configuration.GetSection("OauthServerConfig").GetValue<string>("Authority") + "protocol/openid-connect/token")
                        },
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(Configuration.GetSection("OauthServerConfig").GetValue<string>("Authority") + "protocol/openid-connect/token")
                        }
                    },
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.OperationFilter<AuthenticationRequirementsOperationFilter>();
                c.EnableAnnotations();                       
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "bearer"
                //            }
                //        }, new List<string>()
                //    }
                //});
            });
            services.AddSwaggerGenNewtonsoftSupport();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;                
            });

            //services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            // TODO: Move to Production
            app.UseClientRateLimiting();
            app.UseIpRateLimiting();
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseResponseCompression();
            //app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                },                
            });

            app.UseRouting();

            //app.UseCookiePolicy();

            //Important! Register Cors Policy before Using Authentication and Authorization
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
                });                
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ODH Tourism API V1");
                c.RoutePrefix = "swagger";
                c.OAuthClientSecret("");
                c.OAuthRealm("noi");
                c.EnableDeepLinking();
            });

           
            //LOG EVERY REQUEST WITH HEADERs
            app.Use(async (context, next) =>
            {
                //If Root is requested forward to Databrowser (Compatibility reason)
                if(String.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path.Value == "/")
                {  
                    if(context.Request.Host.ToString().Equals("tourism.opendatahub.bz.it"))
                    {
                        context.Response.Redirect(Configuration.GetSection("DataBrowserConfig").GetValue<string>("Url"));
                        return;
                    }
                    else
                    {
                        context.Response.Redirect("/swagger");
                        return;
                    }
                }

                else if (context.Request.Path.Value == "/api")
                {
                    context.Response.Redirect("/v1");
                    return;
                }

                else if(context.Request.Path.Value.StartsWith("/swagger/ui/index"))
                {
                    context.Response.Redirect("/swagger");
                    return;
                }
                Stopwatch requesttime = new Stopwatch();
                requesttime.Start();

                await next();

                //Log only if api is requested! including HTTP Statuscode therefore after await next();
                //if(context.Request.Path.StartsWithSegments("/v1/", StringComparison.OrdinalIgnoreCase))
                if (!String.IsNullOrEmpty(context.Request.Path.Value) && context.Request.Path.Value.StartsWith("/v1", StringComparison.OrdinalIgnoreCase))
                {                   
                    //TODO Make a Referer Class/Method for the logic
                    var referer = "not provided";

                    if (context.Request.Headers.ContainsKey("Referer"))
                        referer = context.Request.Headers["Referer"].ToString();
                    else
                    {
                        //Search the QS for Referer
                        if (context.Request.Query.ContainsKey("Referer"))
                            referer = context.Request.Query["Referer"].ToString();
                    }

                    //Quick Fix, Android is passing http://localhost/ as referer
                    if(referer == "http://localhost/" && context.Request.Query.ContainsKey("Referer"))
                        referer = context.Request.Query["Referer"].ToString();

                    //Origin
                    var origin = "not provided";
                    if (context.Request.Query.ContainsKey("Origin"))
                        origin = context.Request.Query["Origin"].ToString();

                    //User Agent
                    var useragent = "not provided";
                    if (context.Request.Headers.ContainsKey("User-Agent"))
                        useragent = context.Request.Headers["User-Agent"].ToString();
                   
                    var urlparameters = context.Request.QueryString.Value != null ? context.Request.QueryString.HasValue ? context.Request.QueryString.Value.Replace("?", "") : "" : "";

                    //To check
                    var remoteip = RemoteIpHelper.GetRequestIP(context, true);

                    //Request Length                    
                    requesttime.Stop();

                    HttpRequestLog httplog = new HttpRequestLog()
                    {
                        host = context.Request.Host.ToString(),
                        path = context.Request.Path.ToString(),
                        urlparams = urlparameters, //.Replace("&", "-"),  //Helper.StringHelpers.GenerateDictionaryFromQuerystring(context.Request.QueryString.ToString()),
                        referer = referer,
                        schema = context.Request.Scheme,
                        useragent = useragent,
                        username = context.User.Identity != null ? context.User.Identity.Name != null ? context.User.Identity.Name.ToString() : "anonymous" : "anonymous",
                        ipaddress = remoteip,
                        statuscode = context.Response.StatusCode,
                        origin = origin,
                        elapsedtime = requesttime.ElapsedMilliseconds
                    };
                    LogOutput<HttpRequestLog> logoutput = new LogOutput<HttpRequestLog>() { id = "", type = "HttpRequest", log = "apiaccess", output = httplog };

                    Console.WriteLine(JsonConvert.SerializeObject(logoutput));
                    
                }
            });

            //REWRITE, REDIRECT RULES
            //var rwoptions = new RewriteOptions()
            //    .AddRedirect("api/(.*)", "v1/$1");
            //.AddRedirectToHttpsPermanent();
            //.AddRedirect("redirect-rule/(.*)", "redirected/$1")
            //.AddRewrite(@"^rewrite-rule/(\d+)/(\d+)", "rewritten?var1=$1&var2=$2",
            //skipRemainingRules: true)

            app.UseRewriter(
                new RewriteOptions()
                .AddRedirect("api/(.*)", "v1/$1")
                //.AddRewrite(@"^(?=/api)", "/v1", skipRemainingRules: true)
                );

            //Not needed at moment
            //app.UseHttpContext();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");                
            });
        }
    }

    public class AuthenticationRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();


            var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } };
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [scheme] = new List<string>()
            });
        }
    }

}
