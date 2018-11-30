using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Common.Entities;
using System.Linq;

namespace Zealot.Client.Entities
{
    public class StaticGuideGhost : StaticClientNPCAlwaysShow
    {
        public StaticGuideJson mGuideArchetype;
        private float mRadius;
        private bool mTriggered;
        public string ArchetypeName { get; private set; }

        public StaticGuideGhost()
        {
            this.EntityType = EntityType.StaticNPC;
        }

        public void Init(StaticGuideJson npcInfo, Vector3 pos, Vector3 forward, float radius)
        {
            mGuideArchetype = npcInfo;
            ArchetypeName = mGuideArchetype.archetype;
            mArchetypeId = mGuideArchetype.id;
            Name = mGuideArchetype.localizedname;
            mRadius = radius;
            mTriggered = false;

            mActiveQuest = -1;
            mActiveStatus = mGuideArchetype.activeonstartup;

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
            go.AddComponent<NpcPlayerBoxDetect>().Init(this, mRadius);

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

            if (mAvailableQuest.Count > 0)
            {
                mActiveQuest = QuestRepo.GetPriorityQuestId(mAvailableQuest);
            }
            else
            {
                mActiveQuest = -1;
            }

            if (mActiveQuest != -1 && mTriggered)
            {
                questController.OnEnterStaticArea(this);
            }
            questController.UpdateTriggerData(this);
        }

        public void DoInteractAction()
        {
            if (mActiveQuest == -1)
            {
                return;
            }

            if (mTriggered && mActiveQuest != -1)
            {
                RPCFactory.NonCombatRPC.GuidePointReached(mActiveQuest, mArchetypeId);
            }
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

        public void UpdateOngoingQuestList(List<int> ongoinglist)
        {
            mAvailableQuest = ongoinglist;
            UpdateAvtiveQuest();
        }
    }
}
