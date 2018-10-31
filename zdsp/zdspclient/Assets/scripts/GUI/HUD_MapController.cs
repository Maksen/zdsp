using Kopio.JsonContracts;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Bot;

public class MapIconGameObjectPair
{
    public Vector3 iconPos = new Vector3();
    public GameObject entity = null;

    public MapIconGameObjectPair(Vector3 _iconpos, GameObject _entity = null)
    {
        iconPos = _iconpos;
        entity = _entity;
    }

    public Vector3 EntityPos
    {
        get
        {
            if (entity != null)
                return entity.gameObject.transform.position;
            else
                return Vector3.zero;
        }
    }
}

/// <summary>
/// Pairing of NPC icon position on the map and NPC entity
/// NPC entity does not get deleted when NPC is out of camera's perspective
/// </summary>
public class StaticMapIconGameObjectPair
{
    public Vector3 iconPos = new Vector3();
    public StaticClientNPCAlwaysShow npc = null;
    public bool isShop = false;

    public StaticMapIconGameObjectPair(Vector3 _iconpos, StaticClientNPCAlwaysShow _npc)
    {
        iconPos = _iconpos;
        npc = _npc;
        CheckNpcIsShop();
    }
    public bool hasQuest()
    {
        return ((npc != null && npc.ActiveQuest < 0) ? false : true);
    }
    public bool hasQuestAvailable()
    {
        return (hasQuest() && GameInfo.gLocalPlayer.QuestController.IsQuestAvailable(npc.ActiveQuest));
    }
    public bool hasQuestToSubmit()
    {
        return (hasQuest() && GameInfo.gLocalPlayer.QuestController.IsQuestCanSubmit(npc.ActiveQuest));
    }
    public bool hasQuestCompleted()
    {
        return (hasQuest() && GameInfo.gLocalPlayer.QuestController.IsQuestCompleted(npc.ActiveQuest));
    }
    public bool GetNPCQuestType(ref QuestType type)
    {
        if (!hasQuest())
            return false;

        type = QuestRepo.GetQuestTypeByQuestId(npc.ActiveQuest);
        return true;
    }

    private void CheckNpcIsShop()
    {
        if (npc == null || npc.mArchetype == null)
        {
            Debug.LogError("HUD_MapController: StaticClientNPCAlwaysShow NPC or NPC's NPCJson is null.");
            return;
        }
        string[] allfuncStr = npc.mArchetype.npcfunction.Split(';');
        foreach (string funcStr in allfuncStr)
        {
            int npcfuncID = -1;
            if (int.TryParse(funcStr, out npcfuncID) == false)
                continue;

            //Until NPC function has an enum
            switch (npcfuncID)
            {
                //Shop
                case 1:
                //Teleport
                case 2:
                //Job change
                case 3:
                //Storage
                case 4:
                //Special item service
                case 5:
                    isShop = true;
                    return;
            }
        }
    }
}

public static class HUD_MapController
{
    public static Vector2 mWorldDim = Vector2.zero;
    static MapInfoJson mMapInfo = null;
    public static Sprite mMap = null;
    static string mCurMapName = "";
    static int mInterMapPathFindIndex = -1;

    static Coroutine mMapIconRefreshCoroutine = null;
    const float REFRESH_INTERVAL = 1f;

    public static List<Vector3> mPortalPosLst = new List<Vector3>();
    public static List<StaticMapIconGameObjectPair> mQuestNPCPosLst = new List<StaticMapIconGameObjectPair>();
    public static List<StaticMapIconGameObjectPair> mShopNPCPosLst = new List<StaticMapIconGameObjectPair>();
    public static List<Vector3> mRevivePosLst = new List<Vector3>();

    public static List<MapIconGameObjectPair> mPlayerPairLst = new List<MapIconGameObjectPair>();
    public static List<MapIconGameObjectPair> mPartyMemPairLst = new List<MapIconGameObjectPair>();
    public static List<MapIconGameObjectPair> mMonPairLst = new List<MapIconGameObjectPair>();
    public static List<MapIconGameObjectPair> mMiniBossPairLst = new List<MapIconGameObjectPair>();
    public static List<MapIconGameObjectPair> mBossPairLst = new List<MapIconGameObjectPair>();

