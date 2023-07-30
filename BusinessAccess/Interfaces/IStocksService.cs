using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.Stocks;

namespace BusinessAccess.Interfaces
{
    public interface IStocksService
    {
        public Task<ServiceResponse<List<StockTransaction>>> GetCurrentStocks(int portfolioID);

        public Task<ServiceResponse<List<StockTransaction>>> GetAllDividends(int portfolioID);

        //public Task<ServiceResponse<List<StockTransaction>>> GetCurrentDividends(int portfolioID);
        //public Task<ServiceResponse<List<StockTransaction>>> GetRedeemedDividends(int portfolioID);

        public Task<ServiceResponse<List<StockTransaction>>> GetStockRedeems(int portfolioID);
    }
}
