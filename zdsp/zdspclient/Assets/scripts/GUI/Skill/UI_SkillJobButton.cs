using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SkillJobButton : MonoBehaviour , IPointerClickHandler
{

    public UnityEngine.UI.Toggle m_Toggle;
    public Zealot.Common.JobType m_Jobtype;
    public int m_ID;
    public UnityEngine.UI.Image icon;


    public delegate void OnSelectedCallback(UI_SkillJobButton param);
    OnSelectedCallback selected;


    public void Init(Zealot.Common.JobType job, int id)
    {
        m_Toggle = GetComponent<UnityEngine.UI.Toggle>();
        m_ID = id;
        m_Jobtype = job;


        icon.sprite = ClientUtils.LoadIcon(Zealot.Repository.JobSectRepo.GetJobPortraitPath(job));
    }

    public void AddListener(OnSelectedCallback func)
    {
        selected = func;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected(this);
    }
}
