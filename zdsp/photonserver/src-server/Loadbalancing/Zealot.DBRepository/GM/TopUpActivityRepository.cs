namespace Zealot.DBRepository.GM.Compensate
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class TopUpActivityRepository : DBRepoBase
    {
        public TopUpActivityRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<List<Tuple<string, DateTime>>> GetActivity()
        {
            List<Tuple<string, DateTime>> result = new List<Tuple<string, DateTime>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        var sqlString = "TopUpActivity_GetActivity";
                        
                        using (SqlCommand command = new SqlCommand(sqlString, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@current_datetime", DateTime.Now);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                while (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    //result.Add(string.Format("{0}", reader["included_servers"]));
                                    Tuple<string, DateTime> resultRow = new Tuple<string, DateTime>(reader["included_servers"].ToString(), DateTime.Parse(reader["start_datetime"].ToString()));
                                    result.Add(resultRow);
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
