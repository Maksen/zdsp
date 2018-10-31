#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Zealot.Entities;
using Zealot.Client.Entities;
using System.Text;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Common
{
    public enum InteractiveTriggerStep
    {
        None,
        OnTrigger,
        OnProgress
    }
}

namespace Zealot.Spawners
{
    public enum InteractiveType
    {
        Area,
        Target
    }

    [AddComponentMenu("Spawners at Server/InteractiveTrigger")]
    public class InteractiveTrigger : ServerEntityWithEvent
    {
        public bool activeOnStartup = true;
        public string archetype = string.Empty;
        public GameObject sceneModel = null;
        public int interactiveTime = 1;
        public bool interrupt = false;
        public int counter = 1;
        public int keyId = 0;
        public InteractiveType interactiveType = InteractiveType.Area;
        public int min = 1;
        public int max = 1;

        List<PlayerGhost> player = new List<PlayerGhost>();
        int playerCount = 0;
        PlayerGhost progressPlayer = null;
        private InteractiveTriggerStep stepStats = InteractiveTriggerStep.None;

        void Start()
        {
            if (interactiveType == InteractiveType.Target)
            {
                if(gameObject.GetComponent<Collider>() != null)
                    gameObject.GetComponent<Collider>().enabled = false;
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.75f);
            Vector3 centerPosition = transform.position + new Vector3(0, 1, 0);
            Gizmos.DrawCube(centerPosition, new Vector3(1, 2, 1));
        }

        #region InteracitveTriggerEvent
        void OnTriggerEnter(Collider other)
        {
            if (counter == 0)
                return;

            if (other.CompareTag("LocalPlayer"))
            {
                PlayerGhost mPlayer = GameInfo.gLocalPlayer;
                if (!player.Exists(x => x == mPlayer))
                {
                    AddPlayer(mPlayer);
                }

                if (stepStats == InteractiveTriggerStep.None)
                {
                    if (CanProgress(player.Count))
                    {
                        SetStep(InteractiveTriggerStep.OnTrigger);
                    }
                }
                else
                {
                    if (!CanProgress(player.Count))
                    {
                        progressPlayer.InteractiveController.ActionInterupted();
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                PlayerGhost mPlayer = GameInfo.gLocalPlayer;
                if (player.Exists(x => x == mPlayer))
                {
                    player.Remove(mPlayer);
                }

                if (stepStats != InteractiveTriggerStep.None)
                {
                    SetStep(CanProgress(player.Count) ? InteractiveTriggerStep.OnTrigger : InteractiveTriggerStep.None);
                }
            }
        }
        #endregion

        #region InteracitveTriggerFunction
        public bool SetPlayer(PlayerGhost mPlayer)
        {
            if (counter == 0)
            {
                UIManager.SystemMsgManager.ShowSystemMessage("觸發次數已用盡", true);
                return false;
            }
            if (stepStats != InteractiveTriggerStep.OnProgress)
            {
                if (mPlayer.clientItemInvCtrl.itemInvData.HasItem((ushort)keyId))
                {
                    UIManager.SystemMsgManager.ShowSystemMessage("玩家未擁有需求道具", true);
                    return false;
                }
                AddPlayer(mPlayer);
                SetStep(InteractiveTriggerStep.OnTrigger);
                return true;
            }
            else
            {
                UIManager.SystemMsgManager.ShowSystemMessage("有人在使用", true);
                return false;
            }
        }

        void SetStep(InteractiveTriggerStep step)
        {
            stepStats = step;
        }

        public void Init(int count)
        {
            counter = count;
            if (counter > 0 || counter == -1)
                this.enabled = true;
            else
                this.enabled = false;

            InitData();
        }

        void InitData()
        {
            SetStep(InteractiveTriggerStep.None);
            player.Clear();
        }

        public void InterruptAction()
        {
            RPCFactory.CombatRPC.OnInteractiveUse(EntityId, false);
            if (interactiveType == InteractiveType.Target)
            {
                player.Clear();
                SetStep(InteractiveTriggerStep.None);
            }
            else
            {
                SetStep(InteractiveTriggerStep.OnTrigger);
            }
            progressPlayer = null;
        }

        void SetProgressPlayer()
        {
            if(CanProgress(player.Count) && progressPlayer == null)
            {
                progressPlayer = player[player.Count - 1];
            }
        }

        public void TriggerEvent()
        {
            RPCFactory.CombatRPC.OnInteractiveUse(EntityId, true);

            SetProgressPlayer();
            if (interactiveType == InteractiveType.Target)
            {
                progressPlayer.InteractiveController.OnActionEnter(this, interrupt);
                SetStep(InteractiveTriggerStep.OnProgress);
            }
            else
            {
                if (CanProgress(player.Count) && stepStats == InteractiveTriggerStep.OnTrigger)
                {
                    progressPlayer.InteractiveController.OnActionEnter(this, interrupt);
                    SetStep(InteractiveTriggerStep.OnProgress);
                }
            }
        }

        bool CanProgress(int playerC)
        {
            return (playerC >= min && playerC <= max) ? true : false;
        }

        void AddPlayer(PlayerGhost mPlayer)
        {
            if(!player.Exists(x => x == mPlayer))
            {
                player.Add(mPlayer);
            }
        }
        #endregion

        void Update()
        {
            playerCount = player.Count;
            if (stepStats == InteractiveTriggerStep.OnTrigger && CanProgress(playerCount))
            {
                for (int i = 0; i < playerCount; ++i)
                {
                    if (!player[i].IsIdling())
                    {
                        return;
                    }
                }
                TriggerEvent();
            }
        }

        public override string[] Triggers
        {
            get
            {
                return new string[] { "On", "Off", "Reset" };
            }
        }

        public override string[] Events
        {
            get
            {
                return new string[] { "OnInteractive" };
            }
        }
        

        public override ServerEntityJson GetJson()
        {
            InteractiveTriggerJson jsonclass = new InteractiveTriggerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(InteractiveTriggerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.archetype = (archetype == string.Empty || archetype == null) ? sceneModel.name : archetype;
            jsonclass.parentPath = GetPathName();
            jsonclass.forward = transform.forward;
            jsonclass.interactiveTime = interactiveTime;
            jsonclass.interrupt = interrupt;
            jsonclass.counter = (counter == 0) ? 1 : counter;
            if(interactiveType == InteractiveType.Target)
            {
                jsonclass.keyId = keyId;
                jsonclass.isArea = false;
            }
            else
            {
                jsonclass.keyId = 0;
                jsonclass.isArea = true;
            }
            jsonclass.min = min;
            jsonclass.max = max;
            base.GetJson(jsonclass);
        }

        string GetPathName()
        {
            StringBuilder path = new StringBuilder();
            List<string> pathName = new List<string>();
            Transform parentTrans = transform;
            while (parentTrans != null)
            {
                pathName.Add(parentTrans.name);
                parentTrans = parentTrans.parent;
            }
            for (int index = pathName.Count - 1; index >= 0; --index)
            {
                path.Append("/");
                path.Append(pathName[index]);
            }
            path.Remove(0, 1);
            return path.ToString();
        }
    }
}