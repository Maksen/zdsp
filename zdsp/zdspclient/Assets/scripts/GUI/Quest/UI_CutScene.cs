using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class UI_CutScene : MonoBehaviour
{
    [SerializeField]
    GameObject CutsceneData;

    [SerializeField]
    Transform CutsceneContent;

    [SerializeField]
    Text Progress;

    private List<GameObject> mCutsceneList;
    private QuestClientController mQuestController;
    private List<int> mWonderfulList;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        mCutsceneList = new List<GameObject>();
        mQuestController = GameInfo.gLocalPlayer.QuestController;
        mWonderfulList = mQuestController.GetWonderfulList();
        UpdateCutscene();
    }

    private void OnDisable()
    {
        ClearCutscene();
    }

    private void UpdateCutscene()
    {
        ClearCutscene();
        Dictionary<int, WonderfulJson> wonderfullist = QuestRepo.GetWonderful();
        foreach(KeyValuePair<int, WonderfulJson> entry in wonderfullist)
        {
            bool unlocked = mWonderfulList.Contains(entry.Key);
            GameObject cutscene = Instantiate(CutsceneData);
            cutscene.GetComponent<UI_CutsceneData>().Init(entry.Value, unlocked);
            cutscene.transform.SetParent(CutsceneContent, false);
            mCutsceneList.Add(cutscene);
        }
        Progress.text = mWonderfulList.Count + " / " + QuestRepo.GetWonderfulCount();
    }

    private void ClearCutscene()
    {
        if (mCutsceneList != null)
        {
            foreach (GameObject obj in mCutsceneList)
            {
                Destroy(obj);
            }
            mCutsceneList = new List<GameObject>();
        }
    }
}
