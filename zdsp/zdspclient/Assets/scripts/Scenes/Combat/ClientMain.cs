using ExitGames.Client.Photon;
using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.ClientSpawners;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Repository;
using Candlelight.UI;

public enum GameZone
{
    Safe,//0
    Unsafe,//1
    Invalid	//2
}

public class PlayerBasicAttackState
{
    public int[] mBasicAttackIDs;
    public float mLastCastBasicAttackTime;
    public int mLastCastBasicAttackIndex;
    public bool mBasicAttackButtonDown;

    public PlayerBasicAttackState()
    {
        mBasicAttackIDs = new int[] { -1, -1, -1, -1, -1 }; //Different sects will have different ids
        mLastCastBasicAttackTime = 0;
        mLastCastBasicAttackIndex = 0;
        mBasicAttackButtonDown = false;
    }

    public int GetBasicAttack(int skillid)
    {
        for (int i = 0; i < mBasicAttackIDs.Length; i++)
        {
            if (mBasicAttackIDs[i] == skillid)
                return i;
        }
        return -2;
    }
}

public class PlayerSkillCDState
{
    public float mGCDStart; //GCD affects skills of the same skilltype
    public float mGCDEnd;
    public Dictionary<int, float> mCDStart;
    public Dictionary<int, float> mCDEnd;

    public PlayerSkillCDState()
    {
        mGCDStart = 0;
        mGCDEnd = 0;
        mCDStart = new Dictionary<int, float>();
        mCDEnd = new Dictionary<int, float>();
    }

    public bool IsSkillCoolingDown(int sid)
    {
        float now = Time.time;
        if (!mCDEnd.ContainsKey(sid))
            return false;
        return now < mCDEnd[sid] || now < mGCDEnd;
    }

    public bool IsCoolingDown(int sid)
    {
        float now = Time.time;
        return now < mCDEnd[sid];
    }

    public float GetCDDuration(int sid)
    {
        return mCDEnd[sid] - mCDStart[sid];
    }

    public bool HasGlobalCooldown()
    {
        float now = Time.time;
        return now < mGCDEnd;
    }
}

public class RelevanceObjectState
{
    public IRelevanceEntity entity;
    public uint lastTimeFoundRelevant;

    public RelevanceObjectState(IRelevanceEntity entity)
    {
        this.entity = entity;
        lastTimeFoundRelevant = 0;
    }
}

public partial class ClientMain : MonoBehaviour
{
    private const float SNAPDISTANCE = 1.0f; //in square
    private const float RelevanceRadius = 16.0f;
    private const int RelevantTimeOut = 100;

    private int mDefaultCullMask;
    private int mCurrentCullMask = -1;
    private bool mEnableSceneRender;

    [HideInInspector]
    public GameObject PlayerCamera;

    public Timers mTimers;

    public ClientEntitySystem mEntitySystem;
    private NetClient mNetClient;

    [HideInInspector]
    public PlayerInput mPlayerInput;

    private Dictionary<int, RelevanceObjectState> mRelevanceObjects;
    private long mRelevanceUpdateElapsed;

    private GameObject mSelectIndicator;
    private HUD_SelectTarget hud_selectTarget;

    private float timeleft;
    private bool bReclaim = false;
    private AsyncOperation reclaimRequest = null;

    private long mUpdateElapsedTime;

    public CutsceneManager CutsceneManager { get; set; }
    private EnvironmentController mEnvironmentController;

    private GameObject mMonsterHolder; //to organize monster
    private GameObject mStaticNPCHolder; //to organize static npc;
    private GameObject mPlayerHolder; //to organize player ghost;
    private GameObject mPlayerOwnedNPCHolder; //to organize player owned spawns
    private GameObject mLootHolder;

    // Cached list
    private List<Entity> qr = new List<Entity>(32);
    private List<int> removeIDs = new List<int>(32);

    private Dictionary<string, string> strFormatParam;

    // Debug used //
    private UnityEngine.UI.Button bGenderDebug;
    private UnityEngine.UI.Button bWeaponDebug;

    [Flags]
    private enum eSelectedWeapon : ushort
    {
        eSWORD = 1 << 0,
        eBLADE = 1 << 1,
        eLANCE = 1 << 2,
        eHAMMER = 1 << 3,
        eFAN = 1 << 4,
        eXBOW = 1 << 5,
        eDAGGER = 1 << 6,
        eSANSIAN = 1 << 7
    }

    private eSelectedWeapon eAllowed = eSelectedWeapon.eBLADE | eSelectedWeapon.eHAMMER | eSelectedWeapon.eLANCE | eSelectedWeapon.eDAGGER | eSelectedWeapon.eSWORD | eSelectedWeapon.eXBOW;
    //private eSelectedWeapon eAvatarInfo = eSelectedWeapon.eGENDERTYPE | eSelectedWeapon.eHAMMER;
    // Debug used //

    private void Awake()
    {
        this.useGUILayout = false;
        timeleft = 10.0f;
        RPCFactory.SetMainContext(typeof(ClientMain));
        PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;

        mTimers = new Timers();
        mEntitySystem = new ClientEntitySystem(mTimers);
        mNetClient = new NetClient(mEntitySystem);
        mEntitySystem.InitGrid(256, 256);
        //mEntitySystem.InitGrid(levelsizex, levelsizez, leveloriginx, leveloriginz, cellsizex, cellsizez);

        mRelevanceObjects = new Dictionary<int, RelevanceObjectState>();
        mRelevanceUpdateElapsed = 0;

        mPlayerInput = GetComponent<PlayerInput>();
        mPlayerInput.enabled = false;

        RegisterActions();

        GameInfo.gCombat = this;
        mUpdateElapsedTime = 0;

        strFormatParam = new Dictionary<string, string>();
        PathFinder.Init();
    }

    private void OnDestroy()
    {
        PlayerCamera = null;
        mTimers = null;
        mEntitySystem = null;
        mNetClient = null;
        mPlayerInput = null;
        mRelevanceObjects = null;
        mSelectIndicator = null;
        reclaimRequest = null;
        CutsceneManager = null;
    }

    public void OnDisconnected()
    {
        if (mNetClient != null)
            mNetClient.CleanUp();
        mNetClient = null;
        mPlayerInput.enabled = false;
    }

