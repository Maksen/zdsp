using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public class RespawnRepo
    {
        public static Dictionary<int, RespawnJson> respawnIDJsonMap; // RespawnID -> Json

        static RespawnRepo()
        {
            respawnIDJsonMap = new Dictionary<int, RespawnJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, RespawnJson> entry in gameData.Respawn)
            {
                int respawnID = entry.Value.respawnid;
                if(respawnID > 0 && !respawnIDJsonMap.ContainsKey(respawnID))
                {
                    respawnIDJsonMap.Add(respawnID, entry.Value);
                }
            }
        }

        public static RespawnJson GetRespawnDataByID(int respawnId)
        {
            if(respawnIDJsonMap.ContainsKey(respawnId))
            {
                return respawnIDJsonMap[respawnId];
            }

            return null;
        }
    }
}
