using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Hero_ModelTierData : MonoBehaviour
{
    [SerializeField] Image modelImage;
    [SerializeField] Text unlockValueText;
    [SerializeField] Material grayscaleMat;

    private Toggle toggle;
    private Action<int> OnSelectedCallback;
    private int tier;
    private int heroId;
    private Color origTextColor;

    public void Setup(int tier, ToggleGroup toggleGrp, Action<int> callback)
    {
        toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        toggle.onValueChanged.AddListener(OnToggled);
        OnSelectedCallback = callback;
        this.tier = tier;
        switch (tier)
        {
            case 1: unlockValueText.text = HeroData.TIER1_UNLOCK.ToString(); break;
            case 2: unlockValueText.text = HeroData.TIER2_UNLOCK.ToString(); break;
            case 3: unlockValueText.text = HeroData.TIER3_UNLOCK.ToString(); break;
        }
        origTextColor = unlockValueText.color;
    }

    public void Init(HeroJson data, bool unlocked)
    {
        if (heroId != data.heroid)
        {
            heroId = data.heroid;
            switch (tier)
            {
                case 1: ClientUtils.LoadIconAsync(data.t1imagepath, OnImageLoaded); break;
                case 2: ClientUtils.LoadIconAsync(data.t2imagepath, OnImageLoaded); break;
                case 3: ClientUtils.LoadIconAsync(data.t3imagepath, OnImageLoaded); break;
            }
        }
        modelImage.material = unlocked ? null : grayscaleMat;
        unlockValueText.color = unlocked ? origTextColor : Color.red;
    }

    private void OnImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            modelImage.sprite = sprite;
    }

    private void OnToggled(bool isOn)
    {
        if (isOn)
        {
            if (OnSelectedCallback != null)
                OnSelectedCallback(tier);
        }
    }

    public void SetToggleOn(bool value)
    {
        toggle.isOn = value;
    }
}