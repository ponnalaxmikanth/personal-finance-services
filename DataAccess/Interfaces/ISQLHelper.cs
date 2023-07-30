using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Models.DTO;
using MySql.Data.MySqlClient;

namespace DataAccess.Interfaces
{
    public interface ISQLHelper
    {
        Task<ServiceResponse<int>> ExecuteScalar(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments");
        Task<ServiceResponse<int>> ExecuteNonQuery(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments");
        Task<ServiceResponse<DataTable>> ExecuteDataAdapter(string statement, CommandType cmdType, List<MySqlParameter> parameters, string database = "Investments");
    }
}
