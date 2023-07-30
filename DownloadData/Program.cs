using System;
using System.Collections.Generic;
using BusinessAccess.Interfaces;
using BusinessAccess.Services;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Services;
using DownloadData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Excel.FinancialFunctions;
using DownloadData.Services;

namespace DownloadData
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Application Started....");
            // /Users/laxmikanthponna/downloads/MFInvestments.xlsx

            //setup our DI
            // var serviceProvider = new ServiceCollection();

            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();


            //List<double> valList = new List<double>();
            //valList.Add(-5000);
            //valList.Add(-5000);
            //valList.Add(-5000);
            //valList.Add(-5000);
            //valList.Add(-5000);
            //valList.Add(-5000);
            //valList.Add(-5000);

            //valList.Add(51144.92885);
            //List<DateTime> dtList = new List<DateTime>();
            //dtList.Add(new DateTime(2017, 9, 18));
            //dtList.Add(new DateTime(2017, 10, 9));
            //dtList.Add(new DateTime(2018, 4, 27));
            //dtList.Add(new DateTime(2018, 7, 17));

            //dtList.Add(new DateTime(2018, 12, 4));
            //dtList.Add(new DateTime(2022, 1, 5));
            //dtList.Add(new DateTime(2022, 2, 1));


            //dtList.Add(new DateTime(2022, 2, 9));
            //double result = Financial.XIrr(valList, dtList);
            //Console.WriteLine(result);
            //Console.ReadLine();

            // calls the Run method in App, which is replacing Main
            serviceProvider.GetService<Client>().Run(args);

            // ConfigureServices(serviceProvider);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            
            ConfigureServices(services);
            return services;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(options => options.AddSimpleConsole(
                c => c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] "));
            // required to run the application
            services.AddTransient<Client>();
            services.AddTransient<IInvestmentsDataAccess, InvestmentsAccess>();
            services.AddTransient<ISQLHelper, SQLHelper>();
            services.AddTransient<IInvestmentsService, InvestmentsService>();

            services.AddTransient<IBSEIndicesService, BSEIndicesService>();
            services.AddTransient<IBSESensexDataAccess, BSESensexDataAccess>();
            services.AddTransient<IInitializeFundsDataAccess, InitializeFundsDataAccess>();
            services.AddTransient<IInitializeFundInvestments, InitializeFundInvestments>();

            services.AddTransient<IUSAccounts, USAccounts>();
        }
    }
}
