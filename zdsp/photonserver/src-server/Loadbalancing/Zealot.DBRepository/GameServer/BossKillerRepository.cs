using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class BossKillerRepository : DBRepoBase
    {
        public BossKillerRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries
        public List<Dictionary<string, object>> GetRecords(int serverId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("BossKillerRecord_Select", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverid", serverId);

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

        public async Task<bool> Insert_Update(int serverId, int bossid, string killer, string payload)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("BossKillerRecord_Insert_Update", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@bossid", bossid);
                            command.Parameters.AddWithValue("@serverid", serverId);
                            command.Parameters.AddWithValue("@killer", killer);
                            command.Parameters.AddWithValue("@payload", payload);

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            return true;
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
