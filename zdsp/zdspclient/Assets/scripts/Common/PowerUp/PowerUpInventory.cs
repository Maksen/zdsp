using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Entities;

namespace Zealot.Common
{
    public partial class PowerUpInventoryData
    {
        #region serializable properties
        // Method #1
        [JsonProperty(PropertyName = "powerUpLevels")]
        public string powerUpLevels;

        // Method #2
        [JsonProperty(PropertyName = "powerUpSlots")]
        public List<int> powerUpSlots = new List<int>();
        #endregion

        public const int MAX_POWERUPSLOTS = 10;

        public PowerUpInventoryData()
        {
            /*if (powerUpSlots.Count == 0)
            {
                for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
                {
                    powerUpSlots.Add(0);
                }
            }*/
        }

        public void InitDefault()
        {
            if(powerUpSlots.Count == 0)
            {
                for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
                {
                    powerUpSlots.Add(0);
                }
            }
        }
    }
}
