using UnityEngine;
using Zealot.Common;

public class UI_Party_MemberData : UI_Party_RequestBase
{
    [SerializeField] GameObject leaderIcon;

    public void Init(string charName, int charLvl, int portrait, bool isLeader)
    {
        InitBase(charName, charLvl);
        //portraitImage.sprite = 
        leaderIcon.SetActive(isLeader);
    }
}