using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Zealot.DBRepository.GM
{
    public class WelfareRepository : DBRepoBase
    {
        public WelfareRepository(DBAccessor dBRepository) : base(dBRepository) { }

        /// <summary>
        /// Gets Open Service Funds cost by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ServiceFundGetCostByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetServiceFundsCost(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ServiceFundGetCostByServer", connection))
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

        /// <summary>
        /// Gets Open Service Funds cost by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ServiceFundGetLvlRewardsByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetServiceFundsLvlRewards(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ServiceFundGetLvlRewardsByServer", connection))
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

        /// <summary>
        /// Gets Open Service Funds cost by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ServiceFundGetPplRewardsByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetServiceFundsPplRewards(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ServiceFundGetPplRewardsByServer", connection))
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

        /// <summary>
        /// Gets FirstBuy server reset flag by server.
        /// </summary>
        /// <remarks>
        /// calls [Progress_GetProgress]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetFirstBuyFlagServerReset(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_FirstBuyGetServerReset", connection))
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

        /// <summary>
        /// Gets FirstBuy flag by server and char id.
        /// </summary>
        /// <remarks>
        /// calls [Progress_GetProgress]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetFirstBuyFlagCharReset(int serverline, string charId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_FirstBuy_GetPlayerReset", connection))
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

        /// <summary>
        /// Gets First Credit rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_FirstBuyGetRewardsByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetFirstCreditRewards(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_FirstBuyGetRewardsByServer", connection))
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

        /// <summary>
        /// Gets Total Credit event data by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalCreditGetEventByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalCreditEventData(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalCreditGetEventByServer", connection))
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

        /// <summary>
        /// Gets all Total Credit rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalCreditGetRewardsAll]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalCreditRewardsAll(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalCreditGetRewardsAll", connection))
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

        /// <summary>
        /// Gets Total Credit rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalCreditGetRewards]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalCreditRewards(int serverline, int eventId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalCreditGetRewards", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@eventid", eventId);
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

        /// <summary>
        /// Gets Total Spend event data by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalSpendGetEventByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalSpendEventData(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalSpendGetEventByServer", connection))
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

        /// <summary>
        /// Gets all Total Spend rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalSpendGetRewardsAll]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalSpendRewardsAll(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalSpendGetRewardsAll", connection))
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

        /// <summary>
        /// Gets Total Spend rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_TotalSpendGetRewards]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetTotalSpendRewards(int serverline, int eventId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_TotalSpendGetRewards", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@eventid", eventId);
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

        /// <summary>
        /// Gets Gold Jackpot event duration by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_GoldJackpotGetEventByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetGoldJackpotEventData(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_GoldJackpotGetEventByServer", connection))
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

        /// <summary>
        /// Gets Gold Jackpot tier data by server, event and last rolled tier.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_GoldJackpotGetJackpotDataByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetGoldJackpotDataByServerID(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_GoldJackpotGetJackpotDataByServer", connection))
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

        /// <summary>
        /// Gets Gold Jackpot tier data by server, event and last rolled tier.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_GoldJackpotGetJackpotDataByServer]
        /// </remarks>
        //public async Task<List<Dictionary<string, object>>> GetGoldJackpotNextTierDataByServerEventID(int serverId, int eventId, int lastRolledTier)
        //{
        //    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        //    int nextTier = lastRolledTier + 1;

        //    if (isConnected)
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionstring))
        //        {
        //            try
        //            {
        //                connection.Open();
        //                using (SqlCommand command = new SqlCommand("Welfare_GoldJackpotGetJackpotDataByServer", connection))
        //                {
        //                    command.CommandType = CommandType.StoredProcedure;
        //                    command.Parameters.AddWithValue("@serverid", serverId);
        //                    command.Parameters.AddWithValue("@eventid", eventId);
        //                    int currTier = 0;
        //                    using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
        //                    {
        //                        while (await reader.ReadAsync().ConfigureAwait(false))
        //                        {
        //                            Dictionary<string, object> userrow = new Dictionary<string, object>();
        //                            for (int index = 0; index < reader.FieldCount; index++)
        //                            {
        //                                string colname = reader.GetName(index);
        //                                // Reset currTier each time a new row begins
        //                                if (colname == "serverid")
        //                                {
        //                                    currTier = 0;
        //                                }
        //                                else if (colname == "tier")
        //                                {
        //                                    currTier = (int)reader[colname];
        //                                }

        //                                if (currTier == nextTier)
        //                                {
        //                                    if (colname != "serverid" && colname != "eventid" && colname != "tier")
        //                                    {
        //                                        userrow.Add(colname, reader[colname]);
        //                                    }
        //                                }
        //                            }
        //                            if (currTier == nextTier)
        //                            {
        //                                result.Add(userrow);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (SqlException e)
        //            {
        //                HandleQueryException(e);
        //            }
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// Gets Gold Jackpot highest teir by server, event and last rolled tier.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_GoldJackpotGetJackpotDataByServer]
        /// </remarks>
        //public async Task<int> GetGoldJackpotHighestTierByServerEventID(int serverId, int eventId)
        //{
        //    int highestTier = 0;

        //    if (isConnected)
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionstring))
        //        {
        //            try
        //            {
        //                connection.Open();
        //                using (SqlCommand command = new SqlCommand("Welfare_GoldJackpotGetJackpotDataByServer", connection))
        //                {
        //                    command.CommandType = CommandType.StoredProcedure;
        //                    command.Parameters.AddWithValue("@serverid", serverId);
        //                    command.Parameters.AddWithValue("@eventid", eventId);

        //                    using (SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
        //                    {
        //                        while (await reader.ReadAsync().ConfigureAwait(false))
        //                        {
        //                            for (int index = 0; index < reader.FieldCount; index++)
        //                            {
        //                                string colname = reader.GetName(index);

        //                                if (colname == "tier")
        //                                {
        //                                    int currTier = (int)reader[colname];
        //                                    if(currTier > highestTier)
        //                                    {
        //                                        highestTier = currTier;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (SqlException e)
        //            {
        //                HandleQueryException(e);
        //            }
        //        }
        //    }
        //    return highestTier;
        //}

        /// <summary>
        /// Gets Continuous Login event data by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ContLoginGetEventByServer]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetContLoginEventData(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ContLoginGetEventByServer", connection))
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

        /// <summary>
        /// Gets all Continuous Login rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ContLoginGetRewardsAll]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetContLoginRewardsAll(int serverline)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ContLoginGetRewardsAll", connection))
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

        /// <summary>
        /// Gets Continuous Login rewards by server.
        /// </summary>
        /// <remarks>
        /// calls [Welfare_ContLoginGetRewards]
        /// </remarks>
        public async Task<List<Dictionary<string, object>>> GetContLoginRewards(int serverline, int eventId)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("Welfare_ContLoginGetRewards", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@serverline", serverline);
                            command.Parameters.AddWithValue("@eventid", eventId);
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
