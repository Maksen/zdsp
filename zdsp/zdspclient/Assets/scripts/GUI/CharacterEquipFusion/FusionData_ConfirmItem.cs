using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FusionData_ConfirmItem : MonoBehaviour {

    [SerializeField]
    private Transform confirmParent;
    [SerializeField]
    private Text itemName;
    [SerializeField]
    private Text itemUpgrade;
    [SerializeField]
    private Text itemEvolve;
    [SerializeField]
    private Text itemRank;
    [SerializeField]
    private Text[] itemStats;

    public void Init(GameObject confirmPrefab, int id, string name, string upgrade, string evolve, string rank, List<string> stats)
    {
        ClientUtils.DestroyChildren(confirmParent);
        GameObject confirmObj = ClientUtils.CreateChild(confirmParent, confirmPrefab);
        GameIcon_MaterialConsumable material = confirmObj.GetComponent<GameIcon_MaterialConsumable>();
        material.InitWithoutCallback(id, 0);
        material.SetFullStackCount(1);

        itemName.text = name;
        itemUpgrade.text = upgrade;
        itemEvolve.text = evolve;
        itemRank.text = rank;
        for (int i = 0; i < itemStats.Length; ++i)
        {
            itemStats[i].text = stats[i];
        }
    }
}
