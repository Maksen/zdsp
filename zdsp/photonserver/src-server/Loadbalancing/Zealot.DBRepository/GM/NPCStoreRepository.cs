using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Zealot.Common;

namespace Zealot.DBRepository.GM.NPCStore
{
    public class NPCStoreRepository : DBRepoBase
    {
        public Dictionary<int, NPCStoreInfo> Stores = new Dictionary<int, NPCStoreInfo>();                        

        public NPCStoreRepository(DBAccessor dBRepository) : base(dBRepository) { }

        public async Task<Dictionary<int, NPCStoreInfo>> GetStoreData()
        {
            Dictionary<int, NPCStoreInfo> ret = new Dictionary<int, NPCStoreInfo>();

            var storerows = await GetStoresAsync().ConfigureAwait(false);
            foreach (var row in storerows)
            {
                ret.Add((int)row["ID"], new NPCStoreInfo((int)row["ID"], (string)row["localisedname"], (string)row["name"], (NPCStoreInfo.ItemStoreType)((string)row["type"]).ToCharArray()[0]));
            }

            var normalitemrows = await GetSoldItemsAsync().ConfigureAwait(false);
            foreach (var row in normalitemrows)
            {
                try
                {
                    var storeid = (int)row["storeid"];

                    var a = storeid;
                    var b = (int)row["entryid"];
                    var c = (bool)row["show"];
                    var d = (int)row["itemid"];
                    var e = (NPCStoreInfo.ItemStoreType)((string)row["shoptype"]).ToCharArray()[0];
                    var f = (int)row["itemvalue"];
                    var g = (NPCStoreInfo.SoldCurrencyType)((string)row["soldtype"]).ToCharArray()[0];
                    var h = (int)row["soldvalue"];
                    var i = (double)row["discount"];
                    var j = (int)row["sortnumber"];
                    var k = (DateTime)row["starttime"];
                    var l = (DateTime)row["endtime"];
                    var m = (int)row["excount"];
                    var n = (NPCStoreInfo.Frequency)((string)row["dailyorweekly"]).ToCharArray()[0];

                    if (ret[storeid].inventory.ContainsKey(b) == false)
                    {
                        ret[storeid].inventory.Add(b, new NPCStoreInfo.StandardItem(a, b, c, d, e, f, g, h, (float)i, j, k, l, m, n));
                    }
                    else
                    {
                        // signal duplicate store item
                        throw new Exception("NPCStore Exception: Duplicate item entry id: " + b.ToString() + " for npc store id: " + storeid.ToString() + " found. Rectify with GMTools.");
                    }
                }
                catch (Exception ex)
                {
                    GameUtils.DebugWriteLine(ex.Message);
                }
            }

			var barters = await GetBarterData().ConfigureAwait(false);

			foreach (var barter in barters)
			{
				var key = barter.ItemListID;
				var storeid = barter.StoreID;

				if (ret.ContainsKey(storeid))
				{
					if (ret[storeid].inventory.ContainsKey(key))
					{
						var breq = new NPCStoreInfo.BarterReq
						{
							StoreID = storeid,
							ItemListID = key,
							ReqItemID = barter.ReqItemID,
							ReqItemValue = barter.ReqItemValue };

						ret[storeid].inventory[key].required_items.Add(breq);
					}
				}
			}

			return ret;
        }

		public async Task<List<NPCStoreInfo.BarterReq>> GetBarterData()
		{
			List<NPCStoreInfo.BarterReq> ret = new List<NPCStoreInfo.BarterReq>();

			var rows = await GetStoreBartersAsync().ConfigureAwait(false);
			foreach (var entry in rows)
			{
				var a = (int)entry["storeid"];
				var b = (int)entry["itemlistid"];
				var c = (int)entry["requireditemid"];
				var d = (int)entry["requireditemvalue"];

				NPCStoreInfo.BarterReq barter = new NPCStoreInfo.BarterReq { StoreID = a, ItemListID = b, ReqItemID = c, ReqItemValue = d };

				ret.Add(barter);
			}

			return ret;
		}

		public async Task<List<Dictionary<string, object>>> GetStoreBartersAsync()
        {
            var tablename = "StoreBarters";
            return await PullGMTable(tablename);
        }

		public async Task<List<Dictionary<string, object>>> GetStoresAsync()
		{
			var tablename = "Store";
			return await PullGMTable(tablename);
		}

		public async Task<List<Dictionary<string, object>>> GetSoldItemsAsync()
        {
            var tablename = "StoreStandardItems";

            return await PullGMTable(tablename);
        }

        public async Task<List<Dictionary<string, object>>> PullGMTable(string table)
        {
            var db = "GMDBCS";
            return await PerformQueryAsync(string.Format("SELECT * FROM [{0}].[dbo].[{1}]", db, table));
        }

        public async Task<List<Dictionary<string, object>>> PerformQueryAsync(string query)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand userquery = new SqlCommand())
                        {
                            userquery.Connection = connection;
                            userquery.CommandType = CommandType.Text;
                            userquery.CommandText = query;

                            //var reader = userquery.ExecuteReader();
                            using (SqlDataReader reader = await userquery.ExecuteReaderAsync().ConfigureAwait(false))
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

        public async Task<bool> PerformSQLCommandAsync(string command)
        {
            if (isConnected)
            {
                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand userquery = new SqlCommand())
                        {
                            userquery.Connection = connection;
                            userquery.CommandType = CommandType.Text;
                            userquery.CommandText = command;

                            //var reader = userquery.ExecuteReader();
                            await userquery.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                    catch (SqlException e)
                    {
                        HandleQueryException(e);
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<Dictionary<string, NPCStoreInfo.Transaction>> GetPlayerStoreTransactions(string charid)
        {
            var instancedb = "InstanceDBCS";
            var query = string.Format("SELECT [transactionhistory] FROM [{0}].[dbo].[{1}] WHERE [charname] = '{2}'", instancedb, "Character", charid.ToString());
            var result = await PerformQueryAsync(query).ConfigureAwait(false);

            //extract transactions from query
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            if((result[0])["transactionhistory"].GetType() == typeof(System.DBNull))
                return null;

            var serialisedstring = (string)((result[0])["transactionhistory"]);
            var transactions = JsonConvert.DeserializeObject<Dictionary<string, NPCStoreInfo.Transaction>>(serialisedstring, jsonSetting);

            return transactions;
        }

        public async Task UpdateTransactions(Dictionary<string, NPCStoreInfo.Transaction> transactions, string charid)
        {
            var transtr = JsonConvert.SerializeObject(transactions);

            var instancedb = "InstanceDBCS";
            var command = string.Format("UPDATE [{0}].[dbo].[{1}] SET [transactionhistory] = '{3}' WHERE [charname] = '{2}'", instancedb, "Character", charid, transtr);

           var success = await PerformSQLCommandAsync(command).ConfigureAwait(false);
        }
    }
}
