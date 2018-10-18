using UnityEditor;
using UnityEngine;
using Zealot.Entities;
using Zealot.Client.Entities;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Text;
using System.Collections.Generic;

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
        public int interactiveTime = 1;
        public bool interrupt = false;
        public int counter = 1;
        public int keyId = 0;
        public InteractiveType interactiveType;

        PlayerGhost player;
        InteractiveController controller;

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

        void InitData()
        {
            controller.InitProgress();
            player = null;
            controller = null;
        }

        public void InitController(PlayerGhost mPlayer)
        {
            player = mPlayer;
            controller = player.InteractiveController;
            controller.Init((interactiveType == InteractiveType.Area) ? true : false, interrupt, this);
        }
        
        #region ProgressBar
        public void StartProgressBar ()
        {
            if (interactiveTime <= 0)
                WithoutRunProgressBar();
            else
                ProgressBar();
        }

        void WithoutRunProgressBar()
        {
            if (!controller.isCompleted)
                CompletedProgressBar();
            else
                Debug.LogError("Progress order is wrong.");
        }

        void ProgressBar()
        {
            UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, true);
            Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
            progressBar.InitTimeBar(interactiveTime, CompletedProgressBar);
        }

        void CompletedProgressBar()
        {
            CloseProgressBar();
            controller.CompletedProgress();
            RPCFactory.CombatRPC.OnInteractiveTrigger(EntityId, keyId);

            InitData();
        }

        void CloseProgressBar()
        {
            if (UIManager.IsWidgetActived(HUDWidgetType.ProgressBar))
            {
                Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
                progressBar.ForceEnd();
            }
            UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, false);
        }
        #endregion

        #region InteracitveTriggerEvent
        void OnTriggerEnter(Collider other)
        {
            if (counter <= 0 || interactiveType == InteractiveType.Target)
                return;

            if (other.CompareTag("LocalPlayer"))
            {
                if (player == null)
                    InitController(GameInfo.gLocalPlayer);
                else
                    UIManager.SystemMsgManager.ShowSystemMessage("Event is be using！", true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                if (player == GameInfo.gLocalPlayer)
                    InitData();
            }
        }

        public void InterruptActrion()
        {
            CloseProgressBar();
            RPCFactory.CombatRPC.OnInteractiveUse(EntityId, false);
            if (interactiveType == InteractiveType.Target)
                if(controller != null)
                    InitData();
            else
                controller.InitProgress();
        }

        public void OnInteractiveTrigger()
        {
            RPCFactory.CombatRPC.OnInteractiveUse(EntityId, true);
            if (player.InteractiveTriggerStats.waitResponse)
            {
                controller.StartProgress();
                StartProgressBar();
            }
        }

        public PlayerGhost GetPlayer()
        {
            return player;
        }
        #endregion

        void Update()
        {
            if (player != null)
            {
                if(player.IsIdling() && !controller.IsProgressing())
                    OnInteractiveTrigger();
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
            jsonclass.archetype = archetype;
            jsonclass.parentPath = GetPathName();
            jsonclass.forward = transform.forward;
            jsonclass.interactiveTime = interactiveTime;
            jsonclass.interrupt = interrupt;
            jsonclass.counter = counter;
            jsonclass.keyId = keyId;
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