using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Client.Entities;
using Zealot.Entities;

public class TrainingRealmContoller: LevelMonoSingleton<TrainingRealmContoller>
{
      
    protected Timers EntityTimers;
    protected bool bPlayerMoved;
    protected PlayerGhost localplayer;
    protected QuestJson quest;
    protected List<QuestObjectiveJson> listofobjectives;
    //protected UI_QuestLog m_UIQuestlog;
    protected TrainingRealmHighlightMB dialog; 
    public Trainingstep mystep;
    protected HUDWidget[] allHudWidgets;
    protected Dictionary<int, bool> TalksStated;

    void Start()
    {

    }
    public virtual void RealmStart()
    {        
        //quest = QuestRepo.GetQuestByID(QuestRepo.TutorialQuestId);
        lastOrder = 0;
        bPlayerMoved = false;
        //dialog = UIManager.GetWindowGameObject(WindowType.DialogTutorialTraining).GetComponent<TrainingRealmHighlightMB>();
        //dialog.gameObject.SetActive(false);
        listofobjectives = new List<QuestObjectiveJson>();//QuestRepo.GetObjectives(QuestRepo.TutorialQuestId);
        TalksStated = new Dictionary<int, bool>();
        foreach (QuestObjectiveJson objective in listofobjectives)
        {
            if (objective.type == QuestObjectiveType.Talk)
            {
                TalksStated.Add(objective.para2, false);
            }
        }
        //m_UIQuestlog = UIManager.GetWidget(HUDWidgetType.QuestList).GetComponent<UI_QuestLog>();
        localplayer = GameInfo.gLocalPlayer;
        EntityTimers = GameInfo.gLocalPlayer.EntitySystem.Timers; 
        mystep = Trainingstep.SpawnedAndTalk;

        GameObject root = UIManager.UIHud.gameObject;
        allHudWidgets = root.GetComponentsInChildren<HUDWidget>();

        SetUIStep(1);
        TurnOffFootPrint(1);
        TurnOffFootPrint(2);
        TurnOffFootPrint(3);
        TurnOffFootPrint(4);
        //m_UIQuestlog.ShowTrainingQuest(quest, listofobjectives[0], 0, 0);
        //3 seconds later show ui step 2. 
        EntityTimers.SetTimer(2000, (object args)=> {
            //m_UIQuestlog.ShowNewQuest();
            //SetUIStep(2); 
            //m_UIQuestlog.ShowTrainingQuest(quest, listofobjectives[0], 1, 0);

            //start the talk automatically. assuming spawnpos near to npc1
            QuestObjectiveJson objective = listofobjectives[0]; 
            ShowNewQuestAndTalk(1000, objective.para1, objective.para2); 
            
        }, null);
    }

    public void OnPlayerMoveStick()
    {
        if (!bPlayerMoved)
        {
            bPlayerMoved = true; 
            EntityTimers.SetTimer(2000, (object args) => {
                 OnQuestStepDone((int)Trainingstep.GoToArea2);
             }, null); //2 seconds later show path find tips.
        }
    }

    protected void ShowNewQuestAndTalk(long talkdelay, int talknpcid, int talkid)
    {
        //m_UIQuestlog.ShowNewQuest();
        EntityTimers.SetTimer(talkdelay, delegate {
            //validate the talknpc is at current level and start talk. otherwise endRealm
            Vector3 targetnpcpos = Vector3.zero;
            string levelname = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string archetype = StaticNPCRepo.GetNPCById(talknpcid).archetype;
            string level_target = "";
            bool find = NPCPosMap.FindNearestStaticNPC(archetype, levelname, localplayer.Position, ref level_target, ref targetnpcpos);
            if (find && levelname == level_target)
            {
                currentSystemMsg = "";
                //UIManager.OpenWindow(WindowType.NPCTalk, (window) => window.GetComponent<UI_NPCTalk>().InitQuestObjectiveTalk(quest, talkid));

            }
            else
            {
                Debug.LogWarning("Training Realm quest not set tup proppely!!!");
                //GameInfo.gCombat.OnFinishedTraingingRealm();
            }
        }, null);
    }

    public void OnTalkStart( int talkid)
    {
        if (TalksStated.ContainsKey(talkid))
        {
            if(TalksStated[talkid] == false)
            {
                TalksStated[talkid] = true;
                currentSystemMsg = "";
                if (mystep == Trainingstep.GoToArea2)
                    mystep = Trainingstep.DoPathFind;
                if (mystep +1 == Trainingstep.Talk1Start || mystep + 1 == Trainingstep.Talk2Start ||
                    mystep + 1 == Trainingstep.Talk3Start)
                    OnQuestStepDone();
            }
        }
    }

