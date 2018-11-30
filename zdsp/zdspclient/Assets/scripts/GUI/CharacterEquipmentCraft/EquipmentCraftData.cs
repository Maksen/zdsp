using UnityEngine;
using UnityEngine.UI;

public class EquipmentCraftData : MonoBehaviour {
    
    [SerializeField]
    private Image equipIcon;
    [SerializeField]
    private Text equipName;

    public Toggle myToggle;

    public void Init(Sprite icon, string name)
    {
        myToggle = GetComponent<Toggle>();
        equipIcon.sprite = icon;
        equipName.text = name;
    }
}
