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
    public enum SkillNo
    {
        BasicAttack = 0,
        Skill1 = 1,
        Skill2 = 2,
        Skill3 = 3,
        Skill4 = 4
    }

    public HUD_BasicAttackBtn BasicAttackButton;
    public HUD_SkillBtn[] SkillButtons;
    public Animator[] SkillCDEnd;
    public Toggle AutoCombatToggle;
    private List<GameObject> ButtonLocks;
    private GameObject BasicAttackButtonLock;

    private PlayerGhost localplayer;
    private float lastWeaponPromptTime = -99999;

    public void Init()
    {
        BasicAttackButton.Init();
        InitSkillButtons();

        localplayer = GameInfo.gLocalPlayer;
        lastWeaponPromptTime = -99999;
    }

    public void SetBasicAttackButtonImage(Sprite sprite)
    {
        BasicAttackButton.SetButtonImage(sprite);
    }
    
    private void InitSkillButtons()
    {
        Transform ptrans = BasicAttackButton.gameObject.transform.Find("Image_Lock");
        if (ptrans != null)
            BasicAttackButtonLock = ptrans.gameObject;
        ButtonLocks = new List<GameObject>();
        for (int i = 0; i < SkillButtons.Length; i++)
        {
            int skillno = i + 1;
            SkillButtons[i].Init(() => CastSkill(skillno), i, this);
            Transform imgtrans = SkillButtons[i].gameObject.transform.Find("Image_Lock");
            if (imgtrans != null)
                ButtonLocks.Add(imgtrans.gameObject);
            else
                ButtonLocks.Add(null);
        }
    }

    public void UpdateLockStatus(int[] list, bool lockbasicattack)
    {
        if (BasicAttackButtonLock != null)
        {
            BasicAttackButtonLock.SetActive(false);
        }
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
        if (lockbasicattack && BasicAttackButtonLock != null)
        {
            BasicAttackButtonLock.SetActive(true);
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

    public void CastSkill(int number)
    {      
        int skillid = 0;
        SkillNo skillNo = SkillNo.BasicAttack; 

        switch (number)
        {
            case 1:
                skillid = localplayer.SkillStats.JobskillAttackSId;
                skillNo = SkillNo.Skill1;
                break;
            //case 2:
            //    skillid = localplayer.SkillStats.RedHeroCardSkillAttackSId;
            //    skillNo = SkillNo.Skill2;
            //    break;
            //case 3:
            //    skillid = localplayer.SkillStats.GreenHeroCardSkillAttackSId;
            //    skillNo = SkillNo.Skill3;
            //    break;
            //case 4:
            //    skillid = localplayer.SkillStats.BlueHeroCardSkillAttackSId;
            //    skillNo = SkillNo.Skill4;
            //    break;
            //case 5:
            //    break;
        }
        
        if (skillid > 0)
        {
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
        SkillButtons[cooldownIndex].StartCooldown(finalcd);
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

     
    public void OnAutoCombatToggle(bool toggle)
    {
        if (toggle)
            GameInfo.gLocalPlayer.Bot.StartBot();
        else
            GameInfo.gLocalPlayer.Bot.StopBot();
    }

    public void OnHudSkillToggle(bool toggle)
    {
        //if (GameInfo.gLocalPlayer.QuestStats.isTraining)
        {
            TrainingRealmContoller.Instance.HideHighlightDialog(Trainingstep.KillMonster);
        }
        
    }

    public void OnBotStart()
    {
        if (!AutoCombatToggle.isOn)
        {
            AutoCombatToggle.isOn = true;
        }
    }

    public void OnBotStop()
    {
        if (AutoCombatToggle.isOn)
        {
            AutoCombatToggle.isOn = false;
        }
    }
}
