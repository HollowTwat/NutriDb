using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nutri2Service.Models;
using System.Configuration;
namespace BerryBuyerBuilder
{
    public class Startup
    {
        //Scaffold-DbContext "Server=31.31.196.234;Database=u1495815_default;Port=3306;User=u1495815_mariad;Password=a1Az0o9jOBiivd4C;ssl mode=None" Pomelo.EntityFrameworkCore.MySql -OutputDir .\Models\DBModels -f
        //server=localhost;database=u1495815_default;port=3306;user=u1495815_mariad;password=a1Az0o9jOBiivd4C;ssl mode=None
        //server=localhost;database=u1495815_BerryDb;port=3306;user=u1495815_mariad;password=a1Az0o9jOBiivd4C;ssl mode=None
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
            services.AddTransient<RailwayContext>();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V-0.2.8_Release");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseCors(builder => builder
      .WithOrigins("https://elaborate-seahorse-305700.netlify.app")
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
