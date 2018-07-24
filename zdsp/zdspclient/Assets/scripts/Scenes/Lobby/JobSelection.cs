using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;

public class JobSelection : MonoBehaviour {
	GameObject loadedJobModel;

	private string charName;
	private int jobSelected;

    public GameObject jobDescriptionImage;
    public GameObject firstToggleButtn;
    public GameObject toggleJobType2;
    public GameObject toggleJobType3;
    public GameObject toggleJobType4;
    public Text txtJobDescription;
    public GameObject inputField;

    private GameObject toggledBtn;
    private Sprite[] uiJobDescriptSprites;

    // Use this for initialization
    void Start () {
        GetJobDescription();
        InitJobSelection ();
		OnJobClick (null);
	}

    void GetJobDescription()
    {
        UI_DynamicSprites[] uiDynamicSprites = GameObject.Find("UI_JobCreation").GetComponents<UI_DynamicSprites>();
        if (uiDynamicSprites.Length > 0)
        {
            foreach (UI_DynamicSprites ds in uiDynamicSprites)
            {
                if (ds.Name == "JobDescription")
                {
                    uiJobDescriptSprites = ds.SpriteList;
                }
            }
        }
    }

    void InitJobSelection()
	{
	}

	int GetBtnIndex(string btnname)
	{
		switch(btnname)
		{
			case "Toggle_JobType_1":
			return 1;
			case "Toggle_JobType_2":
			return 2;
			case "Toggle_JobType_3":
			return 3;
			case "Toggle_JobType_4":
			return 4;
		}
		return -1;
	}
    	
	public void OnJobClick(GameObject uibutton)
	{
        if (uibutton != null)
        {
            toggledBtn = uibutton;
        }
        else
        {
            firstToggleButtn.SetActive(true);
            toggledBtn = firstToggleButtn;
        }
            
		if(JobSectRepo.mJobTypeMap.Count > 0)
		{
			if (loadedJobModel != null)
			{
				Destroy(loadedJobModel);

                GameObject avatar = GameObject.Find("AvatarSelected");
                if (avatar)
                {
                    Destroy(avatar);
                }
            }
            int jobindex = 1;
            if(uibutton != null)
                jobindex = GetBtnIndex(uibutton.name);
            else
            {
                if (firstToggleButtn != null && firstToggleButtn.GetComponent<Toggle>())
                    firstToggleButtn.GetComponent<Toggle>().isOn = true;
            }
            jobSelected = jobindex;        
            //txtJobDescription.text = JobSectRepo.GetJobByType((JobType)jobSelected).localizeddescription;
        }
	}

	public void OnEndEditingName(Text inputfield)
	{
		charName = inputfield.text;
	}

	public void OnCharCreation()
	{
		if(charName != null && charName != "")
		{
            if (charName.Length > 10)
            {
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_NameToLong", null));
                return;
            }
            else
            {
                GameInfo.gLobby.InsertCharacter(charName, (byte)jobSelected, 0, 0, 0);                
            }
        }
        else
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_EmptyName", null));
            return;
        }
	}

    public void OnPickRandomName()
    {
        /*
        JobsectJson job = JobSectRepo.GetJobById(jobSelected);
        string name;
        if(job != null && (job.jobtype == JobType.S || job.jobtype == JobType.SWD))
        {
            name = CharacterNamingRepo.GetRandomMaleName();
        }
        else
        {
            name = CharacterNamingRepo.GetRandomFemaleName();
        }
        inputField.GetComponent<InputField>().text = name;
        charName = name;
        */
    }

	public string GetJobDescription(string jobname)
	{
		return "";
	}

	public Sprite GetJobTitleSprite(string jobname)
	{
		return null;
	}
}
