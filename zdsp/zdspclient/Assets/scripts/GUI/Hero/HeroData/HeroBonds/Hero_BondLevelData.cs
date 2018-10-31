using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Hero_BondLevelData : MonoBehaviour
{
    [SerializeField] Text levelText;
    [SerializeField] Text[] reqmtText;
    [SerializeField] Text[] seText;
    [SerializeField] CanvasGroup canvasGroup;

    public void Init(HeroBondJson bondData, bool unlocked)
    {
        SetUnlocked(unlocked);
        levelText.text = bondData.bondlevel.ToString();
        reqmtText[0].text = GetBondRequirementText(bondData.bondtype1, bondData.bondvalue1);
        reqmtText[1].text = GetBondRequirementText(bondData.bondtype2, bondData.bondvalue2);

        seText[0].text = ""; // empty out the text first
        seText[1].text = ""; // empty out the text first

        int index = 0;
        foreach (SideEffectJson se in bondData.sideeffects.Values)
        {
            if (index < seText.Length)
            {
                if (se.isrelative)
                    seText[index++].text = string.Format("{0} +{1}%", SideEffectUtils.GetEffectTypeLocalizedName(se.effecttype), se.max);
                else
                    seText[index++].text = string.Format("{0} +{1}", SideEffectUtils.GetEffectTypeLocalizedName(se.effecttype), se.max);
            }
        }
    }

    public void SetUnlocked(bool unlocked)
    {
        canvasGroup.alpha = unlocked ? 1f : 0.5f;
    }

    private string GetBondRequirementText(HeroBondType bondType, int value)
    {
        string guiname = "";
        switch (bondType)
        {
            case HeroBondType.None:
                guiname = "";
                break;

            case HeroBondType.HeroLevel:
                guiname = "hro_bondreq_herolevel";
                break;

            case HeroBondType.HeroSkill:
                guiname = "hro_bondreq_heroskill";
                break;
        }

        if (!string.IsNullOrEmpty(guiname))
            return GUILocalizationRepo.GetLocalizedString(guiname) + GUILocalizationRepo.colon + value;
        return "";
    }
}