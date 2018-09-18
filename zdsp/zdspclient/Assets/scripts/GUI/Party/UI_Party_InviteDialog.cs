using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;

public class UI_Party_InviteDialog : BaseWindowBehaviour
{
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform dataContentTransform;
    [SerializeField] GameObject dataPrefab;

    private enum InviteType { Non, Friend, Guild, Hero };

    private InviteType currentType;
    private PlayerGhost localPlayer;
    private Dictionary<string, GameTimer> cooldownTimers = new Dictionary<string, GameTimer>();  // charName->timer

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();
        localPlayer = GameInfo.gLocalPlayer;
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClearList();
        localPlayer = null;
    }

    private void OnDisable()
    {
        currentType = InviteType.Non;
    }

    private void OnDestroy()
    {
        if (GameInfo.gCombat != null && GameInfo.gCombat.mTimers != null)
        {
            foreach (var timer in cooldownTimers.Values)
                GameInfo.gCombat.mTimers.StopTimer(timer);
        }
        cooldownTimers.Clear();
    }

    private bool CanInvite(PlayerGhost player)
    {
        if (!localPlayer.IsInParty()) // not in party so can invite any player
            return true;
        else if (!localPlayer.PartyStats.IsMember(player.Name)) // is in party, so check whether this player is already my party member
            return true;
        else
            return false;
    }

    private bool CanInvite(Hero hero)
    {
        if (hero.CanSummon(localPlayer.PlayerSynStats.Level) && !hero.IsAway)
        {
            if (!localPlayer.IsInParty()) // not in party so can invite
                return true;
            else
            {
                string name = localPlayer.Name + "_" + hero.HeroId;
                return !localPlayer.PartyStats.IsMember(name);
            }
        }
        else
            return false;
    }

    public void OnToggleFriendTab(bool isOn)
    {
        if (isOn && currentType != InviteType.Friend)
        {
            currentType = InviteType.Friend;
            ClearList();
            if (localPlayer != null)
            {
                //var friendList = GameInfo.gLocalPlayer.SocialStats.GetFriendListDict().Values;
                //foreach (var friend in friendList)
                //{
                //    GameObject obj = ClientUtils.CreateChild(dataContentTransform, dataPrefab);
                //    obj.GetComponent<UI_Party_RequestData>().InitInviteData(friend.charName, friend.charLvl,
                //        (JobType)friend.jobSect, OnClickInvite);
                //}

                var netEnts = GameInfo.gCombat.mEntitySystem.GetAllNetEntities();
                foreach (var kvp in netEnts)
                {
                    PlayerGhost player = kvp.Value as PlayerGhost;
                    if (player != null && player != localPlayer && CanInvite(player))
                    {
                        GameObject obj = ClientUtils.CreateChild(dataContentTransform, dataPrefab);
                        obj.GetComponent<UI_Party_InviteData>().Init(player.Name, player.PlayerSynStats.Level,
                            (JobType)player.PlayerSynStats.jobsect, OnClickInvite);
                    }
                }
            }
        }
    }

    public void OnToggleGuildTab(bool isOn)
    {
        if (isOn && currentType != InviteType.Guild)
        {
            currentType = InviteType.Guild;
            ClearList();
        }
    }

    public void OnToggleHeroTab(bool isOn)
    {
        if (isOn && currentType != InviteType.Hero)
        {
            currentType = InviteType.Hero;
            ClearList();
            if (localPlayer != null)
            {
                var heroes = localPlayer.HeroStats.GetHeroesDict().Values;
                foreach (var hero in heroes)
                {
                    if (CanInvite(hero))
                    {
                        GameObject obj = ClientUtils.CreateChild(dataContentTransform, dataPrefab);
                        obj.GetComponent<UI_Party_InviteData>().InitHero(hero, OnClickInviteHero);
                    }
                }
            }
        }
    }

    private void OnClickInvite(string name)
    {
        if (!cooldownTimers.ContainsKey(name))
        {
            RPCFactory.CombatRPC.InviteToParty(name);
            cooldownTimers[name] = GameInfo.gCombat.mTimers.SetTimer(20000, (arg) => { cooldownTimers.Remove(name); }, null);
        }
    }

    public void OnClickInviteHero(int heroId)
    {
        RPCFactory.CombatRPC.InviteToParty("", heroId);
    }

    private void ClearList()
    {
        ClientUtils.DestroyChildren(dataContentTransform);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}