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
    [SerializeField] Material grayscaleMat;

    private Toggle toggle;
    private Action<int> OnSelectedCallback;
    private int tier;
    private int heroId;
    private Color origTextColor;

    public void Setup(int tier, ToggleGroup toggleGrp, Action<int> callback)
    {
        this.tier = tier;
        toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        OnSelectedCallback = callback;
        origTextColor = unlockText.color;
        gameObject.SetActive(false);
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
                ClientUtils.LoadIconAsync(imagePath, OnImageLoaded);
                unlockText.text = GUILocalizationRepo.GetLocalizedString("hro_unlock_skin_requirement") + reqPts;
            }
            else
                gameObject.SetActive(false);
        }
        modelImage.material = unlocked ? null : grayscaleMat;
        unlockText.color = unlocked ? origTextColor : Color.red;
    }

    public void InitSkinItem(bool unlocked)
    {
        gameObject.SetActive(true);
        HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
        if (skinItem != null)
        {
            ClientUtils.LoadIconAsync(skinItem.HeroItemJson.heroimagepath, OnImageLoaded);
            unlockText.text = GUILocalizationRepo.GetLocalizedString("id_useitem") + skinItem.HeroItemJson.localizedname;
        }
        modelImage.material = unlocked ? null : grayscaleMat;
        unlockText.color = unlocked ? origTextColor : Color.red;
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