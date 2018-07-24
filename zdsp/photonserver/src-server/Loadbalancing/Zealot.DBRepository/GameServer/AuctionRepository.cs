using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository
{
    public class AuctionRepository : DBRepoBase
    {
        public AuctionRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<List<Dictionary<string, object>>> GetAuctionItems()
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionItems_SelectAllUpcoming", connection))
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

        public async Task<Tuple<int, bool>> InsertNewBid(int auctionId, DateTime dtBid, int serverline, string serverName,
                                                        string bidderName, int bidPrice, int lockgold, int gold)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionBid_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@auctionid", auctionId);
                            command.Parameters.AddWithValue("@dtbid", dtBid);
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@servername", serverName);
                            command.Parameters.AddWithValue("@bidder", bidderName);
                            command.Parameters.AddWithValue("@bidprice", bidPrice);
                            command.Parameters.AddWithValue("@lockgold", lockgold);
                            command.Parameters.AddWithValue("@gold", gold);
                            command.Parameters.Add(new SqlParameter("@bidid", SqlDbType.Int));
                            command.Parameters["@bidid"].Direction = ParameterDirection.Output;
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                            int newid = (int)command.Parameters["@bidid"].Value;
                            return new Tuple<int, bool>(newid, true);
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                    }
                }
            }
            return new Tuple<int, bool>(0, false);
        }

        public async Task<List<Dictionary<string, object>>> GetAllAuctionBids(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionBid_SelectAll", connection))
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

        public async Task<List<Dictionary<string, object>>> GetAllAuctionBidsByAuctionId(int auctionId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionBid_SelectByAuctionId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@auctionid", auctionId);

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

        public async Task<bool> CollectAuctionBid(int bidId)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionBid_Collect", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@bidid", bidId);

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

        public async Task<List<Dictionary<string, object>>> GetAuctionRecords()
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionRecord_Select", connection))
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

        public async Task<bool> AddAuctionRecord(int auctionId, string auctionName, DateTime dtStart, DateTime dtEnd, int itemId,
                                                int itemCount, string bid1, string bid2, int winPrice, string winner)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("AuctionRecord_Insert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@auctionid", auctionId);
                            command.Parameters.AddWithValue("@name", auctionName);
                            command.Parameters.AddWithValue("@dtstart", dtStart);
                            command.Parameters.AddWithValue("@dtend", dtEnd);
                            command.Parameters.AddWithValue("@itemid", itemId);
                            command.Parameters.AddWithValue("@itemcount", itemCount);
                            if (string.IsNullOrEmpty(bid1))
                                command.Parameters.AddWithValue("@bid1", DBNull.Value);
                            else
                                command.Parameters.AddWithValue("@bid1", bid1);
                            if (string.IsNullOrEmpty(bid2))
                                command.Parameters.AddWithValue("@bid2", DBNull.Value);
                            else
                                command.Parameters.AddWithValue("@bid2", bid2);
                            if (winPrice == -1)
                                command.Parameters.AddWithValue("@winprice", DBNull.Value);
                            else
                                command.Parameters.AddWithValue("@winprice", winPrice);
                            if (string.IsNullOrEmpty(winner))
                                command.Parameters.AddWithValue("@winner", DBNull.Value);
                            else
                                command.Parameters.AddWithValue("@winner", winner);
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
