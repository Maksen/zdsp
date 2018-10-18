using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_SkillButtonBase : MonoBehaviour, IPointerClickHandler
{

    public Toggle m_Toggle;
    public int m_skgID;
    public Image m_IconFrame;
    public Image m_Icon;
    public int m_Skillid;
    public int m_SkillLevel = 0;

    public delegate void OnSelectedCallback(UI_SkillButtonBase param);
    OnSelectedCallback selected;

    public UI_SkillTree m_parentPanel { get; set; }

    public virtual void AddListener(OnSelectedCallback func)
    {
        selected = func;
    }
    public virtual void OnPointerClick(PointerEventData eventData) {
        selected(this);
    }

}
