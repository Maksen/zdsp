using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Entities;

public class HUD_Map : MonoBehaviour
{
    #region +++ Game Objects +++
    [Header("Game Object Shortcut")]
    [SerializeField]
    Text mTxtMapName;
    [SerializeField]
    Text mTxtChannelName;
    [SerializeField]
    Image mImgMap;
    [SerializeField]
    ToggleGroup mTGExpander;
    [SerializeField]
    Button mBtnWorldMap;
    [SerializeField]
    Button mBtnClose;
    #endregion
    #region +++ Parent Game Objects +++
    [Header("Parents")]
    [SerializeField]
    GameObject mExpanderNPCGO;
    [SerializeField]
    GameObject mExpanderMonsterGO;
    [SerializeField]
    GameObject mTeleportGO;
    [SerializeField]
    GameObject mReviveGO;
    [SerializeField]
    GameObject mMonsterGO;
    [SerializeField]
    GameObject mQuestNPCGO;
    [SerializeField]
    GameObject mShopNPCGO;
    [SerializeField]
    GameObject mBossGO;
    [SerializeField]
    GameObject mMiniBossGO;
    [SerializeField]
    GameObject mPartyGO;
    [SerializeField]
    GameObject mPlayerGO;
    #endregion
    #region +++ Prefab +++
    [Header("Prefab")]
    [SerializeField]
    GameObject mExpanderPrefab;
    [SerializeField]
    GameObject mMapIconPrefab;
    #endregion

    Coroutine mMapUpdateCoroutine = null;
    MapInfoJson mMapInfo = null;
    const float MAP_UPDATEPOS_INTERVAL = 1f;

    Vector2 mWorld2MapRatio = new Vector2();

    private void Awake()
    {
        LoadMap();
    }
    private void OnEnable()
    {
        //Simple update if same map
        if (string.Compare(mTxtMapName.text, ClientUtils.GetCurrentLevelName()) == 0)
        {
            //Update player
            //Update revive
            //Update party
            //Update monster
            //Update miniboss/boss
            return;
        }

        //Update map
        LoadMap();
    }
    private void OnDisable()
    {
        if (mMapUpdateCoroutine != null)
            StopCoroutine(mMapUpdateCoroutine);
        mMapUpdateCoroutine = null;
    }

    private bool LoadMap()
    {
        //Set basic data
        mTxtMapName.text = ClientUtils.GetCurrentLevelName();
        mTxtChannelName.text = "Test Channel #n"; //Waiting for channel to be implemented
        //mImgMap.sprite = ; //Waiting for map images to be ready

        //Retrieve all info about the level
        LevelInfo lvinfo = LevelReader.GetLevel(mTxtMapName.text);
        if (lvinfo == null)
            return false;

        //Get map scale
        LoadMapScale(lvinfo);

        //Create all expander
        CleanAllExpander();
        LoadMapExpander();

        //delete and create all map icon
        CleanAllMapIcons();
        LoadStaticMapIcon(lvinfo);
        
        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        if (mMapUpdateCoroutine != null)
            StopCoroutine(mMapUpdateCoroutine);
        mMapUpdateCoroutine = StartCoroutine(MapIconPosUpdateCoroutine());

        return true;
    }
    private bool LoadMapScale(LevelInfo lvinfo)
    {
        Dictionary<int, ServerEntityJson> mapJsonDic;
        lvinfo.mEntities.TryGetValue("MapInfoJson", out mapJsonDic);
        if (mapJsonDic == null || mapJsonDic.Count == 0 || mapJsonDic.Count > 1)
        {
            Debug.LogError("HUD_Map.LoadMap: Walao, Cannot find MapInfoJson in level info or too many MapInfoJson in level");
            return false;
        }
        foreach (ServerEntityJson data in mapJsonDic.Values)
        {
            //Set the map info
            //Each level should have only 1 mapinfo
            mMapInfo = data as MapInfoJson;
            break;
        }

        float worldMapSizeX = mMapInfo.mapScale.x * mMapInfo.width;
        float worldMapSizeY = mMapInfo.mapScale.y * mMapInfo.height;
        RectTransform uimapRT = mImgMap.GetComponent<RectTransform>();
        float uiMapSizeX = uimapRT.sizeDelta.x;
        float uiMapSizeY = uimapRT.sizeDelta.y;

        //Set scale
        mWorld2MapRatio.x = uiMapSizeX / worldMapSizeX;
        mWorld2MapRatio.y = uiMapSizeY / worldMapSizeY;

        return true;
    }
    private void LoadMapIcon()
    {
        if (GameInfo.gLocalPlayer != null)
        {
            Vector3 pos = ScalePos_WorldToMap(GameInfo.gLocalPlayer.Position);
            GameObject obj = Instantiate(mMapIconPrefab, pos, Quaternion.identity);
            obj.transform.SetParent(mPlayerGO.transform, false);
            //Set Rotation
        }

        //Retrieve all party member name
        List<string> nameLst = new List<string>();
        Dictionary<string, PartyMember> pmLst = GameInfo.gLocalPlayer.PartyStats.GetPartyMemberList();
        foreach (PartyMember mem in pmLst.Values)
        {
            nameLst.Add(mem.name);
        }

        //Retrieve net entities position
        List<Vector3> partyMemPosLst = new List<Vector3>();
        List<Vector3> dummyLst = new List<Vector3>();
        List<Vector3> monPosLst = new List<Vector3>();
        GameInfo.gCombat.mEntitySystem.GetRadarVisibleEntities(nameLst, partyMemPosLst, dummyLst, monPosLst, null);

        //Monster
        //Miniboss + boss
        Vector3 mappos = Vector3.zero;
        for (int i = 0; i < monPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(monPosLst[i]);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mMonsterGO.transform, false);
        }

