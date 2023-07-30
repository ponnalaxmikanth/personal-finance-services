using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Entities.Models.DTO.BSE;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DataAccess
{
    public class BSESensexDataAccess : IBSESensexDataAccess
    {
        private readonly ILogger<BSESensexDataAccess> _logger;
        private readonly ISQLHelper _sQLHelper;

        public BSESensexDataAccess(ILogger<BSESensexDataAccess> logger, ISQLHelper sQLHelper)
        {
            _logger = logger;
            _sQLHelper = sQLHelper;
        }

        public async Task<ServiceResponse<int>> SaveIndices(List<BSEIndex> bseIndicesData)
        {
            try
            {
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "indices", Value = JsonConvert.SerializeObject(bseIndicesData), DbType = DbType.String });
                parameters.Add(new MySqlParameter() { ParameterName = "exchange", Value = "BSE", DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("SaveBSEIndices", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == bseIndicesData.Count)
                    return new ServiceResponse<int>(true, "BSE Indices Saved Successfully", bseIndicesData.Count);
                else
                    new ServiceResponse<int>(false, "Error while saving BSE Indices", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveIndices failed to uploading BSE Indices: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading BSE Indices", default);
        }

        public async Task<ServiceResponse<int>> SaveIndicesData(List<BSEIndexInfo> bseIndicesData)
        {
            try
            {
                if (bseIndicesData == null || bseIndicesData.Count <= 0) return new ServiceResponse<int>(true, "No Data to Update", default);
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter() { ParameterName = "indicesData", Value = JsonConvert.SerializeObject(bseIndicesData), DbType = DbType.String });

                var result = await _sQLHelper.ExecuteScalar("SaveBSEIndicesData", CommandType.StoredProcedure, parameters);
                if (result.Success && result.ResponseObject == bseIndicesData.Count)
                    return new ServiceResponse<int>(true, "BSE Indices data saved successfully", bseIndicesData.Count);
                else
                    return new ServiceResponse<int>(false, "Error while saving BSE Indices data", result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"SaveIndicesData failed to uploading BSE Indices data: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Error while uploading BSE Indices data", default);
        }


        public async Task<ServiceResponse<DataTable>> GetPerformance(int period)
        {
            string statement = @"select p.Code, i.Alias, p.Date, p.Open, p.High, p.Low, p.Close, p.PE, p.PB, p.YL, p.type from (
	                                SELECT *, "
                                    + period.ToString() + @" type, ROW_NUMBER() OVER(PARTITION BY Code ORDER BY Date DESC) AS row_index
	                                FROM IndicesData
                                        where Date < DATE_SUB(CURDATE(), INTERVAL " + (period * 30) + @" DAY)
                                        and Date > DATE_SUB(CURDATE(), INTERVAL " + (period * 30) + 5 + @" DAY)
                                ) p
                                left join indices i on i.Code = p.Code
                                where p.row_index = 1";

            return await _sQLHelper.ExecuteDataAdapter(statement, CommandType.Text, null);
        }
    }
}
