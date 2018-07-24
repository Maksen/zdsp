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
                ret.Add((int)row["ID"], new NPCStoreInfo((int)row["ID"], (string)row["localisedname"], (string)row["name"], (NPCStoreInfo.StoreType)((string)row["type"]).ToCharArray()[0]));
            }

            var normalitemrows = await GetSoldItemsAsync().ConfigureAwait(false);
            foreach (var row in normalitemrows)
            {
                var storeid = (int)row["storeid"];

                var a = (int)row["storeid"];
                var b = (int)row["entryid"];
                var c = (bool)row["show"];
                var d = (int)row["itemid"];
                var e = NPCStoreInfo.ItemStoreType.Normal;
                var f = (int)row["itemvalue"];
                var g = (NPCStoreInfo.SoldCurrencyType)((string)row["soldtype"]).ToCharArray()[0];
                var h = (int)row["soldvalue"];
                var i = (double)row["discount"];
                var j = (int)row["sortnumber"];
                var k = (DateTime)row["starttime"];
                var l = (DateTime)row["endtime"];
                var m = (int)row["excount"];
                var n = (NPCStoreInfo.Frequency)((string)row["dailyorweekly"]).ToCharArray()[0];

                ret[storeid].inventory.Add(new Zealot.Common.NPCStoreInfo.StandardItem(a, b, c, d, e, f, g, h, (float)i, j, k, l, m, n));
            }

            return ret;
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
    }
}
