using System;
namespace Entities.Models.DTO.Home
{
    public class Transaction
    {
        public int RowIndex { get; set; }
        public int AccountID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal Total { get; set; }
        public string TransactedBy { get; set; }
        public string Group { get; set; }
        public string SubGroup { get; set; }
        public string Comments { get; set; }
    }
}

