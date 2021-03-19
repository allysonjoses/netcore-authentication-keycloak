using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Marktplace.Backoffice
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
            services.AddControllers();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = bool.Parse(Configuration["Authentication:RequireHttpsMetadata"]);
                options.Authority = Configuration["Authentication:Authority"];
                options.IncludeErrorDetails = bool.Parse(Configuration["Authentication:IncludeErrorDetails"]);
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = bool.Parse(Configuration["Authentication:ValidateAudience"]),
                    ValidAudience = Configuration["Authentication:ValidAudience"],
                    ValidateIssuerSigningKey = bool.Parse(Configuration["Authentication:ValidateIssuerSigningKey"]),
                    ValidateIssuer = bool.Parse(Configuration["Authentication:ValidateIssuer"]),
                    ValidIssuer = Configuration["Authentication:ValidIssuer"],
                    ValidateLifetime = bool.Parse(Configuration["Authentication:ValidateLifetime"])
                };

                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = e =>
                    {
                        e.NoResult();
                        e.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
