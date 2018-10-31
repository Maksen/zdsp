using UnityEngine;
using System.Collections;
using System;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/PortalExit")]
    public class PortalExit : ServerEntity {
        [Tooltip("Name must be unique")]
        public string myName = "";

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override ServerEntityJson GetJson()
        {
            PortalExitJson jsonclass = new PortalExitJson ();
            GetJson (jsonclass);
            return jsonclass;
        }
        
        public void GetJson(PortalExitJson jsonclass)
        {
            jsonclass.myName = myName;
            jsonclass.forward = transform.forward;
            jsonclass.forward.y = 0;
            base.GetJson (jsonclass);
        }
    }
}