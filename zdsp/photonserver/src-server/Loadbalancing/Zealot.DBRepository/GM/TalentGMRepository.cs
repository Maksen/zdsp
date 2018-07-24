using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Zealot.DBRepository.GM
{
    public class TalentGMRepository : DBRepoBase
    {
        public TalentGMRepository(DBAccessor dbRepository) : base(dbRepository)
        { }

        public async Task<Dictionary<string, object>> GetModifier()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (!isConnected)
                return result;

            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Talent_GetModifierInUse", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                for (int index = 0; index < reader.FieldCount; index++)
                                {
                                    //Data reading
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

            return result;
        }
    }
}
