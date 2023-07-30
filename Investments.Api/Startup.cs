using BusinessAccess.Interfaces;
using BusinessAccess.Services;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Investments.Api
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
            services.AddCors();
            services.AddControllers();

            services.AddLogging(options => options.AddSimpleConsole(
                c => c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] "));

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();

            services.AddDbContext<InvestmentContext>(item => item.UseMySQL(Configuration.GetConnectionString("investments")));

            services.AddTransient<IInvestmentsDataAccess, InvestmentsAccess>();
            services.AddTransient<ISQLHelper, SQLHelper>();
            services.AddTransient<IInvestmentsService, InvestmentsService>();

            services.AddTransient<IBSEIndicesService, BSEIndicesService>();
            services.AddTransient<IBSESensexDataAccess, BSESensexDataAccess>();

            services.AddTransient<IMutualFundsService, MutualFundsService>();
            services.AddTransient<IMutualFundsDataAccess, MutualFundsDataAccess>();

            services.AddTransient<IStocksService, StocksService>();
            services.AddTransient<IStocksDataAccess, StocksDataAccess>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors(builder =>
              builder.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
            );

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investments V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
