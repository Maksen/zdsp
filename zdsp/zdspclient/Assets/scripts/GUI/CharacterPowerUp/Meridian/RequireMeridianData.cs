using UnityEngine;
using UnityEngine.UI;

public class RequireMeridianData : MonoBehaviour {

    [SerializeField]
    private Transform itemIconSlot;
    [SerializeField]
    private Text requireStack;

    public GameIcon_MaterialConsumable equipIcon;
    
    public void SetEquipIcon(GameObject child)
    {
        GameObject obj = ClientUtils.CreateChild(itemIconSlot, child);
        equipIcon = obj.GetComponent<GameIcon_MaterialConsumable>();
    }

    public void SetRequireStack(int mRequireStack)
    {
        requireStack.text = mRequireStack.ToString();
    }
}
