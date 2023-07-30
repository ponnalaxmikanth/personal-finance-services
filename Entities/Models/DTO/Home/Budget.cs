using System;
namespace Entities.Models.DTO.Home
{
    public class Budget
    {
        public int ID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Group { get; set; }
        public string SubGroup { get; set; }
        public decimal Amount { get; set; }
        public Boolean IsActive { get; set; }
    }
}

