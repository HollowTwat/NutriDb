using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
namespace NutriDbService
{
    public class Startup
    {
        //Scaffold-DbContext "Host=viaduct.proxy.rlwy.net;Port=38794;Username=postgres;Password=wTLZPRhYXHSReMKcUHSCNDEQlgQmbFDO;Database=railway" Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir DbModels -f
        public IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc();

            services.AddLogging();

            services.AddControllers().AddNewtonsoftJson();

            services.AddSwaggerGen();
            services.AddScoped<railwayContext>();
            //services.AddScoped<NutriDbContext>();
            services.AddScoped<MealHelper>();
            //    services.AddDbContext<RailwayContext>(options =>
            //options.UseNpgsql(Configuration.GetConnectionString("BloggingContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            // app.UseMvcWithDefaultRoute();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V-0.0.1_Release");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseCors(builder => builder
      //.WithOrigins("https://elaborate-seahorse-305700.netlify.app")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()
  );
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
        // This method gets called by the runtime. Use this method to configure endpoints
        public void Endpoints(IEndpointRouteBuilder builder)
        {
        }
    }
}
