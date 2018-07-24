using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot;

public class SplashLoadingScreen : MonoBehaviour
{
    public Text textDescription;
    public GameObject progressBar;

    private UI_ProgressBarC progressBarC;
    
    [SerializeField]
    SimpleConfirmationDialog patchDialog = null;

    void Awake()
    {
        SetScreenRes();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Application.targetFrameRate = 30;
        textDescription.text = "";
        progressBar.SetActive(false);
    }

    void SetScreenRes()
    {
        int curWidth = Screen.width;
        int curHeight = Screen.height;
        Screen.SetResolution(1024, 1024 * curHeight / curWidth, true);
    }

    void Start()
    {
#if ZEALOT_DEVELOPMENT && !UNITY_EDITOR
        RemoteLog.ConnectToServer("piliqstg.zealotdigital.com.tw:5479");
        RemoteLog.EnableRemoteLog();
#endif
        GameLoader.Instance.Init(this);
        EfxSystem.Instance.Init();
        InitGameSettings();
    }

    void OnDestroy()
    {
        textDescription = null;
        progressBar = null;
    }        

    void OnEnable()
    {
        progressBarC = progressBar.GetComponent<UI_ProgressBarC>();
    }

    private void InitGameSettings()
    {
        ClientSettingsData settingsData = new ClientSettingsData();
        GameSettings.DeserializeClient(settingsData);
    }

    public void SetDownloadProgress(float progress)
    {
        progressBar.SetActive(true);
        if (progressBarC != null)
        {
            progressBarC.Value = (long)(progress * 100);
        }
    }

    public void SetDescriptionText(string text)
    {
        textDescription.text = text;
    }

    public void ShowBuildVersionErrorDialog(Action callback = null)
    {
        string msg = GameLoader.Instance.splashScreenSettings.incorrectBuildVersion;
        string title = GameLoader.Instance.splashScreenSettings.incorrectBuildVersionTitle;
        patchDialog.InitOk(gameObject, msg, () =>
        {
            if (callback != null)
                callback();
            patchDialog.gameObject.SetActive(false);
        }, title);
        patchDialog.gameObject.SetActive(true);

    }

    public void ShowPatchWarningDialog(bool show, string totalsize = "", Action callback = null)
    {
        if (show)
        {
            string msg = ClientUtils.FormatString(GameLoader.Instance.splashScreenSettings.patchWarningText, new Dictionary<string, string>() { { "size", totalsize } });
            patchDialog.InitOk(gameObject, msg, () =>
            {
                if (callback != null)
                    callback();
                patchDialog.gameObject.SetActive(false);
            }, GameLoader.Instance.splashScreenSettings.patchWarningTitle);
            patchDialog.ButtonYes.GetComponentInChildren<Text>().text = GameLoader.Instance.splashScreenSettings.patchWarningButtonText;
        }

        patchDialog.gameObject.SetActive(show);
    }

    public void ShowResumePatchDialog(bool show, int errorcode, Action callback = null)
    {
        if (show)
        {
            string msg = ClientUtils.FormatString(GameLoader.Instance.splashScreenSettings.resumePatchText,
                new Dictionary<string, string>() { { "newline", Environment.NewLine }, { "error", errorcode.ToString() } });

            patchDialog.InitOk(gameObject, msg, () =>
            {
                if (callback != null)
                    callback();
                patchDialog.gameObject.SetActive(false);
            }, GameLoader.Instance.splashScreenSettings.resumePatchTitle);
            patchDialog.ButtonYes.GetComponentInChildren<Text>().text = GameLoader.Instance.splashScreenSettings.resumePatchButtonText;
        }
        
        patchDialog.gameObject.SetActive(show);
    }
}
