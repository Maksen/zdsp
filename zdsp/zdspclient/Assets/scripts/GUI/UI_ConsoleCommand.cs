using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleCommand : MonoBehaviour
{
    public InputField InputField;
    public ScrollRect ScrollviewCmd;
    public Text History;
    public Transform ShortCutParentTrans;
    public GameObject ButtonObj;
    public string[] AllShortCutCommand;
    void Awake()
    {
        //Disabling this lets you skip the GUI layout phase.
        this.useGUILayout = false;
        for (int i=0;i< AllShortCutCommand.Length;i++)
        {
            GameObject obj = Instantiate(ButtonObj);
            obj.transform.SetParent(ShortCutParentTrans);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;

            Button btn = obj.GetComponent<Button>();
            int index = i;
#if ZEALOT_DEVELOPMENT
            btn.onClick.AddListener(()=> { OnClickedCustomCommand(index); });
            btn.transform.GetChild(0).GetComponent<Text>().text = AllShortCutCommand[i];
#endif
        }
    }

#if ZEALOT_DEVELOPMENT
    private int searchcmdidx = -1, searchcmdhistoryidx = -1;
    private List<string> searchcmdList = null;
    private string[] cmdhistory = null;
    private string cmdsufix = "";

    public void OnClickedCustomCommand(int index)
    {
        InputField.text = AllShortCutCommand[index];
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }
    public void OnClicked_Send()
    {
        string message = (string.IsNullOrEmpty(InputField.text)) ? "" : InputField.text;
        InputField.text = "";
        cmdhistory = null;
        AddToContent(message);
        if (CommandUtils.ExecuteCommand(message))
            return;
    }

    public void AddToContent(string message)
    {
        History.text = string.Format("{0}\n{1}", History.text, message);
        if (ScrollviewCmd != null)
        {
            Canvas.ForceUpdateCanvases();
            ScrollviewCmd.verticalNormalizedPosition = 0;
        }
    }

    public void ClearContent()
    {
        History.text = "History Commands";
    }

    public void OnShortcut_BackSlash()
    {
        InputField.text = "\\";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }
    
    public void OnShortcut_Help()
    {
        InputField.text = "\\Help";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortcut_Add()
    {
        InputField.text = "\\Add";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortcut_Show()
    {
        InputField.text = "\\Show";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortcut_Unlock()
    {
        InputField.text = "\\Unlock";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortCut_Reset()
    {
        InputField.text = "\\Reset";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortcut_c()
    {
        InputField.text += "-c";
        InputField.caretPosition = InputField.text.Length;
        searchcmdidx = -1;
        searchcmdList = null;
    }

    public void OnShortcut_Up()
    {
        if (searchcmdhistoryidx < 0 || cmdhistory == null)
        {
            cmdhistory = CommandUtils.GetCommandsHistory();
            searchcmdhistoryidx = cmdhistory.Length - 1;
        }
        else
        {
            searchcmdhistoryidx--;

            if (searchcmdhistoryidx > cmdhistory.Length)
                searchcmdhistoryidx = cmdhistory.Length - 1;
            if (searchcmdhistoryidx < 0)
                searchcmdhistoryidx = 0;
        }
        if (searchcmdhistoryidx < cmdhistory.Length)
        {
            if (searchcmdhistoryidx >= 0)
            {
                InputField.text = cmdhistory[searchcmdhistoryidx];
                InputField.caretPosition = InputField.text.Length;
            }
        }
    }

    public void OnShortcut_Down()
    {
        if (searchcmdhistoryidx < 0 || cmdhistory == null)
        {
            cmdhistory = CommandUtils.GetCommandsHistory();
            searchcmdhistoryidx = cmdhistory.Length - 1;
        }
        else
        {
            searchcmdhistoryidx++;

            if (searchcmdhistoryidx > cmdhistory.Length)
                searchcmdhistoryidx = cmdhistory.Length - 1;
            if (searchcmdhistoryidx < 0)
                searchcmdhistoryidx = 0;
        }
        if (searchcmdhistoryidx < cmdhistory.Length)
        {
            if (searchcmdhistoryidx >= 0)
            {
                InputField.text = cmdhistory[searchcmdhistoryidx];
                InputField.caretPosition = InputField.text.Length;
            }
        }
    }

    public void OnShortcut_Tab()
    {
        if (InputField.text != "" && InputField.text.Length > 1)
        {
            string[] strs = InputField.text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length < 1) return;
            if (searchcmdidx < 0 || searchcmdList == null)
            {
                string cmd = strs[0].Substring(1);
                searchcmdList = CommandUtils.GetCommandsByStartStr(cmd);
                searchcmdidx = 0;
                cmdsufix = "";
                for (int i = 1; i < strs.Length; i++)
                {
                    cmdsufix += " ";
                    cmdsufix += strs[i];
                }
            }
            if (searchcmdidx < searchcmdList.Count)
            {
                InputField.text = "\\" + searchcmdList[searchcmdidx] + cmdsufix;
                InputField.caretPosition = InputField.text.Length;
                searchcmdidx++;
                if (searchcmdidx >= searchcmdList.Count)
                    searchcmdidx = 0;
            }
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return)
            {
                if (InputField.text != "")
                {
                    OnClicked_Send();
                }
            }
            else if (e.keyCode == KeyCode.Tab) 
            {
                OnShortcut_Tab();
            }
            else if (e.keyCode == KeyCode.UpArrow) 
            {
                OnShortcut_Up();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                OnShortcut_Down();
            }
            else if (e.keyCode != KeyCode.None)
            {
                searchcmdList = null;
            }
        }            
    }
#endif
}
