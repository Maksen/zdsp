using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Client.Entities;

public class UI_NPCTalkOld : MonoBehaviour
{
    public GameObject DialogNPC;
    public Transform NPCAvatarParent;
    public Text  NPCTitle;
    public Text  NPCMessage;
    public GameObject NPCNextButton;

    public GameObject DialogPC;
    public Model_3DAvatar PCAvatar;
    public Transform PCAvatarParent;
    public Text PCTitle;
    public Text PCMessage;
    public GameObject PCNextButton;

    public GameObject CloseButton;
    private GameObject NPCModel;
    private int mQuestId = 0;
    private int mTalkStep = 1;
    private bool mPCFirstTalk = true;
    private QuestTalkDetailJson mQuestTalkDetailJson;
    private List<UnityAction> mOnCloseButtonCB;

    void Awake()
    {
        mOnCloseButtonCB = new List<UnityAction>();
    }

    public void InitGetQuest(QuestJson questJson)
    {
        CloseButton.SetActive(false);
        mQuestId = questJson.questid;
        SetCloseButtonClickEvent(QuestCompleteStep);
        //ShowNPC(questJson.callernpc, questJson.callerdialogue, true);
    }

    public void InitQuestObjectiveTalk(QuestJson questJson, int talkid)
    {
        CloseButton.SetActive(false);
        mQuestId = questJson.questid;
        mQuestTalkDetailJson = QuestRepo.GetQuestTalkByID(talkid);
        mTalkStep = 0;
        mPCFirstTalk = true;
        SetNPCNextButtonClickEvent(GoToNext);
        SetPCNextButtonClickEvent(GoToNext);
        SetCloseButtonClickEvent(QuestCompleteStep);
        GoToNext();   
        //if(GameInfo.gLocalPlayer.QuestStats.isTraining)
        //{
        //    TrainingRealmContoller.Instance.OnTalkStart(talkid);
        //}
    }

    private void ShowNPC(int npcid, string dialogue, bool islast)
    {
        DialogPC.SetActive(false);
        DialogNPC.SetActive(true);
        StaticNPCJson staticNPCJson = StaticNPCRepo.GetStaticNPCById(npcid);
        bool needLoadModel = true;
        if (NPCModel != null && staticNPCJson!=null)
        {
            if (NPCModel.name == staticNPCJson.archetype)
                needLoadModel = false;
            else
            {
                Destroy(NPCModel);
                NPCModel = null;
            }
        }
        if (needLoadModel && staticNPCJson != null)
        {
            GameObject npcprefab = AssetManager.LoadSceneNPC(staticNPCJson.modelprefabpath);
            if (npcprefab != null)
            {
                NPCModel = Instantiate(npcprefab);
                NPCModel.name = staticNPCJson.archetype;
                NPCModel.transform.SetParent(NPCAvatarParent, false);
                PlayAnimation("talk", NPCModel);
                ClientUtils.SetLayerRecursively(NPCModel, LayerMask.NameToLayer("UI"));
                float[] camera = StaticNPCRepo.ParseCameraPosInTalk(staticNPCJson.cameraposintalk);
                Vector3 pos = NPCAvatarParent.transform.localPosition;
                NPCAvatarParent.transform.localPosition = new Vector3(camera[0], camera[1], pos.z);
                NPCAvatarParent.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
                NPCAvatarParent.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
            }
        }
        else
        {
            PlayAnimation("talk", NPCModel);
        }
            
        NPCTitle.text = staticNPCJson.localizedname;
        NPCMessage.text = dialogue;
        NPCMessage.gameObject.SetActive(false);
        NPCMessage.gameObject.SetActive(true);
        if (islast)
        {
            NPCNextButton.SetActive(false);
            CloseButton.SetActive(true);
        }
        else
            NPCNextButton.SetActive(true);
    }

    private void PlayAnimation(string animation, GameObject model)
    {
        Animation anim = model.GetComponent<Animation>();
        if (anim != null)
        {
            anim.Stop();
            PlayAnimationWithSound animSoundPlayer = model.GetComponent<PlayAnimationWithSound>();
            if (animSoundPlayer != null)
                animSoundPlayer.PlayAnimation(animation);
            else
                anim.Play(animation);
        }      
    }

