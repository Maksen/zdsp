using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Repository;
using Zealot.Common;
using Kopio.JsonContracts;

public class TutorialController {

    PlayerGhost m_Player;
    TutorialDescriptionJson m_CurrentTutorial;
    int m_CurrentStep;
    int m_Level;

    UI_Tutorial m_Dialog;

    public TutorialController(PlayerGhost player)
    {
        m_Player = player;
    }

    public void UpdateTutorialStatus(int level)
    {
        m_CurrentStep = 0;
        m_Level = level;

        // check for level tutorial
        m_CurrentTutorial = TutorialRepo.GetTutorialInfoWithLevel(level, 0);

        // start the guide if there exist a tutorial
        if (m_CurrentTutorial == null) return;

        int bit = 1 << (int)m_CurrentTutorial.system;
        if ((m_Player.PlayerSynStats.TutorialStatus & bit) == bit) // tutorial done
            return;

        UIManager.OpenDialog(WindowType.DialogTutorial);
        m_Dialog = UIManager.GetWindowGameObject(WindowType.DialogTutorial).GetComponent<UI_Tutorial>();
        m_Dialog.InitTutorialDialog(m_CurrentTutorial);
        UpdateTutorialHooks();
    }

    public void ActivateTutorial(SystemName system)
    {
        m_CurrentStep = 0;
        m_CurrentTutorial = TutorialRepo.GetTutorialWithSystem(system, 0);

        // start the guide if there exist a tutorial
        if (m_CurrentTutorial == null) return;

        int bit = 1 << (int)m_CurrentTutorial.system;
        if ((m_Player.PlayerSynStats.TutorialStatus & bit) == bit) // tutorial done
            return;

        UIManager.OpenDialog(WindowType.DialogTutorial);
        m_Dialog = UIManager.GetWindowGameObject(WindowType.DialogTutorial).GetComponent<UI_Tutorial>();
        m_Dialog.InitTutorialDialog(m_CurrentTutorial);
        UpdateTutorialHooks();
    }

    public void UpdateTutorialHooks()
    {
        switch (m_CurrentTutorial.system)
        {
            case SystemName.Destiny:
                DestinyTutorial();
                break;
            case SystemName.Newbie_Realm:
                Newbie_Realm_Tutorial();
                break;
        }
    }

    public void AdvanceStep()
    {
        ++m_CurrentStep;
        UpdateTutorialHooks();
        m_CurrentTutorial = TutorialRepo.GetTutorialInfoWithLevel(m_Level, m_CurrentStep);
        m_Dialog.InitTutorialDialog(m_CurrentTutorial);
    }

    private void DestinyTutorial()
    {
        GameObject windowObj = null;
        switch (m_CurrentStep)
        {
            case 0:
                windowObj = UIManager.GetWidget(HUDWidgetType.DestinyNews);
                windowObj.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(AdvanceStep);
                break;
            case 1:
                windowObj = UIManager.GetWindowGameObject(WindowType.Destiny);
                List<GameObject> msg = windowObj.GetComponent<UI_Destiny>().GetMessages();//[0].GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(AdvanceStep);
                GameObject obj = null;
                foreach (var item in msg)
                {
                    if (item.GetComponent<UI_ClueMessageData>().GetClueData().ClueId == GameConstantRepo.GetConstantInt("DestinyTutorialId"))
                    {
                        obj = item;
                        break;
                    }
                }
                obj.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(AdvanceStep);
                break;
            case 2:
                windowObj = UIManager.GetWindowGameObject(WindowType.DialogMessageFilter);
                windowObj.GetComponent<UIDialog>().closeButton[0].onClick.AddListener(AdvanceStep);
                break;
            case 3:
                windowObj = UIManager.GetWindowGameObject(WindowType.Destiny);
                windowObj.GetComponent<UIWindow>().closeButton[0].onClick.AddListener(m_Dialog.CompleteTutorial);
                break;
        }
    }

    private void Newbie_Realm_Tutorial()
    {
        switch (m_CurrentStep)
        {
            case 0:
                HUD_Skills hud = UIManager.GetWidget(HUDWidgetType.SkillButtons).GetComponent<HUD_Skills>();
                hud.TutorialMode(m_Dialog.CompleteTutorial);
                break;
        }
    }
}
