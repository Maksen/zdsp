using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Repository;

public class PartyStatsClient : PartyStats
{
    private GameObject widgetObj;
    private HUD_PartyPortrait hudParty;
    private GameObject windowObj;
    private UI_Party uiParty;
    private bool isInit = true;
    private PlayerGhost localPlayer;

    private PlayerGhost followTarget;
    private static int followTargetID = -1;
    private static string followTargetLevel = "";
    private static Vector3 followTargetPos = Vector3.zero;
    private static string followingPlayerName = "";
    private const float FOLLOW_RANGE = 2f;
    private float rangeSqr;

    // called when create or join party
    public void Init()
    {
        rangeSqr = FOLLOW_RANGE * FOLLOW_RANGE;
        localPlayer = GameInfo.gLocalPlayer;
        widgetObj = UIManager.GetWidget(HUDWidgetType.PartyPortrait);
        hudParty = widgetObj.GetComponent<HUD_PartyPortrait>();
        windowObj = UIManager.GetWindowGameObject(WindowType.Party);
        uiParty = windowObj.GetComponent<UI_Party>();

        Debug.Log("join party id: " + partyId);
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
        {
            ShowSystemMessage("sys_party_NewPartyLeader", leaderName);
        }

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
        if (GetFollowingPlayer() == member.name)
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
    }

    public void OnFollowPartyMember(int targetPid, string playerName, string levelName, RPCPosition targetPos)
    {
        if (localPlayer == null)
            return;

        if (targetPid != -1)
        {
            followTargetID = targetPid;
            followingPlayerName = playerName;
            Vector3 targetPosition = targetPos.ToVector3();
            followTargetLevel = levelName;
            followTargetPos = targetPosition;
            Debug.LogFormat("Following: {0},{1},{2},{3}", targetPid, playerName, levelName, targetPosition);
            PathFindToTarget();
        }
        else
        {
            StopFollowTarget();
        }
    }

    public void StopFollowTarget()
    {
        Debug.LogFormat("Clear Follow Target");
        followTarget = null;
        followTargetID = -1;
        followingPlayerName = "";
        followTargetLevel = "";
        followTargetPos = Vector3.zero;
        canStart = false;
        elapsedDt = 0;
        StopMoving();
    }

    // todo: optimize so don't keep calling pathfind if target has not moved
    private void PathFindToTarget()
    {
        string currentLevelName = ClientUtils.GetCurrentLevelName();

        if (currentLevelName == followTargetLevel)
        {
            followTarget = localPlayer.EntitySystem.GetEntityByPID(followTargetID) as PlayerGhost;
            if (followTarget != null)
                followTargetPos = followTarget.Position;
            if (!IsTargetInRange(followTargetPos))
            {
                if (followTarget != null)
                {
                    localPlayer.PathFindToTarget(followTargetPos, followTarget.GetPersistentID(), FOLLOW_RANGE, true, false, null);
                    //Debug.Log("start seeking using target pid ");
                }
                else
                {
                    localPlayer.PathFindToTarget(followTargetPos, -1, FOLLOW_RANGE, true, false, null);
                    //Debug.Log("start seeking using target pos ");
                }
            }
            canStart = true;
        }
        else
        {
            bool found = false;
            Zealot.Bot.BotController.TheDijkstra.DoRouter(currentLevelName, followTargetLevel, out found);
            if (found)
            {
                Zealot.Bot.BotController.DestLevel = followTargetLevel;
                Zealot.Bot.BotController.DestMapPos = followTargetPos;
                Zealot.Bot.BotController.DestMonsterOrNPC = Zealot.Bot.ReachTargetAction.None;
                Zealot.Bot.BotController.DestArchtypeID = -1;
                localPlayer.Bot.SeekingWithRouter();
                Debug.Log("start seeking with router.");
                canStart = true;
            }
            else
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
        }
    }

    private bool IsTargetInRange(Vector3 targetPosition)
    {
        Vector3 vecToTarget = targetPosition - localPlayer.Position;
        vecToTarget.y = 0;
        return vecToTarget.sqrMagnitude <= rangeSqr;
    }

    private bool CheckTargetPosition()
    {
        //Debug.Log("++++++++++++CHECKING TARGET!!!!!!!!!!!!!");
        followTarget = localPlayer.EntitySystem.GetEntityByPID(followTargetID) as PlayerGhost;
        if (followTarget != null)
            return true;
        else
        {
            RPCFactory.CombatRPC.GetPartyMemberPosition(followingPlayerName);
            return false;
        }
    }

    public void OnGetPartyMemberPosition(string targetLevelName, RPCPosition targetPosition)
    {
        if (!string.IsNullOrEmpty(targetLevelName))
        {
            followTargetLevel = targetLevelName;
            followTargetPos = targetPosition.ToVector3();
            if (!GameInfo.gCombat.mPlayerInput.IsControlling() && canStart)
                PathFindToTarget();
        }
        else
            StopFollowTarget();
    }

    private void StopMoving()
    {
        if (localPlayer.IsMoving())
            localPlayer.ForceIdle();
    }

    private long elapsedDt = 0;
    private bool canStart = false;

    public void PauseAutoFollow()
    {
        if (IsFollowing() && canStart)
        {
            canStart = false;
            elapsedDt = 0;
        }          
    }

    public void ResumeAutoFollow()
    {
        if (IsFollowing() && !canStart)
            canStart = true;
    }

    public bool IsAutoFollowPaused()
    {
        return IsFollowing() && !canStart;
    }

    public void Update(long dt)
    {
        if (!IsFollowing())
            return;

        if (GameInfo.gCombat.mPlayerInput.IsControlling() || !localPlayer.CanMove())
        {
            elapsedDt = 0;
            return;
        }

        if (canStart)
        {
            elapsedDt += dt;
            if (elapsedDt < 1000)
                return;

            elapsedDt = 0;

            if (CheckTargetPosition())
                PathFindToTarget();

            if (followTarget != null && IsTargetInRange(followTarget.Position))
            {
                BaseClientCastSkill action = followTarget.GetAction() as BaseClientCastSkill;
                if (action != null && action.mTarget != null)
                {
                    if (localPlayer.IsIdling())
                        GameInfo.gCombat.CommonCastBasicAttack(action.mTarget.GetPersistentID());
                }
                else if (!localPlayer.IsIdling())
                    localPlayer.ForceIdle();
            }
        }
    }

    public string GetFollowingPlayer()
    {
        return followingPlayerName;
    }

    public bool IsFollowing()
    {
        return followTargetID != -1;
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