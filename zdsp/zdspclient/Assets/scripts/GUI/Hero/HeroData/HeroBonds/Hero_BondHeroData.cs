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

    private int heroId;
    private Toggle toggle;
    private Action<int, bool> OnSelectedCallback;
    private Action<int> OnClickCallback;

    public void Init(int heroId, ToggleGroup group, Action<int, bool> selectedCallback)
    {
        this.heroId = heroId;
        toggle = GetComponent<Toggle>();
        toggle.group = group;
        OnSelectedCallback = selectedCallback;
        toggle.onValueChanged.AddListener(OnToggled);
        HeroJson data = HeroRepo.GetHeroById(heroId);
        if (data != null)
            heroImage.sprite = ClientUtils.LoadIcon(data.portraitpath);
    }

    public void Init(int heroId, Action<int> clickCallback)
    {
        this.heroId = heroId;
        Button button = GetComponent<Button>();
        OnClickCallback = clickCallback;
        button.onClick.AddListener(OnClick);
        HeroJson data = HeroRepo.GetHeroById(heroId);
        if (data != null)
            heroImage.sprite = ClientUtils.LoadIcon(data.portraitpath);
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
        if (OnSelectedCallback != null)
            OnSelectedCallback(heroId, isOn);
    }

    public bool IsToggleOn()
    {
        return toggle.isOn;
    }

    public void SetToggleOn(bool value)
    {
        toggle.isOn = value;
    }

    public void OnClick()
    {
        if (OnClickCallback != null)
            OnClickCallback(heroId);
    }
}