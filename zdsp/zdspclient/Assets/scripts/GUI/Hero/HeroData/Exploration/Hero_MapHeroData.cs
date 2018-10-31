using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapHeroData : MonoBehaviour
{
    [SerializeField] Image portraitIconImage;
    [SerializeField] Button button;

    private Image raycastImage;
    private int heroId;
    private int index;
    private Action<int> OnClickCallback;

    public void Setup(int index, Action<int> clickCallback)
    {
        this.index = index;
        button.onClick.AddListener(OnClick);
        OnClickCallback = clickCallback;
        raycastImage = button.GetComponent<Image>();
    }

    public void Init(int heroId, bool interactable)
    {
        this.heroId = heroId;
        portraitIconImage.gameObject.SetActive(heroId > 0);
        raycastImage.raycastTarget = interactable;
        HeroJson heroData = HeroRepo.GetHeroById(heroId);
        if (heroData != null && !string.IsNullOrEmpty(heroData.portraitpath))
            portraitIconImage.sprite = ClientUtils.LoadIcon(heroData.portraitpath);
    }

    public void Clear()
    {
        heroId = 0;
        portraitIconImage.gameObject.SetActive(false);
        raycastImage.raycastTarget = true;
    }

    public void OnClick()
    {
        if (OnClickCallback != null)
            OnClickCallback(index);
    }

    public int GetHeroId()
    {
        return heroId;
    }
}