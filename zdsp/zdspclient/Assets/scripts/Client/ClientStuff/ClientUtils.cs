using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kopio.JsonContracts;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Repository;
using Zealot.Audio;
public static class ClientUtils
{
    //common color
    public static Color ColorBrown = new Color(85 / 255f, 59f / 255f, 39f / 255f, 1);
    public static Color ColorDarkGreen = new Color(21f / 255f, 112f / 255f, 51f / 255f, 1);
    public static Color ColorRed = new Color(164f / 255f, 10f / 255f, 10f / 255f, 1);
    public static Color ColorCyan = new Color(9f / 255f, 118f / 255f, 127f / 255f, 1);
    public static Color ColorYellow = new Color(255f / 255f, 244f / 255f, 125f / 255f, 1);
    public static Color ColorTea = new Color(88f / 255f, 94f / 255f, 42f / 255f, 1);
    public static Color ColorGreen = new Color(75f / 255f, 255f / 255f, 51f / 255f, 1);
    public static Color ColorCurrencyBlack = Color.black;
    public static Color ColorTextMessageBlack = new Color(30f / 225f, 24f / 255f, 10f / 255f, 1);

    enum SDGEnum
    {
        duration,
        min_max,
        min,
        max,
        interval,
        percentage,
        skilleffect,
        maxtargets,
        sideeffectblock,
        weaponaffect,
        skillaffect,
        maxtarget,
        parameter,
    }

    public enum CharacterBasicStats
    {
        STR,
        AGI,
        DEX,
        CON,
        INT,

        NUM_STATS
    };

    public enum CharacterSecondaryStats
    {
        //Str
        WEAPONATK_STR, //+5 if using str weapons
        IGNORE_DEF, //+0.05%

        //Agi
        FLEE,       //+1
        ATK_SPD,    //+1%

        //Dex
        HIT,        //+1
        SKILL_CAST_SPD, //+1%
        WEAPONATK_DEX, //+2 WEAPON_ATK if using dex weapons

        //Vit
        DEF,      //+2
        CLASS_MAX_HP, //Refer to class hp formula
        CLASS_HP_REGEN, //Refer to class hp formula

        //Int
        WEAPONATK_INT, //+2 WEAPON_ATK if using int weapons
        RECOVERY_BONUS,
        ELEM_DEF,
        CLASS_MAX_SP,
        CLASS_SP_REGEN,

        NUM_SECONDARY_STATS
    };

    public static string GetCurrentLevelName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static Vector3 MoveTowards(Vector3 forward, float distToTarget, float speed, float delta)
	{
		float distToMove = speed * delta;
		if (distToMove > distToTarget)
        {
			delta = distToTarget / speed;
			return Physics.gravity * delta + forward * distToTarget;
		}
		return Physics.gravity * delta + forward * distToMove;	
	} 

    public static void SetLayerRecursively(GameObject obj, int layerNumber)
    {
        if (obj == null)
            return;
        int mkglow = LayerMask.NameToLayer("MkGlow");
        foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true))
        {
            if (layerNumber != mkglow)
                trans.gameObject.layer = layerNumber;
        }
    }

    public static GameObject CreateChild(Transform parent, GameObject childPrefab)
    {
        GameObject child = GameObject.Instantiate(childPrefab) as GameObject;
        child.transform.SetParent(parent, false);
        return child;
    }

    public static void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; --i)
            GameObject.Destroy(parent.GetChild(i).gameObject);
        parent.DetachChildren();
    }

	public static Sprite GetCurrencyIcon(CurrencyType currencytype)
    {
        switch (currencytype)
        {
            case CurrencyType.Money:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_Money.png");
            case CurrencyType.GuildContribution:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_GuildContribution.png");
            case CurrencyType.GuildGold:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_GuildGold.png");
            case CurrencyType.Gold:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_gold.png");
            case CurrencyType.LockGold:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_LockGold.png");
            case CurrencyType.LotteryTicket:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_LotteryTicket.png");
            case CurrencyType.HonorValue:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_HonorValue.png");
            case CurrencyType.BattleCoin:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_BattleCoin.png");
            case CurrencyType.Exp:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_Exp.png");
            case CurrencyType.VIP:
                return LoadIcon("UI_PiLiQ_Icons/Item/Currency/currency_VIP.png");
            default:
                return null;
        }
    }

    public static bool CanTeleportToLevel(string sceneName)
    {
        LevelJson lvlJ = LevelRepo.GetInfoByName(sceneName);
        if (lvlJ == null)
        {
            UIManager.ShowSystemMessage("Debug: Level<" + sceneName + "> is not loaded at client.", true);
            return false;
        }
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
            return false;

        RealmWorldJson realmWorldJson = RealmRepo.GetWorldByName(sceneName);
        if (player.GetAccumulatedLevel() < realmWorldJson.reqlvl)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("level", realmWorldJson.reqlvl.ToString());
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_map_FailLvl", parameters));
            return false;
        }
        return true;
    }

    public static long GetScoreBoardDelay()
    {        
        //if (GameInfo.mRealmInfo != null && GameInfo.mRealmInfo.type == RealmType.Arena)
        //    return 200; //arena already has a delay at server

        ActionCommand cmd = GameInfo.gLocalPlayer.GetActionCmd();
        ACTIONTYPE actionType = cmd.GetActionType();
        if (actionType == ACTIONTYPE.CASTSKILL || actionType == ACTIONTYPE.WALKANDCAST)        
            return 3500;        
        else
            return 1500;
    }

    public static void PlayMusic(bool value)
    {
        if (value)
        {
            GameObject bgm = GameObject.FindGameObjectWithTag("BackgroundMusic");
            if (bgm != null)
            {
                MusicService ms = bgm.GetComponent<MusicService>();
                if (ms != null)
                    ms.Play();
                else
                    Debug.LogError("BackgroundMusic has no MusicService attached!");
            }
        }
        else
            Music.Instance.FadeOutMusic(1f);
    }

    public static string FormatString(string str, Dictionary<string, string> parameters)
    {
        foreach (KeyValuePair<string, string> entry in parameters)
        {
            str = str.Replace(string.Format("{{{0}}}", entry.Key), entry.Value);
        }
        return str;
    }

    public static string FormatStringColor(string str, string hexcolor)
    {
        return string.Format("<color={0}>{1}</color>", hexcolor, str);
    }

    public static Sprite LoadIcon(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;
        return AssetLoader.Instance.Load<Sprite>(assetName) as Sprite;
    }

    public static void LoadIconAsync(string assetName, Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(assetName))
            return;
        AssetLoader.Instance.LoadAsync<Sprite>(assetName, callback);
    }

    public static Sprite LoadItemIcon(int itemid)
    {
        var item = GameRepo.ItemFactory.GetItemById(itemid);
        if (item == null)
            return null;
        return ClientUtils.LoadIcon(item.iconspritepath);
    }

    public static Sprite LoadQualityIcon(ItemType type, ItemRarity quality)
    {
        string path = "";
        if (type == ItemType.Equipment)
        {
            switch (quality)
            {
                case ItemRarity.Common:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_common.tif";
                    break;
                case ItemRarity.Uncommon:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_uncommon.tif";
                    break;
                case ItemRarity.Rare:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_rare.tif";
                    break;
                case ItemRarity.Epic:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_epic.tif";
                    break;
                case ItemRarity.Celestial:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_celestial.tif";
                    break;
                case ItemRarity.Legendary:
                    path = "UI_ZDSP_Icons/GameIcon/quality_equip_legendary.tif";
                    break;
            }
        }
        else
        {
            switch (quality)
            {
                case ItemRarity.Common:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_common.tif";
                    break;
                case ItemRarity.Uncommon:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_uncommon.tif";
                    break;
                case ItemRarity.Rare:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_rare.tif";
                    break;
                case ItemRarity.Epic:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_epic.tif";
                    break;
                case ItemRarity.Celestial:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_celestial.tif";
                    break;
                case ItemRarity.Legendary:
                    path = "UI_ZDSP_Icons/GameIcon/quality_default_legendary.tif";
                    break;
            }
        }
        return LoadIcon(path);
    }

    public static bool OpenUIWindowByLinkUI(LinkUIType linkUI, string param = "")
    {
        bool canOpen = true;
        switch (linkUI)
        {
            //case LinkUIType.Equipment_Gem_Equip:
            //    canOpen = UIManager.OpenWindow(WindowType.Equipment, (window) =>
            //    {
            //        var ui = window.GetComponent<UI_Equipment>();
            //        ui.GemToggle.isOn = true;
            //        ui.GemToggle.GetComponent<UIAddons.CustomToggle>().OnValueChanged(true);
            //    });
            //    break;
            //case LinkUIType.Alchemy:
            //    canOpen =  UIManager.OpenWindow(WindowType.Alchemy);
            //    break;
            //case LinkUIType.GoTopUp:
            //    UIManager.OpenDialog(WindowType.DialogGoTopUpAlchemy, (window) => window.GetComponent<UI_DialogGoTopUpAlchemy>().InitGoTopUp());
            //    break;
            //case LinkUIType.GoAlchemy:
            //    UIManager.OpenDialog(WindowType.DialogGoTopUpAlchemy, (window) => window.GetComponent<UI_DialogGoTopUpAlchemy>().InitGoAlchemy());
            //    break;
            //case LinkUIType.GuildStore:
            //    {
            //        UIManager.GetWindowGameObject(WindowType.Store).GetComponent<UI_Store>().OpenAndLoadStoreCategory(UIStoreLinkType.GuildStore);
            //        canOpen =  UIManager.OpenWindow(WindowType.Store);
            //    }
            //    break;
            //case LinkUIType.LotteryStore:
            //    {
            //        UIManager.GetWindowGameObject(WindowType.Store).GetComponent<UI_Store>().OpenAndLoadStoreCategory(UIStoreLinkType.Lottery);
            //        canOpen =  UIManager.OpenWindow(WindowType.Store);
            //    }
            //    break;
            //case LinkUIType.HonorStore:
            //    {
            //        UIManager.GetWindowGameObject(WindowType.Store).GetComponent<UI_Store>().OpenAndLoadStoreCategory(UIStoreLinkType.WuLing);
            //        canOpen =  UIManager.OpenWindow(WindowType.Store);
            //    }
            //    break;
            //case LinkUIType.BattleStore:
            //    {
            //        UIManager.GetWindowGameObject(WindowType.Store).GetComponent<UI_Store>().OpenAndLoadStoreCategory(UIStoreLinkType.WuMen);
            //        canOpen =  UIManager.OpenWindow(WindowType.Store);
            //    }
            //    break;
            //case LinkUIType.Potion:
            //    UIManager.OpenDialog(WindowType.DialogBotSetting);
            //    break;
            //case LinkUIType.Equipment_Upgrade:
            //    canOpen = UIManager.OpenWindow(WindowType.Equipment, (window) =>
            //    {
            //        var ui = window.GetComponent<UI_Equipment>();
            //        ui.EquipToggle.isOn = true;
            //    });
            //    break;
            //case LinkUIType.Dungeon:
            //    canOpen = UIManager.OpenWindow(WindowType.Dungeon);
            //    break;
            //case LinkUIType.WorldBoss:
            //    if (!GameInfo.gLocalPlayer.worldBossController.IsEventOpen())
            //    {
            //        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_activity_End"));
            //        return false;
            //    }
            //    canOpen = UIManager.OpenWindow(WindowType.WorldBoss);
            //    break;
            //case LinkUIType.ItemMall:
            //    canOpen = UIManager.OpenWindow(WindowType.PersistentItemMall);
            //    break;
            //case LinkUIType.Lottery:
            //    canOpen = UIManager.OpenWindow(WindowType.PersistentItemMall, (window) =>
            //    {
            //        window.GetComponent<UI_Mall>().OnOpenLottery();
            //    });
            //    break;
            //case LinkUIType.Spend_Gold:
            //    canOpen = UIManager.OpenWindow(WindowType.PersistentItemMall);
            //    break;
            //case LinkUIType.StoryDungeon:
            //    int realmid = 0;
            //    if (!string.IsNullOrEmpty(param) && int.TryParse(param, out realmid) && realmid > 0)
            //    {
            //        RealmJson realmJson = RealmRepo.GetInfoById(realmid);
            //        if (realmJson != null && GameInfo.gLocalPlayer.GetAccumulatedLevel() < realmJson.reqlvl)
            //        {
            //            ShowFeatureLocked(realmJson.reqlvl);
            //            return false;
            //        }
            //    }
            //    canOpen = UIManager.OpenWindow(WindowType.Dungeon, (window) =>
            //    {
            //        if (realmid > 0)
            //            window.GetComponent<UI_Dungeons>().GoToSpecificRealm(realmid);
            //        else
            //            window.GetComponent<UI_Dungeons>().OnClickStoryDungeon();
            //    });
            //    break;
            //case LinkUIType.DailyDungeon:
            //    int dungeon_realmid = 0;
            //    if (!string.IsNullOrEmpty(param) && int.TryParse(param, out dungeon_realmid) && dungeon_realmid > 0)
            //    {
            //        RealmJson realmJson = RealmRepo.GetInfoById(dungeon_realmid);
            //        if (realmJson != null && GameInfo.gLocalPlayer.GetAccumulatedLevel() < realmJson.reqlvl)
            //        {
            //            ShowFeatureLocked(realmJson.reqlvl);
            //            return false;
            //        }
            //    }
            //    canOpen = UIManager.OpenWindow(WindowType.Dungeon, (window) =>
            //    {
            //        if (dungeon_realmid > 0)
            //            window.GetComponent<UI_Dungeons>().GoToSpecificRealm(dungeon_realmid);
            //        else
            //            window.GetComponent<UI_Dungeons>().OnClickDailyDungeon();
            //    });
            //    break;
            //case LinkUIType.QuestExtraReward:
            //    canOpen = UIManager.OpenWindow(WindowType.QuestExtraRewards);
            //    break;
            //case LinkUIType.OfflineExp:
            //    canOpen = UIManager.OpenWindow(WindowType.OfflineExp);
            //    break;
            //case LinkUIType.TimeCity:
            //    int eliteMapMinLvl = GameConstantRepo.GetConstantInt("EliteMap_UnlockLvl", 1);
            //    if(GameInfo.gLocalPlayer.PlayerSynStats.progressLevel<eliteMapMinLvl)
            //    {
            //        ShowFeatureLocked(eliteMapMinLvl);
            //        return false;
            //    }

            //    canOpen = UIManager.OpenWindow(WindowType.MainMenu, (window) =>
            //    {
            //        window.GetComponent<UI_MenuOthers>().GoToTab(1);
            //    });
            //    break;
            //case LinkUIType.Crafting:
            //    canOpen = UIManager.OpenWindow(WindowType.Crafting);
            //    break;
            default:
                break;
        }
        return canOpen;
    }

    public static string ToTenThousand(long val)
    {
        if (val< 10000)
            return val.ToString();
        else
        {
            float value = (float)val / 10000;
            string valueStr = string.Format("{0:0.0}", Math.Truncate(value * 10) / 10);
            return valueStr + GUILocalizationRepo.GetLocalizedString("com_10K");
        }
    }

    public static GameObject InstantiatePlayer(Gender gender)
    {
        string prefabPath = JobSectRepo.GetGenderInfo(gender).modelpath;
        GameObject prefab = AssetLoader.Instance.Load<GameObject>(prefabPath);
        return GameObject.Instantiate(prefab);
    }

    public static string GetServerStatusColor(ServerLoad serverload)
    {
        switch (serverload)
        {
            case ServerLoad.Normal:
                return "lime";
            case ServerLoad.Busy:
                return "yellow";
            case ServerLoad.Full:
                return "red";
            default:
                return "red";
        }
    }

    public static string GetStandbyAnimationByWeapnType(PartsType type)
    {
        switch (type)
        {
            case PartsType.Sword:
                return "sword_nmstandby";
            case PartsType.Blade:
                return "blade_nmstandby";
            case PartsType.Lance:
                return "lance_shiled";
            case PartsType.Hammer:
                return "hammer_nmstandby";
            case PartsType.Fan:
                return "fan_nmstandby";
            case PartsType.Xbow:
                return "xbow_nmstandby";
            case PartsType.Dagger:
                return "dagger_nmstandby";
            case PartsType.Sanxian:
                return "sanxian_nmstandby";
            default:
                return "hammer_nmstandby";
        }
    }

    public static string GetLocalizedStatsName(CharacterBasicStats e)
    {
        switch (e)
        {
            case CharacterBasicStats.STR:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterBasicStats.AGI:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterBasicStats.CON:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterBasicStats.DEX:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterBasicStats.INT:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterBasicStats.NUM_STATS:
                return "";
        }

        return "";
    }

    public static string GetLocalizedStatsName(CharacterSecondaryStats e)
    {
        switch (e)
        {
            case CharacterSecondaryStats.WEAPONATK_STR:
                return GUILocalizationRepo.GetLocalizedString("stats_weaponattack");
            case CharacterSecondaryStats.WEAPONATK_DEX:
                return GUILocalizationRepo.GetLocalizedString("stats_weaponattack");
            case CharacterSecondaryStats.WEAPONATK_INT:
                return GUILocalizationRepo.GetLocalizedString("stats_weaponattack");
            case CharacterSecondaryStats.IGNORE_DEF:
                return GUILocalizationRepo.GetLocalizedString("stats_ignorearmor");
            case CharacterSecondaryStats.ATK_SPD:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterSecondaryStats.SKILL_CAST_SPD:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterSecondaryStats.HIT:
                return GUILocalizationRepo.GetLocalizedString("stats_accuracy");
            case CharacterSecondaryStats.FLEE:
                return GUILocalizationRepo.GetLocalizedString("stats_evasion");
            case CharacterSecondaryStats.CLASS_MAX_HP:
                return GUILocalizationRepo.GetLocalizedString("stats_maxhealth");
            case CharacterSecondaryStats.CLASS_HP_REGEN:
                return GUILocalizationRepo.GetLocalizedString("stats_healthregen");
            case CharacterSecondaryStats.CLASS_MAX_SP:
                return GUILocalizationRepo.GetLocalizedString("stats_maxmana");
            case CharacterSecondaryStats.CLASS_SP_REGEN:
                return GUILocalizationRepo.GetLocalizedString("stats_manaregen");
            case CharacterSecondaryStats.RECOVERY_BONUS:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterSecondaryStats.DEF:
                return GUILocalizationRepo.GetLocalizedString("stats_armor");
            case CharacterSecondaryStats.ELEM_DEF:
                return GUILocalizationRepo.GetLocalizedString("");
            case CharacterSecondaryStats.NUM_SECONDARY_STATS:
                return "";
        }

        return "";
    }

    public static Dictionary<CharacterSecondaryStats, float> GetSecStatsByStats(CharacterBasicStats e, int value)
    {
        Dictionary<CharacterSecondaryStats, float> resDic = new Dictionary<CharacterSecondaryStats, float>();

        switch (e)
        {
            case CharacterBasicStats.STR:
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.WEAPONATK_STR, value * 5f);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.IGNORE_DEF, value * 0.05f);
                break;
            case CharacterBasicStats.AGI:
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.FLEE, value);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.ATK_SPD, value);
                break;
            case CharacterBasicStats.CON:
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.DEF, value * 2f); 
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.CLASS_MAX_HP, value); //Class specific
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.CLASS_HP_REGEN, value); //Class specific
                break;
            case CharacterBasicStats.DEX:
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.WEAPONATK_DEX, value * 2f);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.HIT, value);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.SKILL_CAST_SPD, value);
                break;
            case CharacterBasicStats.INT:
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.WEAPONATK_INT, value * 2f);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.RECOVERY_BONUS, value);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.ELEM_DEF, value);
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.CLASS_MAX_SP, value); //Class specific
                DictQuickCheckAdd(resDic, CharacterSecondaryStats.CLASS_SP_REGEN, value); //Class specific
                break;
        }

        return resDic;
    }
    private static void DictQuickCheckAdd(Dictionary<CharacterSecondaryStats, float> dict, CharacterSecondaryStats e, float val)
    {
        if (!dict.ContainsKey(e))
            dict.Add(e, 0f);

        dict[e] += val;
    }

    public static string GetLocalizedReformKai(int reformStep)
    {
        System.Text.StringBuilder kaiStr = new System.Text.StringBuilder();
        kaiStr.AppendFormat("{0}{1}", GUILocalizationRepo.GetLocalizedString("com_Kai"), GetLocalizedNumFromInt(reformStep));

        return kaiStr.ToString();
    }

    private static string GetLocalizedNumFromInt(int number)
    {
        if(number > 0 && number < 100)
        {
            int times = 0;
            while(number > 10)
            {
                ++times;
                number -= 10;
            }

            if(number == 10)
            {
                ++times;
                return string.Format("{0}{1}", GetLocalizedSingleDigit(times), GetLocalizedSingleDigit(10));
            }
            else
            {
                if(times > 0)
                {
                    return string.Format("{0}{1}", GetLocalizedSingleDigit(times), GetLocalizedSingleDigit(number));
                }
                else
                {
                    return GetLocalizedSingleDigit(number);
                }
            }
        }

        return "Invalid number or number too big!";
    }

    private static string GetLocalizedSingleDigit(int number)
    {
        if(number > 10)
        {
            return "Num too big!";
        }

        string num_str = "";

        switch(number)
        {
            case 1:
                num_str = GUILocalizationRepo.GetLocalizedString("com_One");
                break;
            case 2:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Two");
                break;
            case 3:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Three");
                break;
            case 4:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Four");
                break;
            case 5:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Five");
                break;
            case 6:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Six");
                break;
            case 7:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Seven");
                break;
            case 8:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Eight");
                break;
            case 9:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Nine");
                break;
            case 10:
                num_str = GUILocalizationRepo.GetLocalizedString("com_Ten");
                break;
        }

        return num_str;
    }
}

