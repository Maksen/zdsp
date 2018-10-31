using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Entities;

namespace Zealot.Client.Entities
{
    public abstract class BaseClientEntity : Entity
    {
        private bool bShow;
        private bool bShowEfx;

        protected GameObject mAnimObj = null;
        protected GameObject mShadow = null;

        public GameObject AnimObj { get { return mAnimObj; } set { mAnimObj = value; } }

        public bool HasAnimObj { get { return mAnimObj != null; } }

        private bool isShadowVisible = true;

        public virtual string Name { get; set; }

        public override Vector3 Position
        {
            get { return mPos; }
            set
            {
                EntitySystem.UpdateGridId(this, value.x, value.z);
                mPos = value;
                if (mAnimObj != null)
                    mAnimObj.transform.position = value;
            }
        }

        public override Vector3 Forward
        {
            get { return mForward; }
            set
            {
                mForward = value;
                if (mAnimObj)
                    mAnimObj.transform.forward = value;
            }
        }

        public virtual bool CanSelect { get { return true; } }

        public EffectController EffectController { get; set; }

        public virtual void Init()
        {
            InitEntityComponents();
        }

        public void InitEntityComponents()
        {
            if (AnimObj != null)
            {
                mShadow = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_CheapShadow/Cheapshadow.prefab", true);
                mShadow.name = "Shadow";
                mShadow.transform.SetParent(AnimObj.transform, false);
                EffectController effectController = AnimObj.AddComponent<EffectController>();
                effectController.Animator = AnimObj.GetComponent<Animator>();
                EffectController = effectController;
                AnimObj.AddComponent<GameObjectToEntityRef>().mParentEntity = this;
            }
        }

        public void SetAnimSpeed(string animation, float speed)
        {
            if (AnimObj != null)
                EffectController.SetAnimSpeed(animation, speed);
        }

        public virtual void PlayEffect(string animation, string effectName = "", Vector3? effectDir = null, float effectDur = -1, Vector3? targetPos = null, Entity targetEntity = null, bool crossfade = true)
        {
            if (AnimObj != null)
                EffectController.PlayEffect(animation, effectName, effectDir, effectDur, targetPos, targetEntity, crossfade);
        }

        /// <summary>
        /// Play effect with orientation not same as parent.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="effectname"></param>
        /// <param name="duration"></param>
        public virtual void PlaySEEffect(string effectname = "", Vector3? dir = null, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null)
        {
            if (AnimObj != null)
                EffectController.PlaySEEffect(effectname, dir, duration, targetPos, targetEntity);
        }

        public virtual void PlayAnimation(string animation, float fadeLength)
        {
            if (AnimObj != null)
                EffectController.PlayAnimation(animation, fadeLength);
        }

        public virtual bool CanPlayEffect()
        {
            return bShow;
        }

        public bool IsVisible()
        {
            return bShow;
        }

        public void StopEffect(string efx)
        {
            if (AnimObj != null)
                EffectController.StopEffect(efx);
        }

        public override void OnRemove()
        {
            if (this == GameInfo.gSelectedEntity)
                GameInfo.gCombat.OnSelectEntity(null);

            Object.Destroy(mAnimObj);
            Object.Destroy(mShadow);
        }

        public virtual void Show(bool val)
        {
            if (mAnimObj != null)
            {
                bShow = val;
                Transform parent = mAnimObj.transform;
                int count = parent.childCount;
                for (int i = 0; i < count; ++i)
                    parent.GetChild(i).gameObject.SetActive(val);

                if (mShadow != null)
                    mShadow.SetActive(isShadowVisible);
            }
        }

        public void ShowEffect(bool val)
        {
            bShowEfx = val;
            if (AnimObj != null)
                EffectController.ShowEfx(val);
        }

        public void ShowShadow(bool val)
        {
            isShadowVisible = val;
            mShadow.SetActive(isShadowVisible);
        }

        public void SetShadowRadius(float radius)
        {
            float size = radius * 4;
            mShadow.transform.localScale = new Vector3(size, size, 1);
        }

        public virtual int GetDisplayLevel()
        {
            return 1;
        }

        public virtual bool Interact()
        {
            return false;
        }

        public virtual void OnAnimObjLoaded(Object asset)
        {
            if (asset != null)
                AnimObj = (GameObject)Object.Instantiate(asset);
        }
    }

    public abstract class StaticClientNPC : BaseClientEntity, IRelevanceEntity
    {
        public virtual void InitAnimObj()
        {
            InitEntityComponents();
        }

        public override void OnAnimObjLoaded(Object asset)
        {
            if (asset != null)
                AnimObj = (GameObject)Object.Instantiate(asset);
            InitAnimObj();
        }

        abstract public void OnIrrelevant();
        abstract public void OnRelevant();
    }

    public class StaticClientNPCAlwaysShow : BaseClientEntity
    {
        public StaticNPCJson mArchetype;
        public int mArchetypeId;

        protected List<int> mQuestList = new List<int>();
        protected List<int> mAvailableQuest = new List<int>();
        protected Dictionary<int, int> mOngoingQuest = new Dictionary<int, int>();
        protected int mActiveQuest;
        protected bool mActiveStatus;

        public virtual void InitAnimObj()
        {
            InitEntityComponents();
        }

        public override void OnAnimObjLoaded(Object asset)
        {
            base.OnAnimObjLoaded(asset);
            InitAnimObj();
        }

        public override int GetDisplayLevel()
        {
            return 0;
        }

        public override bool CanSelect { get { return mActiveStatus; } }

        public int ActiveQuest { get { return mActiveQuest; } }

        public bool HasQuest { get { return mQuestList.Count > 0; } }

        public bool StartUpDisplay { get { return mArchetype.activeonstartup; } }

        public virtual void UpdateDisplayStatus(bool status)
        {
            mActiveStatus = status;
            Show(true);
        }

        public void ResetDisplayStatus()
        {
            mActiveStatus = mArchetype.activeonstartup;
            Show(true);
        }

        public virtual void UpdateQuestList(List<int> availablelist, Dictionary<int, int> ongoinglist)
        {
            mAvailableQuest = availablelist;
            mOngoingQuest = ongoinglist;
        }
    }
}
