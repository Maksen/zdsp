using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class RankingData : MonoBehaviour {

    public Text Rank;
    public Text Name;
    public Text Score;

    public void RefreshRankingData(Dictionary<int, SpecialBossStatus> specialBossStatus)
    {
        foreach (KeyValuePair<int, SpecialBossStatus> status in specialBossStatus)
        {
            InitRanking(status.Value);
        }

        UIManager.StopHourglass();
    }

    public void InitRanking(SpecialBossStatus bossStatus)
    {

    }

    void OnDestroy()
    {
        Rank = null;
        Name = null;
        Score = null;
    }

}
