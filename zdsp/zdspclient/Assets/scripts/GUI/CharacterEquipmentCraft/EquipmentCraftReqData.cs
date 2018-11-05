using UnityEngine;
using UnityEngine.UI;

public class EquipmentCraftReqData : MonoBehaviour {

    [SerializeField]
    private GameObject sampleObject;
    [SerializeField]
    private Text itemReqStack;
    
    public Transform gameIconParent;

    public void Init(string stackCount)
    {
        transform.localScale = new Vector3(1, 1, 1);
        sampleObject.SetActive(false);
        itemReqStack.text = stackCount;
    }
}
