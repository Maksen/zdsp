
using Zealot.Client.Entities;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Entities;
using Zealot.Client.Actions;
using UnityEngine.SceneManagement;
using Zealot.Repository;

namespace Zealot.Bot
{
    public enum ReachTargetAction
    {
        None = 0,
        StartBot=1,
        NPC_Interact =2, 
    }
    public class BotController
    {
        public static Vector3 DestMapPos = Vector3.zero;
        public static string DestLevel = "";
        public static ReachTargetAction DestMonsterOrNPC = ReachTargetAction.None;
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
        /// clear the world map router info,  called when user want to cancel the cross map path finding 
        /// </summary>
        public void ClearRouter()
        {
            DestLevel = "";
            DestMapPos = Vector3.zero;
            if(TheDijkstra.LastRouterByPortal !=null)
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

        //private HUD_Skills mHUDSkills;
        private PlayerGhost mLocalPlayer;
        private PlayerInput mPlayerInput;
        public Vector3 CurrentScreenCenterPos;
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
            //mHUDSkills = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
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
            //UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>().OnBotStart();
            MODE = mode;
            mLocalPlayer.ActionInterupted();
        }

        public void StopBot() //Call when interrupt by player's manual playerinput, teleport, killed, etc
        {
            //UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>().OnBotStop();
            if (mEnabled)
            {
                lastQueried = 0;
                mCurrentTarget = null;
                mSeekingPosition = Vector3.zero;
                StopMoving();
                mEnabled = false;
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

        /// <summary>
        /// this is for cross map pathfinding in the world map.  it is called when a world map is loaded. 
        /// not supposed to be called from Realm. 
        /// the path router info is saved in the  TheDijkstra.LastRouterByPortal
        /// 
        /// </summary>
        public void SeekingWithRouter()
        {
            string levelname = SceneManager.GetActiveScene().name;
            if (levelname == DestLevel) //if this is the last level , work to the destination pos
            {
                if (DestMonsterOrNPC == ReachTargetAction.StartBot)
                    GameInfo.gLocalPlayer.PathFindToTarget(DestMapPos, -1, ATTACK_RANGE, false, false, () => {
                        if (GameSettings.AutoBotEnabled)
                            StartBot();
                    });
                else if (DestMonsterOrNPC == ReachTargetAction.NPC_Interact)
                {
                    //GameInfo.gLocalPlayer.ProceedToTarget(DestMapPos, DestArchtypeID, Common.CallBackAction.Interact);
                } else if (DestMonsterOrNPC == ReachTargetAction.None)
                {
                    //GameInfo.gLocalPlayer.ProceedToTarget(DestMapPos, -1, CallBackAction.None);
                }
                //Reset the Router info
                ClearRouter();

            }
            else if (TheDijkstra.LastRouterByPortal != null && TheDijkstra.LastRouterByPortal.Count > 0)//if this is not last level, dequeue the next level and work to the portal link to the level
            {
                string str = TheDijkstra.LastRouterByPortal[levelname];//the disctionary is levelname, portalname 

                TheDijkstra.LastRouterByPortal.Remove(levelname);
                PortalEntryData ped = PortalInfos.mEntries[str];
                if (ped != null)
                { 
                    GameInfo.gLocalPlayer.PathFindToTarget(ped.mPosition, -1, 0, false, false, null);
                }
            }
        }
        private void StopMoving()
        {
            if(mLocalPlayer.IsMoving())
                mLocalPlayer.ForceIdle();
        }


        private long mElapsedDT = 0;
        private long mResetQuery = 3000;
        private ActorGhost mCurrentTarget;
        private int mNumberOfSkillsUsed = 0;
        private long NoMonstersTimer = 0;
        public void Update(long dt)
        {
            if (!mEnabled)
                return;

            if (MODE == BotMode.ManulControl)
                return;

            mElapsedDT += dt;
            if (mElapsedDT < BotUpdateFreq)
                return;

            mElapsedDT = 0;

            if (!mLocalPlayer.IsAlive())
                return;

            //stun, don't do anything
            if (mLocalPlayer.IsStun())
                return;

            if (mPlayerInput.IsControlling())
            {
                //reset something after player controller
                mCurrentTarget = null;
                mSeekingPosition = Vector3.zero;
                return;
            }

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
               // mHUDSkills.DirectCastSkill(PlayerInputSkill, (HUD_Skills.SkillNo)mPlayerInputAttackType, pid);
                PlayerInputSkill = 0;
                return;
            }
            if (!mLocalPlayer.IsMoving() && MODE != BotMode.AutoSkill)
            {
                //trying to cast the playerInputSkill

                //I am not doing path finding , can start do bot logic
                if (bResetTarget)//always query closet.
                {
                    //excudle self when query for close target. 
                    int[] list = new int[2] {mLocalPlayer.ID, lastQueried }; //not query the same target always
                    mCurrentTarget = QueryForNonSpecificTarget(MaxQueryRadius, true, list);
                    //Debug.Log("setting target; " + mCurrentTarget.ID);

                    if (mCurrentTarget != null && mCurrentTarget.IsAlive())
                    {
                        bResetTarget = false;
                        QueueQueryResult(mCurrentTarget.ID);//not query the same target continously                       
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
                if (mCurrentTarget != null && mCurrentTarget.IsAlive())
                {
                    Vector3 dir = mLocalPlayer.Position - mCurrentTarget.Position;
                    dir.y = 0;
                    int nextskill = -1;
                    CastSkillWithAttackSequence(ref nextskill);
                    float castdist = ATTACK_RANGE;
                    //if (nextskill > 0)
                    //{
                    //    SkillData sdata = SkillRepo.GetSkillByGroupID(nextskill);
                    //    if (sdata.skillgroupJson.dashattack)
                    //    {
                    //        castdist = sdata.skillgroupJson.range;
                    //    }
                    //}
                    if (dir.sqrMagnitude < castdist * castdist)//handle for dashattack distance
                    {
                        //determine the skill to use with the attack sequence, cast skill with high priority first
                        CastSkillWithAttackSequence(ref nextskill);
                    }
                    else
                    {
                        //if (!mLocalPlayer.IsRooted())
                        if (mLocalPlayer.IsIdling())
                        {
                            Vector3 pos = mCurrentTarget.Position;
                            Debug.Log("Pathfinding to target " + pos.ToString());
                            mLocalPlayer.PathFindToTarget(pos, -1, castdist - 1f, true, false, () => { StopMoving(); }); //  move closer thank the ATTACK_RANGE for hit easily
                            //QueueQueryResult(mCurrentTarget.ID);//the target may be unreachable.
                        }
                    }
                } else
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

        private bool IsInRealm()
        {
            return GameInfo.mRealmInfo.type == RealmType.DungeonStory
                || GameInfo.mRealmInfo.type == RealmType.DungeonDailySpecial;
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
                //if (mPlayerInputSkill == mLocalPlayer.SkillStats.RedHeroCardSkillAttackSId)
                //    mPlayerInputAttackType = AttackType.RedHeroCardSKill;
                //if (mPlayerInputSkill == mLocalPlayer.SkillStats.GreenHeroCardSkillAttackSId)
                //    mPlayerInputAttackType = AttackType.GreenHeroCardSKill;
                //if (mPlayerInputSkill == mLocalPlayer.SkillStats.BlueHeroCardSkillAttackSId)
                //    mPlayerInputAttackType = AttackType.BlueHeroCardSKill;
            }
        }

        private int mBasicAttackIndex = 0;
        /// <summary>
        /// it will check the skills in the order of mAttackSequence,
        /// basic attack is the last option if no skill is ready.
        /// </summary>
        /// <param name="castskillid"></param>
        void CastSkillWithAttackSequence(ref int castskillid)
        {
            mLastAttackType = 0;//always check skill in the queue.  
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
                    //Check if learnt and cooldowned                    
                    int skillid = 0;
                    SkillSynStats skillStats = mLocalPlayer.SkillStats;
                    PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
                    if (attackType == AttackType.JobSkill)
                    {
                        if (  cdstate.IsSkillCoolingDown(0))
                            continue;

                        skillid = skillStats.JobskillAttackSId;
                    }
                    //else if (attackType == AttackType.RedHeroCardSKill)
                    //{
                    //    if (  cdstate.IsSkillCoolingDown(1))
                    //        continue;

                    //    skillid = skillStats.RedHeroCardSkillAttackSId;
                    //}
                    //else if (attackType == AttackType.GreenHeroCardSKill)
                    //{
                    //    if (  cdstate.IsSkillCoolingDown(2))
                    //        continue;

                    //    skillid = skillStats.GreenHeroCardSkillAttackSId;
                    //}
                    //else if (attackType == AttackType.BlueHeroCardSKill)
                    //{
                    //    if (  cdstate.IsSkillCoolingDown(3))
                    //        continue;

                    //    skillid = skillStats.BlueHeroCardSkillAttackSId;
                    //}
                    //castskillid -1 means it is checking next avaible skill. not casting yet.                
                    if (skillid > 0)//if skill is availiable, cast and record the order 
                    {
                        if (castskillid < 0 && i != mAttackSequence.Length -1)//not basic attack;
                        {
                            castskillid = skillid;
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
        /// a helper function for select the next target automatically for cast skill.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="includeEliteAndBoss"></param>
        /// <param name="ExcludeList"></param>
        /// <returns></returns>
        public ActorGhost QueryForNonSpecificTarget(float radius, bool includeEliteAndBoss, int[] ExcludeList)
        {
            EntitySystem es = mLocalPlayer.EntitySystem; 
            
            Entity target = es.QueryForClosestEntityInSphere(mLocalPlayer.Position, radius, (queriedEntity) =>
            {
                if (queriedEntity.EntityType == EntityType.HeroGhost)
                    return false;
                int entId = queriedEntity.ID;
                if (ExcludeList!=null && ExcludeList.Contains(entId))
                    return false;
                MonsterGhost ghost = queriedEntity as MonsterGhost;
                if (ghost != null && ghost.IsAlive() && CombatUtils.IsValidEnemyTarget(mLocalPlayer, ghost))
                { 
                    MonsterClass monsterClass = ghost.mArchetype.monsterclass;
                    if (!includeEliteAndBoss && (monsterClass == MonsterClass.Mini || monsterClass == MonsterClass.Boss))
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
