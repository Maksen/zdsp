using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_DamangeRankData : MonoBehaviour {

    public Text Leaderboard_Name;
    public Text PartyPersonal_NameRank;
    public Text PartyPersonalData_Rnak;
    public Text PartyPersonalData_Name;
    public Text PartyPersonalData_DamangePartyScore;

    public GameObject RankDataContent;
    public GameObject RankDataPrefab;
    private Dictionary<int, SpecialBossStatus> RankDataList;
    private List<GameObject> bigBossObjList;
    private List<GameObject> miniBossObjList;

    UI_SpecialBoss_Detail UI_SpecialBoss_Detail;

    public void RefreshDamangeRankData(Dictionary<int, SpecialBossStatus> specialBossStatus, UI_SpecialBoss_Detail uiSpecialBossDialog)
    {
        RankDataList = specialBossStatus;

        UI_SpecialBoss_Detail = uiSpecialBossDialog;
    }

    public void InitDamangeRank()
    {
        ClearBigBossRankingData();
        ClearMiniBossRankingData();

        if (UI_SpecialBoss_Detail.BigBossCategory == true)
        {
            List<SpecialBossStatus> bigBosses = new List<SpecialBossStatus>();
            foreach (KeyValuePair<int, SpecialBossStatus> status in RankDataList)
            {
                SpecialBossJson boss = SpecialBossRepo.GetInfoById(status.Value.id);

                if (boss.category == BossCategory.BIGBOSS)
                {
                    bigBosses.Add(status.Value);
                    Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BigBossTitle");
                    PartyPersonalData_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PartyName");
                    PartyPersonalData_DamangePartyScore.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");
                    PartyPersonal_NameRank.text = GUILocalizationRepo.GetLocalizedString("wb_PartyNameRank");
                }
            }

            for (int i = 0; i < bigBosses.Count; ++i)
            {
                GameObject newBigBossRankingDataObj = Instantiate(RankDataPrefab);
                newBigBossRankingDataObj.transform.SetParent(RankDataContent.transform, false);

                RankingData rankingData = newBigBossRankingDataObj.GetComponent<RankingData>();
                rankingData.InitRanking(bigBosses[i]);

                bigBossObjList.Add(newBigBossRankingDataObj);
            }
        }

        if (UI_SpecialBoss_Detail.MiniBossCategory == true)
        {
            List<SpecialBossStatus> miniBosses = new List<SpecialBossStatus>();
            foreach (KeyValuePair<int, SpecialBossStatus> status in RankDataList)
            {
                SpecialBossJson boss = SpecialBossRepo.GetInfoById(status.Value.id);

                if (boss.category == BossCategory.BOSS)
                {
                    miniBosses.Add(status.Value);
                    Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BossTitle");
                    PartyPersonalData_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalName");
                    PartyPersonalData_DamangePartyScore.text = GUILocalizationRepo.GetLocalizedString("wb_DamangeScore");
                    PartyPersonal_NameRank.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalNameRank");
                }
            }

            for (int i = 0; i < miniBosses.Count; ++i)
            {
                GameObject newMiniBossRankingDataObj = Instantiate(RankDataPrefab);
                newMiniBossRankingDataObj.transform.SetParent(RankDataContent.transform, false);

                RankingData rankingData = newMiniBossRankingDataObj.GetComponent<RankingData>();
                rankingData.InitRanking(miniBosses[i]);

                miniBossObjList.Add(newMiniBossRankingDataObj);
            }
        }
    }

    void ClearBigBossRankingData()
    {
        if (bigBossObjList == null)
        {
            bigBossObjList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < bigBossObjList.Count; ++i)
        {
            Destroy(bigBossObjList[i]);
            bigBossObjList[i] = null;
        }
        bigBossObjList.Clear();
    }

    void ClearMiniBossRankingData()
    {
        if (miniBossObjList == null)
        {
            miniBossObjList = new List<GameObject>();
            return;
        }

        for (int i = 0; i < miniBossObjList.Count; ++i)
        {
            Destroy(miniBossObjList[i]);
            miniBossObjList[i] = null;
        }
        miniBossObjList.Clear();
    }
}
