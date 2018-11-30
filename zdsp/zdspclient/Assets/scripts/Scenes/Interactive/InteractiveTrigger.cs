#if UNITY_EDITOR
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
        public bool npcActiveOnStartup = true;
        public string npcArchetype = string.Empty;
        public string scenesModelArchetype = string.Empty;
        public int interactiveTime = 1;
        public bool interrupt = false;
        public int counter = 1;
        public int keyId = 0;
        public InteractiveType interactiveType = InteractiveType.Area;
        public int min = 1;
        public int max = 1;

        private bool isUsing = false;
        private bool canUse = true;
        private InteractiveUpdater updater = null;
        private Collider mCollider;

        public void Init()
        {
            if (interactiveType == InteractiveType.Target)
            {
                if (gameObject.GetComponent<Collider>() != null)
                {
                    gameObject.GetComponent<Collider>().enabled = false;
                }
            }
            else
            {
                if (gameObject.GetComponent<InteractiveUpdater>() == null)
                {
                    updater = gameObject.AddComponent<InteractiveUpdater>();
                }
                else
                {
                    updater = gameObject.GetComponent<InteractiveUpdater>();
                }

                updater.enabled = false;
            }

            mCollider = gameObject.GetComponent<Collider>();
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
            if (!canUse)
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
        public bool GetUsing()
        {
            return isUsing;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetUsing(bool mIsUsing)
        {
            isUsing = mIsUsing;
        }

        public void SetCanUse(bool mCanUse)
        {
            canUse = mCanUse;
            if(mCollider != null)
            {
                mCollider.enabled = mCanUse;
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
                return new string[] { "TurnOn", "TurnOff", "Reset" };
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
            jsonclass.npcActiveOnStartup = npcActiveOnStartup;
            if (string.IsNullOrEmpty(npcArchetype))
            {
                jsonclass.npcArchetype = scenesModelArchetype;
                jsonclass.isArchetypeNpc = false;
            }
            else
            {
                jsonclass.npcArchetype = npcArchetype;
                jsonclass.isArchetypeNpc = true;
            }
            jsonclass.parentPath = GetPathName();
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