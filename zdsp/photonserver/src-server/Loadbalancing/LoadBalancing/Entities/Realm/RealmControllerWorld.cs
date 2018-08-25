using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class RealmControllerWorld : RealmController
    {
        private RealmControllerWorldJson mLevelData;
        private int mPlaySpawnerCount = 0;
        public RealmWorldJson mRealmWorldInfo;

        public RealmControllerWorld(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mLevelData = (RealmControllerWorldJson)mPropertyInfos;
            mPlaySpawnerCount = mLevelData.spawnPos == null ? 0 : mLevelData.spawnPos.Length;
            mRealmWorldInfo = (RealmWorldJson)mRealmInfo;
        }

        public override bool IsCorrectController()
        {
            return mRealmInfo.type == RealmType.World;
        }

        public override void SetSpawnPos(Player player)
        {
            if (mPlaySpawnerCount == 0)
                base.SetSpawnPos(player);
            else
            {
                int index = 0;
                if (mPlaySpawnerCount > 1)
                    index = GameUtils.RandomInt(0, mPlaySpawnerCount - 1);
                player.Position = mLevelData.spawnPos[index];
                player.Forward = mLevelData.spawnDir[index];
            }
        }
    }
}