    public void OnReconnected()
    {
        RPCFactory.SetMainContext(typeof(ClientMain));
        mRelevanceObjects.Clear();
        mRelevanceUpdateElapsed = 0;
        mEntitySystem.RemoveAllNetEntities();
        mNetClient = new NetClient(mEntitySystem);

        RPCFactory.CombatRPC.OnClientLevelLoaded();
    }

    public bool IsRealmInfoReady { get; set; }

    public void InitPlayerCamera(GameObject camera)
    {
        PlayerCamera = camera;
        mDefaultCullMask = PlayerCamera.GetComponent<Camera>().cullingMask;
        mCurrentCullMask = mDefaultCullMask;
        mEnableSceneRender = true;
    }

    private void Start()
    {
        mEnvironmentController = new EnvironmentController();
        Debug.Log("OnClientLevelLoaded");
        RPCFactory.CombatRPC.OnClientLevelLoaded();

        mMonsterHolder = new GameObject();
        mMonsterHolder.name = "SpawnedMonsters";
        mStaticNPCHolder = new GameObject();
        mStaticNPCHolder.name = "StaticNPCs";
        mPlayerHolder = new GameObject();
        mPlayerHolder.name = "Player";
        mPlayerOwnedNPCHolder = new GameObject();
        mPlayerOwnedNPCHolder.name = "PlayerOwnedNPCs";
        mLootHolder = new GameObject();
        mLootHolder.name = "Loot";

        GameObject cutsceneManager = new GameObject();
        cutsceneManager.transform.SetParent(transform, false);
        cutsceneManager.name = "CutsceneManager";
        CutsceneManager = cutsceneManager.AddComponent<CutsceneManager>();

        TickerTapeSystem.Instance.OnLogin();
    }

    private void RegisterActions()
    {
        ActionManager.RegisterAction(ACTIONTYPE.IDLE, typeof(IdleActionCommand), typeof(NonClientAuthoACIdle));
        ActionManager.RegisterAction(ACTIONTYPE.WALK, typeof(WalkActionCommand), typeof(NonClientAuthoACWalk));
        ActionManager.RegisterAction(ACTIONTYPE.CASTSKILL, typeof(CastSkillCommand), typeof(NonClientAuthoCastSkill));
        //ActionManager.RegisterAction(ACTIONTYPE.Flash, typeof(FlashActionCommand), typeof(NonClientAuthoFlash));
        ActionManager.RegisterAction(ACTIONTYPE.WALKANDCAST, typeof(WalkAndCastCommand), typeof(NonClientAuthoWalkAndCast));
        ActionManager.RegisterAction(ACTIONTYPE.DEAD, typeof(DeadActionCommand), typeof(NonClientAuthoACDead));
        ActionManager.RegisterAction(ACTIONTYPE.SNAPSHOTUPDATE, typeof(SnapShotUpdateCommand), typeof(DummyAction));
        ActionManager.RegisterAction(ACTIONTYPE.APPROACH, typeof(ApproachCommand), typeof(NonClientAuthoACApproach));
        ActionManager.RegisterAction(ACTIONTYPE.WALK_WAYPOINT, typeof(WalkToWaypointActionCommand), typeof(NonClientAuthoWalkWaypoint));
        ActionManager.RegisterAction(ACTIONTYPE.INTERACT, typeof(InteractCommand), typeof(NonClientAuthoACInteract));
        ActionManager.RegisterAction(ACTIONTYPE.KNOCKEDBACK, typeof(KnockedBackCommand), typeof(NonClientAuthoKnockedBackWalk));
        ActionManager.RegisterAction(ACTIONTYPE.KNOCKEDUP, typeof(KnockedUpCommand), typeof(NonClientAuthoKnockedUp));
        ActionManager.RegisterAction(ACTIONTYPE.DASHATTACK, typeof(DashAttackCommand), typeof(NonClientAuthoDashAttack));
        ActionManager.RegisterAction(ACTIONTYPE.DRAGGED, typeof(DraggedActionCommand), typeof(NonClientAuthoDragged));
        ActionManager.RegisterAction(ACTIONTYPE.GETHIT, typeof(GetHitCommand), typeof(NonClientAuthoACGetHit));
    }

    public void OnZealotRPCEvent(PhotonNetworkingMessage eventType, EventData eventData)
    {
        switch (eventType)
        {
            case PhotonNetworkingMessage.OnCombatEvent:
                RPCFactory.CombatRPC.OnCommand(this, eventData);
                break;

            case PhotonNetworkingMessage.OnNonCombatEvent:
                RPCFactory.NonCombatRPC.OnCommand(this, eventData);
                break;

            case PhotonNetworkingMessage.OnUnreliableCombatEvent:
                RPCFactory.UnreliableCombatRPC.OnCommand(this, eventData);
                break;

            case PhotonNetworkingMessage.OnLocalObjectEvent:
                RPCFactory.LocalObjectRPC.OnLocalObject(this, eventData);
                break;

            case PhotonNetworkingMessage.OnActionEvent:
                RPCFactory.ActionRPC.OnAction(this, eventData);
                break;
        }
    }

    public void OnActionCommand(int pid, ActionCommand cmd, Type actiontype)
    {
        Entity entity = mEntitySystem.GetEntityByPID(pid);
        if (entity == null)
        {
            LogManager.DebugLog("OnActionCommand: NetEntityGhost pid [" + pid + "] does not exist);");
            return;
        }
        if (cmd.GetActionType() == ACTIONTYPE.DRAGGED)
        {
            //LogManager.DebugLog("OnActionCommand: NetEntityGhost pid [" + pid + "] does not exist);");
        }

        if (cmd.GetActionType() == ACTIONTYPE.SNAPSHOTUPDATE)
        {
            //Debug.LogFormat("Snapshottupdate: {0}", pid);
            SnapShotUpdateCommand snapcmd = (SnapShotUpdateCommand)cmd;
            Vector3 newPos = snapcmd.pos;
            NetEntityGhost ne = (NetEntityGhost)entity;
            Zealot.Common.Actions.Action currAction = ne.GetAction();
            ACTIONTYPE currActionType = ACTIONTYPE.IDLE;
            bool snap = true;
            if (currAction != null)
            {
                currActionType = currAction.mdbCommand.GetActionType();
                if (currActionType == ACTIONTYPE.WALK_WAYPOINT || currActionType == ACTIONTYPE.WALK || currActionType == ACTIONTYPE.APPROACH)
                    snap = false;
                //not snap position if currently is walking.
            }

            if (snap && ((entity.Position - newPos).sqrMagnitude > SNAPDISTANCE || Vector3.Dot(entity.Forward, snapcmd.forward) < 0.95f))
            {
                //Peter, TODO: to prevent snapping, we can sync position during idle, see how fatehunter do it
                entity.Position = newPos;
                if (currAction != null)
                {
                    if (currActionType != ACTIONTYPE.WALK && currActionType != ACTIONTYPE.APPROACH)
                        entity.Forward = snapcmd.forward;
                }
            }
            return;
        }

        //Debug.LogFormat("OnActionCommand: {0} : {1}", pid, cmd.GetActionType());
        object[] args = new object[2];
        args[0] = entity;
        args[1] = cmd;
        object action = Activator.CreateInstance(actiontype, args);
        NetEntityGhost ghost = (NetEntityGhost)entity;
        ghost.PerformAction((Zealot.Common.Actions.Action)action);
    }

