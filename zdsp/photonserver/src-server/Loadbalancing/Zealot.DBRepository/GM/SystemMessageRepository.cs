using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Zealot.DBRepository.GM
{
    public class SystemMessageRepository : DBRepoBase
    {
        public SystemMessageRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public List<Dictionary<string, object>> GetMessage(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("SystemMessage_GetMessages", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);

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
    }
}
