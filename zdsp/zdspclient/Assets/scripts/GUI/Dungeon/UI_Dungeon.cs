using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Zealot.Common;
using Zealot.Repository;

public class UI_Dungeon : MonoBehaviour
{
    [SerializeField]
    UI_ScrollRectOcclusion scrollrectOcclusion = null;
    [SerializeField]
    Transform contentDungeonTransform = null;

    [Header("Prefabs")]
    [SerializeField]
    GameObject prefabDungeonPage = null;
    [SerializeField]
    GameObject prefabDungeonData = null;

    Dictionary<DayOfWeek, List<DungeonData>> dungeonDataByDays = null;
    List<GameObject> dungeonPages = null;

    // Use this for initialization
    void Awake()
    {
        dungeonDataByDays = new Dictionary<DayOfWeek, List<DungeonData>>();
        dungeonPages = new List<GameObject>();
    }

    void OnEnable()
    {
        // Initialize dungeon data
        int todayDayIdx = (int)DateTime.Today.DayOfWeek;
        for (int i = 0; i < 5; ++i)
        {
            DungeonType dungeonType = (DungeonType)i;
            List<Dictionary<DungeonDifficulty, DungeonJson>> dungeonList = null;
            if (RealmRepo.mDungeons.TryGetValue(dungeonType, out dungeonList))
            {
                Dictionary<int, byte> dungeonDaysOpen = RealmRepo.mDungeonDaysOpen[dungeonType];
                int count = dungeonList.Count;
                for (int j = 0; j < count; ++j)
                {
                    Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryDict = dungeonList[j];
                    DungeonJson dungeonJson = dungeonStoryDict.ContainsKey(DungeonDifficulty.Easy)
                        ? dungeonStoryDict[DungeonDifficulty.Easy] : dungeonStoryDict[DungeonDifficulty.None];

                    int dayIdx = 0;
                    byte daysOpen = dungeonDaysOpen[dungeonJson.sequence];
                    for (int k = 0; k < 7; ++k)
                    {
                        dayIdx = (todayDayIdx + k) % 7;
                        if (GameUtils.IsBitSet(daysOpen, dayIdx))
                            break;
                    }
                    DayOfWeek dayOfWeek = (DayOfWeek)dayIdx;
                    if (!dungeonDataByDays.ContainsKey(dayOfWeek))
                        dungeonDataByDays[dayOfWeek] = new List<DungeonData>();

                    GameObject dungeonDataObj = Instantiate(prefabDungeonData);
                    DungeonData dungeonData = dungeonDataObj.GetComponent<DungeonData>();
                    DungeonDataState dungeonState = (dayIdx != todayDayIdx) ? DungeonDataState.NotOpen : DungeonDataState.Open;
                    dungeonData.Init(dungeonJson, daysOpen, dungeonState);
                    dungeonDataByDays[dayOfWeek].Add(dungeonData);
                }
            }
        }

        // Initialize dungeon pages
        int dungeonDataIdx = 0;
        for (int i = 0; i < 7; ++i)
        {
            DayOfWeek dayOfWeek = (DayOfWeek)((todayDayIdx + i) % 7);
            List<DungeonData> dungeonDataList = null;
            if (dungeonDataByDays.TryGetValue(dayOfWeek, out dungeonDataList))
            {
                int dataListCount = dungeonDataList.Count;
                int idx = 0;
                for (; idx < dataListCount; ++idx)
                {
                    int page = (dungeonDataIdx+idx)/3 + 1;
                    if (dungeonPages.Count < page)
                    {
                        GameObject dungeonPage = Instantiate(prefabDungeonPage);
                        dungeonPage.transform.SetParent(contentDungeonTransform, false);
                        dungeonPages.Add(dungeonPage);
                    }
                    dungeonDataList[idx].transform.SetParent(dungeonPages[page-1].transform, false);
                }
                dungeonDataIdx += idx;
            }
        }

        scrollrectOcclusion.Init();
    }

    void OnDisable()
    {
        foreach (Transform child in contentDungeonTransform)
        {
            Destroy(child.gameObject);
        }
        dungeonDataByDays.Clear();
        dungeonPages.Clear();
        scrollrectOcclusion.CleanUp();
    }
}
