using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class HUD_SelectTarget : MonoBehaviour
{
    [SerializeField] Image portraitImage;
    [SerializeField] Text levelText;
    [SerializeField] UI_ProgressBarC progressBar;
    [SerializeField] Text nameText;
    [SerializeField] Image elementImage;
    [SerializeField] Image weaknessImage;
    [SerializeField] Transform buffDebuffTransform;
    [SerializeField] GameObject buffDebuffGameIcon;
    [SerializeField] Transform portraitFuncTransform;
    [SerializeField] GameObject portraitFuncPrefab;

    private BaseClientEntity thisEntity;
    private HUD_PortraitFunctions portraitFunc;
    private Dictionary<BuffEnum, GameIcon_BuffDebuff> buffDebuffList = new Dictionary<BuffEnum, GameIcon_BuffDebuff>();

    private void Awake()
    {
        for (BuffEnum e = 0; e < BuffEnum.Max_BuffType; e++)
        {
            GameObject obj = ClientUtils.CreateChild(buffDebuffTransform, buffDebuffGameIcon);
            GameIcon_BuffDebuff icon = obj.GetComponent<GameIcon_BuffDebuff>();
            obj.SetActive(false);
            buffDebuffList.Add(e, icon);
        }
    }

    public void InitSelectTarget(BaseClientEntity entity)
    {
        thisEntity = entity;
        ClearAllIcons();

        nameText.text = entity.Name;
        levelText.text = entity.GetDisplayLevel().ToString();

        if (entity.IsPlayer())
        {
            PlayerGhost player = entity as PlayerGhost;
            PlayerSynStats playerStats = player.PlayerSynStats;
            SetTargetHeath(playerStats.DisplayHp);
            SetPortrait(JobSectRepo.GetJobPortraitPath((JobType)playerStats.jobsect));

            // testing
            //SetBuffDebuffIcon(BuffEnum.Buff, 5000, 10);
            //SetBuffDebuffIcon(BuffEnum.Debuff, 0, 99);
        }
        else if (entity.IsMonster())
        {
            MonsterGhost monster = entity as MonsterGhost;
            CombatNPCJson npcJson = monster.mArchetype;
            SetTargetHeath(monster.PlayerStats.DisplayHp);
            SetMonsterElement(npcJson.element);
            SetMonsterWeakness(npcJson.weakness);
            SetPortrait(npcJson.portraitpath);

            // testing
            //SetBuffDebuffIcon(BuffEnum.Buff, 5000, 10);
            //SetBuffDebuffIcon(BuffEnum.Debuff, 0, 99);
        }
        else if (entity.IsNPC())
        {
            StaticClientNPCAlwaysShow npc = entity as StaticClientNPCAlwaysShow;
            SetTargetHeath(1f);
            SetPortrait(npc.mArchetype.portraitpath);
        }
        else if (entity.IsHero())
        {
            HeroGhost hero = entity as HeroGhost;
            HeroJson heroJson = hero.mHeroJson;
            SetTargetHeath(1f);
            SetMonsterElement(heroJson.element);
            SetPortrait(heroJson.smallportraitpath);
        }
    }

    private void ClearAllIcons()
    {
        ClearTopIcons();
        ClearBuffDebuff();
    }

    public void SetTargetHeath(float hp)
    {
        progressBar.Value = (long)(hp * progressBar.Max);  // max is 100
    }

    private void ClearBuffDebuff()
    {
        foreach (var icon in buffDebuffList.Values)
            icon.gameObject.SetActive(false);
    }

    private void ClearTopIcons()
    {
        elementImage.transform.parent.gameObject.SetActive(false);
        weaknessImage.transform.parent.gameObject.SetActive(false);
    }

    private void SetPortrait(string path)
    {
        portraitImage.sprite = ClientUtils.LoadIcon(path);
    }

    private void SetMonsterElement(Element element)
    {
        Sprite icon = LoadMonsterElementIcon(element);
        if (icon != null)
        {
            elementImage.sprite = icon;
            elementImage.transform.parent.gameObject.SetActive(true);
        }
    }

    private void SetMonsterWeakness(AttackStyle weakness)
    {
        Sprite icon = LoadMonsterWeaknessIcon(weakness);
        if (icon != null)
        {
            weaknessImage.sprite = icon;
            weaknessImage.transform.parent.gameObject.SetActive(true);
        }
    }

    private Sprite LoadMonsterElementIcon(Element element)
    {
        string path = "";
        switch (element)
        {
            case Element.Metal:
                path = "zzz_test.png";
                break;
            case Element.Wood:
                path = "zzz_test.png";
                break;
            case Element.Water:
                path = "zzz_test.png";
                break;
            case Element.Fire:
                path = "zzz_test.png";
                break;
            case Element.Earth:
                path = "zzz_test.png";
                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(path))
            return ClientUtils.LoadIcon(string.Format("UI_ZDSP_Icons/Element_Attacks/{0}", path));

        return null;
    }

    private Sprite LoadMonsterWeaknessIcon(AttackStyle weakness)
    {
        string path = "";
        switch (weakness)
        {
            case AttackStyle.Pierce:
                path = "zzz_test.png";
                break;
            case AttackStyle.Slice:
                path = "zzz_test.png";
                break;
            case AttackStyle.Smash:
                path = "zzz_test.png";
                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(path))
            return ClientUtils.LoadIcon(string.Format("UI_ZDSP_Icons/Element_Attacks/{0}", path));

        return null;
    }

    private void SetBuffDebuffIcon(BuffEnum eff, long timeleft, int amt)
    {
        GameIcon_BuffDebuff icon;
        if (buffDebuffList.TryGetValue(eff, out icon))
            icon.Init(eff, timeleft, amt);
    }

    public void OnClickPortrait()
    {
        if (thisEntity.IsPlayer())
        {
            if (portraitFunc == null)
            {
                GameObject obj = ClientUtils.CreateChild(portraitFuncTransform, portraitFuncPrefab);
                portraitFunc = obj.GetComponent<HUD_PortraitFunctions>();
            }

            PlayerGhost player = thisEntity as PlayerGhost;
            portraitFunc.Init(player.Name, player.PlayerSynStats.guildName);
        }
    }
}
