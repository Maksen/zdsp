using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Dialogue : BaseWindowBehaviour
{
    [SerializeField]
    UI_DialogueAvatar NPCSide;

    [SerializeField]
    Transform SelectionContent;

    [SerializeField]
    GameObject SelectionObject;

    [SerializeField]
    GameObject Arrow; 

    private StaticNPCGhost mQuestNPC;
    private int mStep = 0;
    private int mTotalStep = 0;
    private QuestTalkDetailJson mTalkJson = null;
    private List<GameObject> mSelectionObjects;
    private List<int> mQuestList;
    private Dictionary<int, int> mFunctionList;
    private StaticNPCJson mNPCJson;
    private bool mCompletedAllQuest;
    private int mNPCId;
    private int mQuestId;
    private bool mOngoingQuest;    

    enum DialogueAction
    {
        None,
        SelectAnswer,
        SubmitQuest,
        TriggerQuest,
        Selection,
        QuestSelection,
        FunctionSelection,
    }

    private int mSelectedQuestId = -1;
    private int mSelectedQuestGroup = 0;
    private int mSelectedChoice = -1;
    private DialogueAction mDialogueAction = DialogueAction.None;
    private int mQuestionTalkId = -1;

    public void InitQuestDialogue(StaticNPCGhost npc, int talkid, int questid, bool ongoingquest)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
        mNPCJson = npc == null ? null : npc.mArchetype;
        mCompletedAllQuest = false;
        mStep = 0;
        mQuestList = null;
        mNPCId = npc == null ? -1 : npc.mArchetypeId;
        mQuestId = questid;
        mSelectedQuestId = questid;
        mSelectedQuestGroup = 0;
        mOngoingQuest = ongoingquest;
        mDialogueAction = ongoingquest ? DialogueAction.SelectAnswer : DialogueAction.TriggerQuest;
        mQuestionTalkId = -1;
        if (mTalkJson != null)
        {
            mTotalStep = mTalkJson.steps;
            NPCSide.SpawnNPC(GetAllCallerId());
            NPCSide.SpawnPlayer();
        }
        else
        {
            mTotalStep = 1;
            NPCSide.SpawnNPC(GetAllCallerId());
        }

        UpdateDialog();
    }

    public void InitSelectionDialogue(StaticNPCGhost npc, List<int> questlist, Dictionary<int, int> functionlist)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = null;
        mNPCJson = npc.mArchetype;
        mCompletedAllQuest = false;
        mStep = 0;
        mQuestList = questlist;
        mFunctionList = functionlist;
        mNPCId = npc.mArchetypeId;
        mQuestId = -1;
        mOngoingQuest = false;
        mDialogueAction = DialogueAction.Selection;
        mQuestionTalkId = -1;
        mTotalStep = 1;
        NPCSide.SpawnNPC(GetAllCallerId());

        UpdateDialog();
    }

    public void InitQuestSelectionDialogue(StaticNPCGhost npc, List<int> questlist)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = null;
        mNPCJson = npc.mArchetype;
        mCompletedAllQuest = false;
        mStep = 0;
        mQuestList = questlist;
        mFunctionList = null;
        mNPCId = npc.mArchetypeId;
        mQuestId = -1;
        mOngoingQuest = false;
        mDialogueAction = DialogueAction.QuestSelection;
        mQuestionTalkId = -1;
        mTotalStep = 1;
        NPCSide.SpawnNPC(GetAllCallerId());

        UpdateDialog();
    }

    public void InitFunctionSelectionDialogue(StaticNPCGhost npc, Dictionary<int, int> functionlist)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = null;
        mNPCJson = npc.mArchetype;
        mCompletedAllQuest = false;
        mStep = 0;
        mQuestList = null;
        mFunctionList = functionlist;
        mNPCId = npc.mArchetypeId;
        mQuestId = -1;
        mOngoingQuest = false;
        mDialogueAction = DialogueAction.FunctionSelection;
        mQuestionTalkId = -1;
        mTotalStep = 1;
        NPCSide.SpawnNPC(GetAllCallerId());

        UpdateDialog();
    }

    public void InitCommonDialogue(StaticNPCGhost npc, bool completedall)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = null;
        mNPCJson = npc.mArchetype;
        mCompletedAllQuest = completedall;
        mStep = 0;
        mQuestList = null;
        mFunctionList = null;
        mNPCId = npc.mArchetypeId;
        mQuestId = -1;
        mOngoingQuest = false;
        mDialogueAction = DialogueAction.None;
        mQuestionTalkId = -1;
        mTotalStep = 1;
        NPCSide.SpawnNPC(GetAllCallerId());

        UpdateDialog();
    }

    private void UpdateDialog()
    {
        int npcid = GetCallerId();
        string dialog = GetDialog();
        NPCSide.gameObject.SetActive(true);
        NPCSide.ActiveAvatar(npcid, dialog);
        Arrow.SetActive(true);

        if (mStep == mTotalStep - 1)
        {
            UpdateSelection();
            Arrow.SetActive(false);
        }
    }

    private void ClearSelection()
    {
        if (mSelectionObjects == null)
            return;

        int count = mSelectionObjects.Count;
        for (int i = 0; i < count; ++i)
            Destroy(mSelectionObjects[i]);
    }

    private void UpdateSelection()
    {
        ClearSelection();

        if (mTalkJson != null)
        {
            List<QuestSelectDetailJson> seletions = QuestRepo.GetSelectionByGroupId(mTalkJson.selectionid);
            if (seletions != null)
            {
                foreach (QuestSelectDetailJson selection in seletions)
                {
                    GameObject obj = Instantiate(SelectionObject);
                    obj.GetComponent<UI_DialogueSelection>().Init(this, selection, mQuestId, mOngoingQuest);
                    obj.transform.SetParent(SelectionContent, false);
                    mSelectionObjects.Add(obj);
                }
            }
        }
        else
        {
            if (mFunctionList != null)
            {
                foreach (KeyValuePair<int, int> function in mFunctionList)
                {
                    GameObject obj = Instantiate(SelectionObject);
                    obj.GetComponent<UI_DialogueSelection>().Init(this, function.Key, function.Value);
                    obj.transform.SetParent(SelectionContent, false);
                    mSelectionObjects.Add(obj);
                }
            }

            if (mQuestList != null)
            {
                foreach (int questid in mQuestList)
                {
                    GameObject obj = Instantiate(SelectionObject);
                    obj.GetComponent<UI_DialogueSelection>().Init(this, questid);
                    obj.transform.SetParent(SelectionContent, false);
                    mSelectionObjects.Add(obj);
                }
            }
        }
    }

    private string GetDialog()
    {
        if (mTalkJson != null)
        {
            switch(mStep)
            {
                case 0:
                    return mTalkJson.dialogue1;
                case 1:
                    return mTalkJson.dialogue2;
                case 2:
                    return mTalkJson.dialogue3;
                case 3:
                    return mTalkJson.dialogue4;
                case 4:
                    return mTalkJson.dialogue5;
                case 5:
                    return mTalkJson.dialogue6;
                case 6:
                    return mTalkJson.dialogue7;
                default:
                    return "";
            }
        }
        else
        {
            if (mCompletedAllQuest && !string.IsNullOrEmpty(mNPCJson.talktextalt))
            {
                return mNPCJson.talktextalt;
            }
            else
            {
                return mNPCJson.talktext;
            }
        }
    }

    private int GetCallerId()
    {
        if (mTalkJson != null)
        {
            switch (mStep)
            {
                case 0:
                    return mTalkJson.caller1;
                case 1:
                    return mTalkJson.caller2;
                case 2:
                    return mTalkJson.caller3;
                case 3:
                    return mTalkJson.caller4;
                case 4:
                    return mTalkJson.caller5;
                case 5:
                    return mTalkJson.caller6;
                case 6:
                    return mTalkJson.caller7;
                default:
                    return 0;
            }
        }
        else
        {
            return mNPCId;
        }
    }

    private List<int> GetAllCallerId()
    {
        List<int> result = new List<int>();
        if (mTalkJson != null)
        {
            if (!result.Contains(mTalkJson.caller1) && mTalkJson.caller1 != 0)
            {
                result.Add(mTalkJson.caller1);
            }
            if (!result.Contains(mTalkJson.caller2) && mTalkJson.caller2 != 0)
            {
                result.Add(mTalkJson.caller2);
            }
            if (!result.Contains(mTalkJson.caller3) && mTalkJson.caller3 != 0)
            {
                result.Add(mTalkJson.caller3);
            }
            if (!result.Contains(mTalkJson.caller4) && mTalkJson.caller4 != 0)
            {
                result.Add(mTalkJson.caller4);
            }
            if (!result.Contains(mTalkJson.caller5) && mTalkJson.caller5 != 0)
            {
                result.Add(mTalkJson.caller5);
            }
            if (!result.Contains(mTalkJson.caller6) && mTalkJson.caller6 != 0)
            {
                result.Add(mTalkJson.caller6);
            }
        }
        else
        {
            if (!result.Contains(mNPCId))
            {
                result.Add(mNPCId);
            }
        }
        return result;
    }

    public void OnClickedNext()
    {
        mStep += 1;
        if (mStep >= mTotalStep)
        {
            if (mDialogueAction == DialogueAction.SelectAnswer && mOngoingQuest && mQuestId != -1)
            {
                if (mTalkJson != null && mTalkJson.selectionid <= 0)
                {
                    SelectedInteractChoice(mQuestId, -1, -1);
                }
                else if (mTalkJson != null && mTalkJson.selectionid > 0)
                {
                    mStep -= 1;
                    return;
                }
                else
                {
                    GameInfo.gLocalPlayer.QuestController.CloseNpcTalk();
                }
            }
            else if (mDialogueAction == DialogueAction.TriggerQuest)
            {
                if (mTalkJson.selectionid <= 0)
                {
                    StartQuest();
                }
                else if (mTalkJson.selectionid > 0)
                {
                    mStep -= 1;
                    return;
                }                
            }
            else if (mDialogueAction == DialogueAction.SubmitQuest)
            {
                SubmitQuest();
            }
            else if (mDialogueAction == DialogueAction.Selection || mDialogueAction == DialogueAction.QuestSelection || mDialogueAction == DialogueAction.FunctionSelection)
            {
                mStep -= 1;
                return;
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            UpdateDialog();
        }
    }

    private void SubmitQuest()
    {
        if (mSelectedQuestId != -1)
        {
            RPCFactory.NonCombatRPC.NPCInteract(mSelectedQuestId, mNPCId, mSelectedChoice, mQuestionTalkId);
        }
        CloseDialog();
    }

    private void StartQuest()
    {
        if (mSelectedQuestId != -1)
        {
            RPCFactory.NonCombatRPC.StartQuest(mSelectedQuestId, mNPCId, mSelectedQuestGroup);
        }
        CloseDialog();
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        NPCSide.DestroyModel();
        ClearSelection();
        PartyFollowTarget.Resume();  // resume party follow if it's paused
    }

    //Update Next Talk
    public void UpdateTalkId(int talkid)
    {
        mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
        if (mTalkJson != null)
        {
            mStep = 0;
            mTotalStep = mTalkJson.steps;
            NPCSide.SpawnNPC(GetAllCallerId());
            NPCSide.SpawnPlayer();
            ClearSelection();
            if (mStep < mTotalStep)
            {
                UpdateDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    public void CloseDialog()
    {
        if (GameInfo.gLocalPlayer != null)
        {
            GameInfo.gLocalPlayer.QuestController.CloseNpcTalk();
        }
    }

    //Start Quest Dialogue
    public void SelectStartQuest(int questid)
    {
        if (GameInfo.gLocalPlayer != null)
        {
            mSelectedQuestId = mQuestId = questid;
            mSelectedQuestGroup = 0;
            mQuestList = null;
            mOngoingQuest = false;
            mDialogueAction = DialogueAction.TriggerQuest;
            QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
            int talkid = questController.GetTalkId(mQuestId, mNPCId);
            mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
            if (mTalkJson != null)
            {
                mStep = 0;
                mTotalStep = mTalkJson.steps;
                NPCSide.SpawnNPC(GetAllCallerId());
                NPCSide.SpawnPlayer();
                ClearSelection();
                if (mStep < mTotalStep)
                {
                    UpdateDialog();
                }
            }
            else
            {
                StartQuest();
            }
        }
    }

    //Select Quest Start Group
    public void SelectedStartQuestGroup(int questid, int groupid, int nexttalkid = -1)
    {
        mSelectedQuestId = questid;
        mSelectedQuestGroup = groupid;
        mDialogueAction = DialogueAction.TriggerQuest;
        mQuestionTalkId = -1;
        if (nexttalkid != -1)
        {
            UpdateTalkId(nexttalkid);
        }
        else
        {
            StartQuest();
        }
    }

    //Select Quest Submit Choice
    public void SelectedInteractChoice(int questid, int choice, int nexttalkid = -1)
    {
        mSelectedQuestId = questid;
        mSelectedChoice = choice;
        mQuestionTalkId = mTalkJson.talkid;
        mDialogueAction = DialogueAction.SubmitQuest;
        if (nexttalkid != -1)
        {
            UpdateTalkId(nexttalkid);
        }
        else
        {
            SubmitQuest();
        }
    }

    //Open  Function
    public void SelectedFunction(int functiontype, int param)
    {
        NPCFunctionType type = (NPCFunctionType)functiontype;

        switch (type)
        {
            case NPCFunctionType.Shop:
                UIManager.OpenWindow(WindowType.ShopSell, (window) => window.GetComponent<UIShop>().RequestShopInfo(param));
                break;
        }
    }
}
