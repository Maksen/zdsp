using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zealot.Common;
using Zealot.Common.Entities;

public class PowerUpController
{
    public PowerUpInventoryData PowerUpInventory;

    public PowerUpController()
    {
        PowerUpInventory = new PowerUpInventoryData();
        PowerUpInventory.InitDefault();
    }

    public void InitFromStats(PowerUpStats powerupStats)
    {
        PowerUpInventory.InitFromStats(powerupStats);
    }

    public void viewpeer ()
    {
    }

}
