using Kopio.JsonContracts;
using UnityEngine;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Client.Actions;
using System.Linq;

namespace Zealot.Client.Entities
{
    public class StaticTargetGhost : StaticClientNPCAlwaysShow
    {
        private float mRadius;
        private ActorNameTagController mHeadLabel;
        private QuestLabelType mQuestLabelType = QuestLabelType.None;

        public string ArchetypeName { get; private set; }

        public StaticTargetGhost()
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

            mActiveQuest = -1;
            mActiveStatus = mArchetype.activeonstartup;

            Position = pos;
            Forward = forward;

            base.Init();
            OnAnimObjLoaded(AssetManager.LoadSceneNPC(mArchetype.modelprefabpath));
        }

        public override void InitAnimObj()
        {
            GameInfo.gCombat.SetStaticNPCParent(AnimObj);
            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "NPC";
            AnimObj.name = ArchetypeName;

            mHeadLabel = AnimObj.AddComponent<ActorNameTagController>();
            mHeadLabel.CreatePlayerLabel();
            if (mHeadLabel.mPlayerLabel != null)
            {
                mHeadLabel.mPlayerLabel.SetNPC();
                mHeadLabel.mPlayerLabel.Name = Name;
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

            GameObject go = new GameObject("playerDetecter");
            go.transform.SetParent(AnimObj.transform, false);
            go.AddComponent<NpcPlayerDetect>().Init(this, mRadius);

            Show(true);
            ShowEffect(true);
            mShadow.SetActive(false);
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
                return true;
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

        private void UpdateQuestMarker()
        {
            if (GameInfo.gLocalPlayer == null)
                return;

            QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
            Dictionary<int, int> questcansubmit = new Dictionary<int, int>();
            Dictionary<int, int> questongoing = new Dictionary<int, int>();
            List<int> questacceptable = new List<int>();
            foreach (KeyValuePair<int, int> entry in mOngoingQuest)
            {
                if (questController.IsQuestCanSubmitByObjective(entry.Key, entry.Value))
                {
                    questcansubmit.Add(entry.Key, entry.Value);
                }
                else
                {
                    questongoing.Add(entry.Key, entry.Value);
                }
            }

            foreach (int questid in mAvailableQuest)
            {
                questacceptable.Add(questid);
            }

            if (questcansubmit.Count > 0)
            {
                List<int> questlist = questcansubmit.Keys.ToList();
                mActiveQuest = QuestRepo.GetPriorityQuestId(questlist);
                QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                if (questJson != null && questJson.promptobj)
                {
                    if (questJson.type == QuestType.Main)
                    {
                        mQuestLabelType = QuestLabelType.SubmitMainQuest;
                    }
                    else if (questJson.type == QuestType.Destiny)
                    {
                        mQuestLabelType = QuestLabelType.SubmitAdventureQuest;
                    }
                    else if (questJson.type == QuestType.Event)
                    {
                        mQuestLabelType = QuestLabelType.SubmitEventQuest;
                    }
                    else
                    {
                        mQuestLabelType = QuestLabelType.SubmitSubQuest;
                    }
                }
                else
                {
                    mQuestLabelType = QuestLabelType.None;
                }
            }
            else if (questongoing.Count > 0)
            {
                List<int> questlist = questongoing.Keys.ToList();
                mActiveQuest = QuestRepo.GetPriorityQuestId(questlist);
                QuestJson questJson = QuestRepo.GetQuestByID(mActiveQuest);
                if (questJson != null && questJson.promptobj)
                {
                    if (questJson.type == QuestType.Main)
                    {
                        mQuestLabelType = QuestLabelType.OngoingMainQuest;
                    }
                    else if (questJson.type == QuestType.Destiny)
                    {
                        mQuestLabelType = QuestLabelType.OngoingAdventureQuest;
                    }
                    else if (questJson.type == QuestType.Event)
                    {
                        mQuestLabelType = QuestLabelType.OngoingEventQuest;
                    }
                    else
                    {
                        mQuestLabelType = QuestLabelType.OngoingSubQuest;
                    }
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

        public void OnPlayerAway()
        {
            if (UIManager.IsWidgetActived(HUDWidgetType.QuestAction))
                UIManager.SetWidgetActive(HUDWidgetType.QuestAction, false);
        }

        public override void UpdateQuestList(List<int> availablelist, Dictionary<int, int> ongoinglist)
        {
            base.UpdateQuestList(availablelist, ongoinglist);
            UpdateQuestMarker();
        }
    }
}