    public virtual void OnCutSceneFinished(string cutsceneName)
    {
        //if (GameInfo.gLocalPlayer.QuestStats.isTraining == false)
        //    return;
        if (mystep == Trainingstep.PlayCutScene1 || mystep == Trainingstep.PlayCutScene2 ||
            mystep == Trainingstep.PlayCutScene3)
        OnQuestStepDone();
    }

    protected int lastOrder = 1;
    public virtual void OnQuestOrderChange(int questorder)
    {
        if (lastOrder < questorder)
        {
            OnQuestStepDone();
        }
        lastOrder = questorder;
    }
    /// <summary>
    /// the function control the flow of TrainingRealm. 
    /// </summary>
    /// <param name="step">passing value greater than 0 will check with current step, passing 0 will always go next step</param>
    public virtual void OnQuestStepDone(int step = 0)
    {
        //if (!GameInfo.gLocalPlayer.QuestStats.isTraining)
        //    return;
        if (step > 0 && step != (int)mystep)
            return; //step condidtion wrong.
        mystep++;
        Debug.Log("current  step :" + mystep.ToString()); 

        if (mystep == Trainingstep.GoToArea2)
        {
            //once quest talk1 finished, 
            //Tutorial_Dialog1_MB tut1mb = dialog.step1.GetComponent<Tutorial_Dialog1_MB>();
            //tut1mb.OnStep(mystep);
            TurnOnFootPrint(1);
            SetUIStep(3);
            SetHighlightStep(3);
            currentSystemMsg = GUILocalizationRepo.GetLocalizedSysMsgByName("system_msg_goto_area2");
            showSystemMessages(true);
            //m_UIQuestlog.ShowTrainingQuest(quest, listofobjectives[1], 2, 0);//talk npc2
            
        }
        else if(mystep == Trainingstep.DoPathFind)
        {
            //Tutorial_Dialog1_MB tut1mb = dialog.step1.GetComponent<Tutorial_Dialog1_MB>();
            //tut1mb.OnStep(mystep);
        }
        else if(mystep == Trainingstep.Talk2Start)
        {

        }
        else if (mystep == Trainingstep.KillMonster)
        {
            TurnOffWall(1);
            TurnOffFootPrint(1);
            TurnOnFootPrint(2); 
            currentSystemMsg = "";
            SetUIStep(4);
            SetHighlightStep(4);
            //m_UIQuestlog.ShowTrainingQuest(quest, listofobjectives[2], 3, 0);//kill monsters 
        }
        else if (mystep == Trainingstep.GoToArea3)
        {
            TurnOffFootPrint(2);
            TurnOnFootPrint(3);
            TurnOffFootPrint(4);
            currentSystemMsg = GUILocalizationRepo.GetLocalizedSysMsgByName("system_msg_goto_area3");

            showSystemMessages(true);
            //m_UIQuestlog.ShowNewQuest();
            //m_UIQuestlog.ShowTrainingQuest(quest, listofobjectives[3], 4, 0);//talk to last npc
        } 
        else if(mystep == Trainingstep.Talk3Start)
        {
            TurnOffWall(2);
            TurnOffFootPrint(3); 
        }
        else if(mystep == Trainingstep.PlayCutScene1)
        {
            EntityTimers.SetTimer(1000, (object obj) => {
                
                CutsceneManager.instance.PlayQuestCutscene("cutscene1");
            }, null);
        }
        else if(mystep == Trainingstep.EncounterBoss) //dummy step which waiting for boss entcounter
        {
            TurnOnFootPrint(4);//guide player to boss
            //OnQuestStepDone((int)Trainingstep.EncounterBoss);
        }
        /*else if (mystep == Trainingstep.ShowFlashButton)//this step trigger by ClientTrigger.
        {
            //send rpc to server to initiate boss attack
            TurnOffFootPrint(3);
            RPCFactory.CombatRPC.FirstRealmStep((int)(Trainingstep.ShowFlashButton)); 
            currentSystemMsg = "";
            SetUIStep(5);//flash button.  
            SetHighlightStep(5);//start flash button highlight
        }
        else if (mystep == Trainingstep.FlashSuccess)
        {
            //string sysmsg = true ? "tut1_Dodge_Success" : "tut1_Dodge_Fail";
            //UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedString(sysmsg));
            EntityTimers.SetTimer(1000, (object args) => {
                OnQuestStepDone((int)Trainingstep.FlashSuccess);
            }, null);
        }*/
        else if (mystep == Trainingstep.ShowJobButton) {
            //start showing the job skill button 
            TurnOffFootPrint(4);//guide player to boss
            EntityTimers.SetTimer(1000, (object args) => {
                SetUIStep(6);
                SetHighlightStep(6);
            }, null);

        }
        else if (mystep == Trainingstep.PlayCutScene2)
        {
            //skill cast is 1 second. so give 2s for delay
            EntityTimers.SetTimer(2000, (object obj) => {
                
                CutsceneManager.instance.PlayQuestCutscene("cutscene2");
            }, null);
        }
        else if (mystep == Trainingstep.ShowRGB)
        {
            //start showing rgb buttons.
            SetUIStep(7);
            SetHighlightStep(7);
        }
        else if(mystep == Trainingstep.RGBSuccess)
        {
            HideHighlightDialog(Trainingstep.RGBSuccess);
        }
        else if (mystep == Trainingstep.PlayCutScene3)
        {
            //RGB skilldone and waiting for boss hp below 10%
            
             CutsceneManager.instance.PlayQuestCutscene("cutscene3"); 
        }
        else if(mystep == Trainingstep.Finished)
        {
            //TODO:play cutscene and finish realm.
            EntityTimers.SetTimer(1000, (object param) => {
                OnFinished();
            }, null);
        }
        
    }

