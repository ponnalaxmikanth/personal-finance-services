using System;
namespace Entities.Models.DTO.Funds
{
    public class HistoryRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class FundHistory
    {
        public int SchemaCode { get; set; }
        public decimal NAV { get; set; }
        public DateTime NAVDate { get; set; }
    }
}