    private void ShowPC(string dialogue, bool islast)
    {
        DialogPC.SetActive(true);
        DialogNPC.SetActive(false);
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        PlayerSynStats playerstats = localplayer.PlayerSynStats;
        if (mPCFirstTalk)
        {
            byte jobSect = playerstats.jobsect;
            PCAvatar.Change(GameInfo.gLocalPlayer.mEquipmentInvData, (JobType)jobSect, localplayer.mGender, (model) =>
            {
                model.GetComponent<Animator>().PlayFromStart("talk");
            });
            float[] camera = StaticNPCRepo.ParseCameraPosInTalk(JobSectRepo.GetGenderInfo(localplayer.mGender).cameraposintalk);
            Vector3 pos = NPCAvatarParent.transform.localPosition;
            PCAvatarParent.transform.localPosition = new Vector3(camera[0], camera[1], pos.z);
            PCAvatarParent.transform.localRotation = Quaternion.Euler(new Vector3(0, camera[2], 0));
            PCAvatarParent.transform.localScale = new Vector3(camera[3], camera[3], camera[3]);
            PCTitle.text = playerstats.name;
            mPCFirstTalk = false;
        }
        else
        {
            GameObject outfitModel = PCAvatar.GetOutfitModel();
            if (outfitModel != null)
                outfitModel.GetComponent<Animator>().PlayFromStart("talk");
        }
        PCMessage.text = dialogue;
        PCMessage.gameObject.SetActive(false);
        PCMessage.gameObject.SetActive(true);
        if (islast)
        {
            PCNextButton.SetActive(false);
            CloseButton.SetActive(true);
        }
        else
            PCNextButton.SetActive(true);
    }

    private void GoToNext()
    {
        mTalkStep++;
        bool isLast = mTalkStep == mQuestTalkDetailJson.steps;
        int callerid = 0;
        string dialogue = "";
        switch (mTalkStep)
        {
            case 1:
                callerid = mQuestTalkDetailJson.caller1;
                dialogue = mQuestTalkDetailJson.dialogue1;
                break;
            case 2:
                callerid = mQuestTalkDetailJson.caller2;
                dialogue = mQuestTalkDetailJson.dialogue2;
                break;
            case 3:
                callerid = mQuestTalkDetailJson.caller3;
                dialogue = mQuestTalkDetailJson.dialogue3;
                break;
            case 4:
                callerid = mQuestTalkDetailJson.caller4;
                dialogue = mQuestTalkDetailJson.dialogue4;
                break;
            case 5:
                callerid = mQuestTalkDetailJson.caller5;
                dialogue = mQuestTalkDetailJson.dialogue5;
                break;
            case 6:
                callerid = mQuestTalkDetailJson.caller6;
                dialogue = mQuestTalkDetailJson.dialogue6;
                break;
        }
        bool isnpc = callerid > 0;
        if (isnpc)
            ShowNPC(callerid, dialogue, isLast);
        else
            ShowPC(dialogue, isLast);
    }

    private void QuestCompleteStep()
    {         
        //RPCFactory.CombatRPC.QuestCompleteStep(mQuestId);
        //if (GameInfo.gLocalPlayer.QuestStats.isTraining)
        //{
        //    TrainingRealmContoller.Instance.OnTalkEnd(mQuestTalkDetailJson.id);
        //}
    }

    public void SetCloseButtonClickEvent(UnityAction call)
    {         
        CloseButton.GetComponent<Button>().onClick.AddListener(call);
        mOnCloseButtonCB.Add(call); 
    }

    public void SetNPCNextButtonClickEvent(UnityAction call)
    {
        NPCNextButton.GetComponent<Button>().onClick.AddListener(call);
    }

    public void SetPCNextButtonClickEvent(UnityAction call)
    {
        PCNextButton.GetComponent<Button>().onClick.AddListener(call);
    }

    void OnDisable()
    {
        for (int index = 0; index < mOnCloseButtonCB.Count; index++)
            CloseButton.GetComponent<Button>().onClick.RemoveListener(mOnCloseButtonCB[index]);
        mOnCloseButtonCB.Clear();
        NPCNextButton.GetComponent<Button>().onClick.RemoveAllListeners();
        PCNextButton.GetComponent<Button>().onClick.RemoveAllListeners();
        PCAvatar.Cleanup();
    }
}
