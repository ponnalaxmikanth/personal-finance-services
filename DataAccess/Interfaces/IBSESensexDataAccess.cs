using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Models.DTO;
using Entities.Models.DTO.BSE;

namespace DataAccess.Interfaces
{
    public interface IBSESensexDataAccess
    {
        Task<ServiceResponse<int>> SaveIndices(List<BSEIndex> bseIndicesData);
            
        Task<ServiceResponse<int>> SaveIndicesData(List<BSEIndexInfo> bseIndicesData);
        Task<ServiceResponse<DataTable>> GetPerformance(int period);
    }
}
