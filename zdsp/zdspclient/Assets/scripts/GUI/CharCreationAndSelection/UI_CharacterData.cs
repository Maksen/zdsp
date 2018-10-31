using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_CharacterData : MonoBehaviour
{
    [SerializeField]
    Image JobIcon;

    [SerializeField]
    Text Name;

    [SerializeField]
    Sprite DefaultIcon;

    private CharacterCreationData mCharacterCreationData;
    private UI_CharacterSelection mParent;

    public void Init(CharacterCreationData creationData, ToggleGroup group, bool selected, UI_CharacterSelection parent)
    {
        mCharacterCreationData = creationData;
        mParent = parent;

        GetComponent<Toggle>().isOn = selected;
        GetComponent<Toggle>().group = group;
        Name.text = creationData == null ? GUILocalizationRepo.GetLocalizedString("csl_can_create") : creationData.Name;
        JobIcon.sprite = creationData == null ? DefaultIcon : ClientUtils.LoadIcon(JobSectRepo.GetJobPortraitPath((JobType)creationData.JobSect));
    }

    public void OnClickCharacter(bool value)
    {
        mParent.OnSelectedCharacter(mCharacterCreationData);
    }

    public void DisableToggle()
    {
        GetComponent<Toggle>().interactable = false;
    }

    public void EnableToggle()
    {
        GetComponent<Toggle>().interactable = true;
    }
}
