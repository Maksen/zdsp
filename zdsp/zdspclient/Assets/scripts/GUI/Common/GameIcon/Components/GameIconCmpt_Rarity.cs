using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class GameIconCmpt_Rarity : MonoBehaviour
{
    [SerializeField]
    Image imgRarity = null;

    [SerializeField]
    GameObject[] rarityEffects = null;

    public void SetRarity(BagType bagType, ItemRarity itemRarity)
    {
        StringBuilder sb = new StringBuilder("UI_ZDSP_Icons/GameIcon/quality_");
        if (bagType == BagType.Equipment) sb.Append("equip_");
        else sb.Append("default_");

        switch (itemRarity)
        {
            case ItemRarity.Common: sb.Append("common.tif"); break;
            case ItemRarity.Uncommon: sb.Append("uncommon.tif"); break;
            case ItemRarity.Rare: sb.Append("rare.tif"); break;
            case ItemRarity.Epic: sb.Append("epic.tif"); break;
            case ItemRarity.Celestial: sb.Append("celestial.tif"); break;
            case ItemRarity.Legendary: sb.Append("legendary.tif"); break;
        }
        imgRarity.sprite = ClientUtils.LoadIcon(sb.ToString());
    }
}
