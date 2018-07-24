using UnityEngine;
using System.Collections;
using Zealot.Entities;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/Counter")]
    public class Counter : ServerEntityWithEvent {
		public bool activeOnStartup;
        public int count;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers { get {
				return new string[] { "TurnOn", "Reset", "Increase", "Decrease" };
			}}
		public override string[] Events { get {
				return new string[]{ "OnCounter" };
			}}

		public override ServerEntityJson GetJson()
		{
            CounterJson jsonclass = new CounterJson();
			GetJson (jsonclass);
			return jsonclass;
		}
		
		public void GetJson(CounterJson jsonclass)
		{
			jsonclass.activeOnStartup = activeOnStartup;
			jsonclass.count = count;
			base.GetJson (jsonclass);
		}
	}
}
