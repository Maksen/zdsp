using UnityEngine;
using UnityEngine.UI;

public class UI_HeroData : MonoBehaviour
{
    [SerializeField]
    Image Icon;

    private UI_DestinyQuest mParent;
    private int mHeroId;

    public void Init(string path, ToggleGroup toggleGroup, UI_DestinyQuest parent, int heroid, bool actived)
    {
        mParent = parent;
        mHeroId = heroid;
        ClientUtils.LoadIconAsync(path, UpdateIcon);
        GetComponent<Toggle>().group = toggleGroup;
        GetComponent<Toggle>().isOn = actived;
    }

    private void UpdateIcon(Sprite sprite)
    {
        Icon.sprite = sprite;
    }

    public void OnClicked()
    {
        mParent.OnHeroChanged(GetComponent<Toggle>().isOn ? mHeroId : -1);
    }
}
