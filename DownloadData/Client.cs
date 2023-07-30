using System;
using DownloadData.Interfaces;

namespace DownloadData
{
    public class Client
    {
        public readonly IInitializeFundInvestments _initializeFundInvestments;
        public readonly IUSAccounts _iUSAccounts;

        public Client(IInitializeFundInvestments initializeFundInvestments, IUSAccounts iUSAccounts)
        {
            _initializeFundInvestments = initializeFundInvestments;
            _iUSAccounts = iUSAccounts;
        }

        public void Run(string[] args)
        {
            switch (args[0])
            {
                case "InitFunds":
                    _initializeFundInvestments.Initialize("/Users/laxmikanthponna/downloads/Investments_04.04.2022.xlsx");
                    break;

                case "UsAccounts":
                    _iUSAccounts.SaveAccountTransactions("/Users/laxmikanthponna/Documents/Kanth-US.xlsx");
                    break;

                case "XIRR":
                    XIRR xirr = new XIRR();
                    double[] payments = { -6800, 1000, 2000, 4000 }; // payments
                    double[] days = { 01, 08, 16, 25 }; // days of payment (as day of year)
                    double xirrValue = xirr.Newtons_method(0.1, xirr.total_f_xirr(payments, days), xirr.total_df_xirr(payments, days));

                    Console.WriteLine("XIRR value is {0}", xirrValue);
                    break;
            }
        }
    }
}
