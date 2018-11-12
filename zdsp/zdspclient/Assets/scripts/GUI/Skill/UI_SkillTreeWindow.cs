using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillTreeWindow : MonoBehaviour {

    public string m_Identifier;
    public UI_SkillTree m_Tree;
    public Button m_Close;

    public void Start()
    {
        m_Tree.RegisterWindow(m_Identifier, this);
    }

    public void OnOpenWindow()
    {
        // close all other window except this
        m_Tree.CloseWindows(m_Identifier);
    }

    public void CloseWindow()
    {
        m_Close.onClick.Invoke();
    }
}
