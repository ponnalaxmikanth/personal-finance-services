using System;
using System.Collections.Generic;

namespace Entities.Models.DTO.BSE
{
    public class BSEIndex
    {
        public string Indx_cd { get; set; }
        public string shortalias { get; set; }
    }

    public class BSEIndices
    {
        public List<BSEIndex> Table { get; set; }
    }

    public class BSEIndexInfo
    {
        public int Day { get; set; }
        public string Month { get; set; }
        public string year { get; set; }
        public string Turnover { get; set; }
        public DateTime tdate { get; set; }
        public string I_name { get; set; }
        public double? I_open { get; set; }
        public double? I_high { get; set; }
        public double? I_low { get; set; }
        public double? I_close { get; set; }
        public double? I_pe { get; set; }
        public double? I_pb { get; set; }
        public double? I_yl { get; set; }
        public string Turnover_1 { get; set; }
        public string TOTAL_SHARES_TRADED { get; set; }
    }

    public class BSEIndexData
    {
        public List<BSEIndexInfo> Table { get; set; }
    }

    public class BSEIndexPerformance
    {
        public String Code { get; set; }
        public string Alias { get; set; }
        public decimal CurrentClose { get; set; }

        public decimal MonthClose { get; set; }
        public decimal MonthCloseGrowth { get; set; }

        public decimal Month3Close { get; set; }
        public decimal Month3CloseGrowth { get; set; }

        public decimal Month6Close { get; set; }
        public decimal Month6CloseGrowth { get; set; }

        public decimal Month9Close { get; set; }
        public decimal Month9CloseGrowth { get; set; }

        public decimal Month12Close { get; set; }
        public decimal Month12CloseGrowth { get; set; }
    }
}
