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
    private CurrentQuestData mQuestData;

    public void Init(CurrentQuestData questData)
    {
        mQuestData = questData;
        UpdateChapterList();
    }

    private void UpdateChapterList()
    {
        ChapterListClear();
        Dictionary<int, List<ChapterJson>> chapters = QuestRepo.GetChapters();
        int currentchapter = QuestRepo.GetChapterByQuestId(mQuestData.QuestId).groupid;
        foreach(KeyValuePair<int, List<ChapterJson>> chapter in chapters)
        {
            ChapterJson chapterJson = chapter.Value[0];
            GameObject chapterdetail = Instantiate(ChapterDetail);
            string progress;
            if (chapter.Key < currentchapter)
            {
                progress = chapter.Value.Count + " / " + chapter.Value.Count;
            }
            else if (chapter.Key == currentchapter)
            {
                progress = QuestRepo.GetChapterProgress(currentchapter, mQuestData.QuestId);
            }
            else
            {
                progress = "0 / " + chapter.Value.Count;
            }
            chapterdetail.GetComponent<UI_ChapterListData>().Init(chapterJson.icon, chapterJson.name, progress);
            chapterdetail.transform.SetParent(ChapterList, false);
            chapterdetail.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();
            mChapterList.Add(chapterdetail);
        }
    }

    private void ChapterListClear()
    {
        foreach (GameObject obj in mChapterList)
        {
            GameObject.Destroy(obj);
        }
        mChapterList = new List<GameObject>();
    }
}
