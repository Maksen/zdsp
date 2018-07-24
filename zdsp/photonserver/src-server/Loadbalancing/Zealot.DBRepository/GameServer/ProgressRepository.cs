using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class ProgressRepository : DBRepoBase
    {
        public ProgressRepository(DBAccessor dBRepository) : base(dBRepository) { }

        /// <summary>
        /// Gets progress by server.
        /// </summary>
        /// <remarks>
        /// calls [Progress_GetProgress]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetProgress(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Progress_GetProgress", connection))
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

        /// <summary>
        /// Gets progress by server and key.
        /// </summary>
        /// <remarks>
        /// calls [Progress_GetProgressByKey]
        /// </remarks>
        public async Task<Dictionary<string, object>> GetProgressByKey(int serverline, string key)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Progress_GetProgressByKey", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@key", key);
                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read() == true)
                                {
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        result.Add(colname, reader[colname]);
                                    }
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

        public async Task<bool> Update(int serverline, string key, long value)
        {
            return await UpdateHelper(serverline, key, value);
        }

        public async Task<bool> Update(int serverline, string key, string value)
        {
            return await UpdateHelper(serverline, key, value);
        }

        private async Task<bool> UpdateHelper<T>(int serverline, string key, T value)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Progress_UpdateProgress", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@key", key);
                            command.Parameters.AddWithValue("@value", value);

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
    }
}
