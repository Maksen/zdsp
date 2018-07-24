using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository.GM
{
    public class CurrencyExchangeGMRepository : DBRepoBase
    {
        public CurrencyExchangeGMRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<Dictionary<string, object>> GetCurrencyExchangeRate()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("CurrencyExchange_Select", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
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
