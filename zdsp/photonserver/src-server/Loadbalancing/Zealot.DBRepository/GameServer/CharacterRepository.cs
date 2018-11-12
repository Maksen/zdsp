using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class CharacterRepository : DBRepoBase
    {
        public CharacterRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries
        /// <summary>
        /// Insert new Character.
        /// </summary>
        /// <param name="characterdata">pass in serialized default charater data</param>
        /// <remarks>
        /// Calls [Character_Insert]
        /// </remarks>
        public async Task<Tuple<Guid?, bool>> InsertNewCharacter(string userid, string charname, int serverline, byte jobsect, int portraitid, byte faction, string characterdata)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@userid", userid);
                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@jobsect", jobsect);
                            command.Parameters.AddWithValue("@portraitid", portraitid);
                            command.Parameters.AddWithValue("@faction", faction);
                            command.Parameters.AddWithValue("@characterdata", characterdata);

                            command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier));
                            command.Parameters["@id"].Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            Guid newid = (Guid)command.Parameters["@id"].Value;
                            return new Tuple<Guid?, bool>(newid, false);
                        }
                    }
                    catch (SqlException sqlex)
                    {
                        //violation of unique constraint on 'charname'
                        if (sqlex.Number == 2627)
                        {
                            Log.DebugFormat("Insert Failed: charname [{0}] already exists", charname);
                            return new Tuple<Guid?, bool>(null, true);
                        }
                        else
                            HandleQueryException(sqlex);
                    }
                }
            }
            return new Tuple<Guid?, bool>(null, true);
        }

        /// <summary>
        /// Gets a list of characters of the User</summary>
        /// <remarks>
        /// Calls [Character_GetUserChars]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetByUserID(string userid, int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetUserChars", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@userid", userid);
                            command.Parameters.AddWithValue("@serverline", serverline);

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
                                return result;
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get character by character name</summary>
        /// <remarks>
        /// Calls [Character_GetByName]
        /// </remarks>
        public async Task<Dictionary<string, object>> GetByNameAsync(string name)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("Character_GetByName", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", name);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read())
                                {
                                    for (int index = 0; index < reader.FieldCount; ++index)
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

        /// <summary>
        /// Get characters by Guild Id</summary>
        /// <remarks>
        /// Calls [Character_GetByGuildId]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetByGuildId(int guildid)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetByGuildId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@guildid", guildid);

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
        /// Get info for social by character name, includes friend and request list</summary>
        /// <remarks>
        /// Calls [Character_GetSocialByName]
        /// </remarks>
        public async Task<Dictionary<string, object>> GetSocialByName(string name)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetSocialByName", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", name);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read())
                                {
                                    for (int index = 0; index < reader.FieldCount; ++index)
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

        /// <summary>
        /// Get info for social by list of names, includes friend and request list</summary>
        /// <remarks>
        /// Calls [Character_GetSocialByNames]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetSocialByNames(List<string> names)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (names.Count == 0)
                return result;
            DataTable namesDataTable = new DataTable();
            namesDataTable.Columns.Add("name", typeof(string));
            for (int i = 0; i < names.Count; ++i)
            {
                DataRow row = namesDataTable.NewRow();
                row["name"] = names[i];
                namesDataTable.Rows.Add(row);
            }
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetSocialByNames", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter sqlParam = command.Parameters.AddWithValue("@names", namesDataTable);
                            sqlParam.SqlDbType = SqlDbType.Structured;
                            sqlParam.TypeName = "dbo.NamesTableType";

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> row = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
                                    {
                                        string colname = reader.GetName(index);
                                        row.Add(colname, reader[colname]);
                                    }
                                    result.Add(row);
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

        /// <summary> Get lite info for social </summary>
        /// <remarks>
        /// Calls [Character_GetSocialStateByNames]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetSocialStateByNames(List<string> names)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (names.Count == 0)
                return result;
            DataTable namesDataTable = new DataTable();
            namesDataTable.Columns.Add("name", typeof(string));
            for (int i = 0; i < names.Count; ++i)
            {
                DataRow row = namesDataTable.NewRow();
                row["name"] = names[i];
                namesDataTable.Rows.Add(row);
            }
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetSocialStateByNames", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter sqlParam = command.Parameters.AddWithValue("@names", namesDataTable);
                            sqlParam.SqlDbType = SqlDbType.Structured;
                            sqlParam.TypeName = "dbo.NamesTableType";

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> row = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
                                    {
                                        string colname = reader.GetName(index);
                                        row.Add(colname, reader[colname]);
                                    }
                                    result.Add(row);
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

        public async Task<List<Dictionary<string, object>>> GetAllByCharaIds(IList<string> ids)
        {
            return await GetByCharaIds("All", ids);
        }

        public async Task<List<Dictionary<string, object>>> GetSocialByCharaIds(IList<string> ids)
        {
            return await GetByCharaIds("Social",ids);
        }

        /// <summary>
        /// Get info by list of ids
        /// <remarks>
        /// Calls [Character_Get{sp_flag}ByCharaIds]
        /// </remarks>
        /// </summary>
        private async Task<List<Dictionary<string, object>>> GetByCharaIds(string sp_flag,IList<string> ids)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (ids.Count == 0)
                return result;
            DataTable namesDataTable = new DataTable();
            namesDataTable.Columns.Add("id", typeof(string));
            for (int i = 0; i < ids.Count; ++i)
            {
                DataRow row = namesDataTable.NewRow();
                row["id"] = ids[i];
                namesDataTable.Rows.Add(row);
            }
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_Get"+ sp_flag + "ByCharaIds", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter sqlParam = command.Parameters.AddWithValue("@ids", namesDataTable);
                            sqlParam.SqlDbType = SqlDbType.Structured;
                            sqlParam.TypeName = "dbo.IdsTableType";

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> row = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
                                    {
                                        string colname = reader.GetName(index);
                                        row.Add(colname, reader[colname]);
                                    }
                                    result.Add(row);
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
        /// Get random 5 entry of character</summary>
        /// <remarks>
        /// Calls [Character_GetSocialRandom]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetSocialRandom(string friendlist)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetSocialRandom", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@friendlist", friendlist);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> row = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; ++index)
                                    {
                                        string colname = reader.GetName(index);
                                        row.Add(colname, reader[colname]);
                                    }
                                    result.Add(row);
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
        /// Delete a character</summary>
        /// <remarks>
        /// Calls [Character_Delete]
        /// </remarks>
        public async Task<bool> DeleteCharacterByName(string charname)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_DeleteByName", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);

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

        /// <summary>
        /// Save character </summary>
        /// <remarks>
        /// Calls [Character_Save]
        /// </remarks>
        public async Task<bool> SaveCharacterAndUserAsync(string charid, long experience, int progresslvl,
            int combatscore, int portraitid, int money, int gold, int bindgold, int guildid, byte guildrank,
            byte vipLvl, int fundtoday, long fundtotal, int factionkill, int factiondeath, short petcollected, int petscore,
            short herocollected, int heroscore, DateTime guildcdenddt, string friends, string friendrequests,
            int firstbuyflag, int firstbuycollected, string gameSetting,
            string characterdata, DateTime dtlogin, DateTime dtlogout)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    SqlTransaction trans = null;
                    try
                    {
                        connection.Open();
                        trans = connection.BeginTransaction();

                        using (SqlCommand command = new SqlCommand("Character_Save", connection, trans))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charid", charid);
                            command.Parameters.AddWithValue("@experience", experience);
                            command.Parameters.AddWithValue("@progresslevel", progresslvl);
                            command.Parameters.AddWithValue("@combatscore", combatscore);
                            command.Parameters.AddWithValue("@portraitid", portraitid);

                            command.Parameters.AddWithValue("@money", money);
                            command.Parameters.AddWithValue("@gold", gold);
                            command.Parameters.AddWithValue("@bindgold", bindgold);

                            command.Parameters.AddWithValue("@guildid", guildid);
                            command.Parameters.AddWithValue("@guildrank", guildrank);
                            command.Parameters.AddWithValue("@viplevel", vipLvl);
                            command.Parameters.AddWithValue("@fundtoday", fundtoday);
                            command.Parameters.AddWithValue("@fundtotal", fundtotal);

                            command.Parameters.AddWithValue("@factionkill", factionkill);
                            command.Parameters.AddWithValue("@factiondeath", factiondeath);
                            command.Parameters.AddWithValue("@petcollected", petcollected);
                            command.Parameters.AddWithValue("@petscore", petscore);
                            command.Parameters.AddWithValue("@herocollected", herocollected);
                            command.Parameters.AddWithValue("@heroscore", heroscore);

                            if (guildcdenddt > (DateTime)(System.Data.SqlTypes.SqlDateTime.MinValue))
                                command.Parameters.AddWithValue("@guildcdenddt", guildcdenddt);
                            else
                                command.Parameters.AddWithValue("@guildcdenddt", DBNull.Value);
                            command.Parameters.AddWithValue("@friends", friends);
                            command.Parameters.AddWithValue("@friendrequests", friendrequests);  
                            command.Parameters.AddWithValue("@gamesetting", gameSetting);
                            command.Parameters.AddWithValue("@characterdata", characterdata); 
                            command.Parameters.AddWithValue("@dtlogout", dtlogout);

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                        trans.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (trans != null)
                            trans.Rollback();

                        HandleQueryException(e);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Save character data </summary>
        /// <remarks>
        /// only use at character selection scene
        /// </remarks>
        public async Task<bool> SaveCharacterData(string charid, string characterdata)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    SqlTransaction trans = null;
                    try
                    {
                        connection.Open();
                        trans = connection.BeginTransaction();

                        using (SqlCommand command = new SqlCommand("Character_SaveCharacterData", connection, trans))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charid", charid);
                            command.Parameters.AddWithValue("@characterdata", characterdata);

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                        trans.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (trans != null)
                            trans.Rollback();

                        HandleQueryException(e);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if character name exists in database
        /// </summary>
        /// <remarks>
        /// Calls [Character_IsExists]
        /// </remarks>
        public async Task<Tuple<bool, string>> IsCharacterExists(string charName)
        {
            if (isConnected)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_IsExists", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@charname", charName);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read())
                                {
                                    for (int index = 0; index < reader.FieldCount; ++index)
                                    {
                                        string colname = reader.GetName(index);
                                        result.Add(colname, reader[colname]);
                                    }
                                }
                            }
                            if (result.Count > 0)
                                return new Tuple<bool, string>((int)result[""] > 0, (string)result["charname"]);
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return new Tuple<bool, string>(false, "");
        }

        public async Task<bool> UpdateArenaReport(string charname, string reports)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_UpdateArenaReport", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@arenareport", reports);

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

        public string GetArenaReport(string charname)
        {
            string result = "";

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetArenaReport", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@charname", charname);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                    return (string)reader["arenareport"];
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

        public async Task<int> JoinGuild(string charname, int guildid, byte guildrank)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_JoinGuild", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@guildid", guildid);
                            command.Parameters.AddWithValue("@guildrank", guildrank);

                            var combatscoreOutputParameter = command.Parameters.Add("@combatscore", SqlDbType.Int);
                            combatscoreOutputParameter.Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            int combatscore = Convert.IsDBNull(combatscoreOutputParameter.Value) ? -1 : (int)combatscoreOutputParameter.Value;
                            return combatscore;
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return -1;
        }

        public async Task<bool> QuitGuild(string charname, DateTime dtLeaveGuildCDEnd)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_QuitGuild", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);
                            if (dtLeaveGuildCDEnd > (DateTime)(System.Data.SqlTypes.SqlDateTime.MinValue))
                                command.Parameters.AddWithValue("@guildcdenddt", dtLeaveGuildCDEnd);
                            else
                                command.Parameters.AddWithValue("@guildcdenddt", DBNull.Value);
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

        public async Task<bool> ClearFundToday(int serverline)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_ClearFundToday", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
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

        public async Task<bool> UpdateGuildInfo(string charname, int guildid, byte guildrank)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_UpdateGuildInfo", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@guildid", guildid);
                            command.Parameters.AddWithValue("@guildrank", guildrank);

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

        public async Task<bool> UpdateSocialList(string charname, string socialListInfo, bool isRequest)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_UpdateSocialList", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charname);
                            command.Parameters.AddWithValue("@sociallist", socialListInfo);
                            command.Parameters.AddWithValue("@isrequest", isRequest);

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

        public List<Dictionary<string, object>> GetCharacterByNames(List<string> names)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            if (names.Count == 0)
                return result;
            DataTable charnames = new DataTable();
            charnames.Columns.Add("charname", typeof(string));
            for (int index = 0; index < names.Count; ++index)
            {
                DataRow row = charnames.NewRow();
                row["charname"] = names[index];
                charnames.Rows.Add(row);
            }
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetByNames", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter param = command.Parameters.AddWithValue("@names", charnames);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.NamesTableType";

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
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

        public async Task<int> GetDAU(DateTime start, DateTime end)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Character_GetDAU", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);
                            command.Parameters.AddWithValue("@startdt", start);
                            command.Parameters.AddWithValue("@enddt", end);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read())
                                    return (int)reader.GetValue(0);
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return -1;
        }

        public async Task<Dictionary<string, object>> GM_Character_GetServerCurrency()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("GM_Character_GetServerCurrency", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@serverline", _dbRepo.mServerLine);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (reader.Read())
                                {
                                    for (int index = 0; index < reader.FieldCount; ++index)
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
        #endregion
    }
}
