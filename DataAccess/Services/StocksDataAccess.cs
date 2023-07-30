using System;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services
{
    public class StocksDataAccess : IStocksDataAccess
    {
        private readonly ILogger<StocksDataAccess> _logger;
        private readonly ISQLHelper _sQLHelper;
        public StocksDataAccess(ILogger<StocksDataAccess> logger, ISQLHelper sQLHelper)
        {
            _logger = logger;
            _sQLHelper = sQLHelper;
        }

        public async Task<ServiceResponse<DataTable>> GetCurrentStocks(int portfolioID)
        {
            _logger.LogInformation($"Getting Current Stocks Portfolio ID: {portfolioID}");

            string whereClause = GetPortfolioClause(portfolioID, "s");

            string statement = @"select TransactionID, PortfolioID, Quote, PurchaseDate, Quantity, Price, Charges, Dividend, Currency, FYYear
                from Stocks s " + (string.IsNullOrWhiteSpace(whereClause) ? "" : " where " + whereClause);

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetDividends(int portfolioID)
        {
            _logger.LogInformation($"Getting Stocks Dividends ID: {portfolioID}");

            string whereClause = GetPortfolioClause(portfolioID, "d");

            string statement = @"select TransactionID, PortfolioID, Quote, DividendDate, Quantity, Price, Dividend, Currency, FYYear
                from StocksDividends d " + (string.IsNullOrWhiteSpace(whereClause) ? "" : " where " + whereClause);

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        public async Task<ServiceResponse<DataTable>> GetStockRedeems(int portfolioID)
        {
            _logger.LogInformation($"Getting Stocks Redeems ID: {portfolioID}");

            string whereClause = GetPortfolioClause(portfolioID, "r");

            string statement = @"select TransactionID, PortfolioID, Quote, PurchaseDate, Quantity, Price, Charges, Dividend, SellDate,
                    SellPrice, SellCharges, Currency
                from StocksRedeems r " + (string.IsNullOrWhiteSpace(whereClause) ? "" : " where " + whereClause);

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }

        private string GetPortfolioClause(int portfolioID, string alias, string field = "PortfolioID")
        {
            switch (portfolioID)
            {
                case -1:
                    return string.Empty;
                case 92:
                    return alias + "." + field + " in (1, 4)";
                case 91:
                    return alias + "." + field + " in (6, 7)";
                case 99:
                    return alias + "." + field + " in (1, 4, 6, 7)";
                default:
                    return alias + "." + field + " = " + portfolioID.ToString();
            }
        }
    }
}
