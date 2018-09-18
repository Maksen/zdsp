using Kopio.JsonContracts;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapTargetData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text nameText;
    [SerializeField] Text noteText;
    [SerializeField] Button button;

    private int targetId;
    private Action<int> OnSelectCallback;

    public void Init(ExplorationTargetJson data, int reqLevel, Action<int> selectCallback)
    {
        if (data == null)
        {
            targetId = 0;
            //todo: junming set icon for all target
            iconImage.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Portraits/zzz_Test.png");
            nameText.text = GUILocalizationRepo.GetLocalizedString("hro_explore_all");
            noteText.text = "";
        }
        else
        {
            targetId = data.id;
            iconImage.sprite = ClientUtils.LoadIcon(data.iconpath);
            nameText.text = data.localizedname;
            noteText.text = "";
            bool canSelect = GameInfo.gLocalPlayer.PlayerSynStats.vipLvl >= reqLevel; // to revisit after achievement done
            if (!canSelect)
            {
                noteText.text = GUILocalizationRepo.GetLocalizedString("hro_explore_monster_requirement") + GUILocalizationRepo.colon + reqLevel;
                noteText.color = Color.red;
                button.interactable = false;
            }
        }
        OnSelectCallback = selectCallback;
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (OnSelectCallback != null)
            OnSelectCallback(targetId);
    }
}