using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zealot.DBRepository.GM.Compensate
{
    public class CompensateRepository : DBRepoBase
    {

        public CompensateRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public List<Dictionary<string, object>> GetCompensateItem(int server = 1)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        var sqlString = "GetCompensates";


                        using (SqlCommand command = new SqlCommand(sqlString, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", server);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Dictionary<string, object> userrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        userrow.Add(colname, reader[colname]);
                                    }
                                    result.Add(userrow);
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
               
            }
            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetCompensateItemAsync(int server = 1)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        var sqlString = "GetCompensates";


                        using (SqlCommand command = new SqlCommand(sqlString, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", server);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> userrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        userrow.Add(colname, reader[colname]);
                                    }
                                    result.Add(userrow);
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }

            }
            return result;
        }
    }
}
