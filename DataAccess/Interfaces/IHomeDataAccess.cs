using System;
using Entities.Models.DTO;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Entities.Models.DTO.Home;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Interfaces
{
    public interface IHomeDataAccess
    {
        Task<ServiceResponse<DataTable>> GetAccounts();

        Task<ServiceResponse<DataTable>> GetBudgets(DateTime fromDate, DateTime toDate);

        Task<ServiceResponse<int>> SaveTransactions(List<Transaction> transactions);

        Task<ServiceResponse<DataTable>> GetTransactions(DateTime fromDate, DateTime toDate);
    }
}
