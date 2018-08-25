using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Client.Entities
{
    public class StaticNPCGhost : StaticClientNPCAlwaysShow
    {
        private ActorNameTagController mHeadLabel;
        private QuestLabelType mQuestLabelType = QuestLabelType.None;

        public string ArchetypeName { get; private set; }

        public StaticNPCGhost()
        {
            this.EntityType = EntityType.StaticNPC;
        }

        public void Init(StaticNPCJson npcInfo, Vector3 pos, Vector3 forward)
        {
            mArchetype = npcInfo;
            ArchetypeName = mArchetype.archetype;
            mArchetypeId = mArchetype.id;
            Name = mArchetype.localizedname;

            mActiveQuest = -1;
            mActiveStatus = mArchetype.activeonstartup;
            GetQuestList();
            GetOngoingQuest();

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
            mAnimObj.transform.localScale = new Vector3(mArchetype.modelscalex,
                                                        mArchetype.modelscaley,
                                                        mArchetype.modelscalez);
            AnimObj.tag = "NPC";
            AnimObj.name = ArchetypeName;

            mHeadLabel = AnimObj.AddComponent<ActorNameTagController>();
            //float yoffset = 140 * 4 / 3 / GameInfo.gCombat.CameraAspect;
            //mHeadLabel.nameTagOffsetPos = new Vector2(0, yoffset);
            mHeadLabel.CreatePlayerLabel();
            if (mHeadLabel.mPlayerLabel != null)
            {
                mHeadLabel.mPlayerLabel.SetNPC();
                mHeadLabel.mPlayerLabel.Name = Name;
                Transform effectRef = AnimObj.transform.Find("root/effect_buff");
                float heightOffset = 3.8f;
                if (effectRef != null)
                    heightOffset = effectRef.localPosition.y;
                mHeadLabel.CreateNPCLabel(mQuestLabelType);
                mHeadLabel.mNpcLabel.InitChatWithStaticNPC(mArchetype);
                mHeadLabel.SetLabelOffset_WorldSpace(Vector3.zero,new Vector3(0.0f, heightOffset + mHeadLabel.mNpcLabel.height, 0.0f));
            }
           
            base.InitAnimObj();
            //QuestNPC layer set to default so it can detect PlayerGhost.
            ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));//ent no collider with player
            SphereCollider scollider = AnimObj.AddComponent<SphereCollider>();
            scollider.radius = 2f;
            scollider.center = new Vector3(0, 1.0f, 0);

            GameObject go = new GameObject("playerDetecter");
            go.transform.SetParent(AnimObj.transform, false); 
            go.AddComponent<NpcPlayerDetect>().Init(this, 3.0f);

            Show(true);
            ShowEffect(true);
            PreloadEffect();
            string modelPath = mArchetype.modelprefabpath;
            mShadow.SetActive(!string.IsNullOrEmpty(modelPath) && modelPath != "Models/Npcs/Prefabs/npc_null.prefab" && mActiveStatus);
            UpdateQuestMarker();
        }

        protected void PreloadEffect()
        {
            if(mArchetypeId > 0)
            {
                /*if (!string.IsNullOrEmpty(mArchetype.idleeffect))
                    EfxSystem.Instance.GetEffectByName(ArchetypeName + "_idle");
                if (!string.IsNullOrEmpty(mArchetype.standbyeffect))
                    EfxSystem.Instance.GetEffectByName(ArchetypeName + "_standby");*/
            }
            EntitySystem.Timers.SetTimer(1000, (obj) => {
                EffectController.PlayEffect("standby", ArchetypeName + "_standby");
            }, null);
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

            int talkid = -1;
            bool completedall = false;
            bool ongoingquest = false;
            if (mActiveQuest != -1)
            {
                if (mOngoingQuest.Contains(mActiveQuest))
                {
                    talkid = questController.GetTalkId(mActiveQuest, mArchetypeId);
                    ongoingquest = true;
                }
                else if (mAvailableQuest.Count == 1 && mAvailableQuest.Contains(mActiveQuest))
                {
                    talkid = questController.GetTalkId(mActiveQuest, mArchetypeId);
                    ongoingquest = false;
                }
            }
            else
            {
                if (mQuestList.Count > 0 && mAvailableQuest.Count == 0)
                {
                    completedall = questController.CompletedAllQuest(mQuestList);
                }
            }

            UIManager.CloseAllWindows();
            if (talkid != -1)
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, mActiveQuest, ongoingquest));
            }
            else if (talkid == -1 && mAvailableQuest.Count > 0)
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest, mAvailableQuest));
            }
            else if (completedall)
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest, null, completedall));
            }
            else
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest));
            }
        }

        public override int GetDisplayLevel()
        {
            return 1;
        }

        private float _lastNearTime = 0;
        private bool _playerNear = false;
        public void OnPlayerNear()
        {
            _playerNear = true;
            _idleRepeated = false;
            if (Time.realtimeSinceStartup - _lastNearTime > 3.0f)//donot play too often
            {
                //if (EffectController.Anim.GetClip("idle") != null)
                //{
                //    EffectController.StopEffect(mArchetype + "_standby");
                //    EffectController.PlayEffect("idle",  mArchetype + "_idle");
                //    float len = EffectController.Anim.clip.length;
                //    GameInfo.gCombat.mTimers.SetTimer((long)(len * 1000), OnIdleTimeUp, null);
                //    _lastNearTime = Time.realtimeSinceStartup;
                //}
            }
        }

        public void OnPlayerAway()
        {
            _playerNear = false;
        }

        private bool _idleRepeated = false;
        private void OnIdleTimeUp(object arg)
        {
            if (_playerNear && !GameInfo.gLocalPlayer.IsMoving() && !_idleRepeated)
            {
                //play idle again
                _idleRepeated = true;
                if (EffectController.Anim.GetClip("idle") != null)
                {
                    //stop the standby effect. 
                    EffectController.StopEffect(mArchetype + "_standby");
                    EffectController.Anim.Rewind("idle");
                    EffectController.PlayEffect("idle",  mArchetype + "_idle"); 
                    float len = EffectController.Anim.clip.length;
                    GameInfo.gCombat.mTimers.SetTimer((long)(len * 1000), OnIdleTimeUp, null);
                    _lastNearTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                EffectController.PlayEffect("standby",  mArchetype + "_standby");
            }
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

        public void ShowModelOnly(bool val)
        {
            base.Show(val);
        }

        private void UpdateQuestMarker()
        {
            if (GameInfo.gLocalPlayer == null)
                return;

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

            foreach(int questid in mAvailableQuest)
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
            }
            
            mHeadLabel.UpdateQuestLabel(mQuestLabelType);
        }

        private void GetQuestList()
        {
            mQuestList = new List<int>();
            string[] ids = mArchetype.questid.Split(';');
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
                return;

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
            mOngoingQuest = (GameInfo.gLocalPlayer != null) 
                ? GameInfo.gLocalPlayer.QuestController.GetQuestListByNPCId(mArchetypeId) : new List<int>();
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

        private GameObjectToEntityRef mEntityRef = null;
        public override void UpdateDisplayStatus(bool status)
        {
            mActiveStatus = status;
            Show(true);
            if (AnimObj != null)
            {
                mEntityRef = AnimObj.GetComponent<GameObjectToEntityRef>();
                if (mEntityRef != null && !status)
                {
                    GameObject.Destroy(mEntityRef);
                }
                else if (mEntityRef == null && status)
                {
                    mEntityRef = AnimObj.AddComponent<GameObjectToEntityRef>();
                    mEntityRef.mParentEntity = this;
                }
            }
        }
    }
}
