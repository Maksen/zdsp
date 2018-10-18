﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Zealot.Common;
using Zealot.Repository;

class EnumUtils
{
    public static Dictionary<string, T> GetEnumMap<T>()
    {
        Dictionary<string, T> ret = new Dictionary<string, T>();

        ret = new Dictionary<string, T>();
        var names = Enum.GetNames(typeof(T));
        for (int i = 0, size = names.Length; i < size; ++i)
        {
            ret.Add(Enum.GetName(typeof(T), i), (T)Enum.ToObject(typeof(T), i));
        }

        return ret;
    }
}

public class Jobs
{
    public enum Jobtype
    {
        BladeMaster,
        SwordMaster,
        General,
        Commando,
        Strategist,
        Schemer,
        Executioner,
        Slaughter
    };    
    public readonly static Jobs instance = new Jobs();

    public Dictionary<string, Jobtype> JobMap = null;
    Jobs()
    {
        JobMap = EnumUtils.GetEnumMap<Jobtype>();
    }
}

public class JECharacterManager : MonoBehaviour
{
    public GameObject main = null;
    public JELeftUI left = null;
    public JERightUI right = null;
    public JEShowUI show = null;
    public GameObject bladeslash = null;
    public PlayableDirector start = null, end = null;
    public List<PlayableDirector> class_cutscenes = new List<PlayableDirector>();
    public Transform class_cutscenes_parent;
    public Panner CameraPanner;

    public void init(GameObject obj = null)
    {
        if (main != null) main.SetActive(true);
        if (left != null) left.gameObject.SetActive(true);
    }

    // Use this for initialization
    void Start () {
        if (start != null)
        {
            start.gameObject.SetActive(true);
            var onenable = start.GetComponent<OnEnableScript>();

            if (onenable == null)
                onenable = start.gameObject.AddComponent<OnEnableScript>();

            onenable.onDisabled += init;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    int selectedjobindex = 0;
    string selectedjob = "";
    public void SetDescription(string job)
    {
        if (job == "")
        {            
            right.gameObject.GetComponent<Animator>().SetBool("On/Off", false);
        }        
        else
        if (right != null)
        {
            selectedjob = job;

            if (Jobs.instance.JobMap.ContainsKey(job) == false)
            {
                Debug.LogError(this.GetType().ToString() + " Error: Jobtype " + job + " does not exist");
                return;
            }            

            var rightanim = right.gameObject.GetComponent<Animator>();

            if (right.gameObject.activeSelf == false)
                right.gameObject.SetActive(true);
            else
            if (rightanim.GetBool("On/Off") == false)
            {                
                right.gameObject.GetComponent<Animator>().SetBool("On/Off", true);
                right.jobname.text = GUILocalizationRepo.GetLocalizedString(job);
                right.description.text = GUILocalizationRepo.GetLocalizedString(job + "_description");
            }
            else
                right.gameObject.GetComponent<Animator>().SetBool("On/Off", false);

            CameraPanner.SetPoint((int)Jobs.instance.JobMap[job]);
        }
    }

    public void Show()
    {
        var job = selectedjob;

        if (Jobs.instance.JobMap.ContainsKey(job) == false)
        {
            Debug.LogError(this.GetType().ToString() + " Error: Jobtype " + job + " does not exist");
            return;
        }

        var jobtype = Jobs.instance.JobMap[job];
        class_cutscenes[(int)jobtype].Play();
    }

    public void Next() 
    {
        if (end != null)
        {
            end.gameObject.SetActive(true);
            var onenable = end.GetComponent<OnEnableScript>();

            if (onenable == null)
                onenable = end.gameObject.AddComponent<OnEnableScript>();

            onenable.onDisabled += GoToCharacterCreate;
        }
    }

    public void GoToCharacterCreate(GameObject obj)
    {
        SceneLoader.Instance.LoadLevel("UI_LoginHierarchy_test", () => { GoToCharacterCreation(); });
    }

    public void GoToCharacterCreation()
    {
        UIManager.StopHourglass();
        if (GameInfo.gLobby.mCharacterList != null && GameInfo.gLobby.mCharacterList.Count > 0)
        {
            UIManager.OpenWindow(WindowType.CharacterCreation, (window) => window.GetComponent<UI_CharacterCreation>().InitFromSelection(GameInfo.gLobby.mCharacterList));
            UIManager.CloseWindow(WindowType.CharacterSelection);
        }
        else
        {
            UIManager.OpenWindow(WindowType.CharacterCreation, (window) => window.GetComponent<UI_CharacterCreation>().InitFromLogin());
            UIManager.CloseWindow(WindowType.CharacterSelection);
        }
    }
}
