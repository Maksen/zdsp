using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_WorldBossRanking_Data : MonoBehaviour {

    public Text Rank;
    public Text Name;
    public Text Score;

    public void InitDmgRanking(int rank, BossKillDmgRecord mdmgRecords)
    {
        Rank.text = rank.ToString();
        Name.text = mdmgRecords.Name;
        Score.text = mdmgRecords.Score.ToString();
    }

    public void InitScoreRanking(int rank, BossKillScoreRecord mscoreRecords)
    {
        Rank.text = rank.ToString();
        Name.text = mscoreRecords.Name[0];
        Score.text = mscoreRecords.Score.ToString();
    }

    public void Clear()
    {
        Rank.text = "-";
        Name.text = "-";
        Score.text = "-";
    }
}
