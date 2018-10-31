using Kopio.JsonContracts;
using Photon.LoadBalancing.GameServer;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class RealmControllerWorld : RealmController
    {
        public RealmWorldJson mRealmWorldInfo;

        private RealmControllerWorldJson mLevelData;
        private int mPlayerSpawnerCount = 0;
        
        public RealmControllerWorld(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mRealmWorldInfo = (RealmWorldJson)mRealmInfo;
            mLevelData = (RealmControllerWorldJson)mPropertyInfos;
            mPlayerSpawnerCount = mLevelData.spawnPos == null ? 0 : mLevelData.spawnPos.Length;
        }

        public override bool IsCorrectController()
        {
            return mRealmInfo.type == RealmType.World;
        }

        public override void SetSpawnPos(Player player)
        {
            if (mPlayerSpawnerCount == 0)
                base.SetSpawnPos(player);
            else
            {
                int index = (mPlayerSpawnerCount > 1) ? GameUtils.RandomInt(0, mPlayerSpawnerCount-1) : 0;
                player.Position = mLevelData.spawnPos[index];
                player.Forward = mLevelData.spawnDir[index];
            }
        }
    }
}
