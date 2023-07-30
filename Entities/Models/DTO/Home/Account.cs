using System;
namespace Entities.Models.DTO.Home
{
    public class Account
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ExcelMapping { get; set; }
        public bool IsActive { get; set; }
    }
}

