using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zealot.Bot;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Repository;

public static class PartyFollowTarget
{
    public static bool ResumeAfterTeleport = true;
    public static bool Enabled = false;
    public static int TargetPID = -1;
    public static string TargetName = "";
    public static string DestLevel = "";
    public static Vector3 TargetPos = Vector3.zero;
    public static float Elapsed = 0;
    public static float Range = 2.5f;
    public static float RangeSqr = 6.25f;

    public static void Reset()
    {
        Enabled = false;
        TargetPID = -1;
        TargetName = "";
        TargetPos = Vector3.zero;
        DestLevel = "";
        Elapsed = 0;
    }

    public static void Remember(int pid, string name, string levelname, Vector3 position)
    {
        TargetPID = pid;
        TargetName = name;
        DestLevel = levelname;
        TargetPos = position;
    }

    public static bool IsFollowing()
    {
        return TargetPID != -1;
    }

    public static bool IsPaused()
    {
        return IsFollowing() && !Enabled;
    }

    public static void Pause(bool enableAfterTeleport = true)
    {
        if (!IsFollowing())
            return;
        if (Enabled)
        {
            Enabled = false;
            Elapsed = 0;
            ResumeAfterTeleport = enableAfterTeleport;
            //Debug.Log("PartyFollow paused: " + ResumeAfterTeleport);
        }
    }

    public static void Resume(bool immediate = false)
    {
        if (IsPaused())
        {
            Enabled = true;
            if (immediate)
                Elapsed = 1000;

            if (!ResumeAfterTeleport)
                ResumeAfterTeleport = true;
            //Debug.Log("PartyFollow resumed");
        }
    }
}

public class PartyStatsClient : PartyStats
{
    private GameObject widgetObj;
    private HUD_PartyPortrait hudParty;
    private GameObject windowObj;
    private UI_Party uiParty;
    private bool isInit = true;

    private PlayerGhost localPlayer;
    private PlayerGhost followTarget;

