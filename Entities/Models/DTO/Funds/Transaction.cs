using System;
using System.Collections.Generic;

namespace Entities.Models.DTO.Funds
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public int PortfolioID { get; set; }
        public string Portfolio { get; set; }

        public string FolioNumber { get; set; }
        public int FolioID { get; set; }

        public string PanCard { get; set; }
        public int FinancialYear => TransactionDate.Month < 4 ? TransactionDate.Year - 1 : TransactionDate.Year;

        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }
        public decimal PurchaseNAV { get; set; }
        public decimal Units { get; set; }
        public decimal Dividend => DividendPerNAV * Units;

        public decimal ActualNAV => PurchaseNAV - DividendPerNAV;
        public decimal RedeemNAV { get; set; }
        public decimal Profit { get; set; }
        public decimal CurrentNAV { get; set; }
        public decimal DividendPerNAV { get; set; }
        
        //public decimal Charges { get; set; }
        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TDS { get; set; }

        public int WithHoldDays { get; set; }
        public decimal XIRR { get; set; }

        public FundDetails FundDetails { get; set; }

        public List<Transaction> History { get; set; }
    }

    public class FundDetails
    {
        public string House { get; set; }
        public string FundType { get; set; }
        public string Classification { get; set; }
        public string Category { get; set; }

        public int SchemaCode { get; set; }
        public string SchemaName { get; set; }
        public string FundOption { get; set; }
    }

    public class FundInvestments
    {
        public FundStats Overall { get; set; }
        public FundStats CurrentInvestment { get; set; }
        public FundStats Redeems { get; set; }

        public List<Transaction> RedeemTransactions { get; set; }
        public List<Transaction> CurrentTransactions { get; set; }
    }

    public class FundStats
    {
        public decimal Investment { get; set; }
        public decimal CurrentValue { get; set; }

        public decimal Profit => CurrentValue - Investment;
        public decimal ProfitPer => Investment == 0 ? 0 : Profit / Investment * 100;
        public decimal XIRR { get; set; }
    }

    public enum FundTransactionTypes
    {
        Investment = 0,
        Payout = 1,
        Redeem = 2,
        RedeemInvest = 3,
        SwitchOut = 4,
        SwitchIn = 5,
        ReInvest = 6,

        Purchase = 7,
        Switch = 8,
        Reinvest = 9
    }
}
