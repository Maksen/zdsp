using UnityEngine;
using UnityEngine.UI;

public class GameIconCmpt_SelectCheckmark : MonoBehaviour
{
    [SerializeField]
    Toggle toggleSelect = null;

    [SerializeField]
    GameObject gameobjCheckmark = null;

    public Toggle GetToggleSelect()
    {
        return toggleSelect;
    }

    public void SetCheckmarkVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void OnValueChangedToggleSelect(bool value)
    {
        gameobjCheckmark.SetActive(value);
    }
}
