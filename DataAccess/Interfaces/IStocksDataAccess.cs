using System;
using System.Data;
using System.Threading.Tasks;
using Entities.Models.DTO;

namespace DataAccess.Interfaces
{
    public interface IStocksDataAccess
    {
        Task<ServiceResponse<DataTable>> GetCurrentStocks(int portfolioID);

        Task<ServiceResponse<DataTable>> GetDividends(int portfolioID);

        Task<ServiceResponse<DataTable>> GetStockRedeems(int portfolioID);
    }
}
