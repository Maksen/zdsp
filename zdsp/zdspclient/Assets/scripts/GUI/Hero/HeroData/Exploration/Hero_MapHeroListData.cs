using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapHeroListData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text nameText;
    [SerializeField] Text noteText;
    [SerializeField] Button button;

    private int heroId;
    private Action<int> OnSelectCallback;

	public void Init(Hero hero, Action<int> selectCallback, ExplorationMapJson mapData)
    {
        heroId = hero.HeroId;
        OnSelectCallback = selectCallback;
        button.onClick.AddListener(OnClick);

        iconImage.sprite = ClientUtils.LoadIcon(hero.HeroJson.portraitpath);
        nameText.text = hero.HeroJson.localizedname;

        bool canSelect = true;
        string notes = "";
        if (hero.Level < mapData.reqherolevel)
        {
            notes = GUILocalizationRepo.GetLocalizedString("hro_explore_level_requirement") + GUILocalizationRepo.colon + mapData.reqherolevel;
            canSelect = false;
        }
        else if (hero.TrustLevel < mapData.reqherotrust)
        {
            notes = GUILocalizationRepo.GetLocalizedString("hro_explore_trust_requirement") + GUILocalizationRepo.colon + mapData.reqherotrust;
            canSelect = false;
        }
        else
            notes = GUILocalizationRepo.GetLocalizedString("hro_meet_map_requirement") + GUILocalizationRepo.colon + hero.GetNoOfMapCriteriaMet(mapData);
        noteText.text = notes;
        if (!canSelect)
        {
            noteText.color = Color.red;
            button.interactable = false;
        }
    }

    public void OnClick()
    {
        if (OnSelectCallback != null)
            OnSelectCallback(heroId);
    }
}
