using Kopio.JsonContracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_DamangeRankData : MonoBehaviour
{
    public Text Leaderboard_Name;
    public Text PartyPersonal_NameRank;
    public Text PartyPersonalData_Name;
    public Text PartyPersonalData_DamangePartyScore;

    public int Boss_id;
    public GameObject RankDataContent;
    public GameObject Me_RankDataContent;
    public GameObject RankDataPrefab;

    private GameObject mMyRankData;

    public void GetRankingData(int boss_id)
    {
        Boss_id = boss_id;
        SpecialBossJson boss = SpecialBossRepo.GetInfoById(boss_id);
        if (boss.category == BossCategory.BIGBOSS)
        {
            Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BigBossTitle");
            PartyPersonalData_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PartyName");
            PartyPersonalData_DamangePartyScore.text = GUILocalizationRepo.GetLocalizedString("wb_PartyScore");
            PartyPersonal_NameRank.text = GUILocalizationRepo.GetLocalizedString("wb_PartyNameRank");
        }
        else
        {
            Leaderboard_Name.text = GUILocalizationRepo.GetLocalizedString("wb_BossTitle");
            PartyPersonalData_Name.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalName");
            PartyPersonalData_DamangePartyScore.text = GUILocalizationRepo.GetLocalizedString("wb_DamangeScore");
            PartyPersonal_NameRank.text = GUILocalizationRepo.GetLocalizedString("wb_PersonalNameRank");
        }

        RPCFactory.CombatRPC.GetWorldBossDmgList(Boss_id);
        UIManager.StartHourglass();
    }

    public void InitDamangeRankData(BossKillData _bossKillData)
    {
        Clear();

        if (_bossKillData != null)
        {
            string myName = GameInfo.gLocalPlayer.Name;
            SpecialBossJson boss = SpecialBossRepo.GetInfoById(Boss_id);
            if (boss.category == BossCategory.BOSS)
            {
                List<BossKillDmgRecord> dmgRecords = _bossKillData.dmgRecords;
                for (int i = 0; i < dmgRecords.Count; ++i)
                {
                    GameObject newMiniBossListObj = Instantiate(RankDataPrefab);
                    newMiniBossListObj.transform.SetParent(RankDataContent.transform, false);
                    RankingData rankingData = newMiniBossListObj.GetComponent<RankingData>();
                    rankingData.InitDmgRanking(i + 1, dmgRecords[i]);
                    if (dmgRecords[i].Name == myName)
                    {
                        if (mMyRankData == null)
                        {
                            mMyRankData = Instantiate(RankDataPrefab);
                            mMyRankData.transform.SetParent(Me_RankDataContent.transform, false);
                        }
                        mMyRankData.GetComponent<RankingData>().InitDmgRanking(i + 1, dmgRecords[i]);
                    }
                }
            }
            else
            {
                List<BossKillScoreRecord> scoreRecords = _bossKillData.scoreRecords;
                for (int i = 0; i < scoreRecords.Count; ++i)
                {
                    GameObject newMiniBossListObj = Instantiate(RankDataPrefab);
                    newMiniBossListObj.transform.SetParent(RankDataContent.transform, false);
                    RankingData rankingData = newMiniBossListObj.GetComponent<RankingData>();
                    rankingData.InitScoreRanking(i + 1, scoreRecords[i]);
                    if (scoreRecords[i].Name.Contains(myName))
                    {
                        if (mMyRankData == null)
                        {
                            mMyRankData = Instantiate(RankDataPrefab);
                            mMyRankData.transform.SetParent(Me_RankDataContent.transform, false);
                        }
                        mMyRankData.GetComponent<RankingData>().InitScoreRanking(i + 1, scoreRecords[i]);
                    }
                }
            }
        }
    }

    void Clear()
    {
        ClearBossListData();
        if (mMyRankData != null)
            mMyRankData.GetComponent<RankingData>().Clear();
    }

    void ClearBossListData()
    {
        foreach (Transform child in RankDataContent.transform)
            Destroy(child.gameObject);
    }
}
