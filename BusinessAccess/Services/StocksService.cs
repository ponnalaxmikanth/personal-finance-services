using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BusinessAccess.Interfaces;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Stocks;
using Microsoft.Extensions.Logging;

namespace BusinessAccess.Services
{
    public class StocksService : IStocksService
    {
        private readonly ILogger<StocksService> _logger;
        private readonly IStocksDataAccess _iStocksDataAccess;

        public StocksService(ILogger<StocksService> logger, IStocksDataAccess iStocksDataAccess)
        {
            _logger = logger;
            _iStocksDataAccess = iStocksDataAccess;
        }

        public async Task<ServiceResponse<List<StockTransaction>>> GetCurrentStocks(int portfolioID)
        {
            var currentStocks = await _iStocksDataAccess.GetCurrentStocks(portfolioID);

            var response = MapCurrentStocks(currentStocks);

            if (response == null)
                return new ServiceResponse<List<StockTransaction>>(false, "Failed to get Current Stocks", null);

            return new ServiceResponse<List<StockTransaction>>(true, "Succes", response);
        }

        private List<StockTransaction> MapCurrentStocks(ServiceResponse<DataTable> currentStocks)
        {
            try
            {
                if (currentStocks != null && currentStocks.Success && currentStocks.ResponseObject != null)
                {
                    return (from t in currentStocks.ResponseObject.AsEnumerable()
                            select new StockTransaction()
                            {
                                TransactionID = Conversions.ToInt(t["TransactionID"], 0),
                                PortfolioID = Conversions.ToInt(t["PortfolioID"], 0),
                                FYYear = Conversions.ToInt(t["FYYear"], 0),

                                Quantity = Conversions.ToDecimal(t["Quantity"], 0),
                                Price = Conversions.ToDecimal(t["Price"], 0),
                                Charges = Conversions.ToDecimal(t["Charges"], 0),
                                Dividend = Conversions.ToDecimal(t["Dividend"], 0),

                                Quote = Conversions.ToString(t["Quote"], ""),
                                Currency = Conversions.ToString(t["Currency"], ""),

                                PurchaseDate = Conversions.ToDateTime(t["PurchaseDate"], DateTime.Now),

                                TransactionType = "I",

                                CurrentPrice = Conversions.ToDecimal(t["Price"], 0)
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to map Current Stocks Exception: {ex.Message}");
            }
            return null;
        }



        public async Task<ServiceResponse<List<StockTransaction>>> GetAllDividends(int portfolioID)
        {
            var currentStocks = await _iStocksDataAccess.GetDividends(portfolioID);

            var response = MapDividends(currentStocks);

            if (response == null)
                return new ServiceResponse<List<StockTransaction>>(false, "Failed to get Current Dividends", null);

            return new ServiceResponse<List<StockTransaction>>(true, "Succes", response);
        }

        private List<StockTransaction> MapDividends(ServiceResponse<DataTable> dividends)
        {
            try
            {
                if (dividends != null && dividends.Success && dividends.ResponseObject != null)
                {
                    return (from t in dividends.ResponseObject.AsEnumerable()
                            select new StockTransaction()
                            {
                                TransactionID = Conversions.ToInt(t["TransactionID"], 0),
                                PortfolioID = Conversions.ToInt(t["PortfolioID"], 0),
                                FYYear = Conversions.ToInt(t["FYYear"], 0),

                                Quantity = Conversions.ToDecimal(t["Quantity"], 0),
                                Price = Conversions.ToDecimal(t["Price"], 0),
                                Dividend = Conversions.ToDecimal(t["Dividend"], 0),

                                Quote = Conversions.ToString(t["Quote"], ""),
                                Currency = Conversions.ToString(t["Currency"], ""),

                                PurchaseDate = Conversions.ToDateTime(t["DividendDate"], DateTime.Now),

                                TransactionType = "D",

                                CurrentPrice = Conversions.ToDecimal(t["Price"], 0)
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to map Current Stocks Exception: {ex.Message}");
            }
            return null;
        }



        public async Task<ServiceResponse<List<StockTransaction>>> GetStockRedeems(int portfolioID)
        {
            var redeemStocks = await _iStocksDataAccess.GetStockRedeems(portfolioID);

            List<StockTransaction> redeemInvests = MapRedeemInvests(redeemStocks);
            List<StockTransaction> redeems = MapRedeems(redeemStocks);

            List<StockTransaction> response = redeemInvests == null ? null : redeemInvests;

            response.AddRange(redeems);

            if (response == null)
                return new ServiceResponse<List<StockTransaction>>(false, "Failed to get Redeem Stocks", null);

            return new ServiceResponse<List<StockTransaction>>(true, "Succes", response);
        }

        private List<StockTransaction> MapRedeems(ServiceResponse<DataTable> redeemStocks)
        {
            try
            {
                if (redeemStocks != null && redeemStocks.Success && redeemStocks.ResponseObject != null)
                {
                    var response = (from t in redeemStocks.ResponseObject.AsEnumerable()
                            select new StockTransaction()
                            {
                                TransactionID = Conversions.ToInt(t["TransactionID"], 0),
                                PortfolioID = Conversions.ToInt(t["PortfolioID"], 0),
                                //FYYear = Conversions.ConvertToInt(t["FYYear"], 0),

                                Quantity = Conversions.ToDecimal(t["Quantity"], 0),
                                Price = Conversions.ToDecimal(t["SellPrice"], 0),
                                Charges = Conversions.ToDecimal(t["SellCharges"], 0),
                                Dividend = Conversions.ToDecimal(t["Dividend"], 0),

                                Quote = Conversions.ToString(t["Quote"], ""),
                                Currency = Conversions.ToString(t["Currency"], ""),

                                PurchaseDate = Conversions.ToDateTime(t["SellDate"], DateTime.Now),
                                FYYear = Conversions.GetFinancialYear(Conversions.ToDateTime(t["SellDate"], DateTime.Now)),
                                TransactionType = "R",

                                CurrentPrice = Conversions.ToDecimal(t["Price"], 0)
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to map Redeems Stocks Exception: {ex.Message}");
            }
            return null;
        }

        private List<StockTransaction> MapRedeemInvests(ServiceResponse<DataTable> redeemStocks)
        {
            try
            {
                if (redeemStocks != null && redeemStocks.Success && redeemStocks.ResponseObject != null)
                {
                    return (from t in redeemStocks.ResponseObject.AsEnumerable()
                            select new StockTransaction()
                            {
                                TransactionID = Conversions.ToInt(t["TransactionID"], 0),
                                PortfolioID = Conversions.ToInt(t["PortfolioID"], 0),
                                FYYear = Conversions.GetFinancialYear(Conversions.ToDateTime(t["PurchaseDate"], DateTime.Now)),

                                Quantity = Conversions.ToDecimal(t["Quantity"], 0),
                                Price = Conversions.ToDecimal(t["Price"], 0),
                                Charges = Conversions.ToDecimal(t["Charges"], 0),
                                Dividend = Conversions.ToDecimal(t["Dividend"], 0),

                                Quote = Conversions.ToString(t["Quote"], ""),
                                Currency = Conversions.ToString(t["Currency"], ""),

                                PurchaseDate = Conversions.ToDateTime(t["PurchaseDate"], DateTime.Now),

                                TransactionType = "RI",

                                CurrentPrice = Conversions.ToDecimal(t["Price"], 0)
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to map Redeem Invest Stocks Exception: {ex.Message}");
            }
            return null;
        }

        

    }
}
