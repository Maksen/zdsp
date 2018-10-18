using Newtonsoft.Json;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class InteractiveTriggerInventoryData
    {
        [JsonProperty(PropertyName = "canTrigger")]
        public bool canTrigger;

        [JsonProperty(PropertyName = "waitResponse")]
        public bool waitResponse;

        public InteractiveTriggerInventoryData()
        {
            canTrigger = false;
            waitResponse = false;
        }

        public void InitDefault()
        {
            canTrigger = false;
            waitResponse = false;
        }

        public void InitFromInventory(InteractiveTriggerStats interactiveTriggerStats)
        {
            interactiveTriggerStats.canTrigger = canTrigger;
            interactiveTriggerStats.waitResponse = waitResponse;
        }

        public void InitFromStats(InteractiveTriggerStats interactiveTriggerStats)
        {
            canTrigger = interactiveTriggerStats.canTrigger;
            waitResponse = interactiveTriggerStats.waitResponse;
        }

        public void SaveToInventory(InteractiveTriggerStats interactiveTriggerStats)
        {
            InitFromStats(interactiveTriggerStats);
        }
    }
}
