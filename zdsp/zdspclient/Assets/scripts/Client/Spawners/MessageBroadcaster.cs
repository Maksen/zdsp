using UnityEngine;
using System.Collections;
using Zealot.Entities;

namespace Zealot.Spawners
{
    public enum MessageBroadcasterType : byte
    {
        World,
        Room,
        Personal,
    }

    [AddComponentMenu("Spawners at Server/MessageBroadcaster")]
    public class MessageBroadcaster : ServerEntity {
        public MessageBroadcasterType messageType;
        public bool emergency;

        [Tooltip("Max 5 messages")]
        public string[] messages;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers { get {
				return new string[] { "SendMessage1", "SendMessage2", "SendMessage3", "SendMessage4", "SendMessage5" };
			}}

		public override ServerEntityJson GetJson()
		{
            MessageBroadcasterJson jsonclass = new MessageBroadcasterJson();
			GetJson (jsonclass);
			return jsonclass;
		}
		
		public void GetJson(MessageBroadcasterJson jsonclass)
		{
            jsonclass.type = (byte)messageType;
            jsonclass.emergency = emergency;
            jsonclass.messages = messages;
			base.GetJson (jsonclass);
		}
	}
}
