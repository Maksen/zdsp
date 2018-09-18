using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;

public class EquipFushionController
{
    public EquipFushionInventoryData EquipFushionInventory;

    public EquipFushionController ()
    {
        EquipFushionInventory = new EquipFushionInventoryData();
        EquipFushionInventory.InitDefault();
    }

    public void InitFromStats (EquipFushionStats equipFushionStats)
    {
        EquipFushionInventory.InitFromStats(equipFushionStats);
    }
}