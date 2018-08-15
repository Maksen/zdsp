﻿using System;
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
            if (powerUpSlots.Count <= 0)
            {
                Console.Write("void InitFromInventory(PowerUpStats powerUpStats) error: powerUpSlots empty");
                return;
            }

            // Updating 10 elements
            for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
            {
                powerUpStats.powerUpSlots[i] = powerUpSlots[i];
            }

            powerUpStats.powerUpLvl = powerUpLevels;
        }

        public void InitFromStats(PowerUpStats powerUpStats)
        {
            if (powerUpSlots.Count <= 0)
            {
                Console.Write("void InitFromStats(PowerUpStats powerUpStats) error: powerUpSlots empty");
                return;
            }

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
