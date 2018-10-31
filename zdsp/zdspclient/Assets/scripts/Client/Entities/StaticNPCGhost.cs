using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<int, int> mFunction;
        private List<int> mLockedList = new List<int>();

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

            Position = pos;
            Forward = forward;

            GetFunctionList();
            GetQuestList();

            base.Init();
            OnAnimObjLoaded(AssetManager.LoadSceneNPC(mArchetype.modelprefabpath));
        }

        private void GetFunctionList()
        {
            mFunction = new Dictionary<int, int>();
            if (mArchetype != null && !GameUtils.IsEmptyString(mArchetype.npcfunction))
            {
                string[] functions = mArchetype.npcfunction.Split(';');
                foreach(string data in functions)
                {
                    string[] values = mArchetype.npcfunction.Split('/');
                    if (values.Length >= 2)
                    {
                        int type = -1;
                        int param = -1;
                        int.TryParse(values[0], out type);
                        int.TryParse(values[1], out param);

                        if (type != -1 && param != -1)
                        {
                            if (mFunction.ContainsKey(type))
                            {
                                mFunction[type] = param;
                            }
                            else
                            {
                                mFunction.Add(type, param);
                            }
                        }
                    }
                }
            }
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
            string modelPath = mArchetype.modelprefabpath;
            ShowShadow(!string.IsNullOrEmpty(modelPath) && modelPath != "Models/Npcs/Prefabs/npc_null.prefab" && mActiveStatus);
            PreloadEffect();
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

            if (mActiveQuest != -1 && mOngoingQuest.ContainsKey(mActiveQuest))
            {
                questController.AddNewDialogue(this, questController.GetTalkId(mActiveQuest, mArchetypeId), mActiveQuest, true);
            }
            else if (mAvailableQuest.Count > 0 && mFunction.Count > 0)
            {
                questController.AddNewDialogue(this, mAvailableQuest, mFunction, mLockedList);
            }
            else if (mActiveQuest != -1 && mAvailableQuest.Count == 1 && mAvailableQuest.Contains(mActiveQuest))
            {
                questController.AddNewDialogue(this, questController.GetTalkId(mActiveQuest, mArchetypeId), mActiveQuest, false);
            }
            else if (mActiveQuest == -1 && mAvailableQuest.Count > 0)
            {
                questController.AddNewDialogue(this, mAvailableQuest, mLockedList);
            }
            else if (mFunction.Count > 0)
            {
                questController.AddNewDialogue(this, mFunction);
            }
            else if (mQuestList.Count > 0 && mAvailableQuest.Count == 0 && questController.CompletedAllQuest(mQuestList))
            {
                questController.AddNewDialogue(this, true);
            }
            else if (GameUtils.IsEmptyString(mArchetype.talktext))
            {
                questController.AddNewDialogue(this, false);
            }
            
            RPCFactory.CombatRPC.AchievementNPCInteract(mArchetypeId);
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
                //if (EffectController.Anim.GetClip("idle") != null)
                //{
                //    //stop the standby effect. 
                //    EffectController.StopEffect(mArchetype + "_standby");
                //    EffectController.Anim.Rewind("idle");
                //    EffectController.PlayEffect("idle",  mArchetype + "_idle"); 
                //    float len = EffectController.Anim.clip.length;
                //    GameInfo.gCombat.mTimers.SetTimer((long)(len * 1000), OnIdleTimeUp, null);
                //    _lastNearTime = Time.realtimeSinceStartup;
                //}
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
            Dictionary<int, int> questcansubmit = new Dictionary<int, int>();
            Dictionary<int, int> questongoing = new Dictionary<int, int>();
            List<int> questacceptable = new List<int>();
            foreach(KeyValuePair<int, int> entry in mOngoingQuest)
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

            foreach(int questid in mAvailableQuest)
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
            }
            
            mHeadLabel.UpdateQuestLabel(mQuestLabelType);
        }

        public override void UpdateQuestList(List<int> availablelist, Dictionary<int, int> ongoinglist)
        {
            base.UpdateQuestList(availablelist, ongoinglist);
            mLockedList = new List<int>();
            if (GameInfo.gLocalPlayer.QuestController != null)
            {
                foreach (int questid in mQuestList)
                {
                    if (!availablelist.Contains(questid) && GameInfo.gLocalPlayer.QuestController.IsQuestShowable(questid))
                    {
                        mLockedList.Add(questid);
                    }
                }
            }
            UpdateQuestMarker();
        }

        public override void UpdateDisplayStatus(bool status)
        {
            mActiveStatus = status;
            Show(true);
            if (AnimObj != null)
            {
                GameObjectToEntityRef entityRef = AnimObj.GetComponent<GameObjectToEntityRef>();
                if (entityRef != null && !status)
                {
                    Object.Destroy(entityRef);
                }
                else if (entityRef == null && status)
                {
                    entityRef = AnimObj.AddComponent<GameObjectToEntityRef>();
                    entityRef.mParentEntity = this;
                }
            }
        }
    }
}
