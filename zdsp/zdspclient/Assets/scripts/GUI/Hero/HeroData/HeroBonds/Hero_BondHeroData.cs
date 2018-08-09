using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_BondHeroData : MonoBehaviour
{
    [SerializeField] Image heroImage;
    [SerializeField] Text statusText;
    [SerializeField] Material grayscaleMat;

    private Action<int> OnSelectedCallback;
    private int heroId;
    private Toggle toggle;

    public void Setup(ToggleGroup group, Action<int> selectedCallback)
    {
        toggle = GetComponent<Toggle>();
        toggle.group = group;
        OnSelectedCallback = selectedCallback;
    }

    public void Init(int heroId, bool fulfilled)
    {
        this.heroId = heroId;
        HeroJson data = HeroRepo.GetHeroById(heroId);
        if (data != null)
            heroImage.sprite = ClientUtils.LoadIcon(data.portraitpath);
        SetFulfilled(fulfilled);
    }

    public void SetFulfilled(bool fulfilled)
    {
        if (fulfilled)
        {
            statusText.text = GUILocalizationRepo.GetLocalizedString("hro_bond_fulfilled");
            statusText.color = Color.white;
            heroImage.material = null;
        }
        else
        {
            statusText.text = GUILocalizationRepo.GetLocalizedString("hro_bond_unfulfilled");
            statusText.color = Color.red;
            heroImage.material = grayscaleMat;
        }
    }

    public void OnToggled(bool isOn)
    {
        if (isOn)
        {
            if (OnSelectedCallback != null)
                OnSelectedCallback(heroId);
        }
    }

    public void SetToggleOn(bool value)
    {
        toggle.isOn = value;
    }
}