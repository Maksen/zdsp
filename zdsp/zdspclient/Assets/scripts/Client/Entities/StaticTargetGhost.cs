using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Repository;
using Zealot.Common.Entities;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Client.Actions;
using Zealot.Common.Actions;

namespace Zealot.Client.Entities
{
    public class StaticTargetGhost : StaticClientNPCAlwaysShow
    {
        protected string mModelPath;
        protected string mArchetype;
        public int mArchetypeID;
        private float mRadius;
        private ActorNameTagController mHeadLabel;
        private QuestLabelType mQuestLabelType = QuestLabelType.None;

        public StaticTargetGhost()
        {
            this.EntityType = EntityType.StaticNPC;
        }

        public string Archetype { get { return mArchetype; } }

        public void Init(string archetype, Vector3 pos, Vector3 forward, float radius)
        {
            staticNPCJson = StaticNPCRepo.GetStaticNPCByName(archetype);
            mArchetype = archetype;
            mModelPath = staticNPCJson == null ? "" : staticNPCJson.modelprefabpath;
            this.mArchetypeID = staticNPCJson == null ? 0 : staticNPCJson.id;
            this.Name = staticNPCJson == null ? "" : staticNPCJson.localizedname;
            mRadius = radius;
            mActiveQuest = -1;
            mActiveStatus = staticNPCJson.activeonstartup;
            GetQuestList();
            GetOngoingQuest();

            Position = pos;
            Forward = forward;

            base.Init();
            OnAnimObjLoaded(AssetManager.LoadSceneNPC(mModelPath));
        }

        public override void InitAnimObj()
        {
            GameInfo.gCombat.SetStaticNPCParent(AnimObj);
            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "NPC";
            AnimObj.name = mModelPath;

            mHeadLabel = AnimObj.AddComponent<ActorNameTagController>();
            mHeadLabel.CreatePlayerLabel();
            if (mHeadLabel.mPlayerLabel != null)
            {
                mHeadLabel.mPlayerLabel.SetNPC();
                mHeadLabel.mPlayerLabel.Name = StaticNPCRepo.GetStaticNPCById(mArchetypeID).localizedname;
                Transform effectRef = AnimObj.transform.Find("root/effect_buff");
                float heightOffset = 3.8f;
                if (effectRef != null)
                    heightOffset = effectRef.localPosition.y;
                mHeadLabel.SetLabelOffset_WorldSpace();
                mHeadLabel.CreateNPCLabel(mQuestLabelType);
            }

            base.InitAnimObj();
            ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));
            SphereCollider scollider = AnimObj.AddComponent<SphereCollider>();
            scollider.radius = 2f;
            scollider.center = new Vector3(0, 1.0f, 0);

            GameObject go = new GameObject();
            go.name = "playerDetecter";
            go.transform.SetParent(AnimObj.transform, false);
            NpcPlayerDetect detect = go.AddComponent<NpcPlayerDetect>();
            detect.Init(this, mRadius);
            mShadow.SetActive(false);

