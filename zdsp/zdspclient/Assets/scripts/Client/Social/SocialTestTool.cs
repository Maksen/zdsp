using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;

using Zealot.Common.Entities.Social;

public class SocialTestTool : MonoBehaviour {
    FriendType SocialMenuType= FriendType.Good;
    string input;
    string SocialTestMenuType;

    string OtherPlayerName;

    bool waiting = false;

    public static void DoAction( System.Action<SocialTestTool> act)
    {
        var testTool = FindObjectOfType<SocialTestTool>();
        if (testTool != null && act != null)
        {
            act(testTool);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
        Debug.Log(player.SocialStats.data.Root.ToString());
        Debug.LogFormat("[Social] #Count goodFriends:{0} blackFriends:{1} requestFriends:{2} recommandFriends.Count:{3} goodFriendStates.Count:{4} "
            , player.SocialStats.data.goodFriends.Count
            , player.SocialStats.data.blackFriends.Count
            , player.SocialStats.data.requestFriends.Count
            , player.SocialStats.data.recommandFriends.Count
            , player.SocialStats.data.goodFriendStates.Count);
        waiting = false;
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
        Debug.Log("[Social] Method:SocialRemoveGood_Completed " + " Code:" + code.ToString());
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
            SocialMenuType = FriendType.Recommand;
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

        }

        if (GUI.Button(new Rect(startx, starty - 30, 100, 30), new GUIContent("關閉社群選單")))
        {
            CloseMenu();
        }



        

        offsety += 40;

        switch (SocialMenuType)
        {
            case FriendType.Good:
                {
                    var friends = player.SocialStats.data.getFriends(SocialMenuType);
                    var states = player.SocialStats.data.goodFriendStates;
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30),new GUIContent(friend.name));
                        offsetx += 90;
                        if (i < states.Count)
                        {
                            GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(states[i].online?"上線":"離線"));
                            offsetx += 50;
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
                    var friends = player.SocialStats.data.getFriends(SocialMenuType);
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 90;
                        if (GUI.Button(new Rect(offsetx, offsety, 80, 30), new GUIContent("刪除")))
                        {

                        }
                        offsety += 40;
                    }
                }
                break;
            case FriendType.Request:
                {
                    var friends = player.SocialStats.data.getFriends(SocialMenuType);
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 70;
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
            case FriendType.Recommand:
                {
                    var friends = player.SocialStats.data.getFriends(SocialMenuType);
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var friend = friends[i];

                        offsetx = 25;
                        GUI.Label(new Rect(offsetx, offsety, 80, 30), new GUIContent(friend.name));
                        offsetx += 70;
                        if (GUI.Button(new Rect(offsetx, offsety, 60, 30), new GUIContent("申請")))
                        {

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

        }
    }
}
