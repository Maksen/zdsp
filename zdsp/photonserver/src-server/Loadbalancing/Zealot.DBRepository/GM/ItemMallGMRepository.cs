using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Zealot.DBRepository.GM
{
    public class ItemMallGMRepository : DBRepoBase
    {
        public ItemMallGMRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<List<Dictionary<string, object>>> GetItemMallItem(int category)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        var sqlString = "";
                        if (category == 0)
                            sqlString = "ItemMall_ServerGetAll";
                        else
                            sqlString = "ItemMall_ServerGetByCat";


                        using (SqlCommand command = new SqlCommand(sqlString, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            if (category != 0)
                            {
                                command.Parameters.AddWithValue("@category", category);
                            }

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

        public async Task<List<Dictionary<string, object>>> GetShopStatus()
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        var sqlString = "";
                        sqlString = "ItemMallStatus_GetAll";

                        using (SqlCommand command = new SqlCommand(sqlString, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

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
    }
}
