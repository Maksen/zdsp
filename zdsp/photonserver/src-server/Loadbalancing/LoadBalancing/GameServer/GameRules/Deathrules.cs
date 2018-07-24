using Photon.LoadBalancing.GameServer;


namespace Zealot.Server.Rules
{
    public static class DeathRules
    {
        public static void LogDeathRespawnType(string method, int mapId, GameClientPeer peer)
        {
            string message = string.Format("Respawn Method: {0} | Map Id: {1}",
                method,
                mapId);

            Zealot.Logging.Client.LogClasses.DeathRespawnType deathRespawnTypeLog = new Zealot.Logging.Client.LogClasses.DeathRespawnType();
            deathRespawnTypeLog.userId          = peer.mUserId;
            deathRespawnTypeLog.charId          = peer.GetCharId();
            deathRespawnTypeLog.message         = message;
            deathRespawnTypeLog.respawnMethod   = method;
            deathRespawnTypeLog.mapId           = mapId;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(deathRespawnTypeLog);
        }

        public static void LogDeathRespawnFree(string action, int useAmount, int freeReviveBef, int freeReviveAft, GameClientPeer peer)
        {
            string message = string.Format("Action: {0} | Use Amount: {1} | Free Revive Before: {2} | Free Revive After: {3}",
                action,
                useAmount,
                freeReviveBef,
                freeReviveAft);

            Zealot.Logging.Client.LogClasses.DeathRespawnFree deathRespawnFreeLog = new Zealot.Logging.Client.LogClasses.DeathRespawnFree();
            deathRespawnFreeLog.userId          = peer.mUserId;
            deathRespawnFreeLog.charId          = peer.GetCharId();
            deathRespawnFreeLog.message         = message;
            deathRespawnFreeLog.actionType      = action;
            deathRespawnFreeLog.useAmount       = useAmount;
            deathRespawnFreeLog.freeReviveBef   = freeReviveBef;
            deathRespawnFreeLog.freeReviveAft   = freeReviveAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(deathRespawnFreeLog);
        }

        public static void LogDeathRespawnItem(string action, int itemId, int itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Action: {0} | Item Id: {1} | Item Amount: {2}",
                action,
                itemId,
                itemAmount);

            Zealot.Logging.Client.LogClasses.DeathRespawnItem deathRespawnItemLog = new Zealot.Logging.Client.LogClasses.DeathRespawnItem();
            deathRespawnItemLog.userId      = peer.mUserId;
            deathRespawnItemLog.charId      = peer.GetCharId();
            deathRespawnItemLog.message     = message;
            deathRespawnItemLog.actionType  = action;
            deathRespawnItemLog.itemId      = itemId;
            deathRespawnItemLog.itemAmount  = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(deathRespawnItemLog);
        }

        public static void LogDeathRespawnLockGold(string action, int useAmount, int lockGoldBef, int lockGoldAft, GameClientPeer peer)
        {
            string message = string.Format("Action: {0} | Use Amount: {1} | Lock Gold Before: {2} | Lock Gold After: {3}",
                action,
                useAmount,
                lockGoldBef,
                lockGoldAft);

            Zealot.Logging.Client.LogClasses.DeathRespawnLockGold deathRespawnLockGoldLog = new Zealot.Logging.Client.LogClasses.DeathRespawnLockGold();
            deathRespawnLockGoldLog.userId      = peer.mUserId;
            deathRespawnLockGoldLog.charId      = peer.GetCharId();
            deathRespawnLockGoldLog.message     = message;
            deathRespawnLockGoldLog.actionType  = action;
            deathRespawnLockGoldLog.useAmount   = useAmount;
            deathRespawnLockGoldLog.lockGoldBef = lockGoldBef;
            deathRespawnLockGoldLog.lockGoldAft = lockGoldAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(deathRespawnLockGoldLog);
        }

        public static void LogDeathRespawnGold(string action, int useAmount, int goldBef, int goldAft, GameClientPeer peer)
        {
            string message = string.Format("Action: {0} | Use Amount: {1} | Gold Before: {2} | Gold After: {3}",
                action,
                useAmount,
                goldBef,
                goldAft);

            Zealot.Logging.Client.LogClasses.DeathRespawnGold deathRespawnGoldLog = new Zealot.Logging.Client.LogClasses.DeathRespawnGold();
            deathRespawnGoldLog.userId      = peer.mUserId;
            deathRespawnGoldLog.charId      = peer.GetCharId();
            deathRespawnGoldLog.message     = message;
            deathRespawnGoldLog.actionType  = action;
            deathRespawnGoldLog.useAmount   = useAmount;
            deathRespawnGoldLog.goldBef     = goldBef;
            deathRespawnGoldLog.goldAft     = goldAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(deathRespawnGoldLog);
        }
    }
}
