using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

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
        this.tier = tier;
        toggle = GetComponent<Toggle>();
        toggle.group = toggleGrp;
        toggle.onValueChanged.AddListener(OnToggled);
        OnSelectedCallback = callback;
        origTextColor = unlockValueText.color;
        gameObject.SetActive(false);
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
                    case 1:
                        imagePath = heroJson.t1imagepath;
                        break;
                    case 2:
                        imagePath = heroJson.t2imagepath;
                        break;
                    case 3:
                        imagePath = heroJson.t3imagepath;
                        break;
                }
                ClientUtils.LoadIconAsync(imagePath, OnImageLoaded);
            }
            else
                gameObject.SetActive(false);
            unlockValueText.text = reqPts.ToString();
        }
        modelImage.material = unlocked ? null : grayscaleMat;
        SetTextColor(unlocked ? origTextColor : Color.red);
    }

    public void InitSkinItem(bool unlocked)
    {
        gameObject.SetActive(true);
        HeroItem skinItem = GameRepo.ItemFactory.GetInventoryItem(tier) as HeroItem;
        if (skinItem != null)
            ClientUtils.LoadIconAsync(skinItem.HeroItemJson.heroimagepath, OnImageLoaded);

        unlockValueText.text = "ItemId " + tier;  // temp

        modelImage.material = unlocked ? null : grayscaleMat;
        SetTextColor(unlocked ? origTextColor : Color.red);
    }

    private void SetTextColor(Color color)
    {
        if (unlockValueText.color != color)
        {
            Transform parent = unlockValueText.transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Text childText = parent.GetChild(i).GetComponent<Text>();
                childText.color = color;
            }
        }
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