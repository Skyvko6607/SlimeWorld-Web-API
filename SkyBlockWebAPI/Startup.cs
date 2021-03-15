using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkyBlockWebAPI.Configs;
using SkyBlockWebAPI.DataObjects;
using SkyBlockWebAPI.DbContexts;
using SkyBlockWebAPI.Repositories;
using SkyBlockWebAPI.Repositories.Interfaces;
using SkyBlockWebAPI.Services;
using SkyBlockWebAPI.Services.Interfaces;
using SkyBlockWebAPI.Tasks;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SkyBlockWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<GlobalDbContext>();
            services.AddSingleton<SlimeWorldTask>();
            services.AddSingleton<SlimeWorldDataObject>();

            services.AddScoped<ISlimeWorldRepository, SlimeWorldRepository>();
            services.AddScoped<ISlimeWorldService, SlimeWorldService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SlimeWorld API Interface",
                    Version = "v1",
                    Description = string.Empty
                });

                c.AddSecurityDefinition("ApiToken", new OpenApiSecurityScheme
                {
                    Description = "Api Token using custom token scheme.",
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
                                Id = "ApiToken"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors();

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseMvc();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SlimeWorld API V1");
                c.DocExpansion(DocExpansion.None);
                c.RoutePrefix = string.Empty;
            });

            app.ApplicationServices.GetService(typeof(SlimeWorldTask));
        }
    }
}