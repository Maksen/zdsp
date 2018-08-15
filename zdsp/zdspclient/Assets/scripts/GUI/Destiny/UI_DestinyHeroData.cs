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
        HeroIcon.sprite = ClientUtils.LoadIcon(path);
        HeroToggle.group = group;
    }

    public void OnValueChanged()
    {
        mParent.UpdateSelectedHero(HeroToggle.isOn ? mHeroid : -1);
    }
}
