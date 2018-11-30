using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common.Entities.Social;
using System;

public class SocialTestTool : MonoBehaviour {

    [Range(4, 20)]
    public int PageItemCount = 6;



    FriendType SocialMenuType = FriendType.Good;
    string input;
    string SocialTestMenuType;

    string OtherPlayerName;

    bool waiting = false;
    bool forceHide;
    bool debugMode=true,debugDetail = false;
    int pageIndex;

    bool ui_found=false;

    public static void DoAction(System.Action<SocialTestTool> act)
    {
        var testTool = FindObjectOfType<SocialTestTool>();
        if (testTool != null && act != null)
        {
            act(testTool);
        }
    }

    private void Awake()
    {

    }

    // Use this for initialization
    void Start() {
        var asset = Resources.LoadAll<TextAsset>("SocialTestConfig");

        forceHide = (asset == null || asset.Length == 0);

    }

    // Update is called once per frame
    void Update()
    {
    }


    public static int[] GetReversedIndices(int count)
    {
        int[] rlt = new int[count];
        for (int i = 0; i < count; i++)
            rlt[i] = count - i-1;
        return rlt;
    }

    /// <summary>
    /// 排序好友名單，依據 [上線] => [等級高] => [加入時間較早] 排序 優先權越高者排越靠近0
    /// </summary>
    public static int[] GetSortedIndices(SocialFriendStateList states)
    {
        int[] indices = new int[states.Count];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        Array.Sort(indices, (x, y) =>
        {
            SocialFriendState sx = states[x], sy = states[y];
            int v;
            v = sy.online.CompareTo(sx.online);
            if (v != 0)
                return v;
            v = sy.progressLevel.CompareTo(sx.progressLevel);
            if (v != 0)
                return v;
            return x.CompareTo(y);
        });

        return indices;
    }

    public void OpenSocialMenu()
    {
        input = string.Empty;
        waiting = true;
        RPCFactory.NonCombatRPC.SocialOnOpenFriendsMenu();
    }

    public void OpenSocialMenu_Completed()
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (debugMode)
            Debug.Log(player.SocialStats.data.Root.ToString());

