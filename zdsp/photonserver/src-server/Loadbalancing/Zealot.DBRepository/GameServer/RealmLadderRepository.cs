using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class RealmLadderRepository : DBRepoBase
    {
        public RealmLadderRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries

        /// <summary>
        /// Gets a list of characters of the User</summary>
        /// <remarks>
        /// Calls [Character_GetUserChars]
        /// </remarks>
        public List<Dictionary<string, object>> GetRecords()
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("RealmLadder_Select", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
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

        public async Task<bool> Insert_Update(int realmid, string charname, int criteria, bool isfast)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("RealmLadder_Insert_Update", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@realmid", realmid);
                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@isfast", isfast);
                            command.Parameters.AddWithValue("@criteria", criteria);

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
