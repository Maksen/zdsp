using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Hero_MapData : MonoBehaviour
{
    [SerializeField] Text levelText;
    [SerializeField] Text nameText;
    [SerializeField] Text subNameText;
    [SerializeField] GameObject ongoingHighlight;
    [SerializeField] GameObject previousHighlight;

    private Toggle toggle;
    private ExplorationMapJson mapData;
    private Action<int> OnSelectedCallback;

    public void Init(ExplorationMapJson data, ToggleGroup group, Action<int> selectCallback)
    {
        toggle = GetComponent<Toggle>();
        toggle.group = group;
        toggle.onValueChanged.AddListener(OnToggle);
        OnSelectedCallback = selectCallback;
        mapData = data;
        levelText.text = data.reqherolevel.ToString();
        nameText.text = data.localizedname;
        subNameText.text = data.localizedsubname;
        ongoingHighlight.SetActive(false);
        previousHighlight.SetActive(false);
    }

    public void SetAsOngoing()
    {
        ongoingHighlight.SetActive(true);
        previousHighlight.SetActive(false);
    }

    public void SetAsPredecessor()
    {
        ongoingHighlight.SetActive(false);
        previousHighlight.SetActive(true);
    }

    public void OnToggle(bool isOn)
    {
        if (isOn)
        {
            if (OnSelectedCallback != null)
                OnSelectedCallback(mapData.mapid);
        }
    }

    public void SetToggleOn(bool on)
    {
        toggle.isOn = on;
    }
}