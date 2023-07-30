using System;
namespace Entities.Models.DTO.Funds
{
    public class FundsDailyTrack
    {
        public int PortfolioID { get; set; }
        public string Portfolio { get; set; }
        public DateTime TrackDate { get; set; }
        public int NoOfFundHouses { get; set; }
        public int NoOfFolios { get; set; }
        public int NoOfFunds { get; set; }
        public decimal Investment { get; set; }
        public decimal Profit { get; set; }
        public decimal AbsoluteReturun { get; set; }
        public decimal XIRR { get; set; }
        public decimal Tax => (Profit > 100000) ? (Profit - 100000) * Convert.ToDecimal(0.1) : 0;
        public decimal TaxPercent => Tax / Investment * 100;
        public decimal ProfitAfterTax => Profit - Tax;
        public decimal PftaftTaxPer => ProfitAfterTax / Investment * 100;
    }
}
