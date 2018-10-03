using UnityEngine;
using System;
using System.Collections.Generic;
using Zealot.Spawners;

public class CutsceneManager : MonoBehaviour
{
    private List<CutsceneEntity> cutsceneEntities;
    
    /// <summary>
    /// index of realm start cutscene in cutsceneEntities
    /// </summary>
    private int indexRealmStart = -1;
    private int indexUI = -1;
    private CutsceneEntity currentPlaying = null;

    private Dictionary<int, CutsceneEntity> eventCutscenes;
    private Dictionary<string, int> questCutscenes;

    [NonSerialized]
    public bool CutsceneLoading = false;

    public Action OnFinishedCutsceneAction;

    void Awake()
    {
        eventCutscenes = new Dictionary<int, CutsceneEntity>();
        questCutscenes = new Dictionary<string, int>();
    }

    void Start()
    {
        cutsceneEntities = new List<CutsceneEntity>();
        var cutsceneObjs = GameObject.FindGameObjectsWithTag("Cutscene");
        for (int i = 0; i < cutsceneObjs.Length; i++)
        {
            var entity = cutsceneObjs[i].GetComponent<CutsceneEntity>();
            if (entity != null)
            {
                cutsceneEntities.Add(entity);
                if (entity.cutsceneTriggerType == CutsceneTriggerType.UI)
                {
                    if (indexUI == -1)
                        indexUI = cutsceneEntities.Count - 1;
                    else
                        Debug.Log("CutsceneManager: more than 1 UI cutscene found");
                }
                else if (entity.cutsceneTriggerType == CutsceneTriggerType.RealmStart)
                {
                    if (indexRealmStart == -1)
                        indexRealmStart = cutsceneEntities.Count - 1;
                    else
                        Debug.Log("CutsceneManager: more than 1 realm start cutscene found");
                }
                else if (entity.cutsceneTriggerType == CutsceneTriggerType.Quest)
                {
                    questCutscenes[entity.CutsceneName] = cutsceneEntities.Count - 1;
                    entity.gameObject.SetActive(false);
                }
            }
        }
    }

    public void PlayCutscene(string name, Action finishedCB = null)
    {
        foreach (CutsceneEntity cutsceneEnt in cutsceneEntities)
        {
            if(cutsceneEnt.CutsceneName == name)
            {
                cutsceneEnt.gameObject.SetActive(true);
                cutsceneEnt.PlayCutscene();
                OnFinishedCutsceneAction = finishedCB;
                break;
            }
        }
    }

    public bool IsCutsceneReady(string name)
    {
        foreach (CutsceneEntity cutsceneEnt in cutsceneEntities)
        {
            if (cutsceneEnt.CutsceneName == name)
                return true;
        }
        return false;
    }

    void OnDestroy()
    {
        for (int i = 0; i < cutsceneEntities.Count; i++)
        {
            cutsceneEntities[i] = null;
        }
        cutsceneEntities = null;

        List<int> keylist = new List<int>(eventCutscenes.Keys);
        for (int i = 0; i < keylist.Count; i++)
        {
            eventCutscenes[keylist[i]] = null;
        }
        eventCutscenes = null;

        questCutscenes.Clear();
        questCutscenes = null;        
    }
      
     
    public bool IsPlaying()
    {
        return currentPlaying != null;
    }

    private bool isBotEnabled;
    private bool isHudVisible;
    private bool isPartyFollowEnabled;

    public void OnStartCutscene(CutsceneEntity cutsceneEntity)
    {
        ZDSPCamera combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
        combatCamera.targetObject = null;
        combatCamera.SetCameraActive(false);
        combatCamera.gameObject.SetActive(false);
        currentPlaying = cutsceneEntity;
        CutsceneLoading = false;
         
        isBotEnabled = GameInfo.gLocalPlayer.Bot.Enabled;
        GameInfo.gLocalPlayer.Bot.StopBot();
        GameInfo.gLocalPlayer.ForceIdle();
        if (PartyFollowTarget.Enabled)
        {
            isPartyFollowEnabled = true;
            PartyFollowTarget.Pause();
        }
        else
            isPartyFollowEnabled = false;

        UIManager.GetWidget(HUDWidgetType.Joystick).GetComponent<ZDSPJoystick>().SetActive(false);
        isHudVisible = UIManager.UIHud.IsVisible();
        UIManager.UIHud.HideHUD();

        GameInfo.gCombat.ShowEntitiesForCutscene(false);
        
        //Debug.Log("WM.Instance[WinPanel.CutScene].GetComponent<UI_CutScene>().Init(cutsceneEntity.Cutscene); ");
    }

    public void StopCutScene()
    {
        if(currentPlaying != null)
            currentPlaying.CleanUp();
    }
   
    public void OnCutsceneFinished(CutsceneEntity cutsceneEntity)
    {
        ZDSPCamera combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
        combatCamera.targetObject = GameInfo.gLocalPlayer.AnimObj;
        combatCamera.SetCameraActive(true);
        combatCamera.gameObject.SetActive(true);
        currentPlaying = null; 
        GameInfo.gCombat.ShowEntitiesForCutscene(true);

        //Debug.Log("WM.Instance[WinPanel.CutScene].GetComponent<UI_CutScene>().Close();");

        if (OnFinishedCutsceneAction != null)
            OnFinishedCutsceneAction();

        UIManager.GetWidget(HUDWidgetType.Joystick).GetComponent<ZDSPJoystick>().SetActive(true);
        UIManager.CloseDialog(WindowType.DialogCutscene);

        if (isBotEnabled)
            GameInfo.gLocalPlayer.Bot.StartBot();

        if (isHudVisible)
            UIManager.UIHud.ShowHUD();
        else
            UIManager.UIHud.HideHUD(); //the HUD also in the hierachy. 

        if (isPartyFollowEnabled)
            PartyFollowTarget.Resume();
    }   

    public void RegisterCutsceneBroadcaster(CutSceneBroadcaster entity)
    {
        eventCutscenes[entity.EntityId] = entity.cutsceneEntity;
    }

    public void SkipCutscene()
    {
        if (currentPlaying != null)
            currentPlaying.SkipCutScene();
    }

    #region Play Cutscene Triggers
    public void PlayRealmStartCutscene()
    {
        if (indexRealmStart >= 0 && indexRealmStart < cutsceneEntities.Count)
        {
            cutsceneEntities[indexRealmStart].PlayCutscene();
        }
    }

    public bool PlayEventCutscene(int objectId)
    {
        CutsceneEntity cutsceneEntity;
        if (eventCutscenes.TryGetValue(objectId, out cutsceneEntity))
        {
            cutsceneEntity.PlayCutscene();
            return true;
        }
        return false;
    }


    public void PlayUICutscene()
    {
        if (indexUI >= 0 && indexUI < cutsceneEntities.Count)
        {
            cutsceneEntities[indexUI].PlayCutscene();
        }
    }

    public bool PlayQuestCutscene(string cutsceneName)
    {
        int idx;
        if (questCutscenes.TryGetValue(cutsceneName, out idx) && idx < cutsceneEntities.Count)
        {
            cutsceneEntities[idx].PlayCutscene();
            return true;
        }
        return false;
    }

    #endregion
}
