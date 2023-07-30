using System;
namespace Entities.Models.DTO.Funds
{
    public class FundDividend
    {
        public int PortfolioID { get; set; }
        public int FolioID { get; set; }
        public string FolioNumber { get; set; }
        public int SchemaCode { get; set; }
        public DateTime DividendDate { get; set; }
        public decimal Units { get; set; }
        public decimal NAV { get; set; }
        public string DividendType { get; set; }
        
        public decimal Amount { get; set; }
        public decimal DividendNAV { get; set; }
        public decimal Charges { get; set; }
        public decimal TDS { get; set; }
        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
    }
}
