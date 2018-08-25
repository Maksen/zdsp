using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public static class UIManager
{
    public delegate void WindowCallbackDelegate(GameObject window);

    public static UIHierarchy UIHierarchy;
    public static UIHud UIHud;
    public static GameLoadingScreen LoadingScreen;
    public static SystemMsgManager SystemMsgManager;
    public static AlertManagerVersion2 AlertManager2;

    private static Dictionary<WindowType, UIWindow> windowsMap = new Dictionary<WindowType, UIWindow>();
    private static Dictionary<HUDWidgetType, HUDWidget> widgetsMap = new Dictionary<HUDWidgetType, HUDWidget>();
    private static WindowStack<WindowType> windowStack = new WindowStack<WindowType>();
    private static WindowStack<WindowType> dialogStack = new WindowStack<WindowType>();

    public static void RegisterLoadingScreen(GameLoadingScreen loadingScreen)
    {
        LoadingScreen = loadingScreen;
    }

    public static void ShowLoadingScreen(bool value)
    {
        if (LoadingScreen != null)
            LoadingScreen.ShowLoadingScreen(value);
    }

    public static void RegisterUIHierarchy(UIHierarchy uihierarchy)
    {
        UIHierarchy = uihierarchy;
    }

    public static void RegisterHUD(UIHud hud)
    {
        UIHud = hud;
    }

    public static void RegisterSystemMsgManager(SystemMsgManager sysMsgMgr)
    {
        SystemMsgManager = sysMsgMgr;
    }

    public static void RegisterJoystick(HUDWidget widget)
    {
        if (!widgetsMap.ContainsKey(HUDWidgetType.Joystick))
            widgetsMap.Add(HUDWidgetType.Joystick, widget);
    }

    public static void RegisterWidget(HUDWidget widget)
    {
        HUDWidgetType widgetType = widget.widgetType;

        if (widgetsMap.ContainsKey(widgetType))
        {
            var widgetGO = widgetsMap[widgetType].gameObject;
            Debug.LogErrorFormat("Duplicate widgetType {0} registered with UI Manager. Check [{1}] and [{2}]", widgetType, widgetGO.name, widget.gameObject.name);
        }
        else
            widgetsMap.Add(widgetType, widget);
    }

    public static void RegisterWindow(UIWindow uiWindow)
    {
        WindowType windowType = uiWindow.windowType;
        if (windowsMap.ContainsKey(windowType))
        {
            var window1 = windowsMap[windowType].gameObject;
            Debug.LogErrorFormat("Duplicate windowtype {0} registered with UI Manager. Check [{1}] and [{2}]", windowType, window1.name, uiWindow.gameObject.name);
        }
        else
            windowsMap.Add(windowType, uiWindow);
    }

    public static GameObject GetWindowGameObject(WindowType windowType)
    {
        if (windowsMap.ContainsKey(windowType))
            return windowsMap[windowType].gameObject;
        else
            return null;
    }

    #region SystemMsgManager

    public static void ShowSystemMessage(string message, bool addToChatLog = false)
    {
        if (SystemMsgManager != null)
            SystemMsgManager.ShowSystemMessage(message, addToChatLog);
    }

    /// <summary>
    /// Default unlimited duration and no message.
    /// </summary>
    public static void StartHourglass(float duration = 0f, string message = "", Action timeOutCallback = null)
    {
        //if (string.IsNullOrEmpty(message))
        //    message = GUILocalizationRepo.GetLocalizedString("com_rpcwait");

        if (SystemMsgManager != null)
            SystemMsgManager.StartHourglass(duration, message, timeOutCallback);
    }

    public static void StopHourglass()
    {
        if (SystemMsgManager != null)
            SystemMsgManager.StopHourglass();
    }

    public static void ShowPartyMessage(string message, Action okCallback, Action timeOutCallback = null, float duration = 5f)
    {
        if (SystemMsgManager != null)
            SystemMsgManager.ShowPartyMessage(message, okCallback, timeOutCallback, duration);
    }

    public static void ShowEventNotification(string message)
    {
        if (SystemMsgManager != null)
            SystemMsgManager.ShowEventNotification(message);
    }
    #endregion SystemMsgManager

    #region Open/Close windows and dialogs

    public static bool OpenWindow(WindowType windowType, WindowCallbackDelegate callback = null)
    {
        if (windowType >= WindowType.DialogStart && windowType <= WindowType.DialogEnd && windowType != WindowType.None)
        {
            Debug.LogError("Trying to open wrong window type, please check " + windowType + " is a window");
            return false;
        }

        if (GameInfo.gLocalPlayer != null && !GameInfo.gLocalPlayer.IsFeatureUnlocked(windowType))
            return false;

        if (windowsMap.ContainsKey(windowType) && !windowsMap[windowType].gameObject.activeInHierarchy)
        {
            HideTopWindow();
            UIHud.HideHUD();
            if (GameInfo.gCombat != null)
                GameInfo.gCombat.DisableSceneRendering();
            windowsMap[windowType].OpenWindow();
            if (callback != null)
                callback(windowsMap[windowType].gameObject);
            return true;
        }
        return false;
    }

    public static void OpenDialog(WindowType windowType, WindowCallbackDelegate callback = null)
    {
        if ((windowType < WindowType.DialogStart || windowType > WindowType.DialogEnd) && windowType != WindowType.None)
        {
            Debug.LogError("Trying to open wrong window type, please check " + windowType + " is a dialog");
        }

        if (windowsMap.ContainsKey(windowType) && !windowsMap[windowType].gameObject.activeInHierarchy)
        {
            windowsMap[windowType].OpenWindow();
            if (callback != null)
                callback(windowsMap[windowType].gameObject);
        }
    }

    public static void CloseWindow(WindowType windowType)
    {
        if (windowsMap.ContainsKey(windowType))
            windowsMap[windowType].OnClosing();
    }

    public static void CloseDialog(WindowType windowType)
    {
        CloseWindow(windowType);
    }

    public static void CloseTopWindow()
    {
        if (windowStack.Count > 0)
        {
            var windowType = windowStack.Peek();
            CloseWindow(windowType);
        }
    }

    public static void CloseTopDialog()
    {
        if (dialogStack.Count > 0)
        {
            var windowType = dialogStack.Peek();
            CloseWindow(windowType);
        }
    }

    public static void CloseAllWindows()
    {
        while (windowStack.Count > 0)
        {
            var windowType = windowStack.Peek();
            CloseWindow(windowType);
        }
    }

    public static void CloseAllDialogs()
    {
        while (dialogStack.Count > 0)
        {
            var windowType = dialogStack.Peek();
            CloseWindow(windowType);
        }
    }

    public static void HideTopWindow()
    {
        if (windowStack.Count > 0)
        {
            var topWindow = windowStack.Peek();

            if (topWindow > WindowType.PersistentWindowStart && topWindow < WindowType.PersistentWindowEnd)
                return;

            windowsMap[topWindow].ShowCanvas(false);
        }
    }

    public static void HideOpenedDialogs(bool hide)
    {
        for (int i = 0; i < dialogStack.Count; i++)
        {
            var windowType = dialogStack[i];
            windowsMap[windowType].ShowCanvas(!hide);
        }
    }

    #endregion Open/Close windows and dialogs

    #region Open specific windows

    public static void OpenYesNoDialog(string content, Action yes_callback, Action no_callback = null, int countdown = 0, 
                                       Action cd_callback = null)
    {
        OpenDialog(WindowType.DialogYesNoOk, (GameObject window) =>
        {
            window.GetComponent<UI_DialogYesNo>().InitDialogYesNo(content, yes_callback, no_callback, countdown, cd_callback);
        });
    }

    public static void OpenOkDialog(string content, Action ok_callback)
    {
        OpenDialog(WindowType.DialogYesNoOk, (GameObject window) =>
        {
            window.GetComponent<UI_DialogYesNo>().InitDialogOk(content, ok_callback);
        });
    }

    #endregion Open specific windows

    public static void ShowTickerTapeMessage(string msg)
    {
        GameObject widget = GetWidget(HUDWidgetType.TickerTape);
        if (widget != null)
            widget.GetComponent<HUD_TickerTape>().SetAnnoucementText(msg);
    }

    /// <summary>
    /// To be called when destroying UIHierarchy
    /// </summary>
    public static void DestroyAllWindows()
    {
        UIHierarchy = null;
        UIHud = null;
        SystemMsgManager = null;
        AlertManager2 = null;
        windowsMap.Clear();
        widgetsMap.Clear();
        windowStack.Clear();
        dialogStack.Clear();
    }

    public static void DestroySystemMsgManager()
    {
        SystemMsgManager = null;
    }

    public static void DestroyLoadingScreen()
    {
        LoadingScreen.DestroyLoadingScreen();
        LoadingScreen = null;
    }

    #region OpenedWindows

    /// <summary>
    /// Push window into stack
    /// </summary>
    public static void OnWindowOpened(WindowType windowType)
    {
        windowStack.Push(windowType);
    }

    public static void OnDialogOpened(WindowType windowType)
    {
        if (!dialogStack.Contains(windowType))
            dialogStack.Push(windowType);
    }

    /// <summary>
    /// Remove window from stack and show previous window
    /// </summary>
    public static void OnWindowClosing(WindowType windowType)
    {
        if (windowStack.Count > 0)
        {
            //show next active window
            if (windowStack.Peek() == windowType)
            {
                windowStack.Pop();
                if (windowStack.Count > 0)
                    windowsMap[windowStack.Peek()].ShowCanvas(true);
            }
            else
            {
                //handle closing of non topmost window
                if (windowStack.Contains(windowType))
                {
                    windowsMap[windowType].ShowCanvas(true);   // turn back on canvas
                    windowStack.Remove(windowType);
                }
            }
        }
    }

    public static void OnDialogClosing(WindowType windowType)
    {
        if (dialogStack.Count > 0)
        {
            //show next active window
            if (dialogStack.Peek() == windowType)
            {
                dialogStack.Pop();
            }
            else
            {
                //handle closing of non topmost dialog
                if (dialogStack.Contains(windowType))
                {
                    dialogStack.Remove(windowType);
                }
            }
        }
    }

    /// <summary>
    /// Handle things to do when window closed is last window
    /// </summary>
    private static void OnWindowClosed(WindowType windowType)
    {
        if (windowStack.Count == 0)
        {
            UIHud.ShowHUD();
            if (GameInfo.gCombat != null)
                GameInfo.gCombat.RestoreSceneRendering();
        }
    }

    public static void OnCloseWindow(UIWindow uiWindow)
    {
        WindowType windowType = uiWindow.windowType;
        if (windowsMap.ContainsKey(windowType))
        {
            windowsMap[windowType].CloseWindow();  // Set window inactive
            OnWindowClosed(windowType);  // Show HUD
        }
    }

    public static void OnCloseDialog(UIWindow uiWindow)
    {
        WindowType windowType = uiWindow.windowType;
        if (windowsMap.ContainsKey(windowType))
            windowsMap[windowType].CloseWindow();
    }

    public static bool IsWindowOnTop(WindowType windowtype)
    {
        if (windowStack.Count > 0)
            return windowStack.Peek() == windowtype;
        return false;
    }

    public static bool IsWindowOpen(WindowType windowType)
    {
        GameObject go = GetWindowGameObject(windowType);
        if (go != null)
            return go.activeInHierarchy;
        return false;
    }

    public static bool IsAnyWindowOpened()
    {
        return windowStack.Count > 0;
    }

    public static bool IsAnyDialogOpened()
    {
        return dialogStack.Count > 0;
    }

    #endregion OpenedWindows

    public static void OnLevelChanged()
    {
        OffAllWidgets();
        foreach (var widget in widgetsMap.Values)
            widget.OnLevelChanged();

        CloseAllDialogs();
        CloseAllWindows();
        foreach (var window in windowsMap.Values)
            window.OnLevelChanged();
    }

    #region Window Behaviour

    public static BaseWindowBehaviour GetWindowBehaviour(WindowType windowType)
    {
        if (windowsMap.ContainsKey(windowType))
            return windowsMap[windowType].GetWindowBehaviour();
        else
            return null;
    }

    public static BaseWindowBehaviour GetTopWindowBehaviour()
    {
        if (windowStack.Count > 0)
            return windowsMap[windowStack.Peek()].GetWindowBehaviour();
        else return null;
    }

    public static WindowType GetTopWindowType()
    {
        if (windowStack.Count > 0)
            return windowStack.Peek();

        return WindowType.None;
    }

    #endregion Window Behaviour

    #region Widget

    public static GameObject GetWidget(HUDWidgetType type)
    {
        if (widgetsMap.ContainsKey(type))
            return widgetsMap[type].gameObject;
        return null;
    }

    public static void SetWidgetActive(HUDWidgetType type, bool active)
    {
        if (widgetsMap.ContainsKey(type))
        {
            if (active)
                widgetsMap[type].OnActivate();
            else
                widgetsMap[type].OnDeactivate();
        }
    }

    public static bool IsWidgetActived(HUDWidgetType type)
    {
        if (widgetsMap.ContainsKey(type))
        {
            return widgetsMap[type].IsActived();
        }
        return false;
    }

    public static void OffAllWidgets()
    {
        foreach (var widget in widgetsMap)
        {
            widget.Value.OnDeactivate();
        }
    }

    public static void OnAllWidgets()
    {
        foreach (var widget in widgetsMap)
        {
            if (widget.Value.ActiveOnStartup)
                widget.Value.OnActivate();
        }
    }

    #endregion Widget

    public static RectTransform GetHUDGameCanvas()
    {
        return UIHud.GetComponent<RectTransform>();
    }

    public static void AddToChat(byte msgType, string message)
    {
        HUD_Chatroom hudChat = GetWidget(HUDWidgetType.Chatroom).GetComponent<HUD_Chatroom>();
        if (hudChat)
            hudChat.AddToChatLog(msgType, message, "", "");
    }

    public static void OpenCutsceneDialog(bool buttonstatus = true)
    {
        if (IsAnyWindowOpened())
        {
            CloseAllWindows();
        }
        if (IsAnyDialogOpened())
        {
            CloseAllDialogs();
        }

        OpenDialog(WindowType.DialogCutscene, (window) => window.GetComponent<UI_Cutscene>().Init(buttonstatus));
    }
}

internal class WindowStack<T> : System.Collections.IEnumerable
{
    private List<T> items = new List<T>();

    public int Count { get { return items.Count; } }

    public void Push(T item)
    {
        items.Add(item);
    }

    public T Pop()
    {
        if (items.Count > 0)
        {
            T temp = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            return temp;
        }
        else
            return default(T);
    }

    public void Remove(int itemAtPosition)
    {
        items.RemoveAt(itemAtPosition);
    }

    public void Remove(T item)
    {
        items.Remove(item);
    }

    public T Peek()
    {
        return items[items.Count - 1];
    }

    public T Get(int index)
    {
        return items[index];
    }

    public void Clear()
    {
        items.Clear();
    }

    public bool Contains(T obj)
    {
        return items.Contains(obj);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public T this[int i]
    {
        get
        {
            return items[i];
        }
        set
        {
            items[i] = value;
        }
    }
}