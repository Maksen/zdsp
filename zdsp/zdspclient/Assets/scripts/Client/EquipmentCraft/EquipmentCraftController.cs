using System.Collections.Generic;
using System.Linq;
using Zealot.Repository;
using Kopio.JsonContracts;
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

    public static string GetEquipmentSideEffect(string equipmentEffect)
    {
        List<int> equipmentStatsList = equipmentEffect.Split(',').Select(int.Parse).ToList();
        if (equipmentStatsList[0] == -1)
        {
            return string.Empty;
        }
        System.Text.StringBuilder bind = new System.Text.StringBuilder();

        for (int index = 0; index < equipmentStatsList.Count; ++index)
        {
            SideEffectJson sideEffect = SideEffectRepo.GetSideEffect(equipmentStatsList[index]);
            bind.Append(sideEffect.description);
            bind.Append("+");
            bind.Append(sideEffect.min);
            bind.Append("\n");
        }
        bind.Remove(bind.Length - 2, 2);

        return bind.ToString();
    }
}
