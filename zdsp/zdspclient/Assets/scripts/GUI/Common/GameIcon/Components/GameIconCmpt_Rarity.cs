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

    public void SetRarity(ItemGameIconType gameIconType, ItemRarity itemRarity)
    {
        Sprite sprite = ClientUtils.LoadItemQualityIcon(gameIconType, itemRarity);
        if (sprite != null)
            imgRarity.sprite = sprite;
    }
}