    internal void OnTalkEnd(int talkid)
    {
        foreach (QuestObjectiveJson obj in listofobjectives)
        {
            if (obj.type == QuestObjectiveType.Talk && obj.para2 == talkid && talkid !=48)
            {
                GameInfo.gCombat.HideClientSpawner(StaticNPCRepo.GetNPCById(obj.para1).archetype);
                break;
            }
        }
    }

    public virtual void SetUIStep(int step)
    {
        //UpdateObjective(step); 
        Debug.Log("Tutorial UI step " + step); 
         
        for (int i = 0; i < allHudWidgets.Length; i++)
        {
            HUDWidget widget = allHudWidgets[i];
            if (widget == null)
                continue;
            if (widget.GetComponent<TrainingRealmStepMB>() != null)
            {
                TrainingRealmStepMB mb = widget.gameObject.GetComponent<TrainingRealmStepMB>();
                mb.OnStep(step);
            }
            else
            {
                TrainingRealmStepMB mb = widget.gameObject.AddComponent<TrainingRealmStepMB>();
                mb.OnStep(step);
            }
        }        
    }

    public void SetHighlightStep(int step)
    {
        dialog.gameObject.SetActive(true); 
        dialog.OnStep(step);
    }

     
     
    protected string currentSystemMsg = "";
    public void showSystemMessages(bool first)
    {
        if(first)
            UIManager.ShowSystemMessage(currentSystemMsg, false);
        EntityTimers.SetTimer(2000, delegate {
             if (string.IsNullOrEmpty(currentSystemMsg) == false)
             {
                 UIManager.ShowSystemMessage(currentSystemMsg, false);
                 showSystemMessages(false);
             }
        }, null);
    }

     
     
    public void HideHighlightDialog(Trainingstep step)
    {
        if (step ==  mystep)
            dialog.gameObject.SetActive(false);
    }

    private List<TutorialFootprint> tutorialfootprints;
    public void RegisterFootprint(TutorialFootprint ft)
    {
        if (tutorialfootprints == null)
        {
            tutorialfootprints = new List<TutorialFootprint>();
        }
        tutorialfootprints.Add(ft);
    }

    protected void TurnOffFootPrint(int index)
    {
        foreach(TutorialFootprint tfp in tutorialfootprints)
        {
            if (tfp !=null && tfp.footprintIndex == index)
            {
                tfp.gameObject.SetActive(false);
            }
        }
    }

    protected void TurnOnFootPrint(int index)
    {
        foreach (TutorialFootprint tfp in tutorialfootprints)
        {
            if (tfp != null && tfp.footprintIndex == index)
            {
                tfp.gameObject.SetActive(true);
            }
        }
    }

    private List<TutorialWallMB> tutorialWalls;
    public void RegisterTutorialWalls(TutorialWallMB go)
    {
        if (tutorialWalls == null)
        {
            tutorialWalls = new List<TutorialWallMB>(); 
        }        
        tutorialWalls.Add(go); 
    }

    protected void TurnOffWall(int index)
    {
        foreach(TutorialWallMB wmb in tutorialWalls)
        {
            if(wmb !=null && wmb.wallIndex == index)
            {
                wmb.gameObject.SetActive(false);
            }
        }
    }

    public virtual void OnFinished()
    {
        localplayer = null;
        EntityTimers = null;
        Array.Clear(allHudWidgets, 0, allHudWidgets.Length);
        dialog = null;
        tutorialfootprints.Clear();
        tutorialWalls.Clear();
        //m_UIQuestlog = null;
        GameInfo.gCombat.OnFinishedTraingingRealm();
    }
}