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

        public string ArchetypeName { get; private set; }

        public StaticNPCGhost()
        {
            this.EntityType = EntityType.StaticNPC;
        }

        NPCFunctionType GetNPCFunction(out int param)
        {
            param = 0;
            if (mArchetype!= null && mArchetype.npcfunction != null && mArchetype.npcfunction.Length > 0)
            {
                var args = mArchetype.npcfunction.Split('/');

                if (args.Length >= 2)
                {
                    param = int.Parse(args[1]);

                    return (NPCFunctionType)int.Parse(args[0]);
                }
            }

            return (NPCFunctionType)(-1);
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

        void onDialogueOver()
        {
            int param = 0;
            var func = GetNPCFunction(out param);

            if(func == NPCFunctionType.Shop)
                UIManager.OpenWindow(WindowType.ShopSell, (window) => window.GetComponent<UIShop>().RequestShopInfo(param));

            return;
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
                if (mOngoingQuest.ContainsKey(mActiveQuest))
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
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, mActiveQuest, ongoingquest, null, false, onDialogueOver));
            }
            else if (talkid == -1 && mAvailableQuest.Count > 0)
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest, mAvailableQuest, false, onDialogueOver));
            }
            else if (completedall)
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest, null, completedall, onDialogueOver));
            }
            else
            {
                UIManager.OpenDialog(WindowType.DialogNpcTalk, (window) => window.GetComponent<UI_Dialogue>().Init(this, talkid, -1, ongoingquest, null, false, onDialogueOver));
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
            UpdateQuestMarker();
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
