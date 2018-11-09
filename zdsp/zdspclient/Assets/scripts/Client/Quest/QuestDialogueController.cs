using System.Collections.Generic;
using Zealot.Common;
using Zealot.Client.Entities;

public enum DialogueType
{
    Common,
    Selection,
    QuestSelection,
    FunctionSelection,
    Quest,
}

public class DialogueSequenceData
{
    public int QuestId;
    public StaticNPCGhost NPCGhost;
    public int TalkId;
    public bool IsOngoingQuest;
    public bool CompletedAllQuest;
    public List<int> QuestList;
    public Dictionary<int, int> FunctionList;
    public DialogueType DialogueType;

    public DialogueSequenceData(StaticNPCGhost npc, int talkid, int questid, bool ongoing)
    {
        QuestId = questid;
        TalkId = talkid;
        NPCGhost = npc;
        IsOngoingQuest = ongoing;
        CompletedAllQuest = false;
        QuestList = null;
        FunctionList = null;
        DialogueType = DialogueType.Quest;
    }

    public DialogueSequenceData(StaticNPCGhost npc, List<int> questlist)
    {
        QuestId = -1;
        TalkId = -1;
        NPCGhost = npc;
        IsOngoingQuest = false;
        CompletedAllQuest = false;
        QuestList = questlist;
        FunctionList = null;
        DialogueType = DialogueType.QuestSelection;
    }

    public DialogueSequenceData(StaticNPCGhost npc, Dictionary<int, int> functionlist)
    {
        QuestId = -1;
        TalkId = -1;
        NPCGhost = npc;
        IsOngoingQuest = false;
        CompletedAllQuest = false;
        QuestList = null;
        FunctionList = functionlist;
        DialogueType = DialogueType.FunctionSelection;
    }

    public DialogueSequenceData(StaticNPCGhost npc, List<int> questlist, Dictionary<int, int> functionlist)
    {
        QuestId = -1;
        TalkId = -1;
        NPCGhost = npc;
        IsOngoingQuest = false;
        CompletedAllQuest = false;
        QuestList = questlist;
        FunctionList = functionlist;
        DialogueType = DialogueType.Selection;
    }

    public DialogueSequenceData(StaticNPCGhost npc, bool completedall)
    {
        QuestId = -1;
        TalkId = -1;
        NPCGhost = npc;
        IsOngoingQuest = false;
        CompletedAllQuest = completedall;
        QuestList = null;
        FunctionList = null;
        DialogueType = DialogueType.Common;
    }
}

public class QuestDialogueController
{
    private List<DialogueSequenceData> mDialogueData;

    private QuestClientController mQuestController;
    private bool bInit = false;
    

    public QuestDialogueController(QuestClientController questController)
    {
        mQuestController = questController;
        mDialogueData = new List<DialogueSequenceData>();
        bInit = true;
    }

    public void OpenQuestDialogue(StaticNPCGhost npc, int talkid, int questid)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, talkid, questid, true);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void OpenStartQuestDialogue(StaticNPCGhost npc, int talkid, int questid)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, talkid, questid, false);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void OpenQuestFunctionDialogue(StaticNPCGhost npc, List<int> questlist, Dictionary<int, int> functionlist)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, questlist);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void OpenQuestSelectionDialogue(StaticNPCGhost npc, List<int> questlist)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, questlist);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void OpenFunctionSelectionDialogue(StaticNPCGhost npc, Dictionary<int, int> functionlist)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, functionlist);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void OpenCommonDialogue(StaticNPCGhost npc, bool completedall)
    {
        DialogueSequenceData dialogue = new DialogueSequenceData(npc, completedall);
        mDialogueData.Add(dialogue);
        ShowDialogue();
    }

    public void CheckDialogueAvailableQuest(CurrentQuestData questData)
    {
        if (!bInit)
        {
            return;
        }

        QuestStatus status = (QuestStatus)questData.Status;
        if (status == QuestStatus.NewObjective || status == QuestStatus.NewQuest)
        {
            int talkid = mQuestController.GetTalkId(questData.QuestId, -1);
            if (talkid != -1)
            {
                DialogueSequenceData dialogue = new DialogueSequenceData(null, talkid, questData.QuestId, true);
                mDialogueData.Add(dialogue);
                ShowDialogue();
            }
        }
    }

    private void ShowDialogue()
    {
        if (UIManager.IsWindowOpen(WindowType.DialogCutscene))
        {
            return;
        }

        if (mDialogueData.Count > 0)
        {
            DialogueSequenceData dialogue = mDialogueData[0];
            OpenDialogueUI(dialogue);
            mDialogueData.RemoveAt(0);
        }
    }

    private void OpenDialogueUI(DialogueSequenceData dialogue)
    {
        if (!UIManager.IsWindowOpen(WindowType.DialogNpcTalk))
        {
            UIManager.OpenDialog(WindowType.DialogNpcTalk);
        }

        Zealot.Bot.BotStateController.Instance.NonCombatQuest();

        UI_Dialogue uidialogue = UIManager.GetWindowGameObject(WindowType.DialogNpcTalk).GetComponent<UI_Dialogue>();
        if (dialogue.DialogueType == DialogueType.Quest)
        {
            uidialogue.InitQuestDialogue(dialogue.NPCGhost, dialogue.TalkId, dialogue.QuestId, dialogue.IsOngoingQuest);
        }
        else if (dialogue.DialogueType == DialogueType.Selection)
        {
            uidialogue.InitSelectionDialogue(dialogue.NPCGhost, dialogue.QuestList, dialogue.FunctionList);
        }
        else if (dialogue.DialogueType == DialogueType.QuestSelection)
        {
            uidialogue.InitQuestSelectionDialogue(dialogue.NPCGhost, dialogue.QuestList);
        }
        else if (dialogue.DialogueType == DialogueType.FunctionSelection)
        {
            uidialogue.InitFunctionSelectionDialogue(dialogue.NPCGhost, dialogue.FunctionList);
        }
        else if (dialogue.DialogueType == DialogueType.Common)
        {
            uidialogue.InitCommonDialogue(dialogue.NPCGhost, dialogue.CompletedAllQuest);
        }
    }

    public bool HasPendingDialogue()
    {
        return mDialogueData.Count > 0 ? true : false;
    }

    public void StartNextDialogue()
    {
        ShowDialogue();
    }
}
