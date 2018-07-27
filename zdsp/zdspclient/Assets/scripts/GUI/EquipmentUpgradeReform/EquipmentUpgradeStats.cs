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

public enum EquipmentModdingType
{
    Upgrade,
    Reform,
}

public enum EquipmentUpgradeStatsType
{
    ToGet,
    Gotten,
}

public class EquipmentUpgradeStats : MonoBehaviour
{
    [Header("Text")]
    public Text statsLabel;
    public Text statsValue;

    public void Init(EquipmentUpgradeStatsType statsType, int upgradeLevel, string description)
    {
        Dictionary<string, string> statsUpgParam = new Dictionary<string, string>();
        statsUpgParam.Add("level", upgradeLevel.ToString());
        statsLabel.text = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_lvl_reached", statsUpgParam);
        string buffLabel = "";
        switch(statsType)
        {
            case EquipmentUpgradeStatsType.ToGet:
                buffLabel = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_to_get");
                break;
            case EquipmentUpgradeStatsType.Gotten:
                buffLabel = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_gotten");
                break;
        }

        statsLabel.text = buffLabel;
        statsValue.text = description;
    }
}
