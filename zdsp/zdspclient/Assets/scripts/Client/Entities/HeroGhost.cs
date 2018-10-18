using Kopio.JsonContracts;
using System;
using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Client.Entities
{
    public class HeroGhost : ActorGhost
    {
        public HeroJson mHeroJson;
        private HeroSynStats HeroSynStats;
        private string modelPath = "";
        private bool isModelLoaded = false;

        public override bool CanSelect { get { return true; } }

        public HeroGhost() : base()
        {
            EntityType = EntityType.HeroGhost;
        }

        public void Init(int heroId, int tier, Vector3 pos, Vector3 dir)
        {
            Position = pos;
            Forward = dir;

            ChangeModel(heroId, tier);
        }

        public void ChangeModel(int heroId, int tier)
        {
            mHeroJson = HeroRepo.GetHeroById(heroId);
            if (mHeroJson != null)
            {
                string prefabPath = GetModelPath(tier);
                if (mAnimObj != null && modelPath != prefabPath)
                {
                    UnityEngine.Object.Destroy(mAnimObj);
                    mAnimObj = null;
                    modelPath = "";
                }

                if (mAnimObj == null)
                {
                    isModelLoaded = false;
                    AssetLoader.Instance.LoadAsync<GameObject>(prefabPath, OnAnimObjLoaded, true);
                    modelPath = prefabPath;
                }
            }
        }

        private string GetModelPath(int tier)
        {
            string path = "";
            switch (tier)
            {
                case 1:
                    path = mHeroJson.t1modelpath;
                    break;
                case 2:
                    path = mHeroJson.t2modelpath;
                    break;
                case 3:
                    path = mHeroJson.t3modelpath;
                    break;
                default:
                    HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
                    if (skinItem != null)
                        path = skinItem.HeroItemJson.heroskinpath;
                    break;
            }
            return path;
        }

        public override void OnAnimObjLoaded(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                base.OnAnimObjLoaded(asset);  // instantiate prefab
                InitAnimObj();
            }
        }

        public void InitAnimObj()
        {
            if (AnimObj != null)
            {
                GameInfo.gCombat.SetPlayerOwnedNPCParent(AnimObj);

                base.Init();

                ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));

                mAnimObj.transform.position = Position;
                mAnimObj.transform.forward = Forward;
                Name = mHeroJson.localizedname;
                SetHeadLabel();

                Show(false);
                Idle();

                isModelLoaded = true;
            }
        }

        public override void AddLocalObject(LOTYPE objtype, LocalObject obj)
        {
            LocalObject mylocalobj;
            if (objtype == LOTYPE.HeroSynStats)
            {
                PlayerStats = new HeroSynStats();
                HeroSynStats = (HeroSynStats)PlayerStats;
                PlayerStats.OnNewlyAdded = OnNewlyAdded;
                PlayerStats.OnValueChanged = OnValueChanged;
                mylocalobj = PlayerStats;
            }
            else
                return;

            base.AddLocalObject(objtype, mylocalobj);
        }

        public void OnNewlyAdded()
        {
        }

        public void OnValueChanged(string field, object value, object oldvalue)
        {
            //Debug.Log(field + " field changed: " + value);
            switch (field)
            {
                case "ModelTier":
                    if (HeroSynStats.IsNewlyAdded)
                        return;
                    ChangeModel(HeroSynStats.HeroId, HeroSynStats.ModelTier);
                    break;
                case "Summoning":
                    GameInfo.gCombat.WaitForHero(() => { return isModelLoaded; }, OnModelReady);
                    break;
            }
        }

        private void Idle()
        {
            IdleActionCommand cmd = new IdleActionCommand();
            PerformAction(new ClientAuthoACIdle(this, cmd));
        }

        private void Summon()
        {
            SummonCommand cmd = new SummonCommand();
            ClientAuthoACSummon action = new ClientAuthoACSummon(this, cmd);
            action.SetCompleteCallback(() => { ShowHeadLabelAndShadow(true); });
            PerformAction(action);
        }

        private bool IsPlayingSummonAnimation()
        {
            if (EffectController.Animator != null)
                return EffectController.Animator.GetCurrentAnimatorStateInfo(0).IsName(mHeroJson.summonaction);
            else if (EffectController.Anim != null)
                return EffectController.Anim.IsPlaying(mHeroJson.summonaction);
            else
                return false;
        }

        private void OnModelReady()
        {
            if (HeroSynStats.Summoning)
            {
                Summon();
                GameInfo.gCombat.WaitForHero(IsPlayingSummonAnimation, () =>
                {
                    Show(true);
                    ShowHeadLabelAndShadow(false);
                });
            }
            else
            {
                if (!IsVisible())
                    Show(true);
            }
        }

        public override void Show(bool val)
        {
            base.Show(val);
            if (HeadLabel != null)
                HeadLabel.Show(val);
        }

        private void ShowHeadLabelAndShadow(bool value)
        {
            if (HeadLabel != null)
                HeadLabel.Show(value);
            ShowShadow(value);
        }

        public override int GetDisplayLevel()
        {
            return HeroSynStats.Level;
        }

        public override void SetHeadLabel(bool init = false)
        {
            HeadLabel.SetHeroLabel(this);
        }

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
            return true;
        }

        public override bool IsInSafeZone()
        {
            return false;
        }

        public override int GetMinDmg()
        {
            throw new System.NotImplementedException();
        }

        public override int GetAccuracy()
        {
            throw new System.NotImplementedException();
        }

        public override int GetAttack()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCritical()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCriticalDamage()
        {
            throw new System.NotImplementedException();
        }

        public override int GetArmor()
        {
            throw new System.NotImplementedException();
        }

        public override int GetEvasion()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCocritical()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCocriticalDamage()
        {
            throw new System.NotImplementedException();
        }

        public override float GetExDamage()
        {
            throw new System.NotImplementedException();
        }
    }
}