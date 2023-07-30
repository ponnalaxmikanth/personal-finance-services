using System;
namespace Entities.Models.DTO.Funds
{
    public class Folio
    {
        public int ID { get; set; }
        public string Number { get; set; }
        public string FundHouse { get; set; }
        public int PortfolioID { get; set; }
        public string RegisteredOwner { get; set; }
        public bool IsDefaultFolio { get; set; }
    }
}
