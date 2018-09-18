using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zealot.DBRepository
{
    public class SignboardRepository : DBRepoBase
    {
        public SignboardRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<bool> UpdateSignboardData(string data, DateTime dateTime)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Signboard_Update", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@dtupdate", dateTime);
                            command.Parameters.AddWithValue("@data", data);

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            return true;
                        }
                    }
                    catch (SqlException e)
                    {
                        Log.ErrorFormat("SQLException: {0}", e.Message);
                    }
                }
            }
            return false;
        }

        public Dictionary<string, object> GetSignboardData()
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Signboard_Get", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    Dictionary<string, object> result = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        result.Add(colname, reader[colname]);
                                    }
                                    return result;
                                }
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        Log.ErrorFormat("SQLException: {0}", e.Message);
                    }
                }
            }
            return null;
        }
    }
}