    public static Dictionary<string, Vector3> mInterMapPathFindDic = new Dictionary<string, Vector3>();

    public static void LoadMapAsync()
    {
        if (mCurMapName == ClientUtils.GetCurrentLevelName())
            return;

        //*** Obtain the level info ***
        mCurMapName = ClientUtils.GetCurrentLevelName();
        LevelInfo lvinfo = LevelReader.GetLevel(mCurMapName);
        if (lvinfo == null)
        {
            Debug.LogError("HUD_MapController.LoadMap: Walaoeh! LevelReader.GetLevel return null");
            return;
        }

        //Stop coroutine
        if (mMapIconRefreshCoroutine != null)
        {
            GameInfo.gCombat.StopCoroutine(mMapIconRefreshCoroutine);
            mMapIconRefreshCoroutine = null;
        }

        LevelJson lvJson = LevelRepo.GetInfoByName(mCurMapName);
        if (lvJson == null)
        {
            Debug.LogError("HUD_Map.LoadMap: Walao, Cannot find LevelJson in level Repo");
            return;
        }

        mMap = null;
        ClientUtils.LoadIconAsync(lvJson.maptexture, (sprite) =>
        {
            mMap = sprite;
            CalculateScaleRatio(lvinfo);
            LoadStaticIcon(lvinfo);
            LoadNPCIcon(lvinfo);

            if (mMapIconRefreshCoroutine == null)
            {
                mMapIconRefreshCoroutine = GameInfo.gCombat.StartCoroutine(MapIconRefresh());
            }

            //Start auto-pilot if player has previously start via world map
            PathFind();
        });
    }
    public static void FreeMapController()
    {
        mCurMapName = "";

        //Stop coroutine
        if (mMapIconRefreshCoroutine != null)
        {
            GameInfo.gCombat.StopCoroutine(mMapIconRefreshCoroutine);
            mMapIconRefreshCoroutine = null;
        }

        mPlayerPairLst.Clear();
        mPartyMemPairLst.Clear();
        mMonPairLst.Clear();
        mMiniBossPairLst.Clear();
        mBossPairLst.Clear();

        mQuestNPCPosLst.Clear();
        mShopNPCPosLst.Clear();
        mPortalPosLst.Clear();
        mRevivePosLst.Clear();
    }
    public static bool isControllerInitialized()
    {
        return (mMapIconRefreshCoroutine != null);
    }


