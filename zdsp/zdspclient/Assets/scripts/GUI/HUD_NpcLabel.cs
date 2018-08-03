using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;
using System;
using Zealot.Client.Entities;

public class HUD_NpcLabel : MonoBehaviour
{
    public delegate Vector2 GetCanvasPosDelegate(Vector3 worldOffset);

    [SerializeField]
    GameObject mChatBox;
    [SerializeField]
    GameObject mQuest;
    [SerializeField]
    GameObject mReturnQuest;

    [SerializeField]
    Text mChatTxt;
    [SerializeField]
    Image mQuestImg;
    [SerializeField]
    Image mRetQuestImg;

    [SerializeField]
    Sprite NewMainQuestImage;
    [SerializeField]
    Sprite NewAdventureQuestImage;
    [SerializeField]
    Sprite NewEventQuestImage;
    [SerializeField]
    Sprite NewSubQuestImage;

    GetCanvasPosDelegate mCanvasPosFunc = null;
    RectTransform mRectTrans;
    Vector3 mOffset_WorldSpace = Vector3.zero;

    //ChatBubble Related Declare_StaticNPC
    float maxTime = 0.0f;
    float minTime = 0.0f;
    float maxminRandom = 0.0f;
    float mChatDuration = 0.0f;
    float mChatDurationDecreas = 0.0f;

    int mRandomTalk = 0;
    string[] TalkSentence = null;

    //ChatBubble Related Declare_CombeatNPC
    float maxTime_mons = 0.0f;
    float minTime_mons = 0.0f;
    float maxminRandom_mons = 0.0f;
    float mChatDuration_mons = 0.0f;
    float mChatDurationDecreas_mons = 0.0f;

    int mRandomTalk_mons = 0;
    string[] TalkSentence_mons = null;

    float mTopHeight = 0.0f;

    #region Property
    public bool ChatOn
    {
        get { return mChatBox.GetActive(); }
        set { mChatBox.SetActive(value); }
    }
    public string Chat
    {
        get { return mChatTxt.text; }
        set
        {
            mChatTxt.text = value;
        }
    }
    public float height
    {
        get { return mTopHeight; }
        set { mTopHeight = value; }
    }
    public Vector3 WorldSpaceOffset
    {
        set { mOffset_WorldSpace = value; }
    }
    public GetCanvasPosDelegate CanvasPosFunc
    {
        set { mCanvasPosFunc = value; }
    }
    #endregion

    public void Awake()
    {
        mRectTrans = gameObject.GetComponent<RectTransform>();
    }

    void Update()
    {
        //StaticNPC Calculation
        if (mChatDurationDecreas > 0.0f)
        {
            mChatDurationDecreas -= Time.deltaTime;
            if (mChatDurationDecreas <= 0.0f)
            {
                mChatDurationDecreas = 0.0f;
                ChatOn = false;
                maxminRandom = (float)(GameUtils.Random(minTime, maxTime));
            }
        }

        if (maxminRandom > 0.0f)
        {
            maxminRandom -= Time.deltaTime;
            if (maxminRandom <= 0.0f)
            {
                maxminRandom = 0.0f;
                mChatDurationDecreas = mChatDuration;
                mRandomTalk = (int)(GameUtils.Random(0, TalkSentence.Length));
                Chat = TalkSentence[mRandomTalk];
                ChatOn = true;
            }
        }

        //Monster Calculation
        if (mChatDurationDecreas_mons > 0.0f)
        {
            mChatDurationDecreas_mons -= Time.deltaTime;
            if (mChatDurationDecreas_mons <= 0.0f)
            {
                mChatDurationDecreas_mons = 0.0f;
                ChatOn = false;
                maxminRandom_mons = (float)(GameUtils.Random(minTime_mons, maxTime_mons));
            }
        }

        if (maxminRandom_mons > 0.0f)
        {
            maxminRandom_mons -= Time.deltaTime;
            if (maxminRandom_mons <= 0.0f)
            {
                maxminRandom_mons = 0.0f;
                mChatDurationDecreas_mons = mChatDuration_mons;
                mRandomTalk_mons = (int)(GameUtils.Random(0, TalkSentence_mons.Length));
                Chat = TalkSentence_mons[mRandomTalk_mons];
                ChatOn = true;
            }
        }
    }

    public void InitChatWithStaticNPC(string mArchetype, bool toggleOn = true)
    {
        //Get static NPC by name
        StaticNPCJson npcJson = StaticNPCRepo.GetStaticNPCByName(mArchetype);
        Chat = npcJson.speechbubbletext;

        if (Chat != "")
        {
            mChatDuration = npcJson.speechbubbleduration;
            height = npcJson.speechbubbleheight;
            maxTime = npcJson.speechbubblemaxint;
            minTime = npcJson.speechbubbleminint;

            mChatDurationDecreas = mChatDuration;

            TalkSentence = Chat.Split(';');
            mRandomTalk = (int)(GameUtils.Random(0, TalkSentence.Length));
            Chat = TalkSentence[mRandomTalk];

            ChatOn = toggleOn; //Turn on or off
        }
        else
        {
            ChatOn = false;
        }
    }

    public void InitChatWithMonsterNPC(string name, bool toggleOn = true)
    {
        //Get Monster NPC by name
        CombatNPCJson monsJson = NPCRepo.GetArchetypeByName(name);
        Chat = monsJson.speechbubbletext;

        if (Chat != "")
        {
            mChatDuration_mons = monsJson.speechbubbleduration;
            height = monsJson.speechbubbleheight;
            maxTime_mons = monsJson.speechbubblemaxint;
            minTime_mons = monsJson.speechbubbleminint;

            mChatDurationDecreas_mons = mChatDuration_mons;

            TalkSentence_mons = Chat.Split(';');
            mRandomTalk_mons = (int)(GameUtils.Random(0, TalkSentence_mons.Length));
            Chat = TalkSentence_mons[mRandomTalk_mons];

            ChatOn = toggleOn; //Turn on or off
        }
        else
        {
            ChatOn = false;
        }
    }

    public void NewMainQuest()
    {
        NewQuest();
        mQuestImg.sprite = NewMainQuestImage;
    }

    public void NewAdventureQuest()
    {
        NewQuest();
        mQuestImg.sprite = NewAdventureQuestImage;
    }

    public void NewEventQuest()
    {
        NewQuest();
        mQuestImg.sprite = NewEventQuestImage;
    }

    public void NewSubQuest()
    {
        NewQuest();
        mQuestImg.sprite = NewSubQuestImage;
    }

    private void NewQuest()
    {
        mQuest.SetActive(true);
        mReturnQuest.SetActive(false);
    }

    public void HideAll()
    {
        mQuest.SetActive(false);
        mReturnQuest.SetActive(false);
    }

    public void ReturnQuest()
    {
        mQuest.SetActive(false);
        mReturnQuest.SetActive(true);
    }

    public void UpdateAchorPos()
    {
        if (mCanvasPosFunc == null)
            return;

        mRectTrans.anchoredPosition = mCanvasPosFunc(mOffset_WorldSpace);
    }
    public void ScaleLabel(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
