using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common.Entities.Social;
using Zealot.Common.Datablock;
using System;
using Zealot.Repository;

public class SocialTestTool : MonoBehaviour {
    FriendType SocialMenuType = FriendType.Good;
    string input;
    string SocialTestMenuType;

    string OtherPlayerName;

    bool waiting = false;
    bool forceHide;
    bool debugMode=true,debugDetail = false;

    public static void DoAction(System.Action<SocialTestTool> act)
    {
        var testTool = FindObjectOfType<SocialTestTool>();
        if (testTool != null && act != null)
        {
            act(testTool);
        }
    }

    // Use this for initialization
    void Start() {
        var asset = Resources.LoadAll<TextAsset>("SocialTestConfig");

        forceHide = (asset == null || asset.Length == 0);

    }

    // Update is called once per frame
    void Update() {

    }

    

    /// <summary>
    /// 排序好友名單，依據 [上線] => [等級高] => [加入時間較早] 排序 優先權越高者排越靠近0
    /// </summary>
    public static int[] GetSortedIndices(SocialFriendList friends,SocialFriendStateList states)
    { 
        int[] indices = new int[ Math.Min( friends.Count, states.Count)];

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
            Debug.LogFormat("[Social] #Count goodFriends:{0} blackFriends:{1} requestFriends:{2} tempFriends.Count:{3} goodFriendStates.Count:{4} "
                , player.SocialStats.data.goodFriends.Count
                , player.SocialStats.data.blackFriends.Count
                , player.SocialStats.data.requestFriends.Count
                , player.SocialStats.data.tempFriends.Count
                , player.SocialStats.data.goodFriendStates.Count);
        waiting = false;
        if (debugMode)
            Debug.Log("[Social] Method:OpenSocialMenu_Completed");
        SocialTestMenuType = "SocialMenu";
    }

    public void SocialRaiseRequest(string name)
    {
        waiting = true;
        RPCFactory.NonCombatRPC.SocialRaiseRequest(name);
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
        if (forceHide)
            return;
        if (waiting)
        {
            GUI.Label(new Rect(startx, starty,200,30),new GUIContent("Please wait..."));
            return;
        }

        switch (SocialTestMenuType)
        {
            case "OtherPlayerMenu":
                if (OtherPlayerName == null)
                    OtherPlayerName = string.Empty;
                OtherPlayerMenu();
                break;
            case "SocialMenu":
                SocialMenu();
                break;

            default:
                if (GUI.Button(new Rect(startx, starty-30, 120, 30), new GUIContent("開啟社群選單")))
                {
                    OpenSocialMenu();
                }

                break;
        }
    }




    void SocialMenu()
    {
        int offsetx= startx, offsety = starty;
        PlayerGhost player = GameInfo.gLocalPlayer;

        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("好友清單")))
        {
            SocialMenuType = FriendType.Good;
        }
        offsetx += 140;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("黑名單")))
        {
            SocialMenuType = FriendType.Black;
        }
        offsetx += 140;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("好友申請")))
        {
            SocialMenuType = FriendType.Request;
        }
        offsetx += 140;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("臨時好友")))
        {
            SocialMenuType = FriendType.Temp;
        }
        offsetx = 20;
        offsety += 40;

        input = GUI.TextField(new Rect(offsetx, offsety, 120, 30), input);

        offsetx += 140;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("新增好友")))
        {
            SocialRaiseRequest(input);
        }

        offsetx += 140;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("加入黑名單")))
        {
            SocialAddBlack(input);
        }

        if (GUI.Button(new Rect(startx, starty - 30, 100, 30), new GUIContent("關閉社群選單")))
        {
            CloseMenu();
        }



        

        offsety += 40;

        var friends = player.SocialStats.data.getFriends(SocialMenuType);
        var states = player.SocialStats.data.getFriendStates(SocialMenuType);

        switch (SocialMenuType)
        {
            case FriendType.Good:
                {
                    var sortIndices = GetSortedIndices(friends, states);

                    for (int i = 0; i < sortIndices.Length; i++)
                    {
                        int index = sortIndices[i];
                        var friend = friends[index];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30),new GUIContent(friend.name));
                        offsetx += 90;
                        if (index < states.Count)
                        {
                            var state = states[index];
                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(state.online ? "上線" : state.offlineTime));
                            offsetx += 40;

                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + state.progressLevel.ToString()));
                            offsetx += 40;
                        }
                        if ( GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("刪除")))
                        {
                            SocialRemoveGood(friend.name);
                        }
                        offsety += 40;
                    }
                }
                break;
            case FriendType.Black:
                {
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 90;

                        if (i < states.Count)
                        {
                            var state = states[i];
                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + state.progressLevel.ToString()));
                            offsetx += 40;
                        }

                        if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("刪除")))
                        {
                            SocialRemoveBlack(friend.name);
                        }
                        offsety += 40;
                    }
                }
                break;
            case FriendType.Request:
                {
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 70;

                        if (i < states.Count)
                        {
                            var state = states[i];
                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + state.progressLevel.ToString()));
                            offsetx += 40;
                        }

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
                }
                break;
            case FriendType.Temp:
                {
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 70;

                        if (i < states.Count)
                        {
                            var state = states[i];
                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent("lv" + state.progressLevel.ToString()));
                            offsetx += 40;
                        }

                        if (GUI.Button(new Rect(offsetx, offsety, 60, 30), new GUIContent("申請")))
                        {
                            SocialRaiseRequest(friend.name);
                        }
                        offsety += 40;
                    }
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
            SocialRaiseRequest(OtherPlayerName);
        }
        offsety += 40;
        if (GUI.Button(new Rect(offsetx, offsety, 120, 30), new GUIContent("黑名單")))
        {
            SocialAddBlack(OtherPlayerName);
        }
    }
}
