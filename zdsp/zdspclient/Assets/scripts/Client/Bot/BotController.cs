
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
            if (TheDijkstra.LastRouterByPortal !=null)
                TheDijkstra.LastRouterByPortal.Clear();
        }

        public enum BotMode : byte
        {
            AutoSkillAndMove = 0,//the full automatic mode, castskill and move, also can be controlled by player.
            ManulControl = 1, //the bot disabled
            AutoMove = 2, //atuo move and basic attack, skill only casted by player
            AutoSkill = 3,//autocast skill if within attack radius only,  does not seek target. 
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
        private int autoSkillIndex = -1;
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
        public bool Enabled {
            get { return mEnabled; }
        }


        //one function only do one task.
        private void SetBotMode(int mode)
        {
            if (mode >= 0 && mode < 4)
            {
                MODE = (BotMode)mode;
            }
        }

        public void StartBot(BotMode mode = BotMode.AutoSkillAndMove)
        {
            mEnabled = true;
            mHUDSkills.OnBotStart();
            MODE = mode;
            GameSettings.AutoBotEnabled = true;
        }

        /// <summary>
        /// Update the auto skills that player can cast in the bot mode.
        /// </summary>
        /// <param name="autoSkillRow">This list stores the skill IDs</param>
        public void UpdateAutoSkillRow(List<int> autoSkillRow)
        {
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

        private int GetNextAutoSkill(ref int index)
        {
            index++;
            if(index < autoSkills.Count)
            {
                return autoSkills[index].skillid;
            }
            index = -1;
            return 0;
        }

        public void StopBot() //Call when interrupt by player's manual playerinput, teleport, killed, etc
        {
            if (mEnabled)
            {
                lastQueried = 0;
                mCurrentTarget = null;
                mSeekingPosition = Vector3.zero;
                StopMoving();
                mEnabled = false;
                mHUDSkills.OnBotStop();
                GameSettings.AutoBotEnabled = false;
            }
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
            if(mLocalPlayer.IsMoving())
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


            PlayerIsControlledByKeyboard();

            if (mSeekingPosition.sqrMagnitude > 0)
            {
                if(mLocalPlayer.IsMoving()) //make sure player is walking .some target maybe unreachable
                    return;
            }

            if (MODE == BotMode.AutoSkill && mLocalPlayer.IsMoving())
            {
                mLocalPlayer.Idle();
                return;
            }

            //if user also init a skill cast, it will cast first . 
            if (PlayerInputSkill > 0 && mCurrentTarget != null)
            {
                int pid = mCurrentTarget.GetPersistentID();
                // TODO directly Cast skill
                //mHUDSkills.DirectCastSkill(PlayerInputSkill, (HUD_Skills.SkillNo)mPlayerInputAttackType, pid);
                PlayerInputSkill = 0;
                return;
            }

            if (!mLocalPlayer.IsMoving() && MODE != BotMode.AutoSkill)
            {
                //trying to cast the playerInputSkill

                ResetTarget();

                if (mCurrentTarget != null && mCurrentTarget.IsAlive())
                {
                    Vector3 direction = mCurrentTarget.Position - mLocalPlayer.Position;
                    direction.y = 0;
                    int nextSkill = -1;
                    //CastSkillWithAttackSequence(ref nextSkill);
                    float skillRange = ATTACK_RANGE; // TODO The cast range should be gotten from the radius field from Skill Table

                    if (direction.sqrMagnitude < skillRange * skillRange)//handle for dashattack distance
                    {
                        //determine the skill to use with the attack sequence, cast skill with high priority first
                        //CastSkillWithAttackSequence(ref nextSkill);
                        //GameInfo.gCombat.TryCastActiveSkill(nextSkill); // TODO cast skill to target

                        GameInfo.gSelectedEntity = mCurrentTarget;
                        int nextSkillid = GetNextAutoSkill(ref autoSkillIndex);
                        GameInfo.gCombat.TryCastActiveSkill(nextSkillid);
                    }
                    else
                    {
                        //if (!mLocalPlayer.IsRooted())
                        if (mLocalPlayer.IsIdling())
                        {
                            Vector3 pos = mCurrentTarget.Position;
                            Debug.Log("Pathfinding to target " + pos.ToString());
                            mLocalPlayer.PathFindToTarget(pos, -1, skillRange - 1f, true, false, () => { StopMoving(); }); //  move closer thank the ATTACK_RANGE for hit easily
                            //QueueQueryResult(mCurrentTarget.ID);//the target may be unreachable.
                        }
                    }
                }
                else
                {
                    bResetTarget = true;
                }
            }
            else
            {
                // mode is AutoSkill 
                if (MODE == BotMode.AutoSkill) //this mode   only cast skill,  
                {
                    mCurrentTarget = QueryForNonSpecificTarget(ATTACK_RANGE, true, null);//this mode is good for boss
                    if (mCurrentTarget != null && mCurrentTarget.IsAlive())
                    {
                        int castskillid = 999999;
                        CastSkillWithAttackSequence(ref castskillid); //passing 99999 means direct check.no check
                    }
                }
                else
                {
                    //let the bot move
                    mMovedTime += BotUpdateFreq;//this is the time walking
                    //Debug.Log("moving time " + mMovedTime);//3s timeout for approaching monster
                    if (mMovedTime > 3000)
                    {
                        mMovedTime = 0;
                        mCurrentTarget = null;
                    }
                }
            }
        }

        private void ResetTarget()
        {
            if (!bResetTarget)
                return;

            LookingForTarget();

            if (mCurrentTarget != null && mCurrentTarget.IsAlive())
            {
                bResetTarget = false;
                QueueQueryResult(mCurrentTarget.ID);//not query the same target continously 
                Debug.Log("Current target is " + mCurrentTarget.Name);
            }
            else
            {
                QueueQueryResult(0);
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

        private void LookingForTarget()
        {
            // Exclude self when query for the closet target. 
            int[] excludeSelfList = new int[2] { mLocalPlayer.ID, lastQueried }; //not query the same target always
            mCurrentTarget = QueryForNonSpecificTarget(MaxQueryRadius, true, excludeSelfList);
            //Debug.Log("setting target; " + mCurrentTarget.ID);
        }

        private void PlayerIsControlledByKeyboard()
        {
            if (mPlayerInput.IsControlling())
            {
                // Reset target and seeking position
                mCurrentTarget = null;
                mSeekingPosition = Vector3.zero;
                return;
            }
        }

        private bool IsInRealm()
        {
            return GameInfo.mRealmInfo.type == RealmType.Dungeon;
                //|| GameInfo.mRealmInfo.type == RealmType.ActivityWorldBoss
                //|| GameInfo.mRealmInfo.type == RealmType.ActivityGuildSMBoss
                //|| GameInfo.mRealmInfo.type == RealmType.EliteMap;
        }

        private long mMovedTime = 0;
        private void QueueQueryResult(int id)
        {
            lastQueried = id;
        }

        private bool bResetTarget = true;
        private int lastQueried = 0;
        private int mLastAttackType = 0;
        
        private int mPlayerInputSkill = 0;
        private AttackType mPlayerInputAttackType = AttackType.BasicAttack;
                

        public int PlayerInputSkill
        {
            get { return mPlayerInputSkill; }//now this is skillgroupid
            set {
                mPlayerInputSkill = value;
                if (mPlayerInputSkill == mLocalPlayer.SkillStats.JobskillAttackSId)
                    mPlayerInputAttackType = AttackType.JobSkill;
            }
        }

        private int mBasicAttackIndex = 0;
        /// <summary>
        /// Check the order of skills in the mAttackSequence,
        /// basic attack is the last option if no skill is ready.
        /// </summary>
        /// <param name="castSkillID"></param>
        void CastSkillWithAttackSequence(ref int castSkillID)
        {
            mLastAttackType = 0; // Always check skill in the queue.
            for (int i = mLastAttackType; i < mAttackSequence.Length; i++)
            {
                AttackType attackType = mAttackSequence[i];
                 
                if (attackType == AttackType.BasicAttack  )
                {
                     
                }
                else
                { 
                    if (MODE == BotMode.AutoMove)
                    {
                        //can only cast basic attack 
                        continue;
                    }
                    if (mLocalPlayer.IsSilenced())
                    { 
                        continue;//here must not use break, as basic Attack is still aviable if Silenced. 
                    } 

                    //Check if the skill was learnt and cooldowned                    
                    int skillID = 0;
                    SkillSynStats skillStats = mLocalPlayer.SkillStats;
                    PlayerSkillCDState cdState = GameInfo.gSkillCDState;
                    if (attackType == AttackType.JobSkill)
                    {
                        if (cdState.IsSkillCoolingDown(skillID))
                            continue;

                        skillID = skillStats.JobskillAttackSId;
                    }
                    
                    //castskillid -1 means it is checking next avaible skill. not casting yet.                
                    if (skillID > 0)//if skill is availiable, cast and record the order 
                    {
                        if (castSkillID < 0 && i != mAttackSequence.Length -1)//not basic attack;
                        {
                            castSkillID = skillID;
                            break;
                        }
                        else
                        {
                            int pid = mCurrentTarget != null ? mCurrentTarget.GetPersistentID() : 0;                             
                            //mHUDSkills.DirectCastSkill(skillid, (HUD_Skills.SkillNo)((int)attackType), pid);
                            mLastAttackType = i;
                            break; //once skill is cast , break out as only cast one skill each update 
                        } 
                    } 
                }
            }
            //if (mLastAttackType == mAttackSequence.Length - 1 && mBasicAttackIndex >= 4)
            //{
               // mLastAttackType = 0; //reset to 0 if last cast is basic attack chain end. 
            //}
        }

        /// <summary>
        /// A helper function to automatically select the next target and cast skill.
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

        /// <summary>
        /// a client helper function to face the nearest target.
        /// </summary>
        /// <returns></returns>
        public int FaceNearTarget()
        { 
            ActorGhost ghost =QueryForNonSpecificTarget(MaxQueryRadius, true,new int[] { mLocalPlayer.ID });
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
