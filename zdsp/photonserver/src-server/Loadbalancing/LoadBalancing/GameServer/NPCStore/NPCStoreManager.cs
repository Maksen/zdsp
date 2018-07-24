using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.DBRepository.GM.NPCStore;

namespace Photon.LoadBalancing.GameServer.NPCStore
{
    class NPCStoreManager
    {
        private static NPCStoreManager _instance;

        private Dictionary<int, NPCStoreInfo> StoreData;
        public Dictionary<int, NPCStoreInfo> storedata
        {
            get { return StoreData; }
        }

        private NPCStoreManager()
        {
        }

        public static async Task<NPCStoreManager> InstanceAsync()
        {
            if (_instance == null || _instance.initialised == false)
            {
                _instance = new NPCStoreManager();

                await _instance.UpdateNPCStoresAsync().ConfigureAwait(false);
            }

            return _instance;
        }

        bool inited = false;
        public bool initialised
        {
            get { return inited; }
        }
        public async Task UpdateNPCStoresAsync()
        {
            StoreData = await GameApplication.dbGM.NPCStoreGMRepo.GetStoreData().ConfigureAwait(false);
            inited = true;
        }
    }
}
