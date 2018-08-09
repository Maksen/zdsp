using UnityEngine;
using Zealot.Repository;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using UnityEngine.UI;

public class UI_ChapterList : MonoBehaviour
{
    [SerializeField]
    UI_QuestList QuestList;

    [SerializeField]
    Transform ChapterList;

    [SerializeField]
    GameObject ChapterDetail;

    private List<GameObject> mChapterList = new List<GameObject>();
    private QuestJson mQuestJson;
    private UI_QuestList mQuestList;
    private UI_MainQuest mParent;

    public void Init(QuestJson questJson, UI_QuestList questList, UI_MainQuest parent)
    {
        mQuestJson = questJson;
        mQuestList = questList;
        mParent = parent;
        UpdateChapterList();
    }

    private void UpdateChapterList()
    {
        ChapterListClear();
        Dictionary<int, List<ChapterJson>> chapters = QuestRepo.GetChapters();
        int currentchapter = QuestRepo.GetChapterByQuestId(mQuestJson.questid).groupid;
        foreach(KeyValuePair<int, List<ChapterJson>> chapter in chapters)
        {
            ChapterJson chapterJson = chapter.Value[0];
            int chapterid = chapterJson.groupid;
            GameObject chapterdetail = Instantiate(ChapterDetail);
            string progress;
            if (chapter.Key < currentchapter)
            {
                progress = chapter.Value.Count + " / " + chapter.Value.Count;
            }
            else if (chapter.Key == currentchapter)
            {
                progress = QuestRepo.GetChapterProgress(currentchapter, mQuestJson.questid);
            }
            else
            {
                progress = "0 / " + chapter.Value.Count;
            }
            chapterdetail.GetComponent<UI_ChapterListData>().Init(chapterJson.icon, chapterJson.name, progress);
            chapterdetail.transform.SetParent(ChapterList, false);
            chapterdetail.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();
            chapterdetail.GetComponent<Toggle>().isOn = chapterid == mQuestList.SelectedChapterId ? true : false;
            chapterdetail.GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnSelectChapterData(chapterid); });
            mChapterList.Add(chapterdetail);
        }
    }

    private void ChapterListClear()
    {
        foreach (GameObject obj in mChapterList)
        {
            Destroy(obj);
        }
        mChapterList = new List<GameObject>();
    }

    private void OnSelectChapterData(int chapterid)
    {
        mQuestList.SelectedChapterId = mQuestList.SelectedChapterId;
        mParent.OnChapterChanged();
    }
}
