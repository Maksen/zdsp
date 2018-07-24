using Zealot.Entities;
using Photon.LoadBalancing.GameServer;

namespace Zealot.Server.Entities
{
    public class RealmControllerBossSlowMotion : RealmController
    {
        protected int mBossPId = 0;

        public RealmControllerBossSlowMotion(RealmControllerJson info, GameLogic instance) : base(info, instance)
        {
        }

        #region Triggers
        public override void CompleteRealm(IServerEntity sender, object[] parameters = null)
        {
            MonsterSpawner sp = sender as MonsterSpawner;
            if (sp != null)
                mBossPId = ((Monster)parameters[1]).GetPersistentID();
            base.CompleteRealm(sender, parameters);
        }
        #endregion
    }
}
