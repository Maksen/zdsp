using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

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

    public static List<string> BuildEquipStats(Equipment equip)
    {
        List<string> lis = new List<string>();
        lis.Add(equip.GetEquipmentName());
        StringBuilder st = new StringBuilder("+");
        st.Append(equip.UpgradeLevel.ToString());
        lis.Add(st.ToString());
        lis.Add(equip.ReformStep.ToString());
        return lis;
    }

    public static List<string> DecodeEffect(string EffectString, bool addColor = true)
    {
        List<string> effectGroup = EffectString.Split('|').ToList();
        List<string> lis = new List<string>();

        for (int i = 0; i < 3; ++i)
        {
            for (int j = 1; j < 3; ++j)
            {
                int order = i * 3 + j;

                if (effectGroup[order] != "0,0")
                {
                    List<string> devideLis = effectGroup[order].Split(',').ToList();
                    StringBuilder bind = new StringBuilder();
                    bind.Append(SideEffectRepo.GetSideEffect(int.Parse(devideLis[0])).localizedname);
                    bind.Append("+");
                    bind.Append(devideLis[1]);
                    lis.Add(bind.ToString());
                }
                else
                {
                    lis.Add(string.Empty);
                }
            }
            if (addColor)
            {
                int order = i * 2;
                int rarity = EquipFusionRepo.GetEffectRarity(int.Parse(effectGroup[i * 3]));
                string color = ItemUtils.GetStrColorByRarity((ItemRarity)rarity);
                lis[order] = (lis[order] == string.Empty) ? string.Empty : string.Format("<color={0}>{1}</color>", color, lis[order]);
                lis[order + 1] = (lis[order + 1] == string.Empty) ? string.Empty : string.Format("<color={0}>{1}</color>", color, lis[order + 1]);
            }
        }
        return lis;
    }
}