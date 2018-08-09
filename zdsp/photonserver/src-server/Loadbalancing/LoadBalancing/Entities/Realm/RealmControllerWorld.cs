using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class RealmControllerWorld : RealmController
    {
        private RealmControllerWorldJson mUnityData;
        private int mPlaySpawnerCount = 0;
        public RealmWorldJson mRealmWorldInfo;

        public RealmControllerWorld(RealmControllerJson info, GameLogic instance)
            : base(info, instance)
        {
            if (!IsCorrectController())
                return;
            mUnityData = (RealmControllerWorldJson)mPropertyInfos;
            mPlaySpawnerCount = mUnityData.spawnPos == null ? 0 : mUnityData.spawnPos.Length;
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
                player.Position = mUnityData.spawnPos[index];
                player.Forward = mUnityData.spawnDir[index];
            }
        }
    }
}