            Show(true);
            ShowEffect(true);
            UpdateQuestMarker();
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
            if (mHeadLabel != null)
                mHeadLabel.Show(mActiveStatus);
        }

        public override int GetArchetypeID()
        {
            return mArchetypeID;
        }

        public override bool Interact()
        {
            if (IsVisible())
            {
                PlayerGhost player = GameInfo.gLocalPlayer;
                Vector3 diff = new Vector3(player.Position.x - Position.x, 0, player.Position.z - Position.z);
                if (diff.sqrMagnitude > 3 * 3)
                    player.PathFindToTarget(Position, -1, 2, false, false, DoInteractAction);
                else
                    DoInteractAction();
            }
            return false;
        }

        public void DoInteractAction()
        {
            if (GameInfo.gLocalPlayer == null)
                return;

            PlayerGhost player = GameInfo.gLocalPlayer;
            QuestClientController questController = player.QuestController;
            player.PerformAction(new ClientAuthoACIdle(player, new IdleActionCommand()));

            int interactid = GetInteractId();
            int questid = mActiveQuest;
            if (interactid != -1)
            {
                if (!UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
                {
                    UIManager.SetWidgetActive(HUDWidgetType.QuestAction, true);
                }
                Hud_QuestAction questAction = UIManager.GetWidget(HUDWidgetType.QuestAction).GetComponent<Hud_QuestAction>();
                questAction.Init(interactid, questid);
            }
        }

        private void GetQuestList()
        {
            mQuestList = new List<int>();
            string[] ids = staticNPCJson.questid.Split(';');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    mQuestList.Add(int.Parse(id));
                }
            }
        }

        public override void UpdateAvailableQuestList()
        {
            PlayerGhost player = GameInfo.gLocalPlayer;
            if (player == null)
            {
                return;
            }

            List<int> availablequest = new List<int>();
            foreach (int questid in mQuestList)
            {
                if (player.QuestController.IsQuestAvailable(questid))
                {
                    availablequest.Add(questid);
                }
            }

            if (availablequest != mAvailableQuest)
            {
                mAvailableQuest = availablequest;
                UpdateQuestMarker();
            }
        }

        private void GetOngoingQuest()
        {
            if (GameInfo.gLocalPlayer != null)
            {
                mOngoingQuest = GameInfo.gLocalPlayer.QuestController.GetQuestListByNPCId(mArchetypeID);
            }
            else
            {
                mOngoingQuest = new List<int>();
            }
        }

        public override void UpdateOngoingQuest(List<int> quests)
        {
            mOngoingQuest = quests;
            UpdateQuestMarker();
        }

        public override void RemoveOngoingQuest(int questid)
        {
            if (mOngoingQuest.Contains(questid))
            {
                mOngoingQuest.Remove(questid);
                UpdateQuestMarker();
            }
        }

        private void UpdateQuestMarker()
        {
            if (GameInfo.gLocalPlayer == null)
            {
                return;
            }

            QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
            List<int> questcansubmit = new List<int>();
            List<int> questongoing = new List<int>();
            List<int> questacceptable = new List<int>();
            foreach (int questid in mOngoingQuest)
            {
                if (questController.IsQuestCanSubmit(questid))
                {
                    questcansubmit.Add(questid);
                }
                else
                {
                    questongoing.Add(questid);
                }
            }

            foreach (int questid in mAvailableQuest)
            {
                questacceptable.Add(questid);
            }

            if (questcansubmit.Count > 0)
            {
                mActiveQuest = QuestRepo.GetPriorityQuestId(questcansubmit);
                QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                if (questJson != null && questJson.promptobj)
                {
                    mQuestLabelType = QuestLabelType.Submit;
                }
                else
                {
                    mQuestLabelType = QuestLabelType.None;
                }
            }
            else if (questongoing.Count > 0)
            {
                mActiveQuest = QuestRepo.GetPriorityQuestId(questongoing);
                QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                if (questJson != null && questJson.promptobj)
                {
                    mQuestLabelType = QuestLabelType.Ongoing;
                }
                else
                {
                    mQuestLabelType = QuestLabelType.None;
                }
            }
            else if (questacceptable.Count > 0)
            {
                mActiveQuest = QuestRepo.GetPriorityQuestId(questacceptable);
                QuestType type = QuestRepo.GetQuestTypeByQuestId(mActiveQuest);
                QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                if (questJson != null && questJson.promptaccept)
                {
                    if (type == QuestType.Main)
                    {
                        mQuestLabelType = QuestLabelType.NewMainQuest;
                    }
                    else if (type == QuestType.Destiny)
                    {
                        mQuestLabelType = QuestLabelType.NewAdventureQuest;
                    }
                    else if (type == QuestType.Event)
                    {
                        mQuestLabelType = QuestLabelType.NewEventQuest;
                    }
                    else
                    {
                        mQuestLabelType = QuestLabelType.NewSubQuest;
                    }
                }
                else
                {
                    mQuestLabelType = QuestLabelType.None;
                }
            }
            else
            {
                mActiveQuest = -1;
                mQuestLabelType = QuestLabelType.None;
                OnPlayerAway();
            }
            mHeadLabel.UpdateQuestLabel(mQuestLabelType);
        }

        public int GetInteractId()
        {
            if (mActiveQuest != -1 && GameInfo.gLocalPlayer != null)
            {
                if (mOngoingQuest.Contains(mActiveQuest))
                {
                    return GameInfo.gLocalPlayer.QuestController.GetInteractiveId(mActiveQuest, mArchetypeID);
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

        public void OnPlayerAway()
        {
            if (UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
            {
                UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
            }
        }
    }
}
