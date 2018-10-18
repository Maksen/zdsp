using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

using Zealot.Common.Actions;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.Common.Entities;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;
using System.Collections.Generic;

public class HUD_Skills : MonoBehaviour
{
    //public HUD_BasicAttackBtn BasicAttackButton;
    public HUD_SkillBtn[] SkillButtons;
    public Animator[] SkillCDEnd;
    public Toggle AutoCombatToggle;
    private List<GameObject> ButtonLocks;

    private PlayerGhost localplayer;
    //private float lastWeaponPromptTime = -99999;

    public void Init()
    {
        //BasicAttackButton.Init();
        localplayer = GameInfo.gLocalPlayer;
        InitSkillButtons();
        
        //lastWeaponPromptTime = -99999;
    }

    //public void SetBasicAttackButtonImage(Sprite sprite)
    //{
    //    BasicAttackButton.SetButtonImage(sprite);
    //}
    
    public void InitSkillButtons()
    {
        //Transform ptrans = BasicAttackButton.gameObject.transform.Find("Image_Lock");
        //if (ptrans != null)
        //    BasicAttackButtonLock = ptrans.gameObject;
        localplayer = GameInfo.gLocalPlayer;
        //ButtonLocks = new List<GameObject>();
        //Zealot.Common.Datablock.CollectionHandler<object> skillequipped;
        
        //skillequipped = localplayer.SkillStats.EquippedSkill;
        //int group = localplayer.SkillStats.EquipGroup;


        for (int i = 0; i < SkillButtons.Length - 1; i++)
        {
            SkillButtons[i].Init(CastSkill, i, this);
            //Transform imgtrans = SkillButtons[i].gameObject.transform.Find("Image_Lock");
            //if (imgtrans != null)
            //    ButtonLocks.Add(imgtrans.gameObject);
            //else
            //    ButtonLocks.Add(null);
        }
    }

    public void UpdateSkillButtons(bool isBot)
    {
        Zealot.Common.Datablock.CollectionHandler<object> skillequipped;
        
        skillequipped = isBot ? localplayer.SkillStats.AutoSkill : localplayer.SkillStats.EquippedSkill;
        int group = localplayer.SkillStats.EquipGroup;
        for (int i = 0; i < SkillButtons.Length; i++)
        {
            int skillno = (int)skillequipped[(5 * (group - 1)) + i];
            SkillButtons[i].OnSkillUpdated(skillno);
            if (skillno == 0)
            {
                SkillButtons[i].SetEmptySkill();
                continue;
            }
            SkillData skd = SkillRepo.GetSkill(skillno);
            SkillButtons[i].UpdateSprite(skd.skillgroupJson.icon);
        }
    }

    public void UpdateLockStatus(int[] list, bool lockbasicattack)
    {

        for (int i = 0; i < ButtonLocks.Count; i++)
        {
            ButtonLocks[i].SetActive(false);
        }
        foreach (int idx in list)
        {
            if (ButtonLocks[idx] != null && SkillButtons[idx].HasSkill)
            {
                ButtonLocks[idx].SetActive(true);
            }
        }
    }

    public void SetSkillImage(int index, Sprite sprite)
    {
        if (index < SkillButtons.Length)
            SkillButtons[index].SetSkillImage(sprite);
    }

    public void PlaySkillCDFlare(int index)
    {
        if (index < SkillCDEnd.Length)
            SkillCDEnd[index].Play("SkillCDFlare");
    }

    public void CastSkill(int skillid)
    {      
        if (skillid > 0)
        {
            //SkillData skill = SkillRepo.GetSkill(skillid);
            //GameTimer timer = null;
            GameInfo.gCombat.TryCastActiveSkill(skillid);
            localplayer.ActionInterupted();
        }
    }

    public void AddActiveSkillCooldown(int cooldownIndex, SkillJson skgp)
    {
        PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
        float now = Time.time;
        cdstate.mCDStart[cooldownIndex] = now;
        float finalcd = skgp.cooldown;
        finalcd = CombatUtils.GetFinalSkillCooldown(GameInfo.gLocalPlayer.SkillPassiveStats, cooldownIndex, finalcd);
        cdstate.mCDEnd[cooldownIndex] = now + finalcd;

        for(int i = 0; i < SkillButtons.Length - 1; ++i)
        {
            if (SkillButtons[i].skillid == cooldownIndex)
            {
                SkillButtons[i].StartCooldown(finalcd);
                break;
            }
        }
        
        //if (skilldata.skillgroupJson.globalcooldown > 0)//no global
        //if (false)
        //{
        //    cdstate.mGCDStart = now;
        //    cdstate.mGCDEnd = now + skgp.globalcooldown;
        //    for (int i = 0; i < SkillButtons.Length; i++)
        //    {
        //        if (i == cooldownIndex)
        //            continue;
        //        if (!cdstate.IsCoolingDown(i))
        //            SkillButtons[i].StartCooldown(skgp.globalcooldown);
        //    }
        //}
    }

    public void StopCooldown(int skillindex)
    {
        SkillButtons[skillindex].StopCooldown();
    }

    public void ChangeCooldown(int skillindex, float amount)
    {
        if(GameInfo.gSkillCDState.IsSkillCoolingDown(skillindex))
            SkillButtons[skillindex].IncreaseCooldown(amount);
    }

     
    public void OnAutoCombatToggle()
    {
        if (AutoCombatToggle.isOn)
            GameInfo.gLocalPlayer.Bot.StartBot();
        else
            GameInfo.gLocalPlayer.Bot.StopBot();

        UpdateSkillButtons(AutoCombatToggle.isOn);
    }

    public void OnHudSkillToggle(bool toggle)
    {
        //if (GameInfo.gLocalPlayer.QuestStats.isTraining)
        {
            TrainingRealmContoller.Instance.HideHighlightDialog(Trainingstep.KillMonster);
        }
        
    }

    /// <summary>
    /// Turn on the Bot HUD
    /// </summary>
    public void OnBotStart()
    {
        if (!AutoCombatToggle.isOn)
        {
            AutoCombatToggle.isOn = true;
        }
    }
    /// <summary>
    /// Turn off the Bot HUD
    /// </summary>
    public void OnBotStop()
    {
        if (AutoCombatToggle.isOn)
        {
            AutoCombatToggle.isOn = false;
        }
    }
}
