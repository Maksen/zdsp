using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapHeroData : MonoBehaviour
{
    [SerializeField] GameObject plusIconObj;
    [SerializeField] Image portraitIconImage;

    private int heroId;

    public void Init(int heroId)
    {
        this.heroId = heroId;
        plusIconObj.SetActive(heroId <= 0);
        portraitIconImage.gameObject.SetActive(heroId > 0);
        if (heroId > 0)
        {
            HeroJson heroData = HeroRepo.GetHeroById(heroId);
            if (heroData != null && !string.IsNullOrEmpty(heroData.smallportraitpath))
                portraitIconImage.sprite = ClientUtils.LoadIcon(heroData.smallportraitpath);
        }
    }

    public void OnClick()
    {
        UIManager.OpenDialog(WindowType.DialogHeroList);  // on click init
    }

    public int GetHeroId()
    {
        return heroId;
    }
}