using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Common.Actions;
using Zealot.Repository;
using Zealot.Client.Actions;

namespace Zealot.Client.Entities
{
	public class MonsterGhost : ActorGhost
	{
		public CombatNPCJson mArchetype;
        
        private SkinnedMeshRenderer mRenderer;
        private Flash mFlashEffect;

        public override bool CanSelect { get { return !IsInvalidTarget(); } }

        public MonsterGhost() : base()
		{
			this.EntityType = EntityType.MonsterGhost;
            mSkillIndicator120 = null;
            mSkillIndicator360 = null;
            mSkillIndicatorLong = null;
        }        

        public void OnNewlyAdded()
        {
            //Do nothing if no realm
            //Do nothing if label is already showing
            if (GameInfo.mRealmInfo == null)
                return;

            bool isEnemy = CombatUtils.IsEnemy(GameInfo.gLocalPlayer, this);

            switch (GameInfo.mRealmInfo.type)
            {
                //case RealmType.Arena:
                //case RealmType.InvitePVP:
                //case RealmType.ActivityGuardWar:
                //case RealmType.ActivityGuildSMBoss:
                //case RealmType.ActivityWorldBoss:
                //case RealmType.EliteMap:
                //    if (isEnemy)
                //    {
                //        switch (mArchetype.monstertype)
                //        {
                //            case MonsterType.Normal:
                //            case MonsterType.MiniBoss:
                //                HeadLabel.mPlayerLabel.SetMonster(); //hide regular monster's label
                //                break;
                //            case MonsterType.Boss:
                //                break;
                //        }
                //    }
                //    else
                //    {
                //        switch (mArchetype.monstertype)
                //        {
                //            case MonsterType.Normal:
                //            case MonsterType.MiniBoss:
                //                HeadLabel.mPlayerLabel.SetMonster(); //hide regular monster's label
                //                break;
                //            case MonsterType.Boss:
                //                break;
                //        }
                //    }
                //    HeadLabel.mPlayerLabel.SetBuffDebuff(this);
                //    break;
                case RealmType.Dungeon:
                //case RealmType.RealmTutorial:
                case RealmType.World:
                    switch (mArchetype.monstertype)
                    {
                        case MonsterType.Normal:
                        default:
                            HeadLabel.mPlayerLabel.SetMonster(); //hide regular monster's label
                            break;
                        case MonsterType.Boss:
                            HeadLabel.mPlayerLabel.SetBoss();
                            HeadLabel.mPlayerLabel.SetBuffDebuff(this);
                            break;
                    }
                    break;
                default:
                    break;
            } // end switch
        }

        public void OnValueChanged(string field, object value, object oldvalue)
        {
            if (field == "TargetPID")
            {
                if (mArchetype.monstertype != MonsterType.Boss)
                {
                    if (value != oldvalue)//mean have new target or back to idle
                    {
                        if ((int)value == 0)//back to idle
                        {
                            if (GameInfo.gLocalPlayer.GetPersistentID() == (int)oldvalue)//only check for localplayer
                            {
                                IsInLocalCombat = false;
                            }
                        }
                        else//new target
                        {
                            if (GameInfo.gLocalPlayer.GetPersistentID() == (int)value)//only check for localplayer
                            {
                                IsInLocalCombat = true;
                            }
                        }
                    }
                }
                //Debug.Log("MG: " + field + " " + value + " " + oldvalue);
            }
            else if (field == "positiveVisualSE" || field == "negativeVisualSE" || field == "VisualEffectTypes" || field == "ElementalVisualSE")
            {
                HandleSideEffectVisuals(field, value);
            }
            else if (field == "DisplayHp")
            {
                PlayerStats.DisplayHp = (float)value; 
                if (HeadLabel != null)
                    HeadLabel.mPlayerLabel.HPf = PlayerStats.DisplayHp;
                //Debug.Log("Monster ID " + GetPersistentID() + " has hp " + value);
                //if (PlayerStats.DisplayHp == 0)
                //{
                //    EnsureDyingAction();
                //}

                if (this == GameInfo.gSelectedEntity)
                    GameInfo.gCombat.UpdateSelectedEntityHealth(PlayerStats.DisplayHp);
            }
            else
                HandleBuffStatus(field, value);
            //Debug.Log("MG: " + field + " " + value + " " + oldvalue);
        }