    #region Coroutine
    static IEnumerator MapIconRefresh()
    {
        while (true)
        {
            AddIconPosUpdate();
            RemoveIconPosUpdate();
            StaticIconUpdate();

            yield return new WaitForSeconds(REFRESH_INTERVAL);
        }
    }
    #endregion
    #region Coroutine Sub function
    private static void AddIconPosUpdate()
    {
        List<string> nameLst = new List<string>();
        List<GameObject> partyMemPosLst = new List<GameObject>();
        List<GameObject> monPosLst = new List<GameObject>();
        List<GameObject> minibossPosLst = new List<GameObject>();
        List<GameObject> bossPosLst = new List<GameObject>();

        GetPartyMemberNames(nameLst);
        GameInfo.gCombat.mEntitySystem.GetRadarVisibleEntities3(nameLst, partyMemPosLst, monPosLst, minibossPosLst, bossPosLst);

        //Monster
        //Miniboss + boss
        Vector3 mappos = Vector3.zero;
        for (int i = 0; i < monPosLst.Count; ++i)
        {
            if (mMonPairLst.Exists(pair => pair.entity == monPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(monPosLst[i].gameObject.transform.position);

            mMonPairLst.Add(new MapIconGameObjectPair(mappos, monPosLst[i]));
        }
        for (int i = 0; i < minibossPosLst.Count; ++i)
        {
            if (mMiniBossPairLst.Exists(pair => pair.entity == minibossPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(minibossPosLst[i].gameObject.transform.position);

            mMiniBossPairLst.Add(new MapIconGameObjectPair(mappos, minibossPosLst[i]));
        }
        for (int i = 0; i < bossPosLst.Count; ++i)
        {
            if (mBossPairLst.Exists(pair => pair.entity == bossPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(bossPosLst[i].gameObject.transform.position);

            mBossPairLst.Add(new MapIconGameObjectPair(mappos, bossPosLst[i]));
        }

        //Party
        for (int i = 0; i < partyMemPosLst.Count; ++i)
        {
            if (mPartyMemPairLst.Exists(pair => pair.entity == partyMemPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(partyMemPosLst[i].gameObject.transform.position);

            mPartyMemPairLst.Add(new MapIconGameObjectPair(mappos, partyMemPosLst[i]));
        }

        //Player
        if (GameInfo.gLocalPlayer != null && !mPlayerPairLst.Exists(pair => pair.entity == GameInfo.gLocalPlayer.AnimObj))
        {
            mappos = ScalePos_WorldToMap(GameInfo.gLocalPlayer.AnimObj.gameObject.transform.position);

            mPlayerPairLst.Add(new MapIconGameObjectPair(mappos, GameInfo.gLocalPlayer.AnimObj));
        }
    }
    private static void RemoveIconPosUpdate()
    {
        //Removes player icon if entity is lost
        for (int i = 0; i < mPlayerPairLst.Count; ++i)
        {
            if (mPlayerPairLst[i].entity == null)
                continue;

            MapIconGameObjectPair pair = mPlayerPairLst[i];
            pair.iconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mPlayerPairLst.RemoveAll(pair => pair.entity == null);

        //Removes icon when not visible
        for (int i = 0; i < mMonPairLst.Count; ++i)
        {
            if (mMonPairLst[i].entity == null)
                continue;

            MapIconGameObjectPair pair = mMonPairLst[i];
            pair.iconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mMonPairLst.RemoveAll(pair => pair.entity == null);

        for (int i = 0; i < mMiniBossPairLst.Count; ++i)
        {
            if (mMiniBossPairLst[i].entity == null)
                continue;

            MapIconGameObjectPair pair = mMiniBossPairLst[i];
            pair.iconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mMiniBossPairLst.RemoveAll(pair => pair.entity == null);

        //Remove icon when entity is dead and dissolved in level
        for (int i = 0; i < mBossPairLst.Count; ++i)
        {
            if (mBossPairLst[i].entity == null)
                continue;

            MapIconGameObjectPair pair = mBossPairLst[i];
            pair.iconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mBossPairLst.RemoveAll(pair => pair.entity == null);

        if (GameInfo.gLocalPlayer.IsInParty())
        {
            //Removes party icon if member leaves party
            for (int i = 0; i < mPartyMemPairLst.Count; ++i)
            {
                if (mPartyMemPairLst[i].entity == null)
                    continue;

                //TODO: Check if party member is still a party member

                MapIconGameObjectPair pair = mPartyMemPairLst[i];
                pair.iconPos = ScalePos_WorldToMap(pair.EntityPos);
            }
            mPartyMemPairLst.RemoveAll(pair => pair.entity == null);
        }
        else
        {
            mPartyMemPairLst.Clear();
        }
    }
    private static void StaticIconUpdate()
    {
        //Copy all shop npc to quest npc if they now have a quest
        for (int i = 0; i < mShopNPCPosLst.Count; ++i)
        {
            if (mShopNPCPosLst[i].hasQuest())
                mQuestNPCPosLst.Add(mQuestNPCPosLst[i]);
        }
        mShopNPCPosLst.RemoveAll(npc => npc.hasQuest());

        //Copy all quest npc to shop npc if they now have no quest, but have shop
        for (int i = 0; i < mQuestNPCPosLst.Count; ++i)
        {
            if (!mQuestNPCPosLst[i].hasQuest() && mQuestNPCPosLst[i].isShop)
                mShopNPCPosLst.Add(mQuestNPCPosLst[i]);
        }
        mQuestNPCPosLst.RemoveAll(npc => !npc.hasQuest());
    }
    #endregion

    #region Helper function
    private static void LoadStaticIcon(LevelInfo lvinfo)
    {
        //Create all static map icons
        Dictionary<int, ServerEntityJson> portalEntryDic;
        Dictionary<int, ServerEntityJson> portalExitDic;
        if (lvinfo.mEntities.TryGetValue("PortalEntryJson", out portalEntryDic))
        {
            foreach (PortalEntryJson porta in portalEntryDic.Values)
            {
                if (!porta.ShowInMap)
                    continue;

                mPortalPosLst.Add(ScalePos_WorldToMap(porta.position));
            }
        }
        if (lvinfo.mEntities.TryGetValue("PortalExitJson", out portalExitDic))
        {
            foreach (PortalExitJson porta in portalExitDic.Values)
            {
                if (!porta.ShowInMap)
                    continue;

                mPortalPosLst.Add(ScalePos_WorldToMap(porta.position));
            }
        }

        //ReviveSpot
        Dictionary<int, ServerEntityJson> reviveDic;
        if (lvinfo.mEntities.TryGetValue("RealmControllerWorldJson", out reviveDic))
        {
            //TO-DO: Check for correct realm if map is reused
            foreach (RealmControllerWorldJson rcw in reviveDic.Values)
            {
                if (!rcw.ShowInMap)
                    continue;

                foreach (Vector3 pos in rcw.spawnPos)
                {
                    mRevivePosLst.Add(ScalePos_WorldToMap(pos));
                }
            }
        }
    }
    private static void LoadNPCIcon(LevelInfo lvinfo)
    {
        Dictionary<int, ServerEntityJson> npcDic;

        //QuestNPC + ShopNPC goes here
        if (lvinfo.mEntities.TryGetValue("StaticClientNPCSpawnerJson", out npcDic))
        {
            foreach (StaticClientNPCSpawnerJson snpc in npcDic.Values)
            {
                //skip if not selected to show in map
                if (!snpc.ShowInMap)
                    continue;

                //Determine if NPC is a shop NPC and a quest NPC
                //Determine if NPC is a shop NPC via StaticNPCJson's npcFunction variable
                //Determine if NPC is a quest NPC via StaticClientNPCAlwaysShow.ActiveQuest
                StaticNPCJson npcJson = StaticNPCRepo.GetNPCByArchetype(snpc.archetype);
                StaticClientNPCAlwaysShow npc = GameInfo.gCombat.mEntitySystem.GetStaticClientNPC(snpc.archetype);
                if (npc == null)
                    Debug.LogError(string.Format("HUD_MapController.LoadStaticIcon: Walaoeh, cannot find npc from archetype [{0}]", snpc.archetype));
                //Do not create icon if it doesnt have a quest nor a shop
                if (npcJson.npcfunction.Length == 0 && npc.ActiveQuest < 0)
                    continue;

                Vector3 mappos = ScalePos_WorldToMap(snpc.position);
                StaticMapIconGameObjectPair newnpc = new StaticMapIconGameObjectPair(mappos, npc);
                if (npc.ActiveQuest >= 0)
                {
                    mQuestNPCPosLst.Add(newnpc);
                }
                else if (newnpc.isShop)
                {
                    mShopNPCPosLst.Add(newnpc);
                }
            }//end for-loop
        }//end NPC
    }
    private static void CalculateScaleRatio(LevelInfo lvinfo)
    {
        //*** Calculate the world to map ratio ***
        Dictionary<int, ServerEntityJson> mapJsonDic;
        lvinfo.mEntities.TryGetValue("MapInfoJson", out mapJsonDic);
        if (mapJsonDic == null || mapJsonDic.Count == 0 || mapJsonDic.Count > 1)
        {
            Debug.LogError("HUD_MapController.LoadMap: Walao, Cannot find MapInfoJson in level info or too many MapInfoJson in level");
            return;
        }
        foreach (ServerEntityJson data in mapJsonDic.Values)
        {
            //Set the map info
            //Each level should have only 1 mapinfo
            mMapInfo = data as MapInfoJson;
            break;
        }

        //world length and width
        mWorldDim.x = mMapInfo.mapScale.x * mMapInfo.width;
        mWorldDim.y = mMapInfo.mapScale.x * mMapInfo.width;
    }
    private static void GetPartyMemberNames(List<string> nameLst)
    {
        nameLst.Clear();
        if (GameInfo.gLocalPlayer.IsInParty())
        {
            Dictionary<string, PartyMember> pmLst = GameInfo.gLocalPlayer.PartyStats.GetPartyMemberList();
            foreach (PartyMember mem in pmLst.Values)
            {
                if (mem.IsHero())
                    continue;

                nameLst.Add(mem.GetName());
            }
        }
    }
    public static Vector3 ScaleNormPosToMapPos(ref Vector3 normPos)
    {
        normPos.x *= (mMap.texture.width * 0.5f);
        normPos.y *= (mMap.texture.height * 0.5f);
        return normPos;
    }
    /// <summary>
    /// Take in world position and transform to normalized map position [-1 ~ 1]
    /// [-1 ~ 1] to preserve floating-point accuracy
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public static Vector3 ScalePos_WorldToMap(Vector3 worldPos)
    {
        Vector3 mappos = Vector3.zero;

        if (mMapInfo == null)
            return mappos;

        mappos.x = (worldPos.x - mMapInfo.centerPoint.x) / (mWorldDim.x * 0.5f);
        mappos.y = (worldPos.z - mMapInfo.centerPoint.z) / (mWorldDim.y * 0.5f);
        mappos.z = 0f;

        return mappos;
    }
    /// <summary>
    /// Takes in normalized position [-1 ~ 1] on the map and transform to world position
    /// [-1 ~ 1] to preserve floating-point accuracy
    /// </summary>
    /// <param name="mapPos"></param>
    /// <returns></returns>
    public static Vector3 ScalePos_MapToWorld(Vector3 mapPos)
    {
        Vector3 worldPos = Vector3.zero;

        if (mMapInfo == null)
            return worldPos;

        worldPos.x = mMapInfo.centerPoint.x + mapPos.x * mWorldDim.x * 0.5f;
        worldPos.y = GameInfo.gLocalPlayer.Position.y;
        worldPos.z = mMapInfo.centerPoint.z + mapPos.z * mWorldDim.y * 0.5f;

        return worldPos;
    }
    #endregion
    #region Inter-Map function
    public static void PathFindCalculateToDestination(string destinationLevelName, bool autoStart = true)
    {
        //if same map, do not need to path find
        if (destinationLevelName == mCurMapName)
            return;

        mInterMapPathFindDic.Clear();

        //Calculate and store which level it will pass through
        //Calculate and store which position it will pas through
        bool pathfound = false;
        BotController.TheDijkstra.DoRouter(mCurMapName, destinationLevelName, out pathfound);
        if (!pathfound)
            return;

        Dictionary<string, string> allportal = BotController.TheDijkstra.LastRouterByPortal;
        foreach (KeyValuePair<string, string> portal in allportal)
        {
            //Get list of portals in the level
            List<PortalEntryData> portalLst = PortalInfos.GetLevelPortals(portal.Key);
            //Compare and record matching portal name as waypoints
            for (int i = 0; i < portalLst.Count; ++i)
            {
                if (portalLst[i].mName == portal.Value)
                    mInterMapPathFindDic.Add(portalLst[i].mLevel, portalLst[i].mPosition);
            }
        }
        mInterMapPathFindIndex = 0;
        GameInfo.gLocalPlayer.Bot.ClearRouter();

        if (autoStart)
            PathFind();
    }
    public static void PathFind()
    {
        if (mInterMapPathFindDic.Count == 0 || mInterMapPathFindIndex >= mInterMapPathFindDic.Count)
            return;

        if (GameInfo.gLocalPlayer == null)
        {
            Debug.LogError("HUD_MapController.PathFindToPortal: Uhh..trying to start path finding but gLocalPlayer is null?!");
            return;
        }

        Vector3 pos = mInterMapPathFindDic.ElementAt(mInterMapPathFindIndex++).Value;
        GameInfo.gLocalPlayer.PathFindToTarget(pos, -1, 0f, false, false, null);
    }
    #endregion
}
