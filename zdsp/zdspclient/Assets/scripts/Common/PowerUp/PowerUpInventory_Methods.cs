using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    public partial class PowerUpInventoryData
    {
        public void InitFromInventory(PowerUpStats powerUpStats)
        {
            // Updating 10 elements
            for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
            {
                powerUpStats.powerUpSlots[i] = powerUpSlots[i];
            }

            powerUpStats.powerUpLvl = powerUpLevels;
        }

        public void InitFromStats(PowerUpStats powerUpStats)
        {
            // Updating 10 elements
            for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
            {
                powerUpSlots[i] = (int)powerUpStats.powerUpSlots[i];
            }

            powerUpLevels = powerUpStats.powerUpLvl;
        }

        public void UpdateInventory(PowerUpStats powerUpStats)
        {
            InitFromStats(powerUpStats);
        }

        public void SaveToInventory(PowerUpStats powerUpStats)
        {
            InitFromStats(powerUpStats);
        }
    }
}
