using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentAPI
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        if (_environment.EnvironmentName != "Offline")
		        services.AddDataProtectionWithSqlServerForPaymentApi(_configuration);

	        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
	        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		        .AddJwtBearer(opt =>
		        {
			        opt.Authority = _configuration["openid:authority"];
			        opt.Audience = "paymentapi";
			        opt.TokenValidationParameters.RoleClaimType = "roles";
			        opt.TokenValidationParameters.NameClaimType = "name";
                    opt.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(60);
			        // IdentityServer emits a type header by default, recommended extra check
			        opt.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
		        });

            services.AddHsts(opts =>
	        {
		        opts.IncludeSubDomains = true;
		        opts.MaxAge = TimeSpan.FromSeconds(15768000);
	        });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
	        if (!env.IsProduction())
	        {
		        app.UseDeveloperExceptionPage();
	        }
	        else
	        {
		        app.UseHsts();
		        app.UseExceptionHandler("/Home/Error");
	        }

            app.UseHttpsRedirection();

            app.UseSecurityHeaders();

            app.UseRouting();

            app.UseRequestLocalization(
	            new RequestLocalizationOptions()
		            .SetDefaultCulture("se-SE"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
