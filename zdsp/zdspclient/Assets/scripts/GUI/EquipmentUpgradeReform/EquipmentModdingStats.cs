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

public enum EquipmentModdingStatsType
{
    ToGet,
    Gotten,
}

public class EquipmentModdingStats : MonoBehaviour
{
    [Header("Text")]
    public Text statsLabel;
    public Text statsValue;

    public void Init(EquipmentModdingType modType, EquipmentModdingStatsType statsType, int upgradeLevel, string description)
    {
        Dictionary<string, string> statsUpgParam = new Dictionary<string, string>();
        statsUpgParam.Add("level", upgradeLevel.ToString());
        statsLabel.text = modType == EquipmentModdingType.Upgrade ? GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_lvl_reached", statsUpgParam)
            : GUILocalizationRepo.GetLocalizedString("eqp_mod_reform_lvl_reached", statsUpgParam);
        string buffLabel = "";
        switch(statsType)
        {
            case EquipmentModdingStatsType.ToGet:
                buffLabel = modType == EquipmentModdingType.Upgrade ? GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_to_get")
                    : GUILocalizationRepo.GetLocalizedString("eqp_mod_reform_to_get");
                break;
            case EquipmentModdingStatsType.Gotten:
                buffLabel = modType == EquipmentModdingType.Upgrade ? GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_gotten")
                    : GUILocalizationRepo.GetLocalizedString("eqp_mod_reform_gotten");
                break;
        }

        statsLabel.text = string.Format("{0} {1}", buffLabel, description);
    }
}
