using UnityEngine;

namespace Zealot.Entities
{
    public class RealmControllerJson : ServerEntityWithEventJson
    {
        public Vector3 forward = Vector3.forward;
        public override string GetServerClassName() { return "RealmController"; }
    }

    public class RealmControllerWorldJson : RealmControllerJson
    {
        public Vector3[] spawnPos;
        public Vector3[] spawnDir;

        public override string GetServerClassName() { return "RealmControllerWorld"; }
    }

    public class RealmControllerDungeonJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerDungeon"; }
    }

    public class RealmControllerDungeonDailySpecialJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerDungeonDailySpecial"; }
    }

    public class RealmControllerPartyJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerParty"; }
    }
 
    public class RealmControllerInvitePVPJson : RealmControllerJson
    {
        public Vector3[] spawnPos;
        public override string GetServerClassName() { return "RealmControllerInvitePVP"; }
    }

    #region Activities
    public class RealmControllerGuildSMBossJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerGuildSMBoss"; }
    }

    public class RealmControllerTutorialJson : RealmControllerJson
    {         
        public override string GetServerClassName() { return "RealmControllerTutorial"; }
    }

    public class RealmControllerArenaJson : RealmControllerJson
    {
        public Vector3 aiPos;
        public Vector3 aiForward;
        public override string GetServerClassName() { return "RealmControllerArena"; }
    }
 
    public class RealmControllerWorldBossJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerWorldBoss"; }
    }

    public class RealmControllerEliteMapJson : RealmControllerJson
    {
        public override string GetServerClassName() { return "RealmControllerEliteMap"; }
    }

    #endregion
}