        private void EnsureDyingAction()
        {
            if (mAction!=null && mAction.GetType() != typeof(NonClientAuthoACDead))
            {
                DeadActionCommand cmd = new DeadActionCommand();
                NonClientAuthoACDead action = new NonClientAuthoACDead(this, cmd);
                PerformAction(action);
            }
        }

        public void Init(int archetypeId, Vector3 pos, Vector3 dir, int health)
		{                    
            Position = pos;
            Forward = dir;
            mArchetype = CombatNPCRepo.GetNPCById(archetypeId);
            //Radius = mArchetype.radius;

            base.Init ();
            OnAnimObjLoaded(AssetManager.LoadSceneNPC(mArchetype.modelprefabpath));

            HeadLabel.mPlayerLabel.Name = mArchetype.localizedname;
            //Transform effectRef = AnimObj.transform.Find("root/effect_buff");
            //float heightOffset = (effectRef == null) ? 3.8f : effectRef.localPosition.y;
            //HeadLabel.SetLabelOffset_WorldSpace();
            //if (GameInfo.mRealmInfo != null)
            //{
            //    bool isEnemy = CombatUtils.IsEnemy(GameInfo.gLocalPlayer, this);

            //    if (isEnemy)
            //    {
            //        switch (mArchetype.monstertype)
            //        {
            //            case MonsterType.Normal:
            //            case MonsterType.Mini:
            //                HeadLabel.mPlayerLabel.SetBattleGroundEnemyMonster();
            //                break;
            //            case MonsterType.Boss:
            //                HeadLabel.mPlayerLabel.SetBattleGroundEnemyBossMonster();
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        switch (mArchetype.monstertype)
            //        {
            //            case MonsterType.Normal:
            //            case MonsterType.Mini:
            //                HeadLabel.mPlayerLabel.SetBattleGroundAllyMonster();
            //                break;
            //            case MonsterType.Boss:
            //                HeadLabel.mPlayerLabel.SetBattleGroundAllyBossMonster();
            //                break;
            //        }
            //    }
            //    HeadLabel.mPlayerLabel.AddBuffDebuffSet(this);
            //}
            //else
            //{
            //    switch (mArchetype.monstertype)
            //    {
            //        case MonsterType.Normal:
            //        default:
            //            HeadLabel.mPlayerLabel.SetMonster();
            //            break;
            //        case MonsterType.Boss:
            //            HeadLabel.mPlayerLabel.SetBoss();
            //            HeadLabel.mPlayerLabel.AddBuffDebuffSet(this);
            //            break;
            //    }
            //}
        }

