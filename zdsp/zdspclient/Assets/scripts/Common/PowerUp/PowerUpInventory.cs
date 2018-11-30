using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zealot.Common
{
    public partial class PowerUpInventoryData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "powerUpSlots")]
        public List<int> powerUpSlots = new List<int>();

        [JsonProperty(PropertyName = "meridianLevelSlots")]
        public List<int> meridianLevelSlots = new List<int>();

        [JsonProperty(PropertyName = "meridianExpSlots")]
        public List<int> meridianExpSlots = new List<int>();
        #endregion

        public const int MAX_POWERUPSLOTS = 10;
        public const int MAX_MERIDIANLEVELSLOTS = 8;
        public const int EXP_GIVE = 100;

        public PowerUpInventoryData()
        {
        }

        public void InitDefault()
        {
            if(powerUpSlots.Count == 0)
            {
                for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
                    powerUpSlots.Add(1);
            }
            if(meridianLevelSlots.Count == 0)
            {
                for (int i = 0; i < MAX_MERIDIANLEVELSLOTS; ++i)
                    meridianLevelSlots.Add(0);
            }
            if(meridianExpSlots.Count == 0)
            {
                for (int i = 0; i < MAX_MERIDIANLEVELSLOTS; ++i)
                    meridianExpSlots.Add(-1);
            }
        }
    }
}
