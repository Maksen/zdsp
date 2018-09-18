using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillButtonBase : MonoBehaviour {

    public Toggle m_Toggle;
    public int m_skgID;
    public Image m_IconFrame;
    public Image m_Icon;
    public int m_Skillid;
    public int m_SkillLevel = 0;

    public UI_SkillTree m_parentPanel { get; set; }

}
