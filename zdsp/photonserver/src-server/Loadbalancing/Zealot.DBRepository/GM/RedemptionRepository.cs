using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zealot.DBRepository.GM
{
    public class RedemptionRepository : DBRepoBase
    {
        public RedemptionRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<Tuple<int, Dictionary<string, object>>> GetSerialInfo(string serial, string charId)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Redemption_GetSerialInfo", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serial", serial);
                            command.Parameters.AddWithValue("@charid", charId);
                            command.Parameters.Add(new SqlParameter("@retval", SqlDbType.Int));
                            command.Parameters["@retval"].Direction = ParameterDirection.ReturnValue;

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    for (int index = 0; index < reader.FieldCount; index++)
                                    {
                                        string colname = reader.GetName(index);
                                        result.Add(colname, reader[colname]);
                                    }
                                }
                            }
                            int errorcode = (int)command.Parameters["@retval"].Value;
                            return new Tuple<int, Dictionary<string, object>>(errorcode, result);
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return new Tuple<int, Dictionary<string, object>>(1, result);
        }

        public async Task<bool> ClaimSerialCode(string serial, string charId)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Redemption_ClaimSerialCode", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serial", serial);
                            command.Parameters.AddWithValue("@charid", charId);

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
