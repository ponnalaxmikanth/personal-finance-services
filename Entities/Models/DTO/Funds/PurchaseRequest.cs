using System;
using System.Collections.Generic;

namespace Entities.Models.DTO.Funds
{
    public class MutualFundTransaction
    {
        public int TransactionID { get; set; }

        public int PortfolioID { get; set; }
        public string GroupOrder { get; set; }
        public int ITRN { get; set; }
        public int FolioID { get; set; }
        public decimal nav { get; set; }
        public decimal DividendPerNAV { get; set; }
        public int SchemaCode { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string FundOption { get; set; } //=> "Purchase";
        public decimal Units { get; set; }
        public decimal Amount { get; set; }
        public decimal Charges { get; set; }

        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TDS { get; set; }

        public int FinancialYear => PurchaseDate.Month < 4 ? PurchaseDate.Year - 1 : PurchaseDate.Year;
        public string FolioNumber { get; set; }
        public string PANCard { get; set; }

        //public string ProductCode { get; set; }
        //public string FundOption { get; set; }

        //public string Comments { get; set; }
        //public bool IsActive { get; set; }

        public decimal Dividend { get; set; }
        //public decimal DividendAmount { get; set; }

        //public decimal STT { get; set; }
        //public decimal StampDuty { get; set; }
        //public decimal TDS { get; set; }



        //public List<History> TransactionHistory { get; set; }
        //public decimal ActualNAV { get; set; }
    }

    //public class History
    //{
    //    public int PortfolioID { get; set; }
    //    public DateTime TransactionDate { get; set; }
    //    public string TransactionType { get; set; }
    //    public decimal? Amount { get; set; }

    //    public decimal? Dividend { get; set; }
    //    public decimal? DividendAmount { get; set; }

    //    public decimal? STT { get; set; }
    //    public decimal? StampDuty { get; set; }
    //    public decimal? TDS { get; set; }
    //    public string? Comments { get; set; }
    //}
}