    public void Init()
    {
        localPlayer = GameInfo.gLocalPlayer;
        widgetObj = UIManager.GetWidget(HUDWidgetType.PartyPortrait);
        hudParty = widgetObj.GetComponent<HUD_PartyPortrait>();
        windowObj = UIManager.GetWindowGameObject(WindowType.Party);
        uiParty = windowObj.GetComponent<UI_Party>();

        //Debug.Log("join party id: " + partyId);
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] != null)
            {
                string info = (string)members[i];
                AddPartyMember(info);
            }
        }

        for (int i = 0; i < requests.Count; i++)
        {
            if (requests[i] != null)
            {
                string info = (string)requests[i];
                AddPartyRequest(info);
            }
        }

        UpdatePartyLeader(leader);
        UpdatePartySetting(partySetting);

        isInit = false;
    }

    public void UpdateMemberList(byte idx, string value)
    {
        PartyMember member = mPartyMembers.Values.FirstOrDefault(x => x.slotIdx == idx);
        if (member == null)  // currently not in list so is new member
        {
            if (!string.IsNullOrEmpty(value))
                AddPartyMember(value);
        }
        else
        {
            if (string.IsNullOrEmpty(value))  // current member info removed
                RemovePartyMember(member);
            else  // member info updated
                UpdatePartyMember(member, value);
        }
    }

    private void AddPartyMember(string str)
    {
        //Debug.Log("Add member: " + str);
        PartyMember member = PartyMember.ToObject(str);
        mPartyMembers[member.name] = member;

        if (!isInit)
        {
            ShowSystemMessage("sys_party_MemberJoinedParty", member.GetName());

            if (windowObj.activeInHierarchy)
                uiParty.AddPartyMember(member);
        }

        if (widgetObj.activeInHierarchy)
            hudParty.AddPartyMember(member);
    }

    private void UpdatePartyMember(PartyMember oldMember, string str)
    {
        //Debug.Log("Update member: " + str);
        bool oldOnlineStatus = oldMember.online;

        PartyMember newMember = PartyMember.ToObject(str);
        if (oldMember.IsHero())
        {
            if (oldMember.name != newMember.name)  // changed hero
                mPartyMembers.Remove(oldMember.name);
        }

        mPartyMembers[newMember.name] = newMember;

        if (oldOnlineStatus != newMember.online)  // show system message if online status changed
        {
            if (!newMember.online)
                OnMemberOffline(newMember);

            string msg = newMember.online ? "sys_party_MemberOnline" : "sys_party_MemberOffline";
            ShowSystemMessage(msg, newMember.GetName());
        }

        if (widgetObj.activeInHierarchy)
            hudParty.UpdatePartyMember(newMember);

        if (windowObj.activeInHierarchy)
            uiParty.UpdatePartyMember(newMember);
    }

    private void RemovePartyMember(PartyMember member)
    {
        //Debug.Log("Remove member: " + member.name);
        mPartyMembers.Remove(member.name);

        ShowSystemMessage("sys_party_MemberLeftParty", member.GetName());

        if (widgetObj.activeInHierarchy)
            hudParty.RemovePartyMember(member);

        if (windowObj.activeInHierarchy)
            uiParty.RemovePartyMember(member);
    }

    public void UpdateRequestList(byte idx, string value)
    {
        PartyRequest request = mPartyRequests.Values.FirstOrDefault(x => x.slotIdx == idx);
        if (request == null)  // currently not in list so is new request
        {
            if (!string.IsNullOrEmpty(value))
                AddPartyRequest(value);
        }
        else
        {
            if (string.IsNullOrEmpty(value))  // current member info removed
                RemovePartyRequest(request.name);
        }
    }

    private void AddPartyRequest(string str)
    {
        PartyRequest request = PartyRequest.ToObject(str);
        mPartyRequests[request.name] = request;

        if (!isInit && IsLeader(localPlayer.Name))
        {
            GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogPartyRequestList);
            if (dialog.activeInHierarchy)
                dialog.GetComponent<UI_Party_RequestListDialog>().RefreshList();
        }
    }

    private void RemovePartyRequest(string name)
    {
        mPartyRequests.Remove(name);

        GameObject dialog = UIManager.GetWindowGameObject(WindowType.DialogPartyRequestList);
        if (dialog.activeInHierarchy)
            dialog.GetComponent<UI_Party_RequestListDialog>().RemoveRequest(name);
    }

    public void UpdatePartyLeader(string leaderName)
    {
        if (!isInit)
            ShowSystemMessage("sys_party_NewPartyLeader", leaderName);

        if (widgetObj.activeInHierarchy)
            hudParty.SetPartyLeader(leaderName);

        if (windowObj.activeInHierarchy)
            uiParty.OnUpdatePartyLeader(leaderName);
    }

    public void UpdatePartySetting(string setting)
    {
        mPartySetting = PartySetting.ToObject(setting);
        if (windowObj.activeInHierarchy)
            uiParty.OnUpdatePartySetting(mPartySetting);
    }

    private void OnMemberOffline(PartyMember member)
    {
        hudParty.OnMemberOffline(member);
        uiParty.OnMemberOffline(member);
        if (PartyFollowTarget.TargetName == member.name)
            StopFollowTarget();
    }

    public void OnLeaveParty()
    {
        hudParty.ResetAll();

        // close any party related dialogs
        for (WindowType type = WindowType.DialogPartySettings; type <= WindowType.DialogPartyInfo; type++)
        {
            if (UIManager.IsWindowOpen(type))
                UIManager.CloseWindow(type);
        }

        // close party window
        if (windowObj.activeInHierarchy)
            UIManager.CloseWindow(WindowType.Party);

        if (PartyFollowTarget.IsFollowing())
            StopFollowTarget();
    }

    public void OnFollowPartyMember(int targetPid, string playerName, string levelName, RPCPosition targetPos)
    {
        if (localPlayer == null)
            return;

        if (targetPid != -1)
        {
            Vector3 targetPosition = targetPos.ToVector3();
            PartyFollowTarget.Remember(targetPid, playerName, levelName, targetPosition);
            PartyFollowTarget.Enabled = true;
            //Debug.LogFormat("Following: {0},{1},{2},{3}", targetPid, playerName, levelName, targetPosition);
            PathFindToTarget();
        }
        else
        {
            StopFollowTarget();
        }
    }

    public void StopFollowTarget()
    {
        //Debug.LogFormat("Clear Follow Target");
        followTarget = null;
        PartyFollowTarget.Reset();
        StopMoving();
    }

    private void PathFindToTarget()
    {
        string currentLevelName = ClientUtils.GetCurrentLevelName();
        if (currentLevelName == PartyFollowTarget.DestLevel)  // in same level as target
        {
            followTarget = localPlayer.EntitySystem.GetEntityByPID(PartyFollowTarget.TargetPID) as PlayerGhost;
            if (followTarget != null)  // can see target
                PartyFollowTarget.TargetPos = followTarget.Position;
            if (!IsTargetInRange(PartyFollowTarget.TargetPos))
            {
                if (followTarget != null)
                {
                    localPlayer.PathFindToTarget(followTarget.Position, followTarget.GetPersistentID(), PartyFollowTarget.Range - 0.5f, true, false, null);
                    //Debug.Log("start seeking using target pid ");
                }
                else
                {
                    localPlayer.PathFindToTarget(PartyFollowTarget.TargetPos, -1, PartyFollowTarget.Range - 0.5f, true, false, null);
                    //Debug.Log("start seeking using target pos ");
                }
            }
        }
        else  // need to cross level to reach target
        {
            bool found = false;
            BotController.TheDijkstra.DoRouter(currentLevelName, PartyFollowTarget.DestLevel, out found);
            if (found)
            {
                BotController.DestLevel = PartyFollowTarget.DestLevel;
                BotController.DestMapPos = PartyFollowTarget.TargetPos;
                BotController.DestAction = ReachTargetAction.PartyFollow;
                BotController.DestArchtypeID = -1;
                localPlayer.Bot.SeekingWithRouter();
                //Debug.Log("start seeking with router.");
            }
            else
            {
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
                StopFollowTarget();
            }
        }
    }

    private bool IsTargetInRange(Vector3 targetPosition)
    {
        Vector3 diff = targetPosition - localPlayer.Position;
        diff.y = 0;
        return diff.sqrMagnitude <= PartyFollowTarget.RangeSqr;
    }

    private bool CheckTargetPosition()
    {
        followTarget = localPlayer.EntitySystem.GetEntityByPID(PartyFollowTarget.TargetPID) as PlayerGhost;
        if (followTarget != null)
        {
            if ((followTarget.Position - PartyFollowTarget.TargetPos).sqrMagnitude > 16) //Target has moved more than 4m
            {
                //Debug.LogWarning("local target moved, pathfind again");
                return true;
            }
            else
            {
                if (localPlayer.IsIdling())
                {
                    //Debug.Log("local target still there but idling so pathfind");
                    return true;
                }
                else
                {
                    //Debug.Log("local target still there I already moving");
                    return false;
                }
            }
        }
        else
        {
            RPCFactory.CombatRPC.GetPartyMemberPosition(PartyFollowTarget.TargetName);
            return false;
        }
    }

    public void OnGetPartyMemberPosition(string targetLevelName, RPCPosition targetPosition, int pid)
    {
        if (!string.IsNullOrEmpty(targetLevelName))
        {
            if (pid > 0)
                PartyFollowTarget.TargetPID = pid;
            Vector3 newPos = targetPosition.ToVector3();
            if (PartyFollowTarget.DestLevel != targetLevelName)  // target moved to another level so do new pathfind
            {
                //Debug.LogWarning("target changed level, pathfind again");
                PartyFollowTarget.DestLevel = targetLevelName;
                PartyFollowTarget.TargetPos = newPos;
                if (PartyFollowTarget.Enabled)
                    PathFindToTarget();
            } 
            else if (PartyFollowTarget.DestLevel == targetLevelName && (newPos - PartyFollowTarget.TargetPos).sqrMagnitude > 16) //Target has moved more than 4m
            {
                PartyFollowTarget.DestLevel = targetLevelName;
                PartyFollowTarget.TargetPos = newPos;
                if (ClientUtils.GetCurrentLevelName() == targetLevelName) // is at same level as target, do new pathfind
                {
                    //Debug.LogWarning("target moved, pathfind again");
                    if (PartyFollowTarget.Enabled)
                        PathFindToTarget();
                }
                else if (localPlayer.IsIdling())
                {
                    //Debug.LogWarning("target moved, but I idling, so pathfind again");
                    if (PartyFollowTarget.Enabled)
                        PathFindToTarget();
                }
                //else
                    //Debug.Log("target moved but I already moving to that level so continue");
            }
            else if (localPlayer.IsIdling())
            {
                //Debug.Log("target still there but idling so pathfind");
                if (PartyFollowTarget.Enabled)
                    PathFindToTarget();
            }
        }
    }

    private void StopMoving()
    {
        if (localPlayer.IsMoving())
            localPlayer.ForceIdle();
    }

    public void Update(long dt)
    {
        if (!PartyFollowTarget.IsFollowing())
            return;

        if (GameInfo.gCombat.mPlayerInput.IsControlling() || !localPlayer.CanMove())
        {
            PartyFollowTarget.Elapsed = 0;
            return;
        }

        if (PartyFollowTarget.Enabled)
        {
            PartyFollowTarget.Elapsed += dt;
            if (PartyFollowTarget.Elapsed < 1000)
                return;
            PartyFollowTarget.Elapsed = 0;

            if (CheckTargetPosition())
                PathFindToTarget();

            if (followTarget != null && IsTargetInRange(followTarget.Position))
            {
                BaseClientCastSkill action = followTarget.GetAction() as BaseClientCastSkill;
                if (followTarget.IsAlive() && action != null && action.mTarget != null)
                {
                    if (localPlayer.IsIdling())
                    {
                        // todo: to be replaced with botting with skill selection
                        GameInfo.gCombat.CommonCastBasicAttack(action.mTarget.GetPersistentID());
                    }
                }
                else if (!localPlayer.IsIdling())
                    localPlayer.ForceIdle();
            }
        }
    }

    private void ShowSystemMessage(string msg, string value = "")
    {
        Dictionary<string, string> param = null;
        if (!string.IsNullOrEmpty(value))
        {
            param = new Dictionary<string, string>();
            param.Add("name", value);
        }
        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName(msg, param));
    }
}