        if (debugDetail)
            Debug.LogFormat("[Social] #Count good:{0} black:{1} request:{2} temp:{3}"
                , player.SocialStats.data.goodFriendStates.Count
                , player.SocialStats.data.blackFriendStates.Count
                , player.SocialStats.data.requestFriendStates.Count
                , player.SocialStats.data.tempFriendStates.Count);
        waiting = false;
        if (debugMode)
            Debug.Log("[Social] Method:OpenSocialMenu_Completed");
        SocialTestMenuType = "SocialMenu";
    }

    public void SocialRaiseRequest(string name,bool fromTemp)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRaiseRequest(name, fromTemp);
    }

    public void SocialRaiseRequest_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialRaiseRequest_Completed " + " Code:" + code.ToString());
        waiting = false;

    }

    public void SocialAcceptRequest(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialAcceptRequest(name);
    }

    public void SocialAcceptRequest_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialAcceptRequest_Completed "+" Code:" + code.ToString());
        waiting = false;
    }

    public void SocialRejectRequest(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRejectRequest(name);
    }

    public void SocialRejectRequest_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social]: Method:SocialRejectRequest_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialRemoveGood(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRemoveGood(name);
    }

    public void SocialRemoveGood_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialRemoveGood_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialAddBlack(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialAddBlack(name);
    }

    public void SocialAddBlack_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialAddBlack_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialRemoveBlack(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRemoveBlack(name);
    }

    public void SocialRemoveBlack_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialRemoveBlack_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialAcceptAllRequest()
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialAcceptAllRequest();
    }

    public void SocialAcceptAllRequest_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialAcceptAllRequest_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialRejectAllRequest()
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRejectAllRequest();
    }

    public void SocialRejectAllRequest_Completed(SocialResult code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialRejectAllRequest_Completed " + " Code:" + code.ToString());
        waiting = false;
    }

    public void SocialRaiseAllTempRequest()
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRaiseAllTempRequest();
    }

    public void SocialRaiseAllTempRequest_Completed()
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialRaiseAllTempRequest_Completed ");
        waiting = false;
    }

    public void SocialClearTemp()
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialClearTemp();
    }

    public void SocialClearTemp_Completed()
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialClearTemp_Completed ");
        waiting = false;
    }

    public void SocialTest_AddTempFriendsSingle(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialTest_AddTempFriendsSingle(name);
    }

    public void SocialTest_AddTempFriendsSingle_Completed(SocialAddTempFriends_Result code)
    {
        if (debugMode)
            Debug.Log("[Social] Method:SocialTest_AddTempFriendsSingle_Completed " + " Code:" + code.ToString());
        waiting = false;

    }

    public void OpenOtherPlayerMenu(string name)
    {
        input = string.Empty;
        OtherPlayerName = name;
        SocialTestMenuType = "OtherPlayerMenu";
    }

    public void CloseMenu()
    {
        SocialTestMenuType = string.Empty;
    }

    int startx = 20;
    int starty = 200;


    private void OnGUI()
    {
        if (waiting)
        {
            GUI.Label(new Rect(startx, starty,200,30),new GUIContent("Please wait..."));
            return;
        }
        if (forceHide)
            return;

        int offsetx = startx, offsety = starty-30;

        switch (SocialTestMenuType)
        {
            //case "OtherPlayerMenu":
            //    if (OtherPlayerName == null)
            //        OtherPlayerName = string.Empty;
            //    OtherPlayerMenu();
            //    break;
            case "SocialMenu":
                SocialMenu();
                break;

            case "AddTempTest":
                AddTempTest();
                break;

            default:
                if (GUI.Button(new Rect(offsetx, offsety, 100, 30), new GUIContent("開啟社群選單")))
                {
                    OpenSocialMenu();
                }
                offsetx += 110;
                if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("AddTempTest")))
                {
                    SocialTestMenuType = "AddTempTest";
                }

                break;
        }
    }

    void AddTempTest()
    {
        int offsetx = startx, offsety = starty;

        input = GUI.TextField(new Rect(offsetx, offsety, 120, 30), input);

        offsetx += 130;

        if (GUI.Button(new Rect(offsetx, offsety, 200, 30), new GUIContent("加入臨時清單(測試用)")))
        {
            SocialTest_AddTempFriendsSingle(input);
        }

        offsetx = startx;
        offsety += 40;
        if (GUI.Button(new Rect(offsetx, offsety, 200, 30), new GUIContent("關閉AddTempTest")))
        {
            CloseMenu();
        }
    }


    static Dictionary<FriendType, string> pageItems = new Dictionary<FriendType, string>() {
        { FriendType.Good, "好友清單" },
        { FriendType.Black, "黑名單" },
        { FriendType.Request, "好友申請" },
        { FriendType.Temp, "臨時好友" },
    };

    void GetFromTo(IList list,out int maxPage, out int from,out int to)
    {
        maxPage = Math.Max((list.Count - 1) / PageItemCount, 0);

        if (pageIndex > maxPage)
            pageIndex = maxPage;
        if (pageIndex < 0)
            pageIndex = 0;

        from = PageItemCount * pageIndex;
        to = Math.Min(PageItemCount * (pageIndex + 1), list.Count);
    }

    void DisplayPageButtons(ref int offsetx,ref int offsety,int maxPages, IList list, int maxCount)
    {
        if (GUI.Button(new Rect(offsetx, offsety-3, 15, 25), new GUIContent("<")))
            pageIndex--;
        offsetx += 20;

        int.TryParse(GUI.TextField(new Rect(offsetx, offsety, 20, 25), pageIndex.ToString()), out pageIndex);
        offsetx += 20;
        GUI.Label(new Rect(offsetx, offsety, 20, 25), new GUIContent(string.Format("/{0}", maxPages)));
        offsetx += 25;

        if (GUI.Button(new Rect(offsetx, offsety-3, 15, 25), new GUIContent(">")))
            pageIndex++;
        offsetx += 20;

        GUI.Label(new Rect(offsetx, offsety, 70, 25), new GUIContent(string.Format("{0}/{1}", list.Count, maxCount)));
        offsetx += 80;

        offsetx = startx;
        offsety += 35;
    }

    void SocialMenu()
    {
        int offsetx = startx, offsety = starty;
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player == null)
            return;
        var data = player.SocialStats.data;

        foreach (var pair in pageItems)
        {
            if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent(pair.Value)))
            {
                if (SocialMenuType != pair.Key)
                {
                    pageIndex = 0;
                    SocialMenuType = pair.Key;
                }
            }
            offsetx += 140;
        }

        GUI.Label(new Rect(offsetx, offsety, 120, 30), new GUIContent(pageItems[SocialMenuType]));

        offsetx = 20;
        offsety += 40;

        if (SocialMenuType == FriendType.Good || SocialMenuType == FriendType.Black || SocialMenuType == FriendType.Temp)
        {
            input = GUI.TextField(new Rect(offsetx, offsety, 120, 30), input);
            offsetx += 130;
        }
        if (SocialMenuType == FriendType.Good)
        {
            if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("新增好友")))
            {
                SocialRaiseRequest(input,false);
            }
            offsetx += 130;
        }
        if (SocialMenuType == FriendType.Black)
        {
            if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("加入黑名單")))
            {
                SocialAddBlack(input);
            }
            offsetx += 130;
        }
        if (SocialMenuType == FriendType.Temp)
        {
            if (GUI.Button(new Rect(offsetx, offsety, 200, 30), new GUIContent("加入臨時清單(測試用)")))
            {
                SocialTest_AddTempFriendsSingle(input);
            }
            offsetx += 210;
        }

        if (GUI.Button(new Rect(startx, starty - 30, 100, 30), new GUIContent("關閉社群選單")))
        {
            CloseMenu();
        }
        offsetx += 100;

        offsetx = startx;
        offsety += 40;

        var states = data.getFriendStates(SocialMenuType);
        int maxPages,from, to;

        switch (SocialMenuType)
        {
            case FriendType.Good:
                {
                    var sortIndices = GetSortedIndices(states);
                    GetFromTo(sortIndices,out maxPages, out from, out to);
                    DisplayPageButtons(ref offsetx, ref offsety, maxPages, sortIndices, data.getFriendMaxCount(SocialMenuType));
                    for (int i = from; i < to; i++)
                    {
                        int index = sortIndices[i];
                        var friend = states[index];

                        offsetx = startx + 5;
                        GUI.Label(new Rect(offsetx, offsety, 120, 30), new GUIContent(friend.name));
                        offsetx += 130;

                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.online ? friend.channel : friend.offlineTime));
                        offsetx += 40;

                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + friend.progressLevel.ToString()));
                        offsetx += 40;

                        if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("刪除")))
                        {
                            SocialRemoveGood(friend.name);
                        }
                        offsety += 40;
                    }

                    if(sortIndices.Length==0)
                    {
                        GUI.Label(new Rect(offsetx, offsety, 130, 30), new GUIContent("目前沒有好友"));
                        offsety += 40;
                    }
                }
                break;
            case FriendType.Black:
                {
                    GetFromTo(states, out maxPages, out from, out to);
                    DisplayPageButtons(ref offsetx, ref offsety, maxPages, states, data.getFriendMaxCount(SocialMenuType));
                    for (int i = from; i < to; i++)
                    {
                        var friend = states[i];

                        offsetx = startx + 5;
                        GUI.Label(new Rect(offsetx, offsety, 120, 30), new GUIContent(friend.name));
                        offsetx += 130;

                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + friend.progressLevel.ToString()));
                        offsetx += 40;

                        if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("刪除")))
                        {
                            SocialRemoveBlack(friend.name);
                        }
                        offsety += 40;
                    }
                    if (states.Count== 0)
                    {
                        GUI.Label(new Rect(offsetx, offsety, 150, 30), new GUIContent("目前沒有任何黑名單玩家"));
                        offsety += 40;
                    }
                }
                break;
            case FriendType.Request:
                {
                    GetFromTo(states, out maxPages, out from, out to);
                    DisplayPageButtons(ref offsetx, ref offsety, maxPages, states, data.getFriendMaxCount(SocialMenuType));
                    for (int i = from; i < to; i++)
                    {
                        var friend = states[i];

                        offsetx = startx + 5;
                        GUI.Label(new Rect(offsetx, offsety, 120, 30), new GUIContent(friend.name));
                        offsetx += 130;

                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + friend.progressLevel.ToString()));
                        offsetx += 40;

                        if (GUI.Button(new Rect(offsetx, offsety, 60, 30), new GUIContent("同意")))
                        {
                            SocialAcceptRequest(friend.name);
                        }
                        offsetx += 70;
                        if (GUI.Button(new Rect(offsetx, offsety, 60, 30), new GUIContent("拒絕")))
                        {
                            SocialRejectRequest(friend.name);
                        }
                        offsety += 40;
                    }

                    if (states.Count == 0)
                    {
                        GUI.Label(new Rect(offsetx, offsety, 130, 30), new GUIContent("目前沒有好友申請"));
                        offsety += 40;
                    }

                    offsetx = startx + 20;
                    if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("一鍵同意")))
                    {
                        SocialAcceptAllRequest();
                    }
                    offsetx += 90;
                    if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("一鍵拒絕")))
                    {
                        SocialRejectAllRequest();
                    }
                    offsetx += 90;
                }
                break;
            case FriendType.Temp:
                {
                    var revIndices = GetReversedIndices(states.Count);
                    GetFromTo(revIndices, out maxPages, out from, out to);
                    DisplayPageButtons(ref offsetx, ref offsety, maxPages, revIndices, data.getFriendMaxCount(SocialMenuType));
                    for (int i = from; i < to; i++)
                    {
                        var index = revIndices[i];
                        var friend = states[index];

                        offsetx = startx + 5;
                        GUI.Label(new Rect(offsetx, offsety, 120, 30), new GUIContent(friend.name));
                        offsetx += 130;

                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + friend.progressLevel.ToString()));
                        offsetx += 40;

                        if (GUI.Button(new Rect(offsetx, offsety, 60, 30), new GUIContent("申請")))
                        {
                            SocialRaiseRequest(friend.name, true);
                        }
                        offsety += 40;
                    }

                    offsetx = startx + 20;
                    if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("全加好友")))
                    {
                        SocialRaiseAllTempRequest();
                    }
                    offsetx += 90;
                    if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("全部清除")))
                    {
                        SocialClearTemp();
                    }
                    offsetx += 90;

                }
                break;
        }
        
    }

    void OtherPlayerMenu()
    {
        int offsetx = startx,offsety=starty;

        GUI.Label(new Rect(offsetx, offsety, 120, 30), OtherPlayerName);
        offsety += 40;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("加入好友")))
        {
            SocialRaiseRequest(OtherPlayerName, false);
        }
        offsety += 40;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("黑名單")))
        {
            SocialAddBlack(OtherPlayerName);
        }
    }
}
