using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.BSE;

namespace BusinessAccess.Interfaces
{
    public interface IBSEIndicesService
    {
        Task<ServiceResponse<int>> DownloadIndicesInfo();
        Task<ServiceResponse<int>> DownloadIndicesData(DateTime fromDate, DateTime toDate);
        Task<ServiceResponse<List<BSEIndexPerformance>>> GetPerformance();
    }
}
