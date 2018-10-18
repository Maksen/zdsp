
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zealot.Spawners;
using UnityEngine.EventSystems;
using Pathfinding;
using CnControls;

public class SingleRPGControl : MonoBehaviour
{
	public GameObject m_Prefab;
    private GameObject mPlayer;
    private RPGCharacterScript mRPGCharacterScript;
    bool gamePaused = false;
	bool speedUp = false;
    private ZDSPCamera mainCam;
    private GameObject _goLevelMap, _goEventSystem;
    private Seeker _seeker;
    private Path _path;
    private bool _showMap, _doingPathfind;    
    //refer to ACApproachWithPathFind
    public float nextWaypointDistance = 0.04f;
    //The waypoint we are currently moving towards
    private int currentWaypoint = 0;
    private int PickableLayerMask;
    private EnvironmentController mEnvironmentController;

    void Awake ()
	{
        Time.timeScale = 1.0f;
        InitEventSystem();
        //InitMap();
        mainCam = GetComponent<ZDSPCamera>();
        PointerScreen.OnPointerClickEvent += OnPointerClick;
        PickableLayerMask = LayerMask.GetMask("Nav_Walkable");
    }

    void Start()  // only spawn and init camera in Start to allow widget register to finish first
    {
        if (mainCam != null)
            SpawnPlayer();
        else
            Debug.LogError("Cannot find camera");

        mEnvironmentController = new EnvironmentController();
    }

    void InitEventSystem()
    {
        if (EventSystem.current == null)
        {
            _goEventSystem = new GameObject("EventSystem");
            _goEventSystem.AddComponent<EventSystem>();
            _goEventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    void OnGUI ()
	{
        GUIStyle style = new GUIStyle();
        int fontsize = Mathf.FloorToInt(15 * ((float)Screen.height / 540f));
        style.fontSize = fontsize;
        style.normal.textColor = Color.black;
        GUIStyle buttonStyle = new GUIStyle("button");
        buttonStyle.fontSize = fontsize;

        int buttonWidth = Screen.width / 10;
		int buttonHeight = Screen.height / 10;
		
		if (GUI.Button (new Rect (20, 20, buttonWidth, buttonHeight), speedUp ? "2x" : "1x", buttonStyle)) {
			speedUp = !speedUp;
			Time.timeScale = speedUp ? 2.0f : 1.0f;
            mRPGCharacterScript.OnSpeedAdjusted(speedUp);
        }
		
		if (GUI.Button (new Rect ((Screen.width / 3 * 2) - (buttonWidth / 2), 20, buttonWidth, buttonHeight), gamePaused ? "Resume": "Pause", buttonStyle)) {
			gamePaused = !gamePaused;
			Time.timeScale = gamePaused ? 0.0f: 1.0f;
		}

        //if (GUI.Button(new Rect(Screen.width - 50 - buttonWidth/2, Screen.height - 50 - buttonHeight/2, buttonWidth, buttonHeight), "Attack", buttonStyle))
        //    mRPGCharacterScript.AttackButtonPressed();

#if UNITY_EDITOR
        //if (GUI.Button(new Rect(Screen.width - 50 - 3 * buttonWidth / 2, Screen.height - 50 - buttonHeight / 2, buttonWidth, buttonHeight), "Map", buttonStyle))
        //{
        //    _showMap = !_showMap;
        //    _goLevelMap.SetActive(_showMap);
        //}
#endif

        if (GUI.Button(new Rect(Screen.width - 50 - 2 * buttonWidth / 2, 20, buttonWidth, buttonHeight), mainCam.cameraMode.ToString(), buttonStyle))
            mainCam.ToggleCameraMode();
    }

    public void EnableZoomInMode(bool enable)
    {
        mEnvironmentController.EnableZoomInMode(enable);
    }

    private void OnPointerClick(PointerEventData eventData)
    {
        return;
        if (mainCam == null || mainCam.cameraMode == ZDSPCamera.CameraMode.Orbit)
            return;

        if (!eventData.dragging)
        {
            Ray ray = mainCam.mainCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, PickableLayerMask))
            {
                Vector3 targetPosition = hit.point;
                PathfindToPosition(targetPosition);
            }
        }
    }

    void Update ()
	{
		bool isIdle = CalculatePlayer();
        if (isIdle) //allow pathfind only when idle
        {             
            if (_path != null)
            {
                if (currentWaypoint >= _path.vectorPath.Count)
                {
                    //Debug.Log("End Of Path Reached");
                    _doingPathfind = false;
                    return;
                }
                _doingPathfind = true;
                Vector3 targetpos = _path.vectorPath[currentWaypoint];
                Vector3 curpos = mPlayer.transform.position;
                targetpos.y = mPlayer.transform.position.y;
                Vector3 distance = targetpos - curpos;                
                Vector3 dir = distance.normalized;
                dir *= mRPGCharacterScript.moveSpeed * Time.deltaTime;
                //Debug.Log("moving from " + curpos + "to pos " + targetpos + ", dir = " + dir);
                MovePlayer(targetpos, dir);
                //Check if close enough to the next waypoint
                if (distance.sqrMagnitude < nextWaypointDistance)
                {
                    currentWaypoint++;
                    return;
                }
            }
        }
       // MapUtilities.MapUpdate((long)(Time.deltaTime * 1000));
	}
	
	void SpawnPlayer ()
	{
		Vector3 spawnLocation = new Vector3(10f, 1f, 10f);
        PlayerSpawner spawner = GameObject.FindObjectOfType (typeof(PlayerSpawner)) as PlayerSpawner;
		if(spawner != null)
			spawnLocation = spawner.transform.position;
        mPlayer = Instantiate (m_Prefab, spawnLocation, Quaternion.identity) as GameObject;
        mRPGCharacterScript =  mPlayer.AddComponent<RPGCharacterScript> ();
        mainCam.Init(mPlayer);

        //for pathfinding
        _seeker = mPlayer.AddComponent<Seeker>();
        StripStraightPointsModifier sspmod = mPlayer.AddComponent<StripStraightPointsModifier>();
        _seeker.RegisterModifier(sspmod);
       // MapUtilities.SingleBindPlayer(mPlayer, this);
    }
	
    //return false if player is not idle
	bool CalculatePlayer ()
	{
        if (mRPGCharacterScript.IsAttacking())
        {
            StopPathfind();
            return false;
        }
		float moveX = CnInputManager.GetAxis ("Horizontal");
		float moveY = CnInputManager.GetAxis ("Vertical");
        if (moveX != 0.0f || moveY != 0.0f)
        {
            Vector3 dir = new Vector3(moveX, 0f, moveY);
            dir = mainCam.transform.TransformDirection(dir);
            dir.y = 0;
            dir.Normalize();
            Vector3 targetPosition = mPlayer.transform.position + dir;
            MovePlayer(targetPosition, dir);
            StopPathfind();
            return false;
        }
        else
        {
            //if (Input.GetKeyDown("space"))
            //{
            //    RaycastHit hit;
            //    Ray ray = mainCam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            //    if (Physics.Raycast(ray, out hit, 100))
            //    {
            //        mPlayer.transform.position = hit.point;
            //        StopPathfind();
            //        return false;
            //    }
            //}
            if(!_doingPathfind) mRPGCharacterScript.PlayEffect("standby");
            return true;
        }
    }

    void MovePlayer(Vector3 pos, Vector3 forward)
    {
        mPlayer.transform.LookAt(pos);
        mRPGCharacterScript.PlayEffect("run");

        float distance = forward.magnitude;
        forward.Normalize();
        Vector3 motion = ClientUtils.MoveTowards(forward, distance, mRPGCharacterScript.moveSpeed, Time.deltaTime);
        mPlayer.GetComponent<CharacterController>().Move(motion);
    }

    void InitMap()
    {
        _goLevelMap = null;
#if UNITY_EDITOR
     //   MapUtilities.InitSingle();
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/UI/Scenes/Map/P_Map/UI_Map.prefab", typeof(GameObject)) as GameObject;
        if (prefab != null)
        {
            _goLevelMap = Instantiate(prefab);
            _showMap = false;
            _goLevelMap.SetActive(_showMap);
        }
#endif
    }

    public void PathfindToPosition(Vector3 pos)
    {
        _seeker.StartPath(mPlayer.transform.position, pos, OnSeekComplete);
    }

    public void StopPathfind()
    {
        _doingPathfind = false;        
        _path = null;
    }

    void OnSeekComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            currentWaypoint = 0;
        }
        else
        {
            Debug.Log("GG, pathfind got error: " + p.error);
        }
    }

    public bool DoingPathFind() { return _doingPathfind; }

    public List<Vector3> GetPathfindWaypoints()
    {
        if (_path != null) return _path.vectorPath;
        else return new List<Vector3>();
    }

    public int GetNextWaypointId()
    {
        return currentWaypoint + 1;
    }

    void OnApplicationQuit()
    {
    }
}
