using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities.Models.DTO.Home
{
    public class HomeExpensesTracker
    {
        //public DateTime FromDate { get; set; }
        public string Group { get; set; }
        public string SubGroup { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Budget { get; set; }

        public decimal Remaining => (Group == "Income" && SubGroup == "Salary") ? Credit - Budget : Budget - Debit;
        public decimal UsedPercent => (Budget == 0 ? 0 : ((Group == "Income" && SubGroup == "Salary") ? Credit : Debit) / Budget) * 100; // (Group == "Income" && SubGroup == "Salary") ? (Budget == 0 ? 0 : Credit / Budget) * 100 : (Budget == 0 ? 0 : Debit / Budget) * 100;
        public decimal RemainingPer { get; set; }
        //public decimal BuedgetUsed => 
    }

    public class HomeTransactionsResponse
    {
        public List<HomeExpensesTracker> transactions { get; set; }

        public decimal Budget => transactions == null ? 0 : transactions.Where(t => t.Group != "Income" && t.SubGroup != "Salary")
            .Sum(t => t.Budget);

        public decimal Debit => transactions == null ? 0 : transactions.Where(t => t.Group != "Credit Card" && t.SubGroup != "Payment")
            .Where(t => t.Group != "Account" && t.SubGroup != "Transfer").Sum(t => t.Debit);

        public decimal Credit => transactions == null ? 0 : transactions.Where(t => t.Group != "Credit Card" && t.SubGroup != "Payment")
            .Where(t => t.Group != "Account" && t.SubGroup != "Transfer").Sum(t => t.Credit);
    }
}

