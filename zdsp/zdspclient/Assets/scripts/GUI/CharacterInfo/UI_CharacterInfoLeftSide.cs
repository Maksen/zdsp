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
            //mJobImg.sprite = ;
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
}
