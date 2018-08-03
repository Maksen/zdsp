using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/RandomTrigger")]
    public class RandomTrigger : ServerEntityWithEvent
    {
        public bool activeOnStartup = true;
        public bool loop = false;
        public byte size = 10;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers
        {
            get
            {
                return new string[] { "On", "Off", "Random", "Reset" };
            }
        }

        public override string[] Events
        {
            get
            {
                string[] events = new string[size];
                for (int index = 0; index < size; index++)
                    events[index] = "Event" + (index + 1);
                return events;
            }
        }

        public override ServerEntityJson GetJson()
        {
            RandomTriggerJson jsonclass = new RandomTriggerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(RandomTriggerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.loop = loop;
            jsonclass.size = size;
            base.GetJson(jsonclass);
        }
    }
}
