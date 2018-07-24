using UIAddons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Repository;

public class ExpanderSubData_Party : MonoBehaviour
{
    public Text nameText;

    private Toggle toggle;
    private CustomToggle customToggle;
    private int locId;
    private UnityAction<int> onToggleCB;
    private bool lastToggle = false;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
        customToggle = GetComponent<CustomToggle>();
    }

    public void Init(int id, ToggleGroup toggleGrp, UnityAction<int> callback)
    {
        locId = id;
        nameText.text = PartyRepo.GetLocationName(id);
        toggle.group = toggleGrp;
        onToggleCB = callback;
    }

    public void OnToggle(bool isOn)
    {
        if (isOn && !lastToggle)
        {
            lastToggle = true;
            if (onToggleCB != null)
                onToggleCB(locId);
        }
        else if (!isOn && lastToggle)
            lastToggle = false;
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
}
