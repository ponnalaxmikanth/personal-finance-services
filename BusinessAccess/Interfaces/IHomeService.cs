using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.Home;
using Microsoft.Extensions.Logging;

namespace BusinessAccess.Interfaces
{
    public interface IHomeService
    {
        public Task<ServiceResponse<List<Account>>> Accounts();
        public Task<ServiceResponse<List<Budget>>> Budgets(DateTime fromDate, DateTime toDate);

        public Task<ServiceResponse<int>> SaveTransactions(DateTime minDate);

        public Task<ServiceResponse<HomeTransactionsResponse>> GetTransactions(DateTime fromDate, DateTime toDate);
    }
}
