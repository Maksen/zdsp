using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Kopio.JsonContracts;

public class UI_Tutorial : BaseWindowBehaviour
{

    [SerializeField]
    GameObject m_SpeechBubble;
    [SerializeField]
    Text m_Speech;
    [SerializeField]
    GameObject m_ClickBox;
    [SerializeField]
    List<Transform> m_LISAPos;
    [SerializeField]
    Animator m_Animator;
    [SerializeField]
    UI_DynamicRuntimeAnimatorController m_AnimatorControllers;

    [Header("List to identify the animator controllers position in list")]
    [SerializeField]
    List<SystemName> m_AnimatorIndex;

    TutorialDescriptionJson m_CurrentTutorial;


    int m_AnimatorSystemIndex;

    public void InitTutorialDialog(TutorialDescriptionJson json)
    {
        LISAPosition position = json.position;
        string res = json.path;
        Sprite lisa;
        m_Speech.text = json.text;

        if (res != string.Empty)
        {
            lisa = ClientUtils.LoadIcon(res);

        }

        // load animator
        int index = m_AnimatorIndex.FindIndex(x => x == json.system);
        m_Animator.runtimeAnimatorController = m_AnimatorControllers.GameObjectList[index];

        m_AnimatorSystemIndex = 0;
        foreach(SystemName item in m_AnimatorIndex)
        {
            if (item == json.system)
                break;

            ++m_AnimatorSystemIndex;
        }

        m_Animator.SetInteger("m_Step", json.tutorial_order);
        m_CurrentTutorial = json;
    }

    public void CompleteTutorial()
    {
        UIManager.CloseDialog(WindowType.DialogTutorial);
        RPCFactory.NonCombatRPC.OnEndTutorial((int)m_CurrentTutorial.system);
    }
}
