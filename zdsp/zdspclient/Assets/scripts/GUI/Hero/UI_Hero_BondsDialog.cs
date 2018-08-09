using UnityEngine;
using UnityEngine.UI;

public class UI_Hero_BondsDialog : MonoBehaviour
{
    [SerializeField] Transform bondDataParent;
    [SerializeField] GameObject bondDataPrefab;
    [SerializeField] GameObject[] pageToggles;
    [SerializeField] Transform buffDataParent;
    [SerializeField] GameObject buffDataPrefab;
    [SerializeField] ScrollRect buffDataScrollRect;

    [Header("Sub Panels")]
    [SerializeField] UI_Hero_LockedPanel lockedPanel;
    [SerializeField] UI_Hero_UnlockedPanel unlockedPanel;

    private Hero hero;

    public void Init(Hero hero)
    {
        this.hero = hero;
    }
}
