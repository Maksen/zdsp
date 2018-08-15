using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class ModdingEquipment
{
    public int mSlotID;
    public Equipment mEquip;
    public bool mIsEquipped;

    public ModdingEquipment(int slotId, Equipment equipment, bool isEquipped)
    {
        mSlotID = slotId;
        mEquip = equipment;
        mIsEquipped = isEquipped;
    }
}

public class UI_EquipmentModding : MonoBehaviour
{
    public List<ModdingEquipment> GetModdingEquipmentList(List<Equipment> equippedEquipList, List<IInventoryItem> invEquipList)
    {
        List<ModdingEquipment> equipUpgList = new List<ModdingEquipment>();
        for (int i = 0; i < equippedEquipList.Count; ++i)
        {
            Equipment equipment = equippedEquipList[i];
            if (equipment != null)
            {
                equipUpgList.Add(new ModdingEquipment(i, equipment, true));
            }
        }

        for (int i = 0; i < invEquipList.Count; ++i)
        {
            Equipment invEquip = invEquipList[i] as Equipment;
            if (invEquip != null)
            {
                equipUpgList.Add(new ModdingEquipment(i, invEquip, false));
            }
        }

        return equipUpgList;
    }

    public List<int> GetSEIDListFromString(string seIDGrp)
    {
        List<int> seIDList = new List<int>();

        List<string> seIDStrList = seIDGrp.Split(';').ToList();
        for(int i = 0; i < seIDStrList.Count; ++i)
        {
            int seId = 0;
            if(int.TryParse(seIDStrList[i], out seId))
            {
                seIDList.Add(seId);
            }
        }

        return seIDList;
    }
}
