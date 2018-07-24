using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillJobButton : MonoBehaviour {

    public UnityEngine.UI.Toggle m_Toggle;
    public Zealot.Common.JobType m_Jobtype;
    public int m_ID;

    public void Init(int id)
    {
        m_Toggle = GetComponent<UnityEngine.UI.Toggle>();
        m_ID = id;
    }
}
