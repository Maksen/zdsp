using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class GameConfigRepository : DBRepoBase
    {
        public GameConfigRepository(DBAccessor dBRepository) : base(dBRepository) { }

        /// <summary>
        /// Gets game server config.
        /// </summary>
        /// <remarks>
        /// calls [GameConfig_GetConfig]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetConfig(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("GameConfig_GetConfig", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
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