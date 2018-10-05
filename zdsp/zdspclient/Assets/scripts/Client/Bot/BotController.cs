using Zealot.Client.Entities;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Entities;
using Zealot.Client.Actions;
using UnityEngine.SceneManagement;
using Zealot.Repository;
using System.Collections.Generic;

namespace Zealot.Bot
{
    public class AutoSkill
    {
        public int skillid;
        public int priority;

        public AutoSkill(int skillid, int skillPriority)
        {
            this.skillid = skillid;
            this.priority = skillPriority;
        }
    }

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
        /// the actack type define the skill casting behavior of the player,
        /// from this enum, you should determine the skillid the player is current is using.
        /// (edited in the combatUI);
        /// </summary>
        public enum AttackType : int
        {
            BasicAttack = 0,
            JobSkill = 1,
            RedHeroCardSKill = 2,
            GreenHeroCardSKill = 3,
            BlueHeroCardSKill = 4
        }

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

        public enum BotMode : byte
        {
            AutoSkillAndMove = 0,//the full automatic mode, castskill and move, also can be controlled by player.
            ManulControl = 1, //the bot disabled
        }

        public static BotMode MODE = BotMode.ManulControl;
        public static readonly float ATTACK_RANGE = 3.0f;
        private static readonly long BotUpdateFreq = 250;//update every 100 msec
        public static readonly float MaxQueryRadius = 12.0f; //should be < selectedentity removal radius  , in server logic, 10 is the reverant query dist, 20 is the dist only update action and snapshot

        private HUD_Skills mHUDSkills;
        private PlayerGhost mLocalPlayer;
        private PlayerInput mPlayerInput;
        public Vector3 CurrentScreenCenterPos;
        private List<AutoSkill> autoSkills = new List<AutoSkill>();
        private int autoSkillIndex = -1; // The first index of list is 0
        private int skillidToCast = 0; // 0 means no skill
        private ActorGhost manualSelectTarget; // The target that is selected by user in bot mode

        private AttackType[] mAttackSequence = new AttackType[] {
            AttackType.JobSkill,
            AttackType.RedHeroCardSKill,
            AttackType.GreenHeroCardSKill,
            AttackType.BlueHeroCardSKill,
            AttackType.BasicAttack
        };


        public BotController(PlayerGhost playerGhost, PlayerInput playerInput)
        {
            mEnabled = false;
            mLocalPlayer = playerGhost;
            mPlayerInput = playerInput;
            mHUDSkills = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
            TheDijkstra.Setup();
        }

        private bool mEnabled;
        public bool Enabled
        {
            get { return mEnabled; }
        }

        public void StartBot(BotMode mode = BotMode.AutoSkillAndMove)
        {
            mEnabled = true;
            mHUDSkills.OnBotStart();
            MODE = mode;
            GameSettings.AutoBotEnabled = true;
            SetupManualSelectedTarget((ActorGhost)GameInfo.gSelectedEntity);
        }

        public void StopBot()
        {
            if (!mEnabled)
                return;

            mSeekingPosition = Vector3.zero;
            StopMoving();
            mEnabled = false;
            mHUDSkills.OnBotStop();
            GameSettings.AutoBotEnabled = false;
            mPlayerInput.ListenForNewEnemy(null);
        }

        /// <summary>
        /// Update the auto skills that player can cast in the bot mode.
        /// </summary>
        /// <param name="autoSkillRow">This list stores the skill IDs</param>
        public void UpdateAutoSkillRow()
        {
            List<int> autoSkillRow = GetAutoSkillRow();
            if (autoSkillRow == null)
                return;

            autoSkills.Clear();
            for (int i = 0; i < autoSkillRow.Count; i++)
            {
                autoSkills.Add(new AutoSkill(autoSkillRow[i], SkillRepo.GetSkillPriority(autoSkillRow[i])));
            }

            SortAutoSkillByPriority();
        }

        private void SortAutoSkillByPriority()
        {
            // Order by descending
            autoSkills.Sort((skill1, skill2) => { return -skill1.priority.CompareTo(skill2.priority); });
        }

        private int GetNextAutoSkill()
        {
            if (++autoSkillIndex < autoSkills.Count)
            {
                return autoSkills[autoSkillIndex].skillid;
            }
            return autoSkillIndex = -1;
        }

        private List<int> GetAutoSkillRow()
        {
            List<int> autoSkillRow = new List<int>();

            for (int i = 0; i < 5; ++i)
            {
                int autoSkillID = (int)GameInfo.gLocalPlayer.SkillStats.AutoSkill[ConvertToAutoSkillIndex(i)];

                if (autoSkillID != 0) // 0 means no skill in the slot
                {
                    autoSkillRow.Add(autoSkillID);
                }
            }

            return autoSkillRow;
        }

        private int ConvertToAutoSkillIndex(int index)
        {
            // TODO hardcode the EquippableSize for temp
            return 5 * (GameInfo.gLocalPlayer.SkillStats.AutoGroup - 1) + index;
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
        /// 
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
                RemoveNextLevel(levelname);
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
            PortalEntryData portalEntryData = PortalInfos.mEntries[str];
            if (portalEntryData != null)
            {
                if (DestAction == ReachTargetAction.PartyFollow && PartyFollowTarget.IsPaused())
                {
                    PartyFollowTarget.Resume(true);
                }
                else
                {
                    GameInfo.gLocalPlayer.PathFindToTarget(portalEntryData.mPosition, -1, 0, false, false, null);
                }
            }
        }

        private void MoveToDestination()
        {
            switch (DestAction)
            {
                case ReachTargetAction.StartBot:
                    GameInfo.gLocalPlayer.PathFindToTarget(DestMapPos, -1, ATTACK_RANGE, false, false, () => {
                        if (GameSettings.AutoBotEnabled)
                            StartBot();
                    });
                    break;
                case ReachTargetAction.NPC_Interact:
                    GameInfo.gLocalPlayer.ProceedToTarget(DestMapPos, DestArchtypeID, CallBackAction.Interact);
                    break;
                case ReachTargetAction.PartyFollow:
                    PartyFollowTarget.Resume(true);
                    break;
                default:
                    //GameInfo.gLocalPlayer.PathFindToTarget(DestMapPos, -1, 0, false, false, null);
                    break;
            }

            ClearRouter(); //Reset the Router info
        }
        #endregion

        private void StopMoving()
        {
            if (mLocalPlayer.IsMoving())
                mLocalPlayer.ForceIdle();
        }


        private long mElapsedDeltaTime = 0;
        private long mResetQuery = 3000;
        private ActorGhost mCurrentTarget;
        private int mNumberOfSkillsUsed = 0;
        private long NoMonstersTimer = 0;


        public void Update(long deltaTime)
        {
            if (!mEnabled)
                return;

            if (MODE == BotMode.ManulControl)
                return;

            mElapsedDeltaTime += deltaTime;
            if (mElapsedDeltaTime < BotUpdateFreq)
                return;

            mElapsedDeltaTime = 0;

            if (!mLocalPlayer.IsAlive())
                return;

            //stun, don't do anything
            if (mLocalPlayer.IsStun())
                return;

            if (IsControlledByKeyboard())
                return;

            if (mSeekingPosition.sqrMagnitude > 0)
            {
                if (mLocalPlayer.IsMoving()) // Make sure player is walking. Some targets might be unreachable
                    return;
            }

            if (!mLocalPlayer.IsMoving())
            {
                if (shouldResetTarget)
                    ResetTarget();

                if (CurrentTargetIsValidAndAlive())
                {
                    SetupNextAutoSkill();

                    if (!CanCastSkill())
                        return;

                    AttackTarget();
                }
                else
                {
                    shouldResetTarget = true;
                }
            }
            else
            {
                if (GameInfo.gSelectedEntity != null)
                    ManuallyChangeTarget();

                shouldResetTarget = true;

                //ClearTargetAfterTimeOut(3000);
            }
        }

        private bool CanCastSkill()
        {
            return IsSkillValid() && !IsSkillCoolingDown() && IsManaEnough();
        }

        private void ClearTargetAfterTimeOut(float time)
        {
            mMovedTime += BotUpdateFreq;//this is the time walking
            //Debug.Log("moving time " + mMovedTime);//3s timeout for approaching monster
            if (mMovedTime > time)
            {
                mMovedTime = 0;
                SetupCurrentTarget(null);
            }
        }

        private void SetupCurrentTarget(ActorGhost entity)
        {
            mCurrentTarget = entity;
            GameInfo.gSelectedEntity = mCurrentTarget;

            if (entity != null)
                mPlayerInput.SetMoveIndicator(Vector3.zero);
        }

        private void SetupManualSelectedTarget(ActorGhost entity)
        {
            manualSelectTarget = entity;
            GameInfo.gCombat.OnSelectEntity(entity);
            SetupCurrentTarget(entity);
        }

        private bool CurrentTargetIsValidAndAlive()
        {
            return mCurrentTarget != null && mCurrentTarget.IsAlive();
        }

        private void ManuallyChangeTarget()
        {
            mPlayerInput.ListenForNewEnemy((ActorGhost entity) =>
            {
                if (entity == null) return;

                SetupManualSelectedTarget(entity);
                shouldResetTarget = false;

                mLocalPlayer.PathFindToTarget(entity.Position, -1, 0, false, false, null);
            });
        }

        private void ResetTarget()
        {
            if (IsManualSelectedTargetAlive())
                return;
            else
                manualSelectTarget = null;

            var newTarget = GetNearestEnemyInRange(MaxQueryRadius);
            SetupCurrentTarget(newTarget);

            if (CurrentTargetIsValidAndAlive())
            {
                shouldResetTarget = false;
            }
            else
            {
                SetupCurrentTarget(null);

                //no target availiable at the momment. 
                NoMonstersTimer += BotUpdateFreq;
                if (NoMonstersTimer >= 1000 && IsInRealm())
                {
                    RPCFactory.CombatRPC.GetClosestValidMonSpawnPos();
                    NoMonstersTimer = 0;
                    Debug.Log("getting nearest monster from server!!!");
                }
            }
        }

        private bool IsManualSelectedTargetAlive()
        {
            return (manualSelectTarget != null && manualSelectTarget.IsAlive());
        }

        private void SetupNextAutoSkill()
        {
            skillidToCast = GetNextAutoSkill();
        }

        private bool IsSkillValid()
        {
            SkillData sdata = SkillRepo.GetSkill(skillidToCast);
            if (sdata == null)
                return false;
            return true;
        }

        private bool IsSkillCoolingDown()
        {
            PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
            if (cdstate.IsSkillCoolingDown(skillidToCast))
                return true;
            return false;
        }

        private bool IsManaEnough()
        {
            SkillData sdata = SkillRepo.GetSkill(skillidToCast);

            float cost = 0;
            if (sdata.skillgroupJson.costab)
                cost = sdata.skillJson.cost;
            else
                cost = mLocalPlayer.GetManaMax() * sdata.skillJson.cost * 0.01f;

            return mLocalPlayer.GetMana() >= cost;
        }

        private void AttackTarget()
        {
            GameInfo.gCombat.TryCastActiveSkill(skillidToCast);
        }

        private bool IsControlledByKeyboard()
        {
            if (mPlayerInput.IsControlling())
            {
                shouldResetTarget = true;
                mSeekingPosition = Vector3.zero; // Reset seeking position
                return true;
            }
            return false;
        }

        private bool IsInRealm()
        {
            return GameInfo.mRealmInfo.type == RealmType.Dungeon;
            //|| GameInfo.mRealmInfo.type == RealmType.ActivityWorldBoss
            //|| GameInfo.mRealmInfo.type == RealmType.ActivityGuildSMBoss
            //|| GameInfo.mRealmInfo.type == RealmType.EliteMap;
        }

        private long mMovedTime = 0;
        private bool shouldResetTarget = true;
        private int mLastAttackType = 0;
        private int mPlayerInputSkill = 0;
        private AttackType mPlayerInputAttackType = AttackType.BasicAttack;

        public int PlayerInputSkill
        {
            get { return mPlayerInputSkill; } // Now this is skillgroupid
            set
            {
                mPlayerInputSkill = value;
                if (mPlayerInputSkill == mLocalPlayer.SkillStats.JobskillAttackSId)
                    mPlayerInputAttackType = AttackType.JobSkill;
            }
        }

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
                {   //Bot initiate attack against other players based on pvp rules when not in questing mode
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

        public ActorGhost GetNearestEnemyInRange(float radius)
        {
            int[] excludeSelfList = new int[1] { mLocalPlayer.ID }; // Exclude self when query for the closet target.
            return QueryForNonSpecificTarget(radius, true, excludeSelfList);
        }

        public ActorGhost GetFriendlyInRange(float radius)
        {
            // TODO Query the friendly(include self) in the range
            return null;
        }

        /// <summary>
        /// a client helper function to face the nearest target.
        /// </summary>
        /// <returns></returns>
        public int FaceNearTarget()
        {
            ActorGhost ghost = QueryForNonSpecificTarget(MaxQueryRadius, true, new int[] { mLocalPlayer.ID });
            if (ghost != null)
            {
                Vector3 direction = ghost.Position - GameInfo.gLocalPlayer.Position;
                direction.y = 0f;
                direction.Normalize();
                GameInfo.gLocalPlayer.Forward = direction;
                return ghost.GetPersistentID();
            }
            return 0;
        }
    }
}
