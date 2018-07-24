using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [Serializable]
    public class EntityLink
    {
        public string mEvent;
        public GameObject mReceiver;
        public string mTrigger;
    }

    [ExecuteInEditMode]
	public class ServerEntity : MonoBehaviour
    {
		public virtual string[] Triggers { get {
			return new string[]{};
		}}

        [HideInInspector]
        [SerializeField]
        private int entityId;
        public virtual int EntityId
        {
            get
            {
                return entityId;
            }

            set
            {
                entityId = value;
            }
        }

        public bool ShowInMap = true;

        public virtual ServerEntityJson GetJson ()
		{
			ServerEntityJson jsonclass = new ServerEntityJson ();
			GetJson (jsonclass);
			return jsonclass;
		}

		public void GetJson(ServerEntityJson jsonclass)
		{
			jsonclass.position = transform.position;
			jsonclass.ObjectID = gameObject.GetInstanceID ();
            jsonclass.ShowInMap = ShowInMap;
        }
	}

	public class ServerEntityWithEvent : ServerEntity
    {
		public List<EntityLink> EntityLinks; 
		public virtual string[] Events { get {
			return new string[]{};
		}}

		public override ServerEntityJson GetJson ()
		{
			ServerEntityWithEventJson jsonclass = new ServerEntityWithEventJson ();
			GetJson (jsonclass);
			return jsonclass;
		}

		public void GetJson(ServerEntityWithEventJson jsonclass)
		{
			Dictionary<string, List<EntityLinkServer>> EntityLinks_Server = new Dictionary<string, List<EntityLinkServer>> ();
            if (EntityLinks != null)
            {
                foreach (EntityLink link in EntityLinks)
                {
                    EntityLinkServer link_server = new EntityLinkServer(link.mReceiver.GetInstanceID(), link.mTrigger);
                    if (!EntityLinks_Server.ContainsKey(link.mEvent))
                        EntityLinks_Server.Add(link.mEvent, new List<EntityLinkServer>());
                    EntityLinks_Server[link.mEvent].Add(link_server);
                }
            }
			jsonclass.EntityLinks_Server = EntityLinks_Server;
			base.GetJson (jsonclass);
		}
	}
}
