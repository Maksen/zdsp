using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Entities;
using Zealot.Bot;
using Kopio.JsonContracts;

public class UI_DialogItemDetail_WhereToGetSub : MonoBehaviour
{
    [SerializeField]
    GameObject mWhereToGetPrefab;
    [SerializeField]
    GameObject mBtnParent;

    List<UI_DialogItemDetailTooltip_WhereToGetBtn> mOriginSourceLst = new List<UI_DialogItemDetailTooltip_WhereToGetBtn>();

    public void Init(string npcOriginStr)
    {
        if (npcOriginStr.Length == 0)
            return;

        for (int i = 0; i < mOriginSourceLst.Count; ++i)
            mOriginSourceLst[i].Clear();

        string[] archetypeLst = npcOriginStr.Split(';');
        UI_DialogItemDetailTooltip_WhereToGetBtn btn = null;
        foreach (string a in archetypeLst)
        {
            Kopio.JsonContracts.StaticNPCJson snpc = StaticNPCRepo.GetNPCByArchetype(a);
            bool newbtn = CreateButton(out btn);
            btn.Name = snpc.localizedname;
            btn.LevelName = string.Empty;
            btn.MapName = string.Empty;
            btn.Portrait = ClientUtils.LoadIcon(snpc.portraitpath);
            btn.ButtonAction = () =>
            {
                //Botcontroller to npc position (inter-map)
                FindNPC(snpc.archetype);
                UIManager.CloseAllDialogs();
                UIManager.CloseAllWindows();
            };

            if (newbtn)
                mOriginSourceLst.Add(btn);
        }
    }

    private void FindNPC(string archetype)
    {
        if (archetype == string.Empty || GameInfo.gLocalPlayer == null)
            return;

        string destinationLvl = string.Empty;
        Vector3 npcPos = Vector3.zero;

        if (!NPCPosMap.FindNearestStaticNPC(archetype, ClientUtils.GetCurrentLevelName(), GameInfo.gLocalPlayer.Position, ref destinationLvl, ref npcPos))
        {
            Debug.LogError(string.Format("HUD_MapController.PathFindToNPC: Cannot find npc via archetype={0}", archetype));
            return;
        }

        StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCByArchetype(archetype);
        if (staticNPCJson == null)
            return;

        if (ClientUtils.GetCurrentLevelName() == destinationLvl)
        {
            GameInfo.gLocalPlayer.ProceedToTarget(npcPos, staticNPCJson.id, CallBackAction.Interact);
        }
        else
        {
            bool npcFound = false;
            BotController.TheDijkstra.DoRouter(ClientUtils.GetCurrentLevelName(), destinationLvl, out npcFound);
            if (!npcFound)
            {
                Debug.LogError(string.Format("UI_DialogItemDetail_WhereToGetSub.FindNPC: Cannot find npc of archetype={0}", archetype));
                return;
            }

            BotController.DestLevel = destinationLvl;
            BotController.DestMapPos = npcPos;
            BotController.DestAction = ReachTargetAction.NPC_Interact;
            BotController.DestArchtypeID = staticNPCJson.id;
            GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
        }
    }

    private bool CreateButton(out UI_DialogItemDetailTooltip_WhereToGetBtn btn)
    {
        for (int i = 0; i < mOriginSourceLst.Count; ++i)
        {
            if (!mOriginSourceLst[i].gameObject.GetActive())
            {
                mOriginSourceLst[i].gameObject.SetActive(true);
                btn = mOriginSourceLst[i];
                return false;
            }
        }

        GameObject obj = Instantiate(mWhereToGetPrefab, Vector3.zero, Quaternion.identity);
        UI_DialogItemDetailTooltip_WhereToGetBtn com = obj.GetComponent<UI_DialogItemDetailTooltip_WhereToGetBtn>();
        obj.transform.SetParent(mBtnParent.transform, false);
        btn = com;

        return true;
    }

    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }
}
