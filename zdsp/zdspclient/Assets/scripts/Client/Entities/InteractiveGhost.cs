using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Spawners;
using Zealot.Entities;

namespace Zealot.Client.Entities
{
    public class InteractiveGhost : NetEntityGhost
    {
        public bool isNpcEntity;
        public string npcArchetype;
        public string npcName;

        private GameObject entityObj;
        private ActorNameTagController npcHeadLabel;
        private InteractiveEntities entityScript;
        private InteractiveTrigger interactiveEntity;
        private InteractiveTriggerSynStats mdbInteractiveTriggerSynStats;

        public InteractiveGhost() : base()
        {
            EntityType = EntityType.InteractiveTrigger;
            entityObj = null;
            mdbInteractiveTriggerSynStats = new InteractiveTriggerSynStats();
        }

        public void Init(int pid, string prefab, string parent, bool isNpc, Vector3 pos)
        {
            mdbInteractiveTriggerSynStats.entityId = pid;
            isNpcEntity = isNpc;
            if (isNpc)
            {
                npcArchetype = prefab;
            }

            string path = isNpcEntity ? StaticNPCRepo.GetModelPrefabPathByArchetype(prefab) :
                ScenesModelRepo.GetScenesModelPath(prefab);
            var prefabObj = AssetManager.LoadSceneNPC(path);
            if (prefabObj != null)
            {
                Vector3 scale = ScenesModelRepo.GetScenesModelScale(prefabObj.name);
                SpawnEntity(prefabObj, parent, prefab, pos, scale);
            }
            else
            {
                Debug.LogFormat("Interactive object doesn't found. {0}. {1}.", prefabObj, prefab);
            }
        }

        void SpawnEntity(GameObject npc, string parent, string prefab, Vector3 pos, Vector3 scal)
        {
            AnimObj = UnityEngine.Object.Instantiate(npc);
            AnimObj.transform.position = pos;
            Vector3 lookRota = AnimObj.transform.forward;
            AnimObj.transform.LookAt(lookRota);
            AnimObj.transform.localScale = scal;
            AnimObj.transform.SetParent(GameObject.Find(parent).transform);
            AnimObj.layer = LayerMask.NameToLayer("Entities");
            AnimObj.AddComponent<InteractiveEntities>();
            entityObj = AnimObj;

            entityScript = AnimObj.GetComponent<InteractiveEntities>();
            entityScript.Init();
            interactiveEntity = AnimObj.transform.parent.GetComponent<InteractiveTrigger>();
            interactiveEntity.Init();

            string name = "";
            if (isNpcEntity)
            {
                base.Init();
                name = StaticNPCRepo.GetNPCByArchetype(prefab).localizedname;
            }
            else
            {
                AnimObj.AddComponent<GameObjectToEntityRef>().mParentEntity = this;
                name = ScenesModelRepo.GetScenesModelJson(prefab).name;
            }

            npcName = name;

            npcHeadLabel = AnimObj.AddComponent<ActorNameTagController>();
            npcHeadLabel.CreatePlayerLabel();
            if (npcHeadLabel.mPlayerLabel != null)
            {
                npcHeadLabel.mPlayerLabel.SetNPC();
                npcHeadLabel.mPlayerLabel.Name = name;
                Transform effectRef = AnimObj.transform.Find("root/effect_buff");
                float heightOffset = 3.8f;
                if (effectRef != null)
                    heightOffset = effectRef.localPosition.y;
                npcHeadLabel.SetLabelOffset_WorldSpace();
            }

            OnStepChange(mdbInteractiveTriggerSynStats.step);
        }

        public override void AddLocalObject(LOTYPE objtype, LocalObject obj)
        {
            if (objtype == LOTYPE.InteractiveTriggerSynStats)
            {
                mdbInteractiveTriggerSynStats.OnValueChanged = OnValueChanged;
            }
            else
                return;

            base.AddLocalObject(objtype, mdbInteractiveTriggerSynStats);
        }

        private void OnValueChanged(string field, object value, object oldvalue)
        {
            switch (field)
            {
                case "step":
                    int step = (int)value;
                    mdbInteractiveTriggerSynStats.step = step;
                    OnStepChange(step);
                    break;
                case "playerName":
                    string name = (string)value;
                    mdbInteractiveTriggerSynStats.playerName = name;
                    if(name == GameInfo.gLocalPlayer.Name)
                    {
                        OnTriggeringEnter();
                    }
                    else
                    {
                        OnTriggeringLeave();
                    }
                    break;
                default:
                    break;
            }
        }

        #region Trigger Event
        private void OnStepChange(int step)
        {
            bool canUse = true;
            bool isActive = true;
            switch (step)
            {
                case (int)InteractiveTriggerStep.CannotUse:
                    canUse = false;
                    break;
                case (int)InteractiveTriggerStep.InActive:
                    isActive = false;
                    break;
                default:
                    break;
            }

            entityScript.SetCanUse(canUse);
            interactiveEntity.SetActive(isActive);
            npcHeadLabel.Show(isActive);
        }

        private void OnTriggeringEnter()
        {
            Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
            UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, true);
            InteractiveTrigger mEntity = entityScript.parentTrigger;
            InteractiveTriggerJson json = mEntity.GetJson() as InteractiveTriggerJson;
            progressBar.InitTimeBar(json.interactiveTime, OnTriggeringCompeleted);
        }

        private void OnTriggeringCompeleted()
        {
            GameInfo.gLocalPlayer.InteractiveController.OnActionCompeleted();
            OnTriggeringLeave();
        }

        private void OnTriggeringLeave()
        {
            if (UIManager.IsWidgetActived(HUDWidgetType.ProgressBar))
            {
                Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
                progressBar.ForceEnd();
                UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, false);
            }
        }
        #endregion
    }
}
