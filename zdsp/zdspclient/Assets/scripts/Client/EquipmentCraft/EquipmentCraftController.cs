using Zealot.Common;
using Zealot.Common.Entities;

public class EquipmentCraftController{
    public EquipmentCraftInventoryData EquipmentCraftInventory;

    public EquipmentCraftController()
    {
        EquipmentCraftInventory = new EquipmentCraftInventoryData();
        EquipmentCraftInventory.InitDefault();
    }

    public void InitFromStats (EquipmentCraftStats equipmentCraftStats)
    {
        EquipmentCraftInventory.InitFromStats(equipmentCraftStats);
    }
}
