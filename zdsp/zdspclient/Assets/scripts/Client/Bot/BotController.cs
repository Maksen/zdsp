using UnityEngine;
using UnityEngine.SceneManagement;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Entities;

namespace Zealot.Bot
{
    public enum ReachTargetAction // The action when player reached the target
    {
        None = 0,
        StartBot = 1,
        NPC_Interact = 2,
        PartyFollow = 3
    }

    public class BotController
    {
        public static Vector3 DestMapPos = Vector3.zero;
        public static string DestLevel = "";
        public static ReachTargetAction DestAction = ReachTargetAction.None;
        public static int DestArchtypeID = -1;
        public static bool AutoStartInRealm = false;
        public static WorldMapGraphWithDijkstra TheDijkstra = new WorldMapGraphWithDijkstra();

        /// <summary>
        /// Clear the world map router info. Will be called when user wanna cancel the cross map path finding.
        /// </summary>
        public void ClearRouter()
        {
            DestLevel = "";
            DestMapPos = Vector3.zero;
            DestAction = ReachTargetAction.None;
            DestArchtypeID = -1;
            if (TheDijkstra.LastRouterByPortal != null)
                TheDijkstra.LastRouterByPortal.Clear();
        }

        public static readonly float ATTACK_RANGE = 3.0f;
        public static readonly float MaxQueryRadius = 12.0f; //should be < selectedentity removal radius  , in server logic, 10 is the reverant query dist, 20 is the dist only update action and snapshot

        private HUD_Skills mHUDSkills = null;
        private PlayerGhost mLocalPlayer = null;
        private BotStateController mBotStateController = null;
        public Vector3 CurrentScreenCenterPos;
        private bool mEnabled;


        public BotController(PlayerGhost playerGhost)
        {
            mEnabled = GameSettings.AutoBotEnabled;
            mLocalPlayer = playerGhost;
            mHUDSkills = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
            TheDijkstra.Setup();
            mBotStateController = BotStateController.Instance;
        }

        public void StartBot()
        {
            if (mEnabled)
                return;

            mEnabled = true;
            GameSettings.AutoBotEnabled = true;
            mHUDSkills.OnBotStart();
            mBotStateController.Start();
        }

        public void StopBot()
        {
            if (!mEnabled)
                return;

            mEnabled = false;
            GameSettings.AutoBotEnabled = false;
            mHUDSkills.OnBotStop();
            mSeekingPosition = Vector3.zero;
            mBotStateController.Stop();
        }

        public void Update(long deltaTime)
        {
            if (!mEnabled)
                return;

            mBotStateController.Update(deltaTime);
        }

        private Vector3 mSeekingPosition = Vector3.zero;
        private System.Action OnSeekDone;
        /// <summary>
        /// when the bot is running, call this will pathfind to the new position
        /// and the bot is not stopped , 
        /// if the destination have monster nearby, the bot will continue from there.
        /// if you want to stop bot first, call stopbot();
        /// </summary>
        /// <param name="Pos"></param>
        /// <param name="action"></param>
        public void SeekToPosition(Vector3 Pos, System.Action action = null)
        {
            //seeking to the target then attack
            Debug.Log("seeking to pos: " + Pos.ToString());
            if (Pos.magnitude == 0)
                return;
            mLocalPlayer.ForceIdle();//prepare for next update.
            mSeekingPosition = Pos;
            OnSeekDone = action;

            mLocalPlayer.PathFindToTarget(mSeekingPosition, -1, ATTACK_RANGE, true, false, () => {
                mSeekingPosition = Vector3.zero;
                if (OnSeekDone != null)
                {
                    OnSeekDone.Invoke();
                }
            }); //  move closer thank the ATTACK_RANGE for hit easily
        }

        public bool IsSeekingWithRouter()
        {
            return !string.IsNullOrEmpty(DestLevel);
        }

