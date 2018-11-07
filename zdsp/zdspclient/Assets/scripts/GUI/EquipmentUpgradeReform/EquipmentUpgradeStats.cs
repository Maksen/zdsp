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

    public void Init(int upgradeLevel, int upgradeLimit, string seDescription, int increase)
    {
        statsLabel.text = string.Format("{0}/{1}", upgradeLevel, upgradeLimit);
        statsValue.text = string.Format("{0} + {1}", seDescription, increase);
    }

    public void Init(EquipmentUpgradeStatsType statsType, int upgradeLevel, string seDescription)
    {
        Dictionary<string, string> statsUpgParam = new Dictionary<string, string>();
        statsUpgParam.Add("level", upgradeLevel.ToString());
        statsLabel.text = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_lvl_reached", statsUpgParam);
        string buffLabel = "";
        switch(statsType)
        {
            case EquipmentUpgradeStatsType.ToGet:
                Dictionary<string, string> toGetBuffParam = new Dictionary<string, string>();
                toGetBuffParam.Add("buff", seDescription);
                buffLabel = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_to_get", toGetBuffParam);
                break;
            case EquipmentUpgradeStatsType.Gotten:
                Dictionary<string, string> gottenBuffParam = new Dictionary<string, string>();
                gottenBuffParam.Add("buff", seDescription);
                buffLabel = GUILocalizationRepo.GetLocalizedString("eqp_mod_upgrade_gotten", gottenBuffParam);
                break;
        }

        statsValue.text = buffLabel;
    }
}
