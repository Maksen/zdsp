using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Common.Entities;
using System.Linq;

namespace Zealot.Client.Entities
{
    public class StaticAreaGhost : StaticClientNPCAlwaysShow
    {
        private float mRadius;
        private bool mTriggered;

        public string ArchetypeName { get; private set; }

        public StaticAreaGhost()
        {
            this.EntityType = EntityType.StaticNPC;
        }

        public void Init(StaticNPCJson npcInfo, Vector3 pos, Vector3 forward, float radius)
        {
            mArchetype = npcInfo;
            ArchetypeName = mArchetype.archetype;
            mArchetypeId = mArchetype.id;
            Name = mArchetype.localizedname;
            mRadius = radius;
            mTriggered = false;

            mActiveQuest = -1;
            mActiveStatus = mArchetype.activeonstartup;

            Position = pos;
            Forward = forward;

            base.Init();
            GameObject gameObject = new GameObject();
            OnAnimObjLoaded(gameObject);
            GameObject.Destroy(gameObject);
        }

        public override void InitAnimObj()
        {
            GameInfo.gCombat.SetStaticNPCParent(AnimObj);
            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "NPC";
            AnimObj.name = ArchetypeName;

            base.InitAnimObj();

            GameObject go = new GameObject("playerDetecter");
            go.transform.SetParent(AnimObj.transform, false);
            go.AddComponent<NpcPlayerDetect>().Init(this, mRadius);
            
            Show(true);
            ShowEffect(false);
            ShowShadow(false);
        }

        public override bool Interact()
        {
            return false;
        }

        private void UpdateAvtiveQuest()
        {
            if (GameInfo.gLocalPlayer == null)
                return;

            QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
            
            if (mOngoingQuest.Count > 0)
            {
                List<int> questlist = mOngoingQuest.Keys.ToList();
                mActiveQuest = QuestRepo.GetPriorityQuestId(questlist);
            }
            else if (mAvailableQuest.Count > 0)
            {
                mActiveQuest = QuestRepo.GetPriorityQuestId(mAvailableQuest);
            }
            else
            {
                mActiveQuest = -1;
                UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
            }

            if (mActiveQuest != -1 && mTriggered)
            {
                questController.OnEnterStaticArea(this);
            }
            questController.UpdateTriggerData(this);
        }

        public int GetInteractId()
        {
            if (mActiveQuest != -1 && GameInfo.gLocalPlayer != null)
            {
                if (mOngoingQuest.ContainsKey(mActiveQuest))
                {
                    return GameInfo.gLocalPlayer.QuestController.GetInteractiveId(mActiveQuest, mArchetypeId);
                }
                else if (mAvailableQuest.Contains(mActiveQuest))
                {
                    QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                    if (questJson != null && questJson.triggertype == QuestTriggerType.Interact)
                    {
                        return questJson.triggercaller;
                    }
                }
            }
            return -1;
        }

        public int GetActivedQuestId()
        {
            return mActiveQuest;
        }

        public override int GetDisplayLevel()
        {
            return 1;
        }

        public void Despawn()
        {
            EntitySystem.RemoveEntityByID(ID);
        }

        public override void Show(bool val)
        {
            base.Show(mActiveStatus);
        }

        public void OnPlayerNear()
        {
            mTriggered = true;
            if (GameInfo.gLocalPlayer != null && mActiveQuest != -1)
                GameInfo.gLocalPlayer.QuestController.OnEnterStaticArea(this);
        }

        public void OnPlayerAway()
        {
            mTriggered = false;
            if (GameInfo.gLocalPlayer != null)
                GameInfo.gLocalPlayer.QuestController.OnExitStaticArea(this);
        }

        public override void UpdateQuestList(List<int> availablelist, Dictionary<int, int> ongoinglist)
        {
            base.UpdateQuestList(availablelist, ongoinglist);
            UpdateAvtiveQuest();
        }
    }
}
