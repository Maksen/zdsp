﻿#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Zealot.Entities;
using System.Text;
using System.Collections.Generic;
using Zealot.Common;

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
        
        private InteractiveTriggerStep stepStats = InteractiveTriggerStep.None;
        private InteractiveUpdater updater = null;

        private void Start()
        {
            if (interactiveType == InteractiveType.Target)
            {
                if(gameObject.GetComponent<Collider>() != null)
                    gameObject.GetComponent<Collider>().enabled = false;
            }
            else
            {
                updater = gameObject.AddComponent<InteractiveUpdater>();
                updater.enabled = false;
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 0, 0.75f);
            Vector3 centerPosition = transform.position + new Vector3(0, 1, 0);
            Gizmos.DrawCube(centerPosition, new Vector3(1, 2, 1));
        }
        
        #region InteracitveTriggerEvent
        private void OnTriggerEnter(Collider other)
        {
            if (stepStats == InteractiveTriggerStep.CannotUse)
                return;

            if (other.CompareTag("LocalPlayer"))
            {
                updater.Init(GameInfo.gLocalPlayer, EntityId, interrupt, this);
                OpenUpdate();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                updater.enabled = false;
                GameInfo.gLocalPlayer.InteractiveController.OnActionLeave();
                RPCFactory.CombatRPC.OnInteractiveUse(EntityId, false);
            }
        }
        #endregion

        #region InteracitveTriggerFunction
        public void Init(bool canUse, bool active, int step)
        {
            gameObject.SetActive(active);
            SetStep((!active || !canUse) ? InteractiveTriggerStep.CannotUse : (InteractiveTriggerStep)step);
        }

        public bool CanUse()
        {
            return stepStats != InteractiveTriggerStep.CannotUse;
        }

        public InteractiveTriggerStep GetStep()
        {
            return stepStats;
        }

        public void SetStep(InteractiveTriggerStep step)
        {
            stepStats = step;
        }

        public void InterruptAction()
        {
            if (interactiveType == InteractiveType.Target)
            {
                SetStep(InteractiveTriggerStep.None);
            }
            else
            {
                SetStep(InteractiveTriggerStep.OnTrigger);
            }
        }

        public void OpenUpdate()
        {
            updater.enabled = true;
            updater.ResetTime();
        }
        #endregion

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
            jsonclass.activeObject = true;
            base.GetJson(jsonclass);
        }

        private string GetPathName()
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