using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zealot.DBRepository.GM
{
    public struct AccountInsertResult
    {
        public Guid UserId;
        public AccountInsertErrorCode ErrorCode;
    }

    public enum AccountInsertErrorCode : byte
    {
        Succeed = 0,
        Failed = 1,
        UsernameInUse = 2
    }

    public class PlayerAccountRepository : DBRepoBase
    {
        public PlayerAccountRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries

        /// <summary>
        /// Insert new Player Account to DB
        /// </summary>
        /// <remarks>
        /// Calls [PlayerAccount_Insert]
        /// </remarks>
        public async Task<AccountInsertResult?> InsertAccountAsync(int loginType, string loginId, string password, string deviceId)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@logintype", loginType);
                            command.Parameters.AddWithValue("@loginid", loginId);
                            command.Parameters.AddWithValue("@password", password);
                            command.Parameters.AddWithValue("@deviceid", deviceId);
                            command.Parameters.Add(new SqlParameter("@userid", SqlDbType.UniqueIdentifier));
                            command.Parameters["@userid"].Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            Guid userId = (Guid)command.Parameters["@userid"].Value;
                            return new AccountInsertResult { UserId = userId, ErrorCode = AccountInsertErrorCode.Succeed };
                        }
                    }
                    catch (SqlException sqlex)
                    {
                        if (sqlex.Number == 2627)
                        {
                            Log.DebugFormat("Insert Failed: loginId [{0}] already exists", loginId);
                            return new AccountInsertResult { UserId = Guid.Empty, ErrorCode = AccountInsertErrorCode.UsernameInUse };
                        }
                        else
                            HandleQueryException(sqlex);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a list of player accounts tied to the login
        /// </summary>
        /// <remarks>
        /// Calls [PlayerAccount_GetByLoginId]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetAccountByLoginAsync(int loginType, string loginId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_GetByLoginId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@logintype", loginType);
                            command.Parameters.AddWithValue("@loginid", loginId);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> userrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
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
        /// Gets a list of player accounts tied to the login
        /// </summary>
        /// <remarks>
        /// Calls [PlayerAccount_GetByLoginId]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetAccountByUserIdAsync(string userid)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_GetByUserId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@userid", userid);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> userrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
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

        public async Task<bool> UpdateCookie(string userid, string cookie)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_UpdateCookie", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@userid", userid);
                            command.Parameters.AddWithValue("@cookie", cookie);
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

        public async Task<List<Dictionary<string, object>>> GetAchievementByUserIdAsync(string userid)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_GetAchievementByUserId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@userid", userid);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> userrow = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
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

        public async Task<bool> SaveAchievement(string userid, string achInvData)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("PlayerAccount_SaveAchievement", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@userid", userid);
                            command.Parameters.AddWithValue("@achievement", achInvData);
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
