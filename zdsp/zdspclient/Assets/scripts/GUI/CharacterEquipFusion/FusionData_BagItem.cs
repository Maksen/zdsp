using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FusionData_BagItem : MonoBehaviour {

    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private Text itemName;
    [SerializeField]
    private Text itemUpgrade, itemEvolve, itemRank;
    [SerializeField]
    private Text gemStats1_1, gemStats1_2;
    [SerializeField]
    private Text gemStats2_1, gemStats2_2;
    [SerializeField]
    private Text gemStats3_1, gemStats3_2;

    [SerializeField]
    private GameObject gemEffectIcon1;
    [SerializeField]
    private GameObject gemEffectIcon2;
    [SerializeField]
    private GameObject gemEffectIcon3;

    public void SetGemStats(Sprite img, string name, List<string> statsList)
    {
        itemIcon.sprite = img;
        itemName.text = name;
        itemUpgrade.text = string.Empty;
        itemEvolve.text = string.Empty;
        itemRank.enabled = false;

        gemStats1_1.text = statsList[0];
        gemStats1_2.text = statsList[1];
        gemStats2_1.text = statsList[2];
        gemStats2_2.text = statsList[3];
        gemStats3_1.text = statsList[4];
        gemStats3_2.text = statsList[5];
        gemEffectIcon1.SetActive((statsList[0] != string.Empty) ? true : false);
        gemEffectIcon2.SetActive((statsList[2] != string.Empty) ? true : false);
        gemEffectIcon3.SetActive((statsList[4] != string.Empty) ? true : false);
    }

    public void SetEquipStats(Sprite img, string name, string upgrade, string evolve, List<string> statsList)
    {
        itemIcon.sprite = img;
        itemName.text = name;
        itemUpgrade.text = upgrade;
        itemEvolve.text = evolve;
        itemRank.enabled = true;

        gemStats1_1.text = statsList[0];
        gemStats1_2.text = statsList[1];
        gemStats2_1.text = statsList[2];
        gemStats2_2.text = statsList[3];
        gemStats3_1.text = statsList[4];
        gemStats3_2.text = statsList[5];
        gemEffectIcon1.SetActive((statsList[0] != string.Empty) ? true : false);
        gemEffectIcon2.SetActive((statsList[2] != string.Empty) ? true : false);
        gemEffectIcon3.SetActive((statsList[4] != string.Empty) ? true : false);
    }
}
