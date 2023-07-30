using System;
namespace Entities.Models.DTO.Stocks
{
    public class StockTransaction
    {
        public int TransactionID { get; set; }
        public int PortfolioID { get; set; }
        public string Quote { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Charges { get; set; }
        public decimal Dividend { get; set; }
        public string Currency { get; set; }
        public int FYYear { get; set; }

        public string TransactionType { get; set; }

        public decimal CurrentPrice { get; set; }
    }
}
