using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapHeroData : MonoBehaviour
{
    [SerializeField] GameObject plusIconObj;
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
        plusIconObj.SetActive(heroId == 0);
        portraitIconImage.gameObject.SetActive(heroId > 0);
        raycastImage.raycastTarget = interactable;
        HeroJson heroData = HeroRepo.GetHeroById(heroId);
        if (heroData != null && !string.IsNullOrEmpty(heroData.smallportraitpath))
            portraitIconImage.sprite = ClientUtils.LoadIcon(heroData.smallportraitpath);
    }

    public void Clear()
    {
        heroId = 0;
        plusIconObj.SetActive(true);
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