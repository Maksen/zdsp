using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class LadderRepository : DBRepoBase
    {
        public LadderRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries

        /// <summary>
        /// Gets All Arena Records</summary>
        /// <remarks>
        /// Calls [ArenaRank_Select]
        /// </remarks>
        public string GetArenaRecords()
        {
            string result = "";

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("ArenaRank_Select", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                    return (string)reader["data"];
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        Log.ErrorFormat("SQLException: {0}", e.Message);
                    }
                }
            }
            return result;
        }

        public async Task<bool> UpdateArena(string data)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("ArenaRank_Update", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
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

        #region #LeaderBoard Queries

        /// <summary>
        /// Saves serialized leaderboard data to database
        /// </summary>
        /// <remarks>calls [LeaderBoard_Save]</remarks>
        public async Task<bool> SaveLeaderBoardAsync(int type, string data)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("LeaderBoard_Save", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@type", type);
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


        /// <summary>
        /// Loads serialized leaderboard data from database
        /// </summary>
        /// <remarks>calls [LeaderBoard_Get] synchronously</remarks>
        public Dictionary<string, object> GetLeaderBoard(int type)
        {          
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("LeaderBoard_Get", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@type", type);

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

        public async Task<List<Dictionary<string, object>>> GetLeaderBoardAsync(string storeproc, int ranktotal)
        {           
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(storeproc, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@ranktotal", ranktotal);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> resultrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        resultrow.Add(colname, reader[colname]);
                                    }
                                    result.Add(resultrow);
                                }
                                return result;
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
        #endregion
        #endregion
    }
}