    private void Update()
    {
        mTimers.Update();

        if (GameInfo.DCReconnectingGameServer)
            return;

        long dt = mTimers.GetDeltaTime();

        mUpdateElapsedTime += dt;
        if (mUpdateElapsedTime > 10) //update only after every 50msec, same as server
        {
            //Profiler.BeginSample("EntitySystem_Update");
            mEntitySystem.Update(mUpdateElapsedTime);
            //Profiler.EndSample();

            //Profiler.BeginSample("UpdateRelevantEntities");
            UpdateRelevantEntities(mUpdateElapsedTime);
            //Profiler.EndSample();

            mUpdateElapsedTime = 0;
        }

        //Profiler.BeginSample("Netclient_Update");
        mNetClient.Update(dt);
        //Profiler.EndSample();

        //Profiler.BeginSample("Bot_Update");
        //Profiler.EndSample();
        //ReclaimUpdate();
    }

    public void ReclaimUpdate()
    {
        if (!bReclaim)
        {
            if (timeleft <= 0.0)
            {
                if (GameInfo.gLocalPlayer.IsMoving() == false)
                {
                    bReclaim = true;
                    StartCoroutine(StartReclaim());
                }
            }
            else
            {
                timeleft -= Time.deltaTime;
            }
        }
    }

    private IEnumerator StartReclaim()
    {
        reclaimRequest = Resources.UnloadUnusedAssets();

        while (!reclaimRequest.isDone)
        {
            yield return null;
        }
        System.GC.Collect();
        reclaimRequest = null;
        bReclaim = false;
        timeleft = 10.0f;
    }

    public void UpdateLocalObject(int persid, LOTYPE objtype, bool createnew, ref byte code, Dictionary<byte, object> dic)
    {
        string methodname = "";
        // TODO: Think whether to use global shared localobject as entity (e.g Party, Guild, etc...) so that we can universally located by entitysystem
        BaseNetEntityGhost bne = (persid == -1) ? GameInfo.gLocalPlayer : mEntitySystem.GetEntityByPID(persid) as BaseNetEntityGhost;
        if (bne == null)
        {
            Debug.Log(string.Format("OnLocalObject: type = {0}, pid = {1} does not exist);", objtype, persid));
            return;
        }
        LocalObject objlocal = bne.GetLocalObject(objtype);
        if (objlocal == null)
        {
            bne.AddLocalObject(objtype, null);
            objlocal = bne.GetLocalObject(objtype);
        }
        try
        {
            objlocal.Deserialize(dic, createnew, objtype, ref code, out methodname);
        }
        catch (Exception ex)
        {
            Exception error = ex;
            while (error.InnerException != null)
                error = error.InnerException;

            Debug.LogError("UpdateLocalObject " + objtype + ", " + code + ": " + error.ToString() + ", MethodName: " + methodname);
        }
    }

