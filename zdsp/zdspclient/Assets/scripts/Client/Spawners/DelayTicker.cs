using UnityEngine;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/DelayTicker")]
    public class DelayTicker : ServerEntityWithEvent
    {
        public bool activeOnStartup;
        public long delay;
        public long ticker;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers
        {
            get { return new string[] { "TurnOn", "TurnOff" }; }
        }

        public override string[] Events
        {
            get { return new string[] { "OnDelay", "OnTicker" }; }
        }

        public override ServerEntityJson GetJson()
        {
            DelayTickerJson jsonclass = new DelayTickerJson ();
            GetJson (jsonclass);
            return jsonclass;
        }
        
        public void GetJson(DelayTickerJson jsonclass)
        {
            jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.delay = delay;
            jsonclass.ticker = ticker;
            base.GetJson (jsonclass);
        }
    }
}
