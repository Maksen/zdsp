using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using Zealot.Common;
using Zealot.Repository;

public class UI_CharacterInfoLeftSide : MonoBehaviour
{
    [SerializeField]
    Text mName;
    [SerializeField]
    Text mLevel;
    [SerializeField]
    Image mJobImg;
    [SerializeField]
    Model_3DAvatar m3DAvatar;
    [SerializeField]
    Text mJobTitle;

    public string Name
    {
        set
        {
            mName.text = value;
        }
    }
    public int Level
    {
        set
        {
            value = Mathf.Max(1, value);
            mLevel.text = value.ToString();
        }
    }
    public int Job
    {
        set
        {
            //Load class icon depending on assigned job index
            mJobImg.sprite = LoadJobIcon(value);
            mJobTitle.text = JobSectRepo.GetJobLocalizedName((JobType)value);
        }
    }

    public void OnEnable()
    {
        SetField();
    }

    /// <summary>
    /// Call this when character info window has a popup and now return back to focus
    /// </summary>
    public void OnRegainWindowContext()
    {
        SetField();
    }

    private void SetField()
    {
        if (GameInfo.gLocalPlayer == null)
            return;

        var pss = GameInfo.gLocalPlayer.PlayerSynStats;
        Name = pss.name;
        Level = pss.Level;
        Job = GameInfo.gLocalPlayer.PlayerSynStats.jobsect;
        m3DAvatar.Change(GameInfo.gLocalPlayer.mEquipmentInvData,
                        (JobType)pss.jobsect, 
                        GameInfo.gLocalPlayer.mGender);
        m3DAvatar.gameObject.SetActive(true);
    }

    private Sprite LoadJobIcon(int job)
    {
        JobType jt = (JobType)job;

        switch (jt)
        {
            case JobType.Alchemist:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_alchemist.png");
            case JobType.Assassin:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_assassin.png");
            case JobType.BladeMaster:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_blademaster.png");
            case JobType.Commando:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_commando.png");
            case JobType.Executioner:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_executioner.png");
            case JobType.General:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_general.png");
            case JobType.Killer:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_killer.png");
            case JobType.Lieutenant:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_lieutenant.png");
            case JobType.Newbie:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_newbie.png");
            case JobType.QigongMaster:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_qigongmaster.png");
            case JobType.Samurai:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_samurai.png");
            case JobType.Schemer:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_schemer.png");
            case JobType.Shadow:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_shadow.png");
            case JobType.Slaughter:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_slaughter.png");
            case JobType.Soldier:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_soldier.png");
            case JobType.SpecialForce:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_specialforce.png");
            case JobType.Strategist:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_strategist.png");
            case JobType.SwordMaster:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_swordmaster.png");
            case JobType.Swordsman:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_swordman.png");
            case JobType.Tactician:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_tactician.png");
            case JobType.Warrior:
                return ClientUtils.LoadIcon("UI_ZDSP_Icons/Job/job_alchemist.png");
            case JobType.Tutorial:
            default:
                return null;
        }
    }
}
