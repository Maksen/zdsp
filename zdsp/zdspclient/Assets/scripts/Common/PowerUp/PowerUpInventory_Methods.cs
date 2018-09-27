using System;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class PowerUpInventoryData
    {
        public void InitFromPowerUpInventory(PowerUpStats powerUpStats)
        {
            if (powerUpSlots.Count <= 0)
            {
                Console.Write("void InitFromPowerUpInventory(PowerUpStats powerUpStats) error: powerUpSlots empty");
                return;
            }

            // Updating 10 elements
            for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
            {
                powerUpStats.powerUpSlots[i] = powerUpSlots[i];
            }

            powerUpStats.powerUpLvl = powerUpLevels;
        }

        public void InitFromMeridianInventory(MeridianStats meridianStats)
        {
            if (meridianLevelSlots.Count <= 0)
            {
                Console.Write("void InitFromMeridianInventory(Meridian meridian) error: meridianLevelSlots empty");
                return;
            }
            
            for (int i = 0; i < MAX_MERIDIANLEVELSLOTS; ++i)
            {
                meridianStats.meridianLevelSlots[i] = meridianLevelSlots[i];
                meridianStats.meridianExpSlots[i] = meridianExpSlots[i];
            }
        }

        public void InitFromPowerUpStats(PowerUpStats powerUpStats)
        {
            if (powerUpSlots.Count <= 0)
            {
                Console.Write("void InitFromPowerUpStats(PowerUpStats powerUpStats) error: powerUpSlots empty");
                return;
            }

            // Updating 10 elements
            for (int i = 0; i < MAX_POWERUPSLOTS; ++i)
            {
                powerUpSlots[i] = (int)powerUpStats.powerUpSlots[i];
            }

            powerUpLevels = powerUpStats.powerUpLvl;
        }

        public void InitFromMeridianStats(MeridianStats meridianStats)
        {
            if (meridianLevelSlots.Count <= 0)
            {
                Console.Write("void InitFromMeridianStats(Meridian meridian) error: meridianLevelSlots empty");
                return;
            }
            
            for (int i = 0; i < MAX_MERIDIANLEVELSLOTS; ++i)
            {
                meridianLevelSlots[i] = (int)meridianStats.meridianLevelSlots[i];
                meridianExpSlots[i] = (int)meridianStats.meridianExpSlots[i];
            }
        }

        public void SaveToPowerUpInventoryData(PowerUpStats powerUpStats)
        {
            InitFromPowerUpStats(powerUpStats);
        }

        public void SaveToMeridianInventoryData(MeridianStats meridianStats)
        {
            InitFromMeridianStats(meridianStats);
        }
    }
}