        #region Seeking With Router(Cross map)
        /// <summary>
        /// this is for cross map pathfinding in the world map.  it is called when a world map is loaded. 
        /// not supposed to be called from Realm. 
        /// the path router info is saved in the  TheDijkstra.LastRouterByPortal
        /// </summary>
        public void SeekingWithRouter()
        {
            string levelname = SceneManager.GetActiveScene().name;

            if (levelname == DestLevel) // If this is the last level
            {
                MoveToDestination();
            }
            else if (IsNotTheLastLevel())
            {
                MoveToPortal(levelname);
            }
        }

        private bool IsNotTheLastLevel()
        {
            return (TheDijkstra.LastRouterByPortal != null && TheDijkstra.LastRouterByPortal.Count > 0);
        }

        private void RemoveNextLevel(string levelName)
        {
            TheDijkstra.LastRouterByPortal.Remove(levelName);
        }

        private void MoveToPortal(string levelName)
        {
            string str = TheDijkstra.LastRouterByPortal[levelName];// Disctionary<levelname, portalname>
            RemoveNextLevel(levelName);
            PortalEntryData portalEntryData = PortalInfos.mEntries[str];
            if (portalEntryData != null)
            {
                if (DestAction == ReachTargetAction.PartyFollow && PartyFollowTarget.IsPaused())
                {
                    PartyFollowTarget.Resume(true);
                }
                else
                {
                    mLocalPlayer.PathFindToTarget(portalEntryData.mPosition, -1, 0, false, false, null);
                }
            }
        }

        private void MoveToDestination()
        {
            switch (DestAction)
            {
                case ReachTargetAction.StartBot:
                    mLocalPlayer.PathFindToTarget(DestMapPos, -1, ATTACK_RANGE, false, false, () => {
                        if (GameSettings.AutoBotEnabled)
                            StartBot();
                    });
                    break;
                case ReachTargetAction.NPC_Interact:
                    mLocalPlayer.ProceedToTarget(DestMapPos, DestArchtypeID, CallBackAction.Interact);
                    break;
                case ReachTargetAction.PartyFollow:
                    PartyFollowTarget.Resume(true);
                    break;
                default:
                    mLocalPlayer.PathFindToTarget(DestMapPos, -1, 0, false, false, null);
                    break;
            }

            ClearRouter(); //Reset the Router info
        }
        #endregion

        /// <summary>
        /// A helper function to automatically select the next target.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="includeEliteAndBoss"></param>
        /// <param name="ExcludeList"></param>
        /// <returns></returns>
        public ActorGhost QueryForNonSpecificTarget(float radius, bool includeEliteAndBoss, int[] ExcludeList)
        {
            EntitySystem entitySystem = mLocalPlayer.EntitySystem;

            Entity target = entitySystem.QueryForClosestEntityInSphere(mLocalPlayer.Position, radius, (queriedEntity) =>
            {
                if (queriedEntity.EntityType == EntityType.HeroGhost)
                    return false;
                int entityID = queriedEntity.ID;
                if (ExcludeList != null && ExcludeList.Contains(entityID))
                    return false;

                MonsterGhost ghost = queriedEntity as MonsterGhost;
                if (ghost != null && ghost.IsAlive() && CombatUtils.IsValidEnemyTarget(mLocalPlayer, ghost))
                {
                    MonsterType monsterType = ghost.mArchetype.monstertype;
                    // The target type is set to exclude the mini boss and boss
                    if (!includeEliteAndBoss && (monsterType == MonsterType.MiniBoss || monsterType == MonsterType.Boss))
                        return false;
                    return true;
                }
                else
                {
                    //Bot initiate attack against other players based on pvp rules when not in questing mode
                    PlayerGhost otherPlayer = queriedEntity as PlayerGhost;
                    if (otherPlayer != null && otherPlayer.IsAlive() && CombatUtils.IsValidEnemyTarget(mLocalPlayer, otherPlayer))
                    {
                        return true;
                    }
                }
                return false;
            });

            return target as ActorGhost;
        }
    }
}