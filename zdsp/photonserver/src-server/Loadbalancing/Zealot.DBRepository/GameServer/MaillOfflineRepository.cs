using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class MailOfflineRepository : DBRepoBase
    {
        public MailOfflineRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<List<Dictionary<string, object>>> GetMailOffline(string charName, DateTime expiry)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("MailOffline_GetByName", connection))
                        {

                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charName);
                            command.Parameters.AddWithValue("@expiry", expiry);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    Dictionary<string, object> row = new Dictionary<string, object>();
                                    for (int index = 0; index < reader.FieldCount; index++)
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

        public async Task<Guid?> InsertMailOffline(string charName, string serailizedMail)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("MailOffline_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charName);
                            command.Parameters.AddWithValue("@maildata", serailizedMail);
                            command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier));
                            command.Parameters["@id"].Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            Guid newid = (Guid)command.Parameters["@id"].Value;
                            return newid;
                        }                      
                    }
                    catch (SqlException sqlex)
                    {
                        HandleQueryException(sqlex);
                    }
                }
            }
            return null;
        }

        public async Task<bool> DeleteMailOffline(string charName)
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

                        using (SqlCommand command = new SqlCommand("MailOffline_Delete", connection, trans))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@charname", charName);

                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (SqlException sqlex)
                    {
                        if (trans != null)
                            trans.Rollback();

                        HandleQueryException(sqlex);
                    }
                    catch
                    {
                        if (trans != null)
                            trans.Rollback();

                        throw;
                    }
                }
            }
            return false;
        }
    }
}
