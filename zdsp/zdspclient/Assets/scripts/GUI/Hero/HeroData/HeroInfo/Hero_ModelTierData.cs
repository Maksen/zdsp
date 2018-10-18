using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Hero_ModelTierData : MonoBehaviour
{
    [SerializeField] Image modelImage;
    [SerializeField] Text unlockText;
    [SerializeField] string lockedColorHex;

    private Toggle toggle;
    private Action<int> OnSelectedCallback;
    private int tier;
    private int heroId;
    private Color origTextColor;
    private Color lockedColor;

    public void Setup(int tier, ToggleGroup toggleGrp, Action<int> callback)
    {
        this.tier = tier;
        toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        OnSelectedCallback = callback;
        origTextColor = unlockText.color;
        gameObject.SetActive(false);
        ColorUtility.TryParseHtmlString(lockedColorHex, out lockedColor);
    }

    public void EnableToggleCallback(bool value)
    {
        if (value)
            toggle.onValueChanged.AddListener(OnToggled);
        else
            toggle.onValueChanged.RemoveListener(OnToggled);
    }

    public void Init(HeroJson heroJson, int reqPts, bool unlocked)
    {
        if (heroId != heroJson.heroid)
        {
            heroId = heroJson.heroid;
            if (reqPts > 0)
            {
                gameObject.SetActive(true);
                string imagePath = "";
                switch (tier)
                {
                    case 1: imagePath = heroJson.t1imagepath; break;
                    case 2: imagePath = heroJson.t2imagepath; break;
                    case 3: imagePath = heroJson.t3imagepath; break;
                }
                modelImage.sprite = ClientUtils.LoadIcon(imagePath);
                unlockText.text = GUILocalizationRepo.GetLocalizedString("hro_unlock_skin_requirement") + reqPts;
            }
            else
                gameObject.SetActive(false);
        }
        modelImage.color = unlocked ? Color.white : lockedColor;
        unlockText.color = unlocked ? origTextColor : Color.red;
    }

    public void InitSkinItem(bool unlocked)
    {
        gameObject.SetActive(true);
        HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
        if (skinItem != null)
        {
            modelImage.sprite = ClientUtils.LoadIcon(skinItem.HeroItemJson.heroimagepath);
            unlockText.text = skinItem.HeroItemJson.localizedname;
        }
        modelImage.color = unlocked ? Color.white : lockedColor;
        unlockText.color = unlocked ? origTextColor : Color.red;
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