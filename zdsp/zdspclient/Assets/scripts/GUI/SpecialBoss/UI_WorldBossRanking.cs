using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_WorldBossRanking : MonoBehaviour
{
    public Text Leaderboard_Name;
    public Text Header_Name;
    public Text Header_Score;
    public Text Header_MyRank;
    public Transform RankDataContent;
    public Transform Me_RankDataContent;
    public GameObject RankDataPrefab;

    private GameObject mMyRankData;

    public void Init(BossKillData _bossKillData)
    {
        Clear();

        if (_bossKillData != null)
        {
            string myName = GameInfo.gLocalPlayer.Name;
            SpecialBossJson boss = SpecialBossRepo.GetInfoById(_bossKillData.bossId);
            if (boss.bosstype == BossType.Boss)
            {
                Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BossTitle");
                Header_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalName");
                Header_Score.text = GUILocalizationRepo.GetLocalizedString("wb_DamageScore");
                Header_MyRank.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalNameRank");

                List<BossKillDmgRecord> dmgRecords = _bossKillData.dmgRecords;
                for (int i = 0; i < dmgRecords.Count; ++i)
                {
                    GameObject rankDataObj = Instantiate(RankDataPrefab);
                    rankDataObj.transform.SetParent(RankDataContent, false);
                    UI_WorldBossRanking_Data rankingData = rankDataObj.GetComponent<UI_WorldBossRanking_Data>();
                    rankingData.InitDmgRanking(i + 1, dmgRecords[i]);
                    if (dmgRecords[i].Name == myName)
                    {
                        if (mMyRankData == null)
                        {
                            mMyRankData = Instantiate(RankDataPrefab);
                            mMyRankData.transform.SetParent(Me_RankDataContent.transform, false);
                        }
                        mMyRankData.GetComponent<UI_WorldBossRanking_Data>().InitDmgRanking(i + 1, dmgRecords[i]);
                    }
                }
            }
            else
            {
                Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BigBossTitle");
                Header_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PartyName");
                Header_Score.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");
                Header_MyRank.text = GUILocalizationRepo.GetLocalizedString("wb_PartyNameRank");

                List<BossKillScoreRecord> scoreRecords = _bossKillData.scoreRecords;
                for (int i = 0; i < scoreRecords.Count; ++i)
                {
                    GameObject rankDataObj = Instantiate(RankDataPrefab);
                    rankDataObj.transform.SetParent(RankDataContent, false);
                    UI_WorldBossRanking_Data rankingData = rankDataObj.GetComponent<UI_WorldBossRanking_Data>();
                    rankingData.InitScoreRanking(i + 1, scoreRecords[i]);
                    if (scoreRecords[i].Name.Contains(myName))
                    {
                        if (mMyRankData == null)
                        {
                            mMyRankData = Instantiate(RankDataPrefab);
                            mMyRankData.transform.SetParent(Me_RankDataContent.transform, false);
                        }
                        mMyRankData.GetComponent<UI_WorldBossRanking_Data>().InitScoreRanking(i + 1, scoreRecords[i]);
                    }
                }
            }
        }
    }

    void Clear()
    {
        foreach (Transform child in RankDataContent.transform)
            Destroy(child.gameObject);
        if (mMyRankData != null)
            mMyRankData.GetComponent<UI_WorldBossRanking_Data>().Clear();
    }
}
