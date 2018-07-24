using System.Linq;

namespace Photon.LoadBalancing.GameServer
{
    public static partial class GameConfig
    {
        public static void Init()
        {
            int serverid = GameApplication.Instance.GetMyServerId();
            var dbconfigs = GameApplication.dbRepository.GameConfig.GetConfig(serverid).Result;

            dbconfigs = dbconfigs.OrderBy(dbconfig => dbconfig["serverid"]).ToList();
            foreach (var dbconfig in dbconfigs)
            {
                string key = (string)dbconfig["key"];
                string val = (string)dbconfig["value"];
                var f = typeof(GameConfig).GetField(key);
                if (f != null)
                {
                    if (f.FieldType.Name.Equals("Int32"))
                        f.SetValue(null, int.Parse(val));
                    else if (f.FieldType.Name.Equals("Single"))
                        f.SetValue(null, float.Parse(val));
                    else
                        f.SetValue(null, val);
                }
                else
                {
                    //找不到就忽略
                }
            }
        }
    }
}
