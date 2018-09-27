using Zealot.Common;
using Zealot.Common.Entities;

public class EquipFusionController
{
    public EquipFusionInventoryData EquipFusionInventory;

    public EquipFusionController ()
    {
        EquipFusionInventory = new EquipFusionInventoryData();
        EquipFusionInventory.InitDefault();
    }

    public void InitFromStats (EquipFusionStats equipFusionStats)
    {
        EquipFusionInventory.InitFromStats(equipFusionStats);
    }
}