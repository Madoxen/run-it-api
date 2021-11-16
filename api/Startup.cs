using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Configuration.Options;
using Api.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api
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
            services.Configure<FacebookOptions>(Configuration.GetSection("Authentication:Facebook"));
            services.Configure<AuthenticationOptions>(Configuration.GetSection("Authentication"));
            //TODO: write it nicer
            string DBConnectionString = Configuration["Database:ConnectionStringTemplate"];

            services.AddDbContext<ApiContext>(options => options.UseNpgsql(DBConnectionString));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "run_it_api", Version = "v1" });
            });

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:JWT:Key"]));
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //PLS REMEMBER THAT MATE WHEN DOING AUTHORIZATION

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                // here the cookie authentication option and other authentication providers will are added.
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Authentication:JWT:Issuer"], //TODO: change those with Configuration instead
                        ValidAudience = Configuration["Authentication:JWT:Audience"],
                        IssuerSigningKey = symmetricSecurityKey
                    };
                });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
                options.AddPolicy("TokenHasRefreshClaim", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim("scope", "Refresh");
                });
                options.AddPolicy("CheckUserIDResourceAccess", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new SameUserIDRequirement());
                });
            });

            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();

            services.AddHttpClient();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "run_it_api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
