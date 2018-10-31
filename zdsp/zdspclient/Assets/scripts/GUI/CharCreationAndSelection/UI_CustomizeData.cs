using UnityEngine;
using UnityEngine.UI;

public class UI_CustomizeData : MonoBehaviour
{
    [SerializeField]
    Image Icon;

    [SerializeField]
    Material Mask;

    [SerializeField]
    Sprite DefaultSprite;

    private int mPartId;
    private UI_CharacterCreation mParent;

    public void Init(ToggleGroup group, string iconpath, bool iscolor, int partid, bool selected, UI_CharacterCreation parent)
    {
        mPartId = partid;
        mParent = parent;
        if (iscolor)
        {
            Icon.sprite = DefaultSprite;
            Icon.material = Mask;
            Color color;
            if (ColorUtility.TryParseHtmlString("#" + iconpath, out color))
            {
                Icon.color = color;
            }
        }
        else
        {
            Icon.material = null;
            Icon.sprite = string.IsNullOrEmpty(iconpath) ? null : ClientUtils.LoadIcon(iconpath);
        }
        GetComponent<Toggle>().isOn = selected;
        GetComponent<Toggle>().group = group;
    }

    public void OnClickData(bool value)
    {
        mParent.OnSelectedCustomizeData(mPartId);
    }
}
