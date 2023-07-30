using System;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class FundsNav
    {
        [Key]
        public int SchemaCode { get; set; }
        public DateTime NAVDate { get; set; }
        public string FundHouse { get; set; }
        public string? ISINGrowth { get; set; }
        public string? ISINDivReinvestment { get; set; }
        public string SchemaName { get; set; }
        public string FundOption { get; set; }
        public string FundType { get; set; }
        public string Classification { get; set; }
        public string Category { get; set; }
        public decimal NAV { get; set; }
    }
}