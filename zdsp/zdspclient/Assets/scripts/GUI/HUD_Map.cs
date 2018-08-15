using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public struct IconGameObjectPair
{
    public GameObject icon;
    public GameObject entity;

    public IconGameObjectPair(GameObject _icon = null, GameObject _entity = null)
    {
        icon = _icon;
        entity = _entity;
    }

    public Vector3 IconPos
    {
        set { icon.transform.localPosition = value; }
    }
    public Vector3 EntityPos
    {
        get { return entity.transform.position; }
    }
};

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
    #region +++ Map Icon Texture +++
    [Header("Icons")]
    [SerializeField]
    Sprite mIconMonster;
    [SerializeField]
    Sprite mIconQuest;
    [SerializeField]
    Sprite mIconShop;
    [SerializeField]
    Sprite mIconTeleport;
    [SerializeField]
    Sprite mIconPlayer;
    [SerializeField]
    Sprite mIconParty;
    [SerializeField]
    Sprite mIconBoss;
    [SerializeField]
    Sprite mIconMiniBoss;
    [SerializeField]
    Sprite mIconRevive;
    [SerializeField]
    Sprite mIconQuestComplete;
    #endregion

    Coroutine mMapCloseCoroutine = null;
    Coroutine mMapUpdateCoroutine = null;
    const float MAP_UPDATEPOS_INTERVAL = 1f;

    Vector2 mWorld2MapRatio = new Vector2();

    List<IconGameObjectPair> mPlayerPairlst = new List<IconGameObjectPair>();
    List<IconGameObjectPair> mPartyMemPairlst = new List<IconGameObjectPair>();
    List<IconGameObjectPair> mMonPairlst = new List<IconGameObjectPair>();
    List<IconGameObjectPair> mBossPairlst = new List<IconGameObjectPair>();

    private void Awake()
    {
        LoadMap();
    }
    private void OnEnable()
    {
        //Load map if map changed
        if (string.Compare(mTxtMapName.text, ClientUtils.GetCurrentLevelName()) != 0)
        {
            if (!LoadMap())
                return;
        }

        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        if (mMapUpdateCoroutine != null)
            StopCoroutine(mMapUpdateCoroutine);
        mMapUpdateCoroutine = StartCoroutine(MapIconPosUpdateCoroutine());
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
        mImgMap.sprite = LoadMapSprite();

        //Retrieve all info about the level
        LevelInfo lvinfo = LevelReader.GetLevel(mTxtMapName.text);
        if (lvinfo == null)
            return false;

        //Get map scale
        LoadMapScale(lvinfo);

        //Create all expander
        CleanAllExpander();
        LoadMapExpander(lvinfo);

        //delete and create all map icon
        CleanAllMapIcons();
        LoadStaticMapIcon(lvinfo);
        LoadMapIcon();

        return true;
    }
    private bool LoadMapScale(LevelInfo lvinfo)
    {
        //Dictionary<int, ServerEntityJson> mapJsonDic;
        //lvinfo.mEntities.TryGetValue("MapInfoJson", out mapJsonDic);
        //if (mapJsonDic == null || mapJsonDic.Count == 0 || mapJsonDic.Count > 1)
        //{
        //    Debug.LogError("HUD_Map.LoadMap: Walao, Cannot find MapInfoJson in level info or too many MapInfoJson in level");
        //    return false;
        //}
        //foreach (ServerEntityJson data in mapJsonDic.Values)
        //{
        //    //Set the map info
        //    //Each level should have only 1 mapinfo
        //    mMapInfo = data as MapInfoJson;
        //    break;
        //}

        //Texture length and width
        //Vector2 mapSpriteDim = new Vector2(mImgMap.sprite.texture.width, mImgMap.sprite.texture.height);
        //Map level's length and width
        Vector2 worldMapDim = new Vector2(1f, 1f);
        //Get size of map viewport
        RectTransform uimapRT = mImgMap.GetComponent<RectTransform>();
        float uiMapSizeX = uimapRT.sizeDelta.x;
        float uiMapSizeY = uimapRT.sizeDelta.y;

        //Set scale
        mWorld2MapRatio.x = uiMapSizeX / worldMapDim.x;
        mWorld2MapRatio.y = uiMapSizeY / worldMapDim.y;

        return true;
    }
    private void LoadMapIcon()
    {
        if (GameInfo.gLocalPlayer != null)
        {
            Vector3 pos = ScalePos_WorldToMap(GameInfo.gLocalPlayer.Position);
            GameObject obj = Instantiate(mMapIconPrefab, pos, Quaternion.identity);
            obj.transform.SetParent(mPlayerGO.transform, false);
            //TO-DO: Set Rotation
            //obj.transform.localRotation = GameInfo.gLocalPlayer.AnimObj.transform.localRotation;

            mPlayerPairlst.Add(new IconGameObjectPair(obj, GameInfo.gLocalPlayer.AnimObj));
        }

        //Retrieve all party member name
        List<string> nameLst = new List<string>();
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

        //Retrieve net entities position
        List<GameObject> partyMemPosLst = new List<GameObject>();
        List<GameObject> monPosLst = new List<GameObject>();
        List<GameObject> minibossPosLst = new List<GameObject>();
        List<GameObject> bossPosLst = new List<GameObject>();
        GameInfo.gCombat.mEntitySystem.GetRadarVisibleEntities3(nameLst, partyMemPosLst, monPosLst, minibossPosLst, bossPosLst);

        //Monster
        //Miniboss + boss
        Vector3 mappos = Vector3.zero;
        for (int i = 0; i < monPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(monPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mMonsterGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconMonster;
            obj.SetActive(false);

            mMonPairlst.Add(new IconGameObjectPair(obj, monPosLst[i]));
        }
        for (int i = 0; i < minibossPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(minibossPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mMiniBossGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconMiniBoss;
            obj.SetActive(false);

            mBossPairlst.Add(new IconGameObjectPair(obj, minibossPosLst[i]));
        }
        for (int i = 0; i < bossPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(bossPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mBossGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconBoss;
            obj.SetActive(false);

            mBossPairlst.Add(new IconGameObjectPair(obj, bossPosLst[i]));
        }

        //Party
        for (int i = 0; i < partyMemPosLst.Count; ++i)
        {
            mappos = ScalePos_WorldToMap(partyMemPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mPartyGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconParty;

            mPartyMemPairlst.Add(new IconGameObjectPair(obj, partyMemPosLst[i]));
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
                //skip if not selected to show in map
                if (!qnpc.ShowInMap)
                    continue;

                //Determine if NPC is a shop NPC and a quest NPC
                //Determine if NPC is a shop NPC via StaticNPCJson's npcFunction variable
                //Determine if NPC is a quest NPC via StaticClientNPCAlwaysShow.ActiveQuest
                Kopio.JsonContracts.StaticNPCJson npcJson = StaticNPCRepo.GetStaticNPCByName(qnpc.archetype);
                Zealot.Client.Entities.StaticClientNPCAlwaysShow npc = GameInfo.gCombat.mEntitySystem.GetStaticClientNPC(qnpc.archetype);
                if (npc == null)
                    Debug.LogError("HUD_Map.LoadStaticMapIcon: Walaoeh, cannot find npc from archetype");
                int activeQuest = (npc != null) ? npc.ActiveQuest : -1;
                //Do not create icon if it doesnt have a quest nor a shop
                if (npcJson.npcfunction.Length == 0 && activeQuest < 0)
                    continue;

                //Create icon
                Vector3 mappos = ScalePos_WorldToMap(qnpc.position);
                GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
                Image img = obj.GetComponent<Image>();
                obj.SetActive(false);   //NPC are hidden on start

                //Set icon
                if (activeQuest != -1)
                {
                    obj.transform.SetParent(mQuestNPCGO.transform, false);
                    if (GameInfo.gLocalPlayer.QuestController.IsQuestAvailable(activeQuest))
                        img.sprite = mIconQuest;
                    else if (GameInfo.gLocalPlayer.QuestController.IsQuestCanSubmit(activeQuest))
                        img.sprite = mIconQuestComplete;
                }
                else
                {
                    obj.transform.SetParent(mShopNPCGO.transform, false);
                    string[] allFuncStr = npcJson.npcfunction.Split(';');
                    foreach (string funcStr in allFuncStr)
                    {
                        //Until NPC function has an enum
                        switch (funcStr[0])
                        {
                            //Shop
                            case '1':
                                img.sprite = mIconShop;
                                break;
                            //Teleport
                            case '2':
                                img.sprite = mIconTeleport;
                                break;
                            //Job change
                            case '3':
                                //img.sprite = ;
                                break;
                            //Storage
                            case '4':
                                //img.sprite = ;
                                break;
                            //Special item service
                            case '5':
                                //img.sprite = ;
                                break;
                        }
                    }
                }
            }//end for-loop
        }//end NPC

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
                    Vector3 mappos = ScalePos_WorldToMap(pos);
                    GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
                    obj.transform.SetParent(mReviveGO.transform, false);
                    Image img = obj.GetComponent<Image>();
                    img.sprite = mIconRevive;
                }
            }
        }

        return true;
    }
    private void LoadMapExpander(LevelInfo lvinfo)
    {
        if (lvinfo == null)
            return;

        //NPC
        Dictionary<int, ServerEntityJson> npcDic;
        if (lvinfo.mEntities.TryGetValue("QuestNPCSpawnerDescJson", out npcDic))
        {
            foreach (QuestNPCSpawnerDescJson qnpc in npcDic.Values)
            {
                //Check if archetype is valid
                Kopio.JsonContracts.NPCJson npcJson = NPCRepo.GetArchetypeByName(qnpc.archetype);
                if (npcJson == null)
                    continue;

                GameObject obj = Instantiate(mExpanderPrefab, Vector3.zero, Quaternion.identity);
                obj.transform.SetParent(mExpanderNPCGO.transform, false);

                //Attach ToggleGroup
                Toggle tg = obj.GetComponent<Toggle>();
                if (tg == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Walao, Cannot find Toggle component in ExpanderSubData");
                    continue;
                }
                tg.group = this.mTGExpander;

                //Find the text component in the children, there should be only 1
                Text txt = obj.GetComponentInChildren<Text>();
                if (txt == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Cannot find Text component in ExpanderSubData");
                    continue;
                }

                txt.text = npcJson.localizedname;
            }
        }

        //Monster
        Dictionary<int, ServerEntityJson> monDic;
        if (lvinfo.mEntities.TryGetValue("MonsterSpawnerJson", out monDic))
        {
            foreach (MonsterSpawnerJson ms in monDic.Values)
            {
                //Check if monster archetype is valid
                Kopio.JsonContracts.CombatNPCJson monJson = NPCRepo.GetArchetypeByName(ms.archetype);
                if (monJson == null)
                    continue;

                GameObject obj = Instantiate(mExpanderPrefab, Vector3.zero, Quaternion.identity);
                obj.transform.SetParent(mExpanderMonsterGO.transform, false);

                //Attach ToggleGroup
                Toggle tg = obj.GetComponent<Toggle>();
                if (tg == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Walao, Cannot find Toggle component in ExpanderSubData");
                    continue;
                }
                tg.group = this.mTGExpander;

                //Find the text component in the children, there should be only 1
                Text txt = obj.GetComponentInChildren<Text>();
                if (txt == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Cannot find Text component in ExpanderSubData");
                    continue;
                }

                txt.text = monJson.localizedname;
            }
        }
    }

    public void OnClick_Close()
    {
        if (mMapCloseCoroutine == null)
            StartCoroutine(MapCloseCouroutine());
    }
    public void OnClick_OpenWorldMap()
    {
        UIManager.OpenWindow(WindowType.WorldMap);
    }
    public void OnToggleNPCExpander(bool isON)
    {
        foreach (Transform child in mQuestNPCGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
        foreach (Transform child in mShopNPCGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
    }
    public void OnToggleMonsterExpander(bool isON)
    {
        foreach (Transform child in mMonsterGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
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
    private Sprite LoadMapSprite()
    {
        Kopio.JsonContracts.LevelJson lvJson = LevelRepo.GetInfoByName(mTxtMapName.text);
        if (lvJson == null)
        {
            Debug.LogError("HUD_Map.LoadMap: Walao, Cannot find LevelJson in level Repo");
            return null;
        }
        Sprite mapSprite = ClientUtils.LoadIcon(lvJson.maptexture);

        return ((mapSprite != null) ? mapSprite : mImgMap.sprite);
    }
    private void GetPartyMemberNames(List<string> nameLst)
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
    

    IEnumerator MapIconPosUpdateCoroutine()
    {
        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        while (true)
        {
            UpdateAddMapIcon();
            UpdateRemoveMapIcon();

            //Update position every 1 sec
            yield return new WaitForSeconds(MAP_UPDATEPOS_INTERVAL);
        }
    }
    IEnumerator MapCloseCouroutine()
    {
        yield return new WaitForSeconds(0.75f);
        this.gameObject.SetActive(false);
        mMapCloseCoroutine = null;
    }
    private void UpdateAddMapIcon()
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
            if (mMonPairlst.Exists(pair => pair.entity == monPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(monPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mMonsterGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconMonster;

            mMonPairlst.Add(new IconGameObjectPair(obj, monPosLst[i]));
        }
        for (int i = 0; i < minibossPosLst.Count; ++i)
        {
            if (mBossPairlst.Exists(pair => pair.entity == minibossPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(minibossPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mMiniBossGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconMiniBoss;

            mBossPairlst.Add(new IconGameObjectPair(obj, minibossPosLst[i]));
        }
        for (int i = 0; i < bossPosLst.Count; ++i)
        {
            if (mBossPairlst.Exists(pair => pair.entity == bossPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(bossPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mBossGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconBoss;

            mBossPairlst.Add(new IconGameObjectPair(obj, bossPosLst[i]));
        }

        //Party
        for (int i = 0; i < partyMemPosLst.Count; ++i)
        {
            if (mPartyMemPairlst.Exists(pair => pair.entity == partyMemPosLst[i]))
                continue;

            mappos = ScalePos_WorldToMap(partyMemPosLst[i].transform.position);
            GameObject obj = Instantiate(mMapIconPrefab, mappos, Quaternion.identity);
            obj.transform.SetParent(mPartyGO.transform, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = mIconParty;

            mPartyMemPairlst.Add(new IconGameObjectPair(obj, partyMemPosLst[i]));
        }
    }
    private void UpdateRemoveMapIcon()
    {
        //Removes player icon if entity is lost
        for (int i = 0; i < mPlayerPairlst.Count; ++i)
        {
            if (mPlayerPairlst[i].entity == null)
            {
                Destroy(mPlayerPairlst[i].icon.gameObject);
                continue;
            }

            IconGameObjectPair pair = mPlayerPairlst[i];
            pair.IconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mPlayerPairlst.RemoveAll(pair => pair.entity == null);

        //Removes party icon if member leaves party
        for (int i = 0; i < mPartyMemPairlst.Count; ++i)
        {
            if (mPartyMemPairlst[i].entity == null)
            {
                Destroy(mPartyMemPairlst[i].icon.gameObject);
                continue;
            }

            //Check if party member is still a party member

            IconGameObjectPair pair = mPartyMemPairlst[i];
            pair.IconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mPartyMemPairlst.RemoveAll(pair => pair.entity == null);

        //Removes icon when not visible
        for (int i = 0; i < mMonPairlst.Count; ++i)
        {
            if (mMonPairlst[i].entity == null)
            {
                Destroy(mMonPairlst[i].icon.gameObject);
                continue;
            }

            IconGameObjectPair pair = mMonPairlst[i];
            pair.IconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mMonPairlst.RemoveAll(pair => pair.entity == null);

        //Remove icon when entity is dead and dissolved in level
        for (int i = 0; i < mBossPairlst.Count; ++i)
        {
            if (mBossPairlst[i].entity == null)
            {
                Destroy(mBossPairlst[i].icon.gameObject);
                continue;
            }

            IconGameObjectPair pair = mBossPairlst[i];
            pair.IconPos = ScalePos_WorldToMap(pair.EntityPos);
        }
        mBossPairlst.RemoveAll(pair => pair.entity == null);
    }

    /// <summary>
    /// Meant for clicking minimap 2nd time to close map
    /// </summary>
    public void OnMiniMapClick()
    {
        if (this.gameObject.GetActive() == false)
            this.gameObject.SetActive(true);
        else
        {
            mBtnClose.onClick.Invoke();
            //if (mMapCloseCoroutine == null)
            //    StartCoroutine(MapCloseCouroutine());
        }
    }
}
