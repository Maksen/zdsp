using UnityEngine;
using UnityEngine.UI;

public class UI_DestinyHeroData : MonoBehaviour
{
    [SerializeField]
    Image HeroIcon;

    [SerializeField]
    Toggle HeroToggle;

    private int mHeroid;
    private UI_HistoryFilter mParent;

    public void Init(int heroid, string path, ToggleGroup group, UI_HistoryFilter parent)
    {
        mHeroid = heroid;
        mParent = parent;
        ClientUtils.LoadIconAsync(path, UpdateIcon);
        HeroToggle.group = group;
    }

    private void UpdateIcon(Sprite sprite)
    {
        HeroIcon.sprite = sprite;
    }

    public void OnValueChanged()
    {
        mParent.UpdateSelectedHero(HeroToggle.isOn ? mHeroid : -1);
    }
}
