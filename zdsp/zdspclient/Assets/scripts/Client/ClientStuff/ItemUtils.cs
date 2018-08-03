using Zealot.Common;

public static class ItemUtils
{
    public static string GetStrColorByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "#FFFFFF";
            case ItemRarity.Uncommon: return "#5DFD00";
            case ItemRarity.Rare: return "#00CCFF";
            case ItemRarity.Epic: return "#FF00FF";
            case ItemRarity.Celestial: return "#FFF47D";
            case ItemRarity.Legendary: return "#FFFFFF"; //todo use correct color code.
            default: return "#FFFFFF";
        }
    }

    public static string GetColorTagByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "ico";
            case ItemRarity.Uncommon: return "iun";
            case ItemRarity.Rare: return "ira";
            case ItemRarity.Epic: return "iep";
            case ItemRarity.Celestial: return "ice";
            case ItemRarity.Legendary: return "ile";
            default: return "ico";
        }
    }
}
