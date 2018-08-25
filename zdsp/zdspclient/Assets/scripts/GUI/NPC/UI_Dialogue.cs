using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;
using System.Collections.Generic;

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
    private StaticNPCJson mNPCJson;
    private bool mCompletedAllQuest;
    private int mNPCId;
    private int mQuestId;
    private bool mOngoingQuest;    

    enum DialogueAction
    {
        None,
        StartQuest,
        InteractNpc,
    }

    private int mSelectedQuestId = -1;
    private int mSelectedQuestGroup = 0;
    private int mSelectedChoice = -1;
    private DialogueAction mDialogueAction = DialogueAction.None;
    private int mQuestionTalkId = -1;

    public void Init(StaticNPCGhost npc, int talkid, int questid, bool ongoingquest, List<int> questlist = null, bool completedall = false)
    {
        mQuestNPC = npc;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
        mNPCJson = npc.mArchetype;
        mCompletedAllQuest = completedall;
        mStep = 0;
        mQuestList = questlist;
        mNPCId = npc.mArchetypeId;
        mQuestId = questid;
        mOngoingQuest = ongoingquest;
        mDialogueAction = DialogueAction.None;
        mQuestionTalkId = -1;
        if (mTalkJson != null)
        {
            mTotalStep = mTalkJson.steps;
            NPCSide.SpawnNpc(GetAllCallerId());
            NPCSide.SpawnPlayer();
        }
        else
        {
            mTotalStep = 1;
            NPCSide.SpawnNpc(GetAllCallerId());
        }

        UpdateDialog();
    }

    public void Init(int npcid, int talkid, int questid, bool ongoingquest)
    {
        mQuestNPC = null;
        mSelectionObjects = new List<GameObject>();
        mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
        mNPCJson = null;
        mCompletedAllQuest = false;
        mStep = 0;
        mQuestList = null;
        mNPCId = npcid;
        mQuestId = questid;
        mOngoingQuest = ongoingquest;
        mDialogueAction = DialogueAction.None;
        mQuestionTalkId = -1;
        if (mTalkJson != null)
        {
            mTotalStep = mTalkJson.steps;
            NPCSide.SpawnNpc(GetAllCallerId());
            NPCSide.SpawnPlayer();
        }

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
        foreach(GameObject obj in mSelectionObjects)
        {
            Destroy(obj);
        }
    }

    private void UpdateSelection()
    {
        ClearSelection();
        if (mQuestList != null)
        {
            foreach(int questid in mQuestList)
            {
                GameObject obj = Instantiate(SelectionObject);
                obj.GetComponent<UI_DialogueSelection>().Init(this, questid);
                obj.transform.SetParent(SelectionContent, false);
                mSelectionObjects.Add(obj);
            }
        }
        else if (mTalkJson != null)
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
            if (mDialogueAction == DialogueAction.None)
            {
                if (mQuestId != -1)
                {
                    if (mOngoingQuest)
                    {
                        if (mTalkJson.selectionid <= 0)
                        {
                            SelectedInteractChoice(mQuestId, -1, -1);
                        }
                        else
                        {
                            mStep -= 1;
                            return;
                        }
                    }
                    else
                    {
                        if (mTalkJson.selectionid <= 0)
                        {
                            SelectedStartQuest(mQuestId, 0);
                        }
                    }
                }
                else
                {
                    if (GameInfo.gLocalPlayer != null)
                    {
                        GameInfo.gLocalPlayer.QuestController.CloseNpcTalk();
                    }
                }
            }
            else if(mDialogueAction == DialogueAction.StartQuest)
            {
                StartQuest();
            }
            else
            {
                SubmitQuest();
            }
        }
        else
        {
            UpdateDialog();
        }
    }

    public void SelectQuest(int questid)
    {
        if (GameInfo.gLocalPlayer != null)
        {
            mQuestId = questid;
            mQuestList = null;
            mOngoingQuest = false;
            QuestClientController questController = GameInfo.gLocalPlayer.QuestController;
            int talkid = questController.GetTalkId(mQuestId, mNPCId);
            mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
            if (mTalkJson != null)
            {
                mStep = 0;
                mTotalStep = mTalkJson.steps;
                NPCSide.SpawnNpc(GetAllCallerId());
                NPCSide.SpawnPlayer();
                ClearSelection();
                if (mStep < mTotalStep)
                {
                    UpdateDialog();
                }
            }
            else
            {
                questController.CloseNpcTalk();
            }
        }
    }

    public void SelectedInteractChoice(int questid, int choice, int nexttalkid = -1)
    {
        mSelectedQuestId = questid;
        mSelectedChoice = choice;
        mDialogueAction = DialogueAction.InteractNpc;
        mQuestionTalkId = mTalkJson.talkid;
        if (nexttalkid != -1)
        {
            UpdateTalkId(nexttalkid);
        }
        else
        {
            SubmitQuest();
        }
    }

    private void SubmitQuest()
    {
        if (mSelectedQuestId != -1)
        {
            RPCFactory.NonCombatRPC.NPCInteract(mSelectedQuestId, mNPCId, mSelectedChoice, mQuestionTalkId);
        }
        UIManager.CloseDialog(WindowType.DialogNpcTalk);
    }

    public void SelectedStartQuest(int questid, int groupid, int nexttalkid = -1)
    {
        mSelectedQuestId = questid;
        mSelectedQuestGroup = groupid;
        mDialogueAction = DialogueAction.StartQuest;
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

    private void StartQuest()
    {
        if (mSelectedQuestId != -1)
        {
            RPCFactory.NonCombatRPC.StartQuest(mSelectedQuestId, mNPCId, mSelectedQuestGroup);
        }
        UIManager.CloseDialog(WindowType.DialogNpcTalk);
    }

    private void OnDisable()
    {
        NPCSide.DestroyModel();
        ClearSelection();
    }

    public void UpdateTalkId(int talkid)
    {
        mTalkJson = QuestRepo.GetQuestTalkByID(talkid);
        if (mTalkJson != null)
        {
            mStep = 0;
            mTotalStep = mTalkJson.steps;
            NPCSide.SpawnNpc(GetAllCallerId());
            NPCSide.SpawnPlayer();
            ClearSelection();
            if (mStep < mTotalStep)
            {
                UpdateDialog();
            }
        }
        else
        {
            UIManager.CloseDialog(WindowType.DialogNpcTalk);
        }
    }
}
