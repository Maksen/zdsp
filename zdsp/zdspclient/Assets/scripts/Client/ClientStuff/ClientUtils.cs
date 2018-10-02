using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
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
            default:
                return null;
        }
    }

    public static bool CanTeleportToLevel(string sceneName)
    {
        LevelJson lvlJson = LevelRepo.GetInfoByName(sceneName);
        if (lvlJson == null)
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

    // Note: Set IInventoryItem to null if not available
    public static void InitGameIcon(GameObject gameIcon, IInventoryItem invItem, int itemId, 
        ItemGameIconType gameIconType, int stackCount, bool withToolTip)
    {
        if (gameIcon == null || itemId == 0)
            return;

        switch (gameIconType)
        {
            case ItemGameIconType.Equipment:
                int evolve = 0, upgrade = 0;
                if (invItem != null)
                {
                    Equipment eq = (Equipment)invItem;
                    evolve = eq.ReformStep;
                    upgrade = eq.UpgradeLevel;
                }
                if (withToolTip)
                    gameIcon.GetComponent<GameIcon_Equip>().InitWithToolTipView(itemId, 0, evolve, upgrade);
                else
                    gameIcon.GetComponent<GameIcon_Equip>().InitWithoutCallback(itemId, 0, evolve, upgrade);
                break;
            case ItemGameIconType.Consumable:
            case ItemGameIconType.Material:
                if (withToolTip)
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().InitWithToolTipView(itemId, stackCount);
                else
                    gameIcon.GetComponent<GameIcon_MaterialConsumable>().InitWithoutCallback(itemId, stackCount);
                break;
            case ItemGameIconType.DNA:
                if (withToolTip)
                    gameIcon.GetComponent<GameIcon_DNA>().InitWithToolTipView(itemId, 0, 0);
                else
                    gameIcon.GetComponent<GameIcon_DNA>().InitWithoutCallback(itemId, 0, 0);
                break;
        }
    }

    public static GameObject LoadGameIcon(ItemGameIconType gameIconType)
    {
        switch (gameIconType)
        {
            case ItemGameIconType.Equipment:
                return AssetLoader.Instance.Load<GameObject>("UI_ZDSP_GameIcon/GameIcon_Equip/P_GameIcon_Equip/GameIcon_Equip.prefab");
            case ItemGameIconType.Consumable:
            case ItemGameIconType.Material:
                return AssetLoader.Instance.Load<GameObject>("UI_ZDSP_GameIcon/GameIcon_Material/P_GameIcon_Material/GameIcon_Material.prefab");
            case ItemGameIconType.DNA:
                return AssetLoader.Instance.Load<GameObject>("UI_ZDSP_GameIcon/GameIcon_DNA/P_GameIcon_DNA/GameIcon_DNA.prefab");
            default: return null;
        }
    }

    public static void LoadGameIconAsync(ItemGameIconType gameIconType, Action<GameObject> callback)
    {
        switch (gameIconType)
        {
            case ItemGameIconType.Equipment:
                AssetLoader.Instance.LoadAsync("UI_ZDSP_GameIcon/GameIcon_Equip/P_GameIcon_Equip/GameIcon_Equip.prefab", callback);
                break;
            case ItemGameIconType.Consumable:
            case ItemGameIconType.Material:
                AssetLoader.Instance.LoadAsync("UI_ZDSP_GameIcon/GameIcon_Material/P_GameIcon_Material/GameIcon_Material.prefab", callback);
                break;
            case ItemGameIconType.DNA:
                AssetLoader.Instance.LoadAsync("UI_ZDSP_GameIcon/GameIcon_DNA/P_GameIcon_DNA/GameIcon_DNA.prefab", callback);
                break;
        }
    }

    public static Sprite LoadIcon(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;
        return AssetLoader.Instance.Load<Sprite>(assetName);
    }

    public static void LoadIconAsync(string assetName, Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(assetName))
            return;
        AssetLoader.Instance.LoadAsync(assetName, callback);
    }

    public static Sprite LoadItemIcon(int itemid)
    {
        var item = GameRepo.ItemFactory.GetItemById(itemid);
        if (item == null)
            return null;
        return LoadIcon(item.iconspritepath);
    }

    public static Sprite LoadItemQualityIcon(ItemGameIconType gameIconType, ItemRarity rarity)
    {
        string path = "";
        switch (rarity)
        {
            case ItemRarity.Common:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_common.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_common.tif";
                break;
            case ItemRarity.Uncommon:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_uncommon.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_uncommon.tif";
                break;
            case ItemRarity.Rare:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_rare.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_rare.tif";
                break;
            case ItemRarity.Epic:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_epic.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_epic.tif";
                break;
            case ItemRarity.Celestial:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_celestial.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_celestial.tif";
                break;
            case ItemRarity.Legendary:
                path = (gameIconType == ItemGameIconType.Equipment)
                    ? "UI_ZDSP_Icons/GameIcon/quality_equip_legendary.tif"
                    : "UI_ZDSP_Icons/GameIcon/quality_default_legendary.tif";
                break;
        }
        return LoadIcon(path);
    }

    public static Sprite LoadCurrencyIcon(CurrencyType currencyType)
    {
        string path = "";

        switch(currencyType)
        {
            case CurrencyType.Money:
                path = "";
                break;
            case CurrencyType.Gold:
                path = "";
                break;
        }

        return LoadIcon(path);
    }

    public static VideoClip LoadVideo(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;
        return AssetLoader.Instance.Load<VideoClip>(assetName) as VideoClip;
    }

    public static void LoadVideoAsync(string assetName, Action<VideoClip> callback)
    {
        if (string.IsNullOrEmpty(assetName))
            return;
        AssetLoader.Instance.LoadAsync(assetName, callback);
    }

    public static AudioClip LoadAudio(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;
        return AssetLoader.Instance.Load<AudioClip>(assetName) as AudioClip;
    }

    public static void LoadAudioAsync(string assetName, Action<AudioClip> callback)
    {
        if (string.IsNullOrEmpty(assetName))
            return;
        AssetLoader.Instance.LoadAsync(assetName, callback);
    }

    public static bool OpenUIWindowByLinkUI(LinkUIType linkUI, string param = "")
    {
        bool canOpen = true;
        switch (linkUI)
        {
            case LinkUIType.Equipment_Upgrade:
                break;
            case LinkUIType.Equipment_Reform:
                break;
            case LinkUIType.Equipment_Socket:
                break;
            case LinkUIType.DNA:
                break;
            case LinkUIType.Shop:
                break;
            case LinkUIType.Skill:
                break;
            case LinkUIType.Realm:
                break;
            case LinkUIType.GoTopUp:
                break;
            case LinkUIType.Achievement:
                break;
            case LinkUIType.Hero:
                int heroId;
                if (!string.IsNullOrEmpty(param) && int.TryParse(param, out heroId) && heroId > 0)
                {
                    UI_Hero uiHero = UIManager.GetWindowGameObject(WindowType.Hero).GetComponent<UI_Hero>();
                    uiHero.SelectHero = heroId;
                }
                canOpen = UIManager.OpenWindow(WindowType.Hero);
                break;
            case LinkUIType.Hero_Explore:
                canOpen = UIManager.OpenWindow(WindowType.Hero, (window) => window.GetComponent<UI_Hero>().GoToTab(2));
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

    public static List<string> weaponPrefix = new List<string>() { "sword", "blade", "lance", "hammer", "fan", "xbow", "dagger", "sanxian" };
    public static string GetPrefixByWeaponType(PartsType type)
    {
        switch (type)
        {
            case PartsType.Sword:
                return "sword";
            case PartsType.Blade:
                return "blade";
            case PartsType.Lance:
                return "lance";
            case PartsType.Hammer:
                return "hammer";
            case PartsType.Fan:
                return "fan";
            case PartsType.Xbow:
                return "xbow";
            case PartsType.Dagger:
                return "dagger";
            case PartsType.Sanxian:
                return "sanxian";
            default:
                return "blade";
        }
    }

    public static string GetStandbyAnimationByWeaponType(PartsType type)
    {
        switch (type)
        {
            case PartsType.Sword:
                return "sword_nmstandby";
            case PartsType.Blade:
                return "blade_nmstandby";
            case PartsType.Lance:
                return "lance_nmstandby";
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
                return "blade_nmstandby";
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
        kaiStr.AppendFormat("{0}{1}", GUILocalizationRepo.GetLocalizedString("com_Kai"), GetRomanNumFromInt(reformStep));

        return kaiStr.ToString();
    }

    public static string GetRomanNumFromInt(int number)
    {
        if (number > 0 && number <= 100)
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
                return string.Format("{0}", GetRomanDigitTens(times));
            }
            else
            {
                return (times > 0) ? string.Format("{0}{1}", GetRomanDigitTens(times), GetRomanDigitSgl(number)) 
                                   : GetRomanDigitSgl(number);
            }
        }
        return "Invalid number or number too big!";
    }

    private static string GetRomanDigitSgl(int number)
    {
        if(number > 10)
            return "Num too big!";

        switch(number)
        {
            case 1:  return "I";
            case 2:  return "II";
            case 3:  return "III";
            case 4:  return "IV";
            case 5:  return "V";
            case 6:  return "VI";
            case 7:  return "VII";
            case 8:  return "VIII";
            case 9:  return "IX";
            case 10: return "X";
            default: return "";
        }
    }

    private static string GetRomanDigitTens(int mul)
    {
        // mul > 10 means actual number is > 100
        if(mul > 10)
            return "Num too big!";

        switch (mul)
        {
            case 1:  return "X";
            case 2:  return "XX";
            case 3:  return "XXX";
            case 4:  return "XL";
            case 5:  return "L";
            case 6:  return "LX";
            case 7:  return "LXX";
            case 8:  return "LXXX";
            case 9:  return "XC";
            case 10: return "C";
            default: return "";
        }
    }

    public static string ColorizedText(string text, string colorStr)
    {
        return string.Format("<color={0}>{1}</color>", colorStr, text);
    }

    public delegate string Tokenizer(string token, params object[] param);
    /// <summary>
    /// Parse string with token using this format {xxx}
    /// e.g. This is a test {token}
    /// </summary>
    /// <param name="input">the string to parse</param>
    /// <param name="id">id for the delegate to use</param>
    /// <param name="tokenizer">function to recieve the token and returns a string</param>
    /// <returns></returns>
    public static string ParseStringToken(string input, Tokenizer tokenizer, params object[] param)
    {
        System.Text.StringBuilder output = new System.Text.StringBuilder(input);
        List<Match> matches = new List<Match>();

        foreach(Match match in Regex.Matches(input, @"{([^{}]*)}"))
        {
            matches.Add(match);
        }

        matches.Reverse();

        foreach(Match match in matches)
        {
            string replacement = tokenizer(match.Value, param);
            output.Remove(match.Index, match.Value.Length);
            output.Insert(match.Index, replacement);
        }

        return output.ToString();
    }
}