        //Party
        for (int i = 0; i < partyMemPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(partyMemPosLst[i]);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mPartyGO.transform, false);
        }
    }
    private bool LoadStaticMapIcon(LevelInfo lvinfo)
    {
        if (lvinfo == null)
            return false;

        //Create all static map icons
        Dictionary<int, ServerEntityJson> portalEntryDic;
        Dictionary<int, ServerEntityJson> portalExitDic;
        Dictionary<int, ServerEntityJson> npcDic;
        if (lvinfo.mEntities.TryGetValue("PortalEntryJson", out portalEntryDic))
        {
            foreach (PortalEntryJson porta in portalEntryDic.Values)
            {
                Vector3 mappos = ScalePos_WorldToMap(porta.position);
                GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
                obj.transform.SetParent(mTeleportGO.transform, false);

            }
        }
        if (lvinfo.mEntities.TryGetValue("PortalExitJson", out portalExitDic))
        {
            foreach (PortalExitJson porta in portalExitDic.Values)
            {
                Vector3 mappos = ScalePos_WorldToMap(porta.position);
                GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
                obj.transform.SetParent(mTeleportGO.transform, false);
            }
        }
        //QuestNPC + ShopNPC goes here
        if (lvinfo.mEntities.TryGetValue("QuestNPCSpawnerDescJson", out npcDic))
        {
            foreach (QuestNPCSpawnerDescJson qnpc in npcDic.Values)
            {
                Vector3 mappos = ScalePos_WorldToMap(qnpc.position);
                GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
                obj.transform.SetParent(mQuestNPCGO.transform, false);
            }
        }
        //ReviveSpot

        return true;
    }
    private void LoadMapExpander()
    {

    }
    private void CreateExpanderSubData()
    {

    }

    public void OnClick_WorldMap()
    {

    }
    public void OnClick_Close()
    {

    }

    private Vector3 ScalePos_WorldToMap(Vector3 worldPos)
    {
        Vector3 mappos = Vector3.zero;

        mappos.x = worldPos.x * mWorld2MapRatio.x;
        mappos.y = worldPos.y * mWorld2MapRatio.y;
        mappos.z = 0f;

        return mappos;
    }
    private void CleanAllMapIcons()
    {
        foreach (Transform child in mTeleportGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mReviveGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mMonsterGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mQuestNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mShopNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mBossGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mMiniBossGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mPartyGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mPlayerGO.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void CleanAllExpander()
    {
        foreach (Transform child in mExpanderMonsterGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mExpanderNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
    }

    IEnumerator MapIconPosUpdateCoroutine()
    {
        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        foreach (Transform child in mPlayerGO.transform)
        {

        }
        foreach (Transform child in mMonsterGO.transform)
        {

        }
        foreach (Transform child in mBossGO.transform)
        {

        }
        foreach (Transform child in mMiniBossGO.transform)
        {

        }
        foreach (Transform child in mPartyGO.transform)
        {

        }

        //Update position every 1 sec
        yield return new WaitForSeconds(MAP_UPDATEPOS_INTERVAL);
    }
}