    //-------------------------------------------------------------------------------------------------------
    //RPC Calls:
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.SpawnPlayerEntity)]
    public void SpawnPlayerEntity(bool isLocal, int ownerid, string playername, int pid, byte jobsect, byte gender, int mountID, RPCPosition rpcpos, RPCDirection rpcdir, int health, int healthmax)
    {
        Vector3 pos = rpcpos.ToVector3();
        Debug.Log("SpawnPlayerEntity owner = " + ownerid + ", playername = " + playername + ", persistentid = " + pid + ",pos = (" + pos.x + ", " + pos.y + "," + pos.z);

        PlayerGhost playerghost = mEntitySystem.SpawnNetEntityGhost<PlayerGhost>(pid);
        playerghost.IsLocal = isLocal;
        playerghost.SetOwnerID(ownerid);

        playerghost.Init(playername, (JobType)jobsect, gender, mountID, pos, rpcdir.ToVector3(), health, healthmax);

        //Setup camera and control if this is local playerghost
        if (playerghost.IsLocal)
        {
            Debug.Log("Spawn local player = " + playername);//please do not remove, is for logging purpose
            GameInfo.gLocalPlayer = playerghost;
            playerghost.Idle();

            mPlayerInput.Init(playerghost);
            mPlayerInput.enabled = true;
            GameInfo.gLocalPlayer.InitBot(mPlayerInput);

            mNetClient.AddLocalEntity(playerghost);
            mTimers.SetTimer(100, UnLoadingScreen, null);
            GameInfo.gBasicAttackState = new PlayerBasicAttackState();
            GameInfo.gSkillCDState = new PlayerSkillCDState();
            //OnSelectEntity(null);
            GameInfo.OnLocalPlayerSpawned();
            SpawnClientSpawners();

            if (GameInfo.gDmgLabelPool == null)
            {
                GameObject go = new GameObject();
                go.name = "damageLabelPool";
                GameInfo.gDmgLabelPool = go.AddComponent<HUD_DamageLabelPool>();
                go.layer = LayerMask.NameToLayer("UI_HUD");
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UnLoadingScreen(object args)
    {
        StartCoroutine(OnClientReady());
        //EfxSystem.Instance.ClearAssets();//if effect loaded into momery use too much memory , call this at loading level time.
    }

    private IEnumerator OnClientReady()
    {
        yield return new WaitUntil(() => { return GameInfo.gLocalPlayer.IsModelLoaded && GameInfo.mIsPlayerReady; });
        yield return null;
        OnAllClientStuffReady();
        UIManager.ShowLoadingScreen(false);
    }

    private void OnAllClientStuffReady()
    {
        GameInfo.gLocalPlayer.InitMap();
        GameInfo.gLocalPlayer.SetHeadLabel(true);
        GameInfo.gLocalPlayer.CheckQuestTeleportAction();
        GameInfo.gLocalPlayer.UpdatePlayerCompanion();
        //handle auto combat .
        bool startbot = false;
        RealmJson mRealmInfo = GameInfo.mRealmInfo;
        if (mRealmInfo == null)
            startbot = false;
        else
        {
            RealmType realmType = mRealmInfo.type;
            switch (realmType)
            {
                //case RealmType.RealmTutorial:
                //    startbot = false;
                //    if (GameInfo.gLocalPlayer.QuestStats.isTraining)
                //    {
                //        TrainingRealmContoller.Instance.RealmStart();
                //    }
                //    break;
                case RealmType.Dungeon:
                //case RealmType.ActivityGuildSMBoss:
                //case RealmType.Arena:
                //case RealmType.InvitePVP:
                //case RealmType.EliteMap:
                    startbot = true;
                    break;

                default:
                    startbot = false;
                    break;
            }
        }

        if (!GameSettings.AutoBotEnabled)
            startbot = false;

        if (!startbot)
        {
            //handle world map router.
            GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
        }
        else
        {
            GameInfo.gLocalPlayer.Bot.StartBot();
        }

        // Debug //
        //RectTransform canvas = UIManager.GetHUDGameCanvas();
        //GameObject button = new GameObject();
        //button.transform.parent = canvas;
        //button.transform.localPosition = new Vector3(-300, -100, 1);
        //RectTransform tr = button.AddComponent<RectTransform>();

        //bWeaponDebug = button.AddComponent<UnityEngine.UI.Button>();
        //tr.localScale = new Vector3(1, 1, 1);
        //tr.sizeDelta = new Vector2(100, 30);
        //bWeaponDebug.onClick.AddListener(OnWeaponButton);
        //button.AddComponent<CanvasRenderer>();
        //bWeaponDebug.targetGraphic = button.AddComponent<UnityEngine.UI.Image>();
        //GameObject tx = new GameObject();
        //UnityEngine.UI.Text text = tx.AddComponent<UnityEngine.UI.Text>();
        //tx.transform.parent = button.transform;
        //text.alignment = TextAnchor.MiddleCenter;
        //text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        //RectTransform tr_tx = tx.GetComponent<RectTransform>();
        //tr_tx.localScale = new Vector3(1, 1, 1);
        //tr_tx.localPosition = new Vector3(0, 0, 1);
        //text.color = Color.black;
        //text.text = ((PartsType)(GameInfo.gLocalPlayer.WeaponTypeUsed)).ToString();

        //GameObject button2 = new GameObject();
        //button2.transform.parent = canvas;
        //button2.transform.localPosition = new Vector3(-400, -100, 1);
        //RectTransform tr2 = button2.AddComponent<RectTransform>();

        //bGenderDebug = button2.AddComponent<UnityEngine.UI.Button>();
        //tr2.localScale = new Vector3(1, 1, 1);
        //tr2.sizeDelta = new Vector2(80, 30);
        //bGenderDebug.onClick.AddListener(OnAvatarButton);
        //button2.AddComponent<CanvasRenderer>();
        //bGenderDebug.targetGraphic = button2.AddComponent<UnityEngine.UI.Image>();
        //GameObject tx2 = new GameObject();
        //UnityEngine.UI.Text text2 = tx2.AddComponent<UnityEngine.UI.Text>();
        //tx2.transform.parent = button2.transform;
        //text2.alignment = TextAnchor.MiddleCenter;
        //text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        //RectTransform tr_tx2 = tx2.GetComponent<RectTransform>();
        //tr_tx2.localScale = new Vector3(1, 1, 1);
        //tr_tx2.localPosition = new Vector3(0, 0, 1);
        //text2.color = Color.black;
        //text2.text = (GameInfo.gLocalPlayer.PlayerSynStats.Gender == 0) ? "Male" : "Female";
    }

    private void UpdateBuffsTime(object args)
    {
        //Peter, TODO: actually this only need to be done when the buff panel is displayed
        //And with the UI, the implementation could be totally different from below:
        PlayerGhost player = GameInfo.gLocalPlayer;
        if (player != null && player.IsAlive())
        {
            CollectionHandler<object> positives = player.BuffTimeStats.Positives;
            //CollectionHandler<object> buffStartTime = player.BuffTimeStats.StartTime;
            //CollectionHandler<object> durations = player.BuffTimeStats.Duration;
            long now = mTimers.GetSynchronizedTime();
            for (int i = 0; i < positives.Count; ++i)
            {
                int id = (int)positives[i];
                if (id > 0)
                {
                    //long duration = (long)durations[i];
                    //long startTime = (long)buffStartTime[i];
                    //long endTime = startTime + duration;
                    //long timeRemaining = Math.Max(endTime - now, 0);
                    //Debug.LogFormat("Buff [{0}] Sideffect id = {1} time remaining = {2}", i, id, timeRemaining / 1000);
                }
            }
        }
        mTimers.SetTimer(1000, UpdateBuffsTime, null);
    }

    //Spawn all client spawners on scene load as they are relatively cheap without actual mesh
    //These spawners are responsible for spawning the actual entities e.g. quest npc, shop npc when local player is near.
    private void SpawnClientSpawners()
    {
        ClientSpawnerBase[] spawners = FindObjectsOfType(typeof(ClientSpawnerBase)) as ClientSpawnerBase[];
        foreach (ClientSpawnerBase spawner in spawners)
        {
            spawner.Spawn(mEntitySystem);
        }
    }

    public void HideClientSpawner(string archetype)
    {
        ClientSpawnerBase[] spawners = FindObjectsOfType(typeof(ClientSpawnerBase)) as ClientSpawnerBase[];
        foreach (ClientSpawnerBase spawner in spawners)
        {
            if (spawner is StaticNPCSpawner)
            {
                StaticNPCSpawner staticNPCSpawner = spawner as StaticNPCSpawner;
                if (staticNPCSpawner.archetype == archetype)
                    staticNPCSpawner.Show(false);
            }
        }
    }

    //Do something similar to netserverslot for checking relevant non-net entities
    private bool UpdateRelevantEntities(long dt)
    {
        mRelevanceUpdateElapsed += dt;
        if (mRelevanceUpdateElapsed < 800) //update every this interval
            return false;

        mRelevanceUpdateElapsed = 0;

        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        if (localplayer == null)
            return false;

        Vector3 refpos = localplayer.Position;
        uint tick = mTimers.GetTick();

        qr.Clear();
        mEntitySystem.QueryEntitiesInSphere(refpos, RelevanceRadius, (queriedEntity) =>
        {
            IRelevanceEntity re = queriedEntity as IRelevanceEntity;
            return (re != null);
        }, qr);

        int count = qr.Count;
        for (int i = 0; i < count; ++i)
        {
            var entity = qr[i];
            if (mRelevanceObjects.ContainsKey(entity.ID))
            {
                RelevanceObjectState os = mRelevanceObjects[entity.ID];
                os.lastTimeFoundRelevant = tick;
            }
            else
            {
                IRelevanceEntity re = entity as IRelevanceEntity;
                RelevanceObjectState os = new RelevanceObjectState(re);
                os.lastTimeFoundRelevant = tick;
                mRelevanceObjects.Add(entity.ID, os);
                re.OnRelevant();
            }
        }

        removeIDs.Clear();
        foreach (KeyValuePair<int, RelevanceObjectState> kvp in mRelevanceObjects)
        {
            RelevanceObjectState os = kvp.Value;
            if (tick - os.lastTimeFoundRelevant > RelevantTimeOut)
                removeIDs.Add(kvp.Key);
        }

        count = removeIDs.Count;
        for (int i = 0; i < count; ++i)
        {
            int id = removeIDs[i];
            RelevanceObjectState os = mRelevanceObjects[id];
            IRelevanceEntity re = os.entity;
            mRelevanceObjects.Remove(id);
            re.OnIrrelevant();
        }

        return true;
    }

    public void OpenHUDChat(UnityAction callback = null)
    {
        GameObject hudChat = UIManager.GetWidget(HUDWidgetType.Chatroom);
        if (hudChat.activeSelf == false)
        {
            var miniChatAnimator = UIManager.GetWidget(HUDWidgetType.Minichat).GetComponent<Animator>();
            StartCoroutine(WaitForAnimation(miniChatAnimator, "HUD_MiniChatHide", () =>
            {
                hudChat.SetActive(true);
                UIManager.SetWidgetActive(HUDWidgetType.Minichat, false);
                GameInfo.ResetJoystick();

                if (callback != null)
                    callback.Invoke();
            }));
        }
        else
        {
            if (callback != null)
                callback.Invoke();
        }
    }

    public void CloseHUDChat()
    {
        GameObject hudChat = UIManager.GetWidget(HUDWidgetType.Chatroom);
        if (hudChat.activeSelf == true)
        {
            Animator animator = hudChat.GetComponent<Animator>();
            if (animator != null)
            {
                StartCoroutine(WaitForAnimation(animator, "ChatRoom_Close", () =>
                {
                    hudChat.SetActive(false);
                    UIManager.SetWidgetActive(HUDWidgetType.Minichat, true);
                    var miniChatAnimator = UIManager.GetWidget(HUDWidgetType.Minichat).GetComponent<Animator>();
                    StartCoroutine(WaitForAnimation(miniChatAnimator, "HUD_MiniChatShow"));
                }));
            }
        }
    }

    public IEnumerator WaitForAnimation(Animator animator, string stateName, UnityAction callback = null)
    {
        animator.PlayFromStart(stateName);
        do
        {
            yield return null;
        }
        while (animator.IsPlaying(stateName));

        if (callback != null)
            callback.Invoke();
    }

    public void InspectPlayer(string playername)
    {
        if (!string.IsNullOrEmpty(playername))
        {
            if (playername == GameInfo.gLocalPlayer.Name)
                return;

            RPCFactory.CombatRPC.GetInspectPlayerInfo(playername);
        }
    }

    public void OnChatWhisper(string playername)
    {
        if (!string.IsNullOrEmpty(playername))
        {
            OpenHUDChat(() =>
            {
                GameObject hudChat = UIManager.GetWidget(HUDWidgetType.Chatroom); // Open HUD Chat is not active
                //if (hudChat != null) // Show player name in textfield
                //    hudChat.GetComponent<HUD_Chat>().OnClickFriendsData(playername);
            });
        }
    }

    #region Social

    private string ConcatPlayerListHelper(List<string> playerNameList)
    {
        int cnt = playerNameList.Count;
        if (cnt == 0)
            return "";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < cnt; ++i)
        {
            sb.Append(playerNameList[i]);
            if (i < cnt - 1)
                sb.Append('`');
        }
        return sb.ToString();
    }

    public void SocialAcceptFriendRequest(List<string> playerNameList)
    {
        string playerListStr = ConcatPlayerListHelper(playerNameList);
        SocialAcceptFriendRequest(playerListStr);
    }

    public void SocialAcceptFriendRequest(string playerNameStr)
    {
        if (!string.IsNullOrEmpty(playerNameStr))
        {
            RPCFactory.CombatRPC.SocialAcceptRequest(playerNameStr);
        }
    }

    public void SocialRemoveFriendRequest(List<string> playerNameList)
    {
        string playerListStr = ConcatPlayerListHelper(playerNameList);
        SocialRemoveFriendRequest(playerListStr);
    }

    public void SocialRemoveFriendRequest(string playerNameStr)
    {
        if (!string.IsNullOrEmpty(playerNameStr))
            RPCFactory.CombatRPC.SocialRemoveRequest(playerNameStr);
    }

    public void SocialSendFriendRequest(List<string> playerNameList)
    {
        string playerListStr = ConcatPlayerListHelper(playerNameList);
        SocialSendFriendRequest(playerListStr);
    }

    public void SocialSendFriendRequest(string playerNameStr)
    {
        if (!string.IsNullOrEmpty(playerNameStr))
        {
            if (!playerNameStr.Equals(GameInfo.gLocalPlayer.Name))
                RPCFactory.CombatRPC.SocialSendRequest(playerNameStr);
        }
    }

    public void SocialRemoveFriend(string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
            RPCFactory.CombatRPC.SocialRemoveFriend(playerName);
    }

    public void SocialGetRecommendedFriends()
    {
        RPCFactory.CombatRPC.SocialGetRecommendedFriends();
    }

    public void SocialUpdateFriendsInfo()
    {
        RPCFactory.CombatRPC.SocialUpdateFriendsInfo();
    }

    #endregion Social

    public void OnClickHyperText(HyperText source, HyperText.LinkInfo linkInfo)
    {
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        if (linkInfo.Name.StartsWith("|pn:|"))
        {
            GameInfo.gCombat.InspectPlayer(linkInfo.Name.Substring(5));
        }
        else if (linkInfo.Name.StartsWith("|party|"))
        {
            if (localplayer.IsInParty())
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hasParty"));
            else
            {
                string[] partyInfo = linkInfo.Name.Substring(7).Split(';');
                int partyId = int.Parse(partyInfo[0]);
                int memberCount = int.Parse(partyInfo[1]);
                int idx = 2;
                List<PartyMemberInfo> members = new List<PartyMemberInfo>();
                for (int i = 0; i < memberCount; ++i)
                {
                    string name = partyInfo[idx++];
                    int level = int.Parse(partyInfo[idx++]);
                    int portraitId = int.Parse(partyInfo[idx++]);
                    bool isLeader = int.Parse(partyInfo[idx++]) == 1 ? true : false;
                    PartyMemberInfo memInfo = new PartyMemberInfo(name, level, portraitId, isLeader);
                    members.Add(memInfo);
                }
                UIManager.OpenDialog(WindowType.DialogPartyInfo,
                    (window) => window.GetComponent<UI_Party_InfoDialog>().Init(partyId, members));
            }
        }
        else if (linkInfo.Name.StartsWith("|guild|"))
        {
            if (localplayer.SecondaryStats.guildId > 0)
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Guild_Request_HasGuild"));
            else
            {
                string[] guildInfo = linkInfo.Name.Substring(7).Split(';');
                if (guildInfo.Length == 5)
                {
                    int guildId = int.Parse(guildInfo[0]);
                    byte faction = byte.Parse(guildInfo[1]);
                    int combatscore = int.Parse(guildInfo[2]);
                    int progresslvl = int.Parse(guildInfo[3]);
                    byte viplvl = byte.Parse(guildInfo[4]);
                    if (localplayer.PlayerSynStats.faction != faction)
                        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Guild_IncompatibleFaction"));
                    else if (localplayer.PlayerSynStats.Level < progresslvl)
                        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Guild_Request_LvlTooLow"));
                    else if (localplayer.PlayerSynStats.vipLvl < viplvl)
                        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Guild_Request_VIPLvlTooLow"));
                    else
                        RPCFactory.CombatRPC.GuildJoin(guildId);
                }
            }
        }
        else
        {
            IInventoryItem item = GameRepo.ItemFactory.GetItemFromCode(linkInfo.Name, true);
            if (item == null)
                return;
            //ItemUtils.OnViewItem(item);
        }
    }

    public void OpenDungeonEnterConfirmDialog(int realmId, bool hasEntry)
    {
        RealmJson realmJson = RealmRepo.GetInfoById(realmId);
        if (realmJson == null)
            return;

        string areYouSureEnterStr = GUILocalizationRepo.GetLocalizedString("dun_AreYouSureEnter");
        strFormatParam.Clear();
        strFormatParam.Add("name", realmJson.localizedname);
        strFormatParam.Add("lvl", realmJson.reqlvl.ToString());
        areYouSureEnterStr = GameUtils.FormatString(areYouSureEnterStr, strFormatParam);
        StringBuilder sb = new StringBuilder(areYouSureEnterStr);
        //if (realmJson.type == RealmType.DungeonDailySpecial)
        //    sb.Append(GUILocalizationRepo.GetLocalizedString("dun_DifficultyWarning"));
        sb.AppendLine();
        if (!hasEntry)
        {
            sb.Append(GUILocalizationRepo.GetLocalizedString("dun_NoRewardAvail"));
            sb.AppendLine();
        }
        sb.Append(GUILocalizationRepo.GetLocalizedString("dun_ConfirmCountdown"));

        System.Action okCallBack = () => { RPCFactory.CombatRPC.DungeonEnterState(realmId, 1); };
        System.Action cancelCallBack = () => { RPCFactory.CombatRPC.DungeonEnterState(realmId, 2); };
        UIManager.OpenYesNoDialog(sb.ToString(), okCallBack, cancelCallBack, 5, okCallBack);
    }

    public void OnLevelChanged()
    {
        mEntitySystem.RemoveAllNetEntities();
        if (mNetClient != null)
        {
            mNetClient.CleanUp();
            mNetClient = null;
        }
        HUD_MapController.FreeMapController();
        GameInfo.gCombat = null;
        gameObject.SetActive(false);
    }

    #region Select Entity
    public void OnSelectEntity(BaseClientEntity entity)
    {
        if (entity == GameInfo.gSelectedEntity)
            return;

        if (hud_selectTarget == null)
        {
            GameObject widgetObj = UIManager.GetWidget(HUDWidgetType.SelectTarget);
            if (widgetObj != null)
                hud_selectTarget = widgetObj.GetComponent<HUD_SelectTarget>();
        }

        //Unset player label
        if (GameInfo.gSelectedEntity != null && GameInfo.gSelectedEntity.IsPlayer())
        {
            PlayerGhost pg = GameInfo.gSelectedEntity as PlayerGhost;
            pg.HeadLabel.mPlayerLabel.SetFieldPlayer();
        }

        if (entity != null && entity.CanSelect)
        {
            //print("OnSelectEntity: " + entity.AnimObj.name);
            GameInfo.gSelectedEntity = entity;

            DestroySelectIndicator();
            if ((entity.IsPlayer() || entity.IsMonster()) && CombatUtils.IsEnemy(GameInfo.gLocalPlayer, entity))
                mSelectIndicator = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/IndicatorEnemy.prefab", true);
            else
                mSelectIndicator = ObjPoolMgr.Instance.GetObject(OBJTYPE.MODEL, "Effects_ZDSP_Indicators_prefab/IndicatorFriend.prefab", true);

            if (entity.AnimObj != null)
                mSelectIndicator.transform.SetParent(entity.AnimObj.transform, false);

            if (hud_selectTarget != null)
            {
                hud_selectTarget.InitSelectTarget(entity);
                UIManager.SetWidgetActive(HUDWidgetType.SelectTarget, true);
            }

            //set player label
            if (entity.IsPlayer())
            {
                PlayerGhost pg = entity as PlayerGhost;
                pg.HeadLabel.mPlayerLabel.SetSelectedFieldPlayer();
            }
        }
        else
        {
            //print("OnSelectEntity changed to : NULL");
            GameInfo.gSelectedEntity = null;
            DestroySelectIndicator();
            UIManager.SetWidgetActive(HUDWidgetType.SelectTarget, false);
        }
    }

    private void DestroySelectIndicator()
    {
        if (mSelectIndicator != null)
        {
            Destroy(mSelectIndicator);
            mSelectIndicator = null;
        }
    }

    public void UpdateSelectedEntityHealth(float displayHp)
    {
        if (hud_selectTarget != null)
            hud_selectTarget.SetTargetHeath(displayHp);
    }
    #endregion

    public ActorGhost GetClosestValidEnemy(float dist = 12.0f)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        //Entity entity = mEntitySystem.QueryForClosestEntityInSphere(player.Position, dist, (queriedEntity) =>
        //{
        //    ActorGhost target = queriedEntity as ActorGhost;
        //    return (target != null && !target.Destroyed && target.IsAlive() && CombatUtils.IsEnemy(player, target));
        //});

        //if (entity != null)
        //    return (ActorGhost)entity;
        //else
        //    return null;

        ActorGhost ghost = player.Bot.QueryForNonSpecificTarget(dist, true, new int[] { });
        if (ghost != null)
            return ghost;
        else
            return null;
    }

    public void DisableSceneRendering()
    {
        mEnableSceneRender = false;
        PlayerCamera.GetComponent<Camera>().cullingMask = 0;
    }

    public void RestoreSceneRendering()
    {
        mEnableSceneRender = true;
        RestoreCameraCurrentCullMask();
    }

    public void RestoreCameraDefaultCullMask()
    {
        mCurrentCullMask = mDefaultCullMask;

        if (mEnableSceneRender)
            PlayerCamera.GetComponent<Camera>().cullingMask = mDefaultCullMask;
    }

    public void SetCameraCurrentCullMask(int mask)
    {
        mCurrentCullMask = mask;

        if (mEnableSceneRender)
            PlayerCamera.GetComponent<Camera>().cullingMask = mCurrentCullMask;
    }

    public void RestoreCameraCurrentCullMask()
    {
        if (mEnableSceneRender)
            PlayerCamera.GetComponent<Camera>().cullingMask = mCurrentCullMask;
    }

    public void ShowEntitiesForCutscene(bool show)
    {
        Dictionary<int, Entity> entities = mEntitySystem.GetAllEntities();
        foreach (var entity in entities.Values)
        {
            if (entity != null && entity is BaseClientEntity)
            {
                if (entity is PlayerGhost && entity != GameInfo.gLocalPlayer && GameSettings.HideOtherPlayers)
                    ((BaseClientEntity)entity).Show(false);
                else
                    ((BaseClientEntity)entity).Show(show);
            }
        }
    }

    public void ShowAllOtherEntities(bool show)
    {
        Dictionary<int, Entity> entities = mEntitySystem.GetAllEntities();
        foreach (var entity in entities.Values)
        {
            if (entity != null && entity is BaseClientEntity)
            {
                if (entity is PlayerGhost && entity == GameInfo.gLocalPlayer)
                    continue;
                else if (entity is PlayerGhost && GameSettings.HideOtherPlayers)
                    ((BaseClientEntity)entity).Show(false);
                else
                    ((BaseClientEntity)entity).Show(show);
            }
        }
    }

    public void ShowAllPlayerGhost(bool val)
    {
        Dictionary<int, Entity> netEnts = mEntitySystem.GetAllNetEntities();
        foreach (KeyValuePair<int, Entity> kvp in netEnts)
        {
            PlayerGhost player = kvp.Value as PlayerGhost;
            if (player != null && player != GameInfo.gLocalPlayer)
            {
                player.Show(val);
            }
        }
    }

    public void OnFinishedTraingingRealm()
    {
        RPCFactory.CombatRPC.FirstRealmStep((int)Trainingstep.Finished);
    }

    public void SetUIAmbientLight(bool enable)
    {
        mEnvironmentController.SetUIAmbientLight(enable);
    }

    public void EnableZoomInMode(bool enable)
    {
        mEnvironmentController.EnableZoomInMode(enable);
    }

    public void SetMonsterParent(GameObject AnimObj)
    {
        AnimObj.transform.SetParent(mMonsterHolder.transform, false);
    }

    public void SetStaticNPCParent(GameObject AnimObj)
    {
        AnimObj.transform.SetParent(mStaticNPCHolder.transform, false);
    }

    public void SetPlayerParent(GameObject AnimObj)
    {
        AnimObj.transform.SetParent(mPlayerHolder.transform, false);
    }

    public void SetPlayerOwnedNPCParent(GameObject AnimObj)
    {
        AnimObj.transform.SetParent(mPlayerOwnedNPCHolder.transform, false);
    }

    public void SetLootParent(GameObject AnimObj)
    {
        AnimObj.transform.SetParent(mLootHolder.transform, false);
    }
    
    #region Coroutine for hero use
    public void WaitForHero(Func<bool> del, UnityAction callback)
    {
        StartCoroutine(WaitUntilHeroReady(del, callback));
    }

    private IEnumerator WaitUntilHeroReady(Func<bool> del, UnityAction callback)
    {
        yield return new WaitUntil(del);

        if (callback != null)
            callback.Invoke();
    }
    #endregion

    #region Skill casting

    /// <summary>
    /// the function to initial basic attack.
    /// It will do path find and then cast.
    /// </summary>
    /// <param name="targetpid"></param>
    public void CommonCastBasicAttack(int targetpid)
    {
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        Debug.Log("common basic attack " + targetpid);
        if (localplayer.IsStun())
            return;
        ActorGhost ghost = null;
        if (targetpid != 0)
        {
            ghost = localplayer.EntitySystem.GetEntityByPID(targetpid) as ActorGhost;
        }
        if (ghost == null)
            return;

        mPlayerInput.SetMoveIndicator(Vector3.zero);
        int pid = ghost.GetPersistentID();
        Vector3 dir = localplayer.Position - ghost.Position;
        //SkillData skdata = SkillRepo.GetSkillByGroupID(SkillRepo.Rage_BasicAtk1);
        int weaponType = (int)(localplayer.WeaponTypeUsed);
        string genderStr = (localplayer.PlayerSynStats.Gender == 0) ? "M" : "F";
        SkillData skdata = SkillRepo.GetGenderWeaponBasicAttackData((PartsType)weaponType, 1, genderStr);

        //hot fix for now
        if(skdata == null)
            skdata = SkillRepo.GetWeaponsBasicAttackData((PartsType)weaponType, 1);

        float dist = skdata.skillJson.radius;
        if (dir.magnitude >= dist)
            localplayer.ProceedToTarget(ghost.Position, pid, CallBackAction.BasicAttack);
        else
            DirectCastSkill(skdata.skillJson.id, pid);
    }

    protected bool IsDoingBasicAttack(out int pid)
    {
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        Zealot.Common.Actions.Action currAction = localplayer.GetAction();
        ClientAuthoCastSkill skillAction = currAction as ClientAuthoCastSkill;
        pid = 0;
        if (skillAction == null)
            return false;
        pid = skillAction.Targetpid();
        return skillAction.PlayerBasicAttack;
    }

    /// <summary>
    /// function for cast ActiveSkill,
    /// it will apporach first then cast.
    /// </summary>
    /// <param name="skillid"></param>
    /// <param name="pid"></param>
    /// <param name="pos"></param>
    public void ApproachAndCastSkill(int skillid, int pid, Vector3 pos)
    {
        SkillData sdata = SkillRepo.GetSkill(skillid);
        if (sdata == null)
            return;
        if (sdata.skillgroupJson.skilltype != SkillType.Active)
            return;

        if (sdata.skillgroupJson.skillbehavior == SkillBehaviour.Self)
            return;

        float distRequirement = sdata.skillJson.range;
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        if (GameUtils.InRange(localplayer.Position, pos, distRequirement))
        {
            DirectCastSkill(skillid, pid, pos);
        }
        else
        {
            //targetpos is the cast location.  not destination pos;
            float range = sdata.skillJson.radius;
            localplayer.ProceedToTarget(pos, pid, CallBackAction.ActiveSkill, range, skillid);
        }
    }

    /// <summary>
    /// casting skill function with local player with selection result.
    /// this function not supposed to do
    /// pathfind .  do not call this directly from else where;
    /// </summary>
    /// <param name="skillid"></param>
    /// <param name="targetpid"></param>
    /// <param name="pos"></param>
    private void DirectCastSkill(int skillid, int targetpid = 0, Vector3? pos = null)
    {
        PlayerGhost localplayer = GameInfo.gLocalPlayer;
        if (localplayer == null || !localplayer.CanCastSkill(true))
            return;
        PlayerSkillCDState cdstate = GameInfo.gSkillCDState;
        if (cdstate.IsSkillCoolingDown(skillid))
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CastSkillFail_CD"));
            return;
        }
        CastSkillCommand castcmd = new CastSkillCommand();
        castcmd.skillid = skillid;
        castcmd.targetpid = targetpid;
        if (pos.HasValue)
        {
            castcmd.targetPos = pos.Value;
        }

        Zealot.Common.Actions.Action action;
        SkillData skilldata;
        skilldata = SkillRepo.GetSkill(castcmd.skillid);
        if (skilldata == null)
            return;
        if (skilldata.skillgroupJson.skilltype == SkillType.Active && localplayer.IsSkillSilenced(skilldata))
            return;

        if (skilldata.skillgroupJson.moonwalk)
        {
            WalkAndCastCommand cmd = new WalkAndCastCommand();
            cmd.skillid = castcmd.skillid;
            cmd.targetPos = localplayer.Position;
            cmd.targetpid = castcmd.targetpid;
            action = new ClientAuthoWalkAndCast(localplayer, cmd);
        }
        // might not have dash attack anymore
        //else if (skilldata.skillgroupJson.dashattack)
        //{
        //    int pid = localplayer.Bot.FaceNearTarget();
        //    DashAttackCommand cmd = new DashAttackCommand();
        //    cmd.targetpid = pid;
        //    cmd.targetpos = GameInfo.gLocalPlayer.Position + GameInfo.gLocalPlayer.Forward * skilldata.skillJson.range;
        //    cmd.range = 0;
        //    //cmd.dashduration = 2.0f;// skilldata.skillgroupJson.skillduration;//not needed. dur in skilldata
        //    cmd.skillid = castcmd.skillid;
        //    action = new ClientAuthoDashAttack(localplayer, cmd);
        //}
        else
        {
            action = new ClientAuthoCastSkill(localplayer, castcmd);
        }
        int currpid = 0;
        if (IsDoingBasicAttack(out currpid))
        {
            action.SetCompleteCallback(() =>
            {
                CommonCastBasicAttack(currpid);
            });
        }
        else
        {
            action.SetCompleteCallback(() =>
            {
                localplayer.Idle();
            });
        }

        bool canStart = localplayer.PerformAction(action);
        if (canStart)
        {
            //todo: add cooldown in HUD
            //int cooldownIndex = (int)skillno - 1; //starts from 0 to 4
            //AddActiveSkillCooldown(cooldownIndex, skilldata.skillgroupJson);

        }
    }

    /// <summary>
    /// this is the starting function for casting skill in the Authoritive client.
    /// handling all nessaray target selection stuff;
    /// </summary>
    /// <param name="skillid"></param>
    public void TryCastActiveSkill(int skillid)
    {
        SkillData sdata = SkillRepo.GetSkill(skillid);
        if (sdata == null)
            return;
        if (sdata.skillgroupJson.skilltype == SkillType.BasicAttack)
        {
            //basic attack must click the target. if need auto select nearest enemy, do a query and callbackattack
            //CommonCastBasicAttack();
            return;
        }
        if (sdata.skillgroupJson.skilltype != SkillType.Active)
            return;
        if (sdata.skillgroupJson.skillbehavior == SkillBehaviour.Self)
        {
            int pid = 0;
            ActorGhost ghost = GameInfo.gLocalPlayer.Bot.QueryForNonSpecificTarget(10, true, new int[] { });
            if (ghost != null)
            {
                pid = ghost.GetPersistentID();
            }
            mPlayerInput.SetMoveIndicator(Vector3.zero);
            DirectCastSkill(skillid, pid);
        }
        else if (sdata.skillgroupJson.skillbehavior == SkillBehaviour.Ground)
        {
            mPlayerInput.ListenForPos((Vector3 pos) =>
            {
                Debug.Log("selected pos is " + pos.ToString());
                mPlayerInput.SetMoveIndicator(Vector3.zero);
                ApproachAndCastSkill(skillid, -1, pos);
            }, true);
        }
    }

    #endregion skill casting
}
