using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Infrastructure;
using Infrastructure.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SessionStore;

namespace Client
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
	        services.AddAuthentication(options =>
	        {
		        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
	        }).AddCookie( options =>
		        {
			        options.LogoutPath = "/User/Logout";
			        options.AccessDeniedPath = "/User/AccessDenied";
			        //options.SessionStore = new MySessionStore();
				}
	        ).AddOpenIdConnect(options =>
		        {
					options.AccessDeniedPath = "/User/AccessDenied";
					options.Authority = _configuration["openid:authority"];
					options.BackchannelHttpHandler = new BackChannelListener();
					options.BackchannelTimeout = TimeSpan.FromSeconds(5);
					options.ClientId = _configuration["openid:clientid"];
					options.ClientSecret = "mysecret";
			        options.ResponseType = "code";
			        options.Scope.Clear();
			        options.Scope.Add("openid");
			        options.Scope.Add("profile");
			        options.Scope.Add("email");
			        options.Scope.Add("employee");
			        options.Scope.Add("payment");
					options.Scope.Add("offline_access");
			        options.GetClaimsFromUserInfoEndpoint = true;
			        options.SaveTokens = true;
			        options.Prompt = "consent";

			        options.TokenValidationParameters = new TokenValidationParameters
			        {
				        NameClaimType = JwtClaimTypes.Name,
				        RoleClaimType = JwtClaimTypes.Role
			        };
				}
	        );

	        if (_environment.EnvironmentName != "Offline")
		        services.AddDataProtectionWithSqlServerForClient(_configuration);

	        services.AddTransient<SerilogHttpMessageHandler>();

			services.AddHttpClient("paymentapi", client =>
	        {
		        client.BaseAddress = new Uri(_configuration["paymentApiUrl"]);
		        client.Timeout = TimeSpan.FromSeconds(5);
		        client.DefaultRequestHeaders.Add("Accept", "application/json");
	        }).AddHttpMessageHandler<SerilogHttpMessageHandler>();

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

            app.UseStaticFiles();

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
