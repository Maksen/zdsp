using UnityEngine;
using UnityEngine.UI;

public class UI_DialogItemDetail_TextValue : MonoBehaviour
{
    [SerializeField]
    Text mIdentifier;
    [SerializeField]
    Text mColon;
    [SerializeField]
    Text mValue;

    public string Identifier
    {
        set { mIdentifier.text = value; }
    }

    public bool ShowColon
    {
        get { return mColon.gameObject.GetActive(); }
        set { mColon.gameObject.SetActive(value); }
    }

    public string Value
    {
        set { mValue.text = value; }
    }
}
