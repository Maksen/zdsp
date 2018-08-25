using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Hero_MapRequirementData : MonoBehaviour
{
    [SerializeField] Image reqmtImage;
    [SerializeField] Text reqmtValueText;
    [SerializeField] Text descriptionText;

    public void Init(ChestRequirementType type, int value)
    {
        gameObject.SetActive(type != ChestRequirementType.None);
        switch (type)
        {
            case ChestRequirementType.HeroID:
                HeroJson heroData = HeroRepo.GetHeroById(value);
                if (heroData != null)
                    reqmtImage.sprite = ClientUtils.LoadIcon(heroData.smallportraitpath);
                reqmtValueText.gameObject.SetActive(false);
                descriptionText.text = heroData.localizedname;
                break;
            case ChestRequirementType.HeroInterest:
                HeroInterestJson interestData = HeroRepo.GetInterestByType((HeroInterestType)value);
                if (interestData != null)
                    ClientUtils.LoadIconAsync(interestData.iconpath, OnImageLoaded);
                reqmtValueText.gameObject.SetActive(false);
                descriptionText.text = interestData.localizedname;
                break;
            case ChestRequirementType.HeroTrust:
                reqmtImage.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Portraits/zzz_Test.png");  // temp
                reqmtValueText.gameObject.SetActive(true);
                reqmtValueText.text = value.ToString();
                descriptionText.text = GUILocalizationRepo.GetLocalizedString("hro_relationship") + " " + value;
                break;
        }
    }

    private void OnImageLoaded(Sprite sprite)
    {
        reqmtImage.sprite = sprite;
    }
}