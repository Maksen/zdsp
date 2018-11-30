using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CutsceneTriggerType
{   
    TriggerBox = 0,
    RealmStart,
    Quest,
    UI,    
    AutoStart 
}

public enum CanSkip
{
    True,
    False,
    First,
    Only
}

public class CutsceneEntity : MonoBehaviour
{
    #region Inspector
    public string CutsceneName;
    public CutsceneTriggerType cutsceneTriggerType;
    public TimelineAssist Cutscene; 
    public string cutscenePath;
    public CanSkip canSkip;

    public UnityEvent OnCutsceneFinished;
    private bool skipped = false;
    private bool destroyed = false;
    #endregion

    void Awake()
    {
        if (PhotonNetwork.connected && Cutscene != null )//in game Mode, it is turnned off , which start time is controlled by code.
            Cutscene.gameObject.SetActive(false);
    }

    void Start()
    {
        //if (cutsceneTriggerType == CutsceneTriggerType.AutoStart && !PhotonNetwork.connected)
        //    PlayCutscene();        
    }

    private void OnEnable()
    {
        if (cutsceneTriggerType == CutsceneTriggerType.AutoStart && !PhotonNetwork.connected)
            PlayCutscene();

        if (Cutscene != null)
        {
            var timeline = Cutscene.gameObject.GetComponent<PlayableDirector>();
            if (timeline != null)
            {
                if (timeline.isActiveAndEnabled && timeline.state == PlayState.Playing)
                {
                    CutsceneManager.instance.currentPlaying = this;
                    HandleSkip();
                }
            }
        }
    }

    void OnValidate()
    {
        gameObject.tag = "Cutscene";
    }

    void HandleSkip()
    {
        var key = "Cutscene " + CutsceneName;
        var recorded = PlayerPrefs.HasKey(key);
        bool showskipbutton = (canSkip != CanSkip.False) && ((canSkip == CanSkip.True) || (canSkip == CanSkip.First && recorded == true));

        if(CutsceneManager.instance != null && CutsceneManager.instance.skip_button != null)
            CutsceneManager.instance.skip_button.gameObject.SetActive(showskipbutton); 
    }

    public void PlayCutscene()
    {
        HandleSkip();

        skipped = false;
        destroyed = false;
        if (Cutscene != null)
            StartCutscene();
        else if (!string.IsNullOrEmpty(cutscenePath))
        {
#if UNITY_EDITOR
            GameObject cutscenePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/" + cutscenePath);
            OnPrefabLoaded(cutscenePrefab);
#else
            UIManager.StartHourglass(120);
            CutsceneManager.instance.CutsceneLoading = true;

            AssetLoader.Instance.LoadAsync<GameObject>(cutscenePath, OnPrefabLoaded);
#endif
        }
    }

    public void SkipCutScene()
    {
        if (Cutscene != null)
            Cutscene.Skip();
        else
            skipped = true;
    }

    public void CleanUp()
    {        
        if (Cutscene != null)
            Cutscene.Skip();
        else
            destroyed = true;
    }

    private void StartCutscene()
    {
        if (Cutscene.IsPlaying())
            return;
        if (GameInfo.gLocalPlayer != null)
            CutsceneManager.instance.OnStartCutscene(this);
        Cutscene.Play(Cutscene_CutsceneFinished);
    }

    private void Cutscene_CutsceneFinished()
    {
        if (GameInfo.gCombat != null)
            CutsceneManager.instance.OnCutsceneFinished(this);
        OnCutsceneFinished.Invoke();

        var key = "Cutscene " + CutsceneName;
        PlayerPrefs.SetString(key, "Played");
    }

    private void OnPrefabLoaded(GameObject cutscenePrefab)
    {
        if (destroyed)
            return;
        if (skipped)
            Debug.Log("skipped before prefab loaded " + cutscenePath);
        else if (cutscenePrefab == null)
            Debug.Log(string.Concat("Error loading cutscene assetbundle ", cutscenePath));
        else
        {
            GameObject go = Instantiate(cutscenePrefab);
            go.transform.SetParent(transform, false);
            Cutscene = go.GetComponent<TimelineAssist>();
            if (Cutscene != null)
                StartCutscene();
        }

        CutsceneManager.instance.CutsceneLoading = false;
        UIManager.StopHourglass();
    }
}