        public void InitAnimObj()
        {
            if(AnimObj != null)
            {
                GameInfo.gCombat.SetMonsterParent(AnimObj);

                InitEntityComponents();

                //SetShadowRadius(mArchetype.shadowradius);

                AnimObj.tag = "Monster";
                mAnimObj.transform.position = Position;
                mAnimObj.transform.forward = Forward;
                mAnimObj.transform.localScale = new Vector3(mArchetype.modelscalex, 
                                                            mArchetype.modelscaley, 
                                                            mArchetype.modelscalez);
                this.Name = mArchetype.localizedname;

                InitCharacterController();
                Transform effectRef = AnimObj.transform.Find("root/effect_buff");
                float heightOffset = 3.8f;
                if (effectRef != null)
                    heightOffset = effectRef.localPosition.y;
                mHeadLabel.CreateNPCLabel(QuestLabelType.None);
                mHeadLabel.mNpcLabel.InitChatWithMonsterNPC(mArchetype.archetype);
                mHeadLabel.SetLabelOffset_WorldSpace(Vector3.zero, new Vector3(0.0f, heightOffset + mHeadLabel.mNpcLabel.height, 0.0f));

                //HeadLabel.SetActorName (mArchetype.localizedname);            
                //if (mArchetype.monstertype == MonsterType.Destructible)
                //gameobject in Destructible layer will collide those in Entities layer
                //    ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Destructible"));
                if (mArchetype.monstertype == MonsterType.Boss)
                {
                    ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Boss"));
                    //----------------------------------------------------------------------
                    //loading the boss effect. 
                    List<int> skillidList = new List<int>();
                    if (mArchetype.bossai1 != 0)
                    {
                        BossAIJson aijson = CombatNPCRepo.GetBossAIByID(mArchetype.bossai1);
                        skillidList.Add(aijson.skillid);
                        
                    }
                    if (mArchetype.bossai2 != 0)
                    {
                        BossAIJson aijson = CombatNPCRepo.GetBossAIByID(mArchetype.bossai2);
                        skillidList.Add(aijson.skillid);                        
                    }
                    foreach (int skillid in skillidList)
                    {
                        if (skillid <= 0)
                            continue;
                        SkillData skillData = SkillRepo.GetSkill(skillid);
                        if (!string.IsNullOrEmpty(skillData.skillgroupJson.name))
                        {
                            //preload boss skill effect.
                            EfxSystem.Instance.GetEffectByName(skillData.skillgroupJson.name);
                            EfxSystem.Instance.GetEffectByName(skillData.skillgroupJson.name + "_gethit");
                            //EffectController.AddEffect(skillData.skillgroupJson.name, Vector3.zero);
                            //EffectController.AddEffect(skillData.skillgroupJson.name + "_gethit", Vector3.zero);
                        }
                    }
                }
                else
                    //gameobject in Entities layer will not collide each other.
                    ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));

                mRenderer = AnimObj.GetComponentInChildren<SkinnedMeshRenderer>();
                SetModelMaterial();
                InitDyingDuration();
                Show(true);
                ShowEffect(true);
            }
        }

        protected long dyingduration = 2000;
        protected void InitDyingDuration()
        {
            if (AnimObj != null)
            {
                //Animation anim = AnimObj.GetComponent<Animation>();
                Animator anim = AnimObj.GetComponent<Animator>();
                RuntimeAnimatorController ac = anim.runtimeAnimatorController;
                for(int i = 0; i < ac.animationClips.Length; ++i) {
                    if(ac.animationClips[i].name == "dying") {
                        dyingduration = (long)ac.animationClips[i].length * 1000;
                        break;
                    }
                }
            }
        }

        private void SetModelMaterial()
        {
            string matpath = mArchetype.modelmaterialpath;
            if (string.IsNullOrEmpty(matpath))
                return;

            Material mat = AssetManager.LoadAsset<Material>(matpath) as Material;
            if (mat != null)
                mRenderer.material = mat;
        }

        public override void AddLocalObject(LOTYPE objtype, LocalObject obj)
        {
            LocalObject mylocalobj;
            if (objtype == LOTYPE.NPCSynStats)
            {
                this.PlayerStats = new NPCSynStats();
                PlayerStats.OnValueChanged = this.OnValueChanged;
                PlayerStats.OnNewlyAdded = OnNewlyAdded;
                mylocalobj = PlayerStats;
            }           
            else
                return;

            base.AddLocalObject(objtype, mylocalobj);
        }        

        public override int GetDisplayLevel()
        {
            return mArchetype.level;
        }

        public override void OnRemove()
        {
            ((ClientEntitySystem)EntitySystem).RemoveClientEntityByType(this);
            base.OnRemove();
        }

        #region Async Loading
        public override void OnAnimObjLoaded(UnityEngine.Object asset)
        {
            if (asset == null)
                Debug.LogErrorFormat("{0} not found in SceneNPCContainer", mArchetype.modelprefabpath);
            else
                AnimObj = (GameObject) GameObject.Instantiate(asset);
            InitAnimObj();
        }
        #endregion

        public void Flash()
        {
            //if (mFlashEffect == null)
            //{                
            //    mFlashEffect = AnimObj.AddComponent<Flash>();                
            //    mFlashEffect.Init(mRenderer);
            //    mFlashEffect.Restart();
            //}
            //else if (!mFlashEffect.IsFlashing())
            //{
            //    mFlashEffect.Restart();
            //}
            FlashReset();
        }

        public void FlashDone()
        {
            if (mFlashEffect != null)
            {
                //mFlashEffect.StopAllCoroutines
                mFlashEffect.Revert();
                mFlashEffect = null;
            }
        }

        public override string GetStandbyAnimation()
        {
            if (lastKnockupIndex>=0 && lastKnockupIndex <= 5)
            {
                return "";//can not play
            }
            else
            {
                string animname = "standby";
                if (mArchetype.monstertype == MonsterType.Boss)
                {
                    animname = "standby";
                }
                else if (IsInLocalCombat)
                {
                    animname = "standby";
                }
                else
                {
                    return base.GetStandbyAnimation();
                }

                return animname;
               
            }
        }

        public override string GetDyingEffect()
        {
            return mArchetype.archetype + "dying"; 
        }

        public override long GetDyingDuration()
        { 
            return dyingduration;
        }

        public override string GetHitAnimation()
        {
            string hit = "gethit" + GameUtils.RandomInt(1, 2);
            return hit;
        }

        public bool Canbeknockedback()
        {
            if (mArchetype.monstertype == MonsterType.Normal/* && mArchetype.canbeknockback*/)
                return true;
            else
                return false;
        }
        
        private long resetTime = 0;
        private int lastKnockupIndex = -1;

        public override bool MaxEvasionChance
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool MaxCriticalChance
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsInvalidTarget()
        {
            return !IsAlive();
        }

        public override bool IsInSafeZone()
        {
            return false;
        }
         
        public override void Show(bool val)
        {
            base.Show(val);
            ShowEffect(val);             
            HideSkillIndicator();//always hide , for it to hide when cutscene finished ,
            
        }

        public int GetHealthMax()
        {
            return mArchetype.healthmax;
        }

        public override void SetHeadLabel(bool init=false)
        {
            HeadLabel.SetMonsterLabelByRealm(this, init);
        } // end function

        public override ICombatStats CombatStats
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
         
        public override SkillPassiveCombatStats SkillPassiveStats
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override int GetMinDmg()
        {
            throw new NotImplementedException();
        }

        public override int GetAccuracy()
        {
            throw new NotImplementedException();
        }

        public override int GetAttack()
        {
            throw new NotImplementedException();
        }

        public override int GetCritical()
        {
            throw new NotImplementedException();
        }

        public override int GetCriticalDamage()
        {
            throw new NotImplementedException();
        }

        public override int GetArmor()
        {
            throw new NotImplementedException();
        }

        public override int GetEvasion()
        {
            throw new NotImplementedException();
        }

        public override int GetCocritical()
        {
            throw new NotImplementedException();
        }

        public override int GetCocriticalDamage()
        {
            throw new NotImplementedException();
        }

        public override float GetExDamage()
        {
            throw new NotImplementedException();
        }

        #region FLASH

        private float mFlashAmt = 0;
        public static float FlashDuration = 0.15f;
        public static float MaxFlashAmt = 1.0f;
        private bool enabled = false;

        public override void Update(long dt)
        {
            base.Update(dt);

            if (enabled)
            {
                mFlashAmt += Time.deltaTime; //animate within 1 sec (0 to 1sec)        

                float intensity = 0;

                if (mFlashAmt >= FlashDuration) {
                    enabled = false;
                    mFlashAmt = 0;
                    intensity = 0;
                }
                else {
                    intensity = FlashComputeIntensity();
                }
                mRenderer.material.SetFloat("_Flash", intensity);
            }      
        }

        private float FlashComputeIntensity()
        {
            //0 to 1
            //float ratio = Math.Min(mFlashAmt / FlashDuration, 1.0f);

            //if (ratio <= 0.5f)
            //    return ratio * 2 * MaxFlashAmt;
            //else
            //    return (1.0f - ratio) * 2 * MaxFlashAmt;
            float x = mFlashAmt / FlashDuration;

            return (2 * Mathf.Sin(x)) * MaxFlashAmt;
        }

        private void FlashReset()
        {
            enabled = true;
            mFlashAmt = 0;
            //float intensity = FlashComputeIntensity();
            //mRenderer.material.SetFloat("Flash", intensity);
        }

        #endregion
    }
}
