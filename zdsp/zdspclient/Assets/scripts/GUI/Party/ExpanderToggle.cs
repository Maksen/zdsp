using UnityEngine;
using UnityEngine.UI;
using UIAddons;

[RequireComponent(typeof(Toggle))]
public class ExpanderToggle : MonoBehaviour
{
    public Transform childContent;

    private Toggle toggle;
    private CustomToggle customToggle;

    public int ChildCount { get { return childContent.childCount; } }

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        customToggle = GetComponent<CustomToggle>();
    }

    public void SetToggleOn(bool value)
    {
        toggle.isOn = value;
        if (toggle.isOn)
        {
            customToggle.offState.SetActive(false);
            customToggle.onState.SetActive(true);
        }
        else
        {
            customToggle.offState.SetActive(true);
            customToggle.onState.SetActive(false);
        }
    }

    public void AddChild(GameObject child)
    {
        child.transform.SetParent(childContent, false);
    }

    public GameObject GetChild(int index)
    {
        return childContent.GetChild(index).gameObject;
    }

    public void DestroyChildren()
    {
        for (int i = childContent.childCount - 1; i >= 0; --i)
            Destroy(childContent.GetChild(i).gameObject);
        childContent.DetachChildren();
    }

}
