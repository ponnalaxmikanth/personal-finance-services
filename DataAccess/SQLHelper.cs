using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using Entities.Models.DTO;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace DataAccess
{
    public class SQLHelper : ISQLHelper
    {
        private readonly ILogger<SQLHelper> _logger;
        public SQLHelper(ILogger<SQLHelper> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResponse<int>> ExecuteScalar(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments")
        {
            
            try
            {
                using (MySqlConnection lconn = new MySqlConnection($"Server=localhost; Database={database};User=application; Password=Admin@123;"))
                {
                    lconn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(statement, lconn))
                    {
                        cmd.CommandType = cmdType;
                        if (parameters != null && parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        var result = cmd.ExecuteScalar();
                        int res = -1;
                        bool isNumber = result == null ? false : int.TryParse(result.ToString(),out res);
                        return isNumber ? new ServiceResponse<int>(true, "Successfully Executed", res) : new ServiceResponse<int>(false, "Failed to Executed", res);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExecuteScalar {statement}, exception: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Execute Procedure", 0);
        }

        public async Task<ServiceResponse<int>> ExecuteNonQuery(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments")
        {
            try
            {
                using (MySqlConnection lconn = new MySqlConnection($"Server=localhost; Database={database};User=application; Password=Admin@123;"))
                {
                    lconn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(statement, lconn))
                    {
                        cmd.CommandType = cmdType;
                        if (parameters != null && parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        var result = cmd.ExecuteNonQuery();
                        int res = -1;
                        bool isNumber = int.TryParse(result.ToString(), out res);
                        return isNumber ? new ServiceResponse<int>(true, "Successfully Executed", res): new ServiceResponse<int>(false, "Failed to Executed", res);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExecuteNonQuery {statement}, exception: {ex.Message}");
            }
            return new ServiceResponse<int>(false, "Failed to Execute", 0);
        }

        public async Task<ServiceResponse<DataTable>> ExecuteDataAdapter(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments")
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection lconn = new MySqlConnection($"Server=localhost; Database={database};User=application; Password=Admin@123;"))
                {
                    lconn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(statement, lconn))
                    {
                        cmd.CommandType = cmdType;
                        if (parameters != null && parameters.Count > 0)
                            cmd.Parameters.AddRange(parameters.ToArray());

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        da.Fill(dataTable);
                        return new ServiceResponse<DataTable>(true, "Success", dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExecuteNonQuery {statement}, exception: {ex.Message}");
            }
            return new ServiceResponse<DataTable>(false, "Error", null);
        }

    }
}
