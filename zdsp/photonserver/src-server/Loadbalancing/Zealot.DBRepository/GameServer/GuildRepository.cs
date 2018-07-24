using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class GuildRepository : DBRepoBase
    {
        public GuildRepository(DBAccessor dBRepository) : base(dBRepository) { }

        #region Queries

        public async Task<Tuple<int, bool>> InsertNewGuild(int serverline, string guildName, int guildIcon, string guildLeader, byte faction, 
                                                           DateTime dtBossLastReset, string guilddata)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Guild_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@guildname", guildName);
                            command.Parameters.AddWithValue("@guildicon", guildIcon);
                            command.Parameters.AddWithValue("@guildleader", guildLeader);
                            command.Parameters.AddWithValue("@faction", faction);
                            if (dtBossLastReset > (DateTime)(System.Data.SqlTypes.SqlDateTime.MinValue))
                                command.Parameters.AddWithValue("@dtbosslastreset", dtBossLastReset);
                            else
                                command.Parameters.AddWithValue("@dtbosslastreset", DBNull.Value);
                            command.Parameters.AddWithValue("@guilddata", guilddata);
                            command.Parameters.Add(new SqlParameter("@guildid", SqlDbType.Int));
                            command.Parameters["@guildid"].Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            int newid = (int)command.Parameters["@guildid"].Value;
                            return new Tuple<int, bool>(newid, true);
                        }
                    }
                    catch (SqlException sqlex)
                    {
                        //violation of unique constraint on 'guildname'
                        if (sqlex.Number == 2627)
                        {
                            Log.DebugFormat("Insert Failed: guildname [{0}] already exists", guildName);
                            return new Tuple<int, bool>(0, false);
                        }
                        else
                            HandleQueryException(sqlex);
                    }
                }
            }
            return new Tuple<int, bool>(0, false);
        }      

        public async Task<bool> DeleteGuild(int guildId)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Guild_Delete", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@guildid", guildId);

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

        public async Task<bool> SaveGuildAsync(int guildId, int guildIcon, byte guildLevel, long guildGold, string guildNotice, 
            string guildHistory, string techs, int smBossLevel, int smBossDmgDone, int dhFavourability, DateTime dtbosslastreset, 
            DateTime dtSecretShop, string guildData)
        {
            if(isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    SqlTransaction trans = null;
                    try
                    {
                        connection.Open();
                        trans = connection.BeginTransaction();
                        using (SqlCommand command = new SqlCommand("Guild_Save", connection, trans))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@guildid", guildId);
                            command.Parameters.AddWithValue("@guildicon", guildIcon);
                            command.Parameters.AddWithValue("@guildlevel", guildLevel);
                            command.Parameters.AddWithValue("@guildgold", guildGold);
                            command.Parameters.AddWithValue("@guildnotice", guildNotice);
                            command.Parameters.AddWithValue("@guildhistory", guildHistory);
                            command.Parameters.AddWithValue("@techs", techs);
                            command.Parameters.AddWithValue("@smbosslvl", smBossLevel);
                            command.Parameters.AddWithValue("@smbossdmgdone", smBossDmgDone);
                            command.Parameters.AddWithValue("@dhfavourability", dhFavourability);
                            if (dtbosslastreset > (DateTime)(System.Data.SqlTypes.SqlDateTime.MinValue))
                                command.Parameters.AddWithValue("@dtbosslastreset", dtbosslastreset);
                            else
                                command.Parameters.AddWithValue("@dtbosslastreset", DBNull.Value);
                            if (dtSecretShop > (DateTime)(System.Data.SqlTypes.SqlDateTime.MinValue))
                                command.Parameters.AddWithValue("@dtsecretshop", dtSecretShop);
                            else
                                command.Parameters.AddWithValue("@dtsecretshop", DBNull.Value);
                            command.Parameters.AddWithValue("@guilddata", guildData);

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

        public async Task<List<Dictionary<string, object>>> GetAllGuilds(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Guild_SelectAll", connection))
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

        public async Task<List<Dictionary<string, object>>> GetGuildByGuildName(string guildName)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Guild_SelectByGuildName", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@guildname", guildName);
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
        #endregion
    }
}
