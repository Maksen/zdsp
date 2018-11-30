#if ZEALOT_DEVELOPMENT
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Datablock;

public class CharacterDebugTool : MonoBehaviour
{
    public static void DoAction(Action<CharacterDebugTool> act)
    {
        var testTool = FindObjectOfType<CharacterDebugTool>();
        if (testTool != null && act != null)
        {
            act(testTool);
        }
    }

    [Serializable]
    public class CopyResultAction : UnityEngine.Events.UnityEvent<int, JToken[], JToken[]> { }
    [Serializable]
    public class CharacterSelect
    {
        public string CharName = string.Empty;
        public string UserID = string.Empty;
    }
    [Serializable]
    public class CharacterFix
    {
        public string CharName = string.Empty;
        public string UserID = string.Empty;
        public bool ReadOnly;
    }

    [Serializable]
    public class DebugSelectTool_Data
    {
        public bool Execute;
        public CharacterSelect[] Chars = new CharacterSelect[1] { new CharacterSelect() };
        public DB_Column DB_Column;
        public string Path = string.Empty;
        public string Key = string.Empty;

        [TextArea(10, 10)]
        public string[] Result = new string[1] { string.Empty };
        public bool CopyToFix;
        public DebugSelectTool_CopySettings CopySettings;
        public CopyResultAction CopyAction;
        public bool LoadListFromJson;
        public TextAsset JsonFile;
    }

    public enum DebugSelectTool_CopySettings
    {
        DoNotCopyResult,
        CopyResultAll,
        OnlyFirst,
        CopyResultWithAction,
    }

    [SerializeField]
    private DebugSelectTool_Data _DebugSelectTool;

    [Serializable]
    public class DebugFixTool_Data
    {
        public bool Execute;
        
        public CharacterFix[] Chars = new CharacterFix[1] { new CharacterFix() };
        public DB_Column DB_Column;
        public string Path = string.Empty;
        public string Key = string.Empty;

        public bool SetJsonFile = false;

        [TextArea(10, 10)]
        public string[] Value = new string[1] { string.Empty };
        public TextAsset[] JsonFileValue;
        public bool CopyToFix;
        public bool ForceRest;
    }

    [SerializeField]
    private DebugFixTool_Data _DebugFixTool;

    public enum DB_Column
    {
        characterdata,
        friends,
    }

    private void Awake()
    {
        if (_DebugFixTool == null)
        {
            _DebugFixTool = new DebugFixTool_Data();
        }
        if (_DebugSelectTool == null)
        {
            _DebugSelectTool = new DebugSelectTool_Data();
        }

        DontDestroyOnLoad(gameObject);
    }

    public void DebugFixTool(string userid, string charname, string db_column, string path, string key, string value, bool forceReset)
    {
        RPCFactory.NonCombatRPC.DebugFixTool(userid, charname, db_column, path, key, value, forceReset);
    }

    public void DebugFixTool_Completed(string msg)
    {
        Debug.Log("[Hook] Method:DebugFixTool_Completed " + " msg:" + msg);
    }

    public void DebugSelectTool(string userid, string charname, string db_column, string path, string key)
    {
        RPCFactory.NonCombatRPC.DebugSelectTool(userid, charname, db_column, path, key);
    }

    public void DebugSelectTool_Completed(bool success, string charname, string result)
    {
        if (_DebugSelectTool.Chars != null && _DebugSelectTool.Result != null)
        {
            for (int i = 0; i < _DebugSelectTool.Chars.Length; i++)
            {
                if (_DebugSelectTool.Chars[i].CharName == charname)
                {
                    if (_DebugSelectTool.Result != null && _DebugSelectTool.Result.Length > i)
                        _DebugSelectTool.Result[i] = result;
                }
            }
        }
        Debug.Log("[Hook] Method:DebugSelectTool_Completed " + " success:" + success);
    }



    string DebugFixTool_GetValue(int index)
    {
        string val;

        if (_DebugFixTool.SetJsonFile)
        {
            if (_DebugFixTool.JsonFileValue == null || _DebugFixTool.JsonFileValue.Length == 0)
                return "null";
            if (index >= _DebugFixTool.JsonFileValue.Length)
                val = _DebugFixTool.JsonFileValue[0].text;
            else
                val = _DebugFixTool.JsonFileValue[index].text;
        }
        else
        {
            if (_DebugFixTool.Value == null || _DebugFixTool.Value.Length == 0)
                return "null";
            if (index >= _DebugFixTool.Value.Length)
                val = _DebugFixTool.Value[0];
            else
                val = _DebugFixTool.Value[index];
        }
        if (string.IsNullOrEmpty(val))
            return "null";
        return val;
    }

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

        if (_DebugFixTool.Execute)
        {
            _DebugFixTool.Execute = false;
            if (_DebugFixTool.Chars != null)
            {
                for (int i = 0; i < _DebugFixTool.Chars.Length; i++)
                {
                    if (!_DebugFixTool.Chars[i].ReadOnly)
                    {
                        DebugFixTool(
                        _DebugFixTool.Chars[i].UserID,
                        _DebugFixTool.Chars[i].CharName,
                        _DebugFixTool.DB_Column.ToString(),
                        _DebugFixTool.Path,
                        _DebugFixTool.Key,
                        DebugFixTool_GetValue(i),
                        _DebugFixTool.ForceRest);
                    }
                }
            }
        }
        if (_DebugSelectTool.Execute)
        {
            _DebugSelectTool.Execute = false;
            if (_DebugSelectTool.Chars != null)
            {
                if (_DebugSelectTool.Result == null || _DebugSelectTool.Result.Length != _DebugSelectTool.Chars.Length)
                    _DebugSelectTool.Result = new string[_DebugSelectTool.Chars.Length];
                for (int i = 0; i < _DebugSelectTool.Chars.Length; i++)
                {
                    DebugSelectTool(
                    _DebugSelectTool.Chars[i].UserID,
                    _DebugSelectTool.Chars[i].CharName,
                    _DebugSelectTool.DB_Column.ToString(),
                    _DebugSelectTool.Path,
                    _DebugSelectTool.Key);
                }
            }
        }
        if (_DebugSelectTool.CopyToFix)
        {
            _DebugSelectTool.CopyToFix = false;
            var fix = _DebugFixTool;
            var sel = _DebugSelectTool;
            if (sel.Chars != null)
            {
                fix.Chars = new CharacterFix[sel.Chars.Length];
                for (int i = 0; i < sel.Chars.Length; i++)
                {
                    fix.Chars[i] = new CharacterFix();
                    fix.Chars[i].UserID = sel.Chars[i].UserID;
                    fix.Chars[i].CharName = sel.Chars[i].CharName;
                }
            }
            else
                fix.Chars = new CharacterFix[0];
            fix.DB_Column = sel.DB_Column;
            fix.Path = sel.Path;
            fix.Key = sel.Key;

            if (_DebugSelectTool.CopySettings == DebugSelectTool_CopySettings.CopyResultAll)
            {
                if (sel.Result != null)
                {
                    fix.Value = new string[sel.Result.Length];
                    sel.Result.CopyTo(fix.Value, 0);
                }
            }
            else if (_DebugSelectTool.CopySettings == DebugSelectTool_CopySettings.OnlyFirst)
            {
                if (sel.Result != null && sel.Result.Length > 0)
                {
                    fix.Value = new string[1] { sel.Result[0] };
                }
            }
            else if (_DebugSelectTool.CopySettings == DebugSelectTool_CopySettings.CopyResultWithAction)
            {
                if (sel.Result != null && sel.Chars != null && sel.Result.Length == sel.Chars.Length)
                {
                    fix.Value = new string[sel.Result.Length];
                    JToken[] result = new JToken[sel.Result.Length];
                    JToken[] value = new JToken[sel.Result.Length];
                    for (int i = 0; i < sel.Result.Length; i++)
                    {
                        bool error = false;
                        if (string.IsNullOrEmpty(sel.Result[i]))
                        {
                            error = true;
                        }
                        else
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings();
                            settings.Error = (o, e) => { e.ErrorContext.Handled = true; };
                            result[i] = JsonConvert.DeserializeObject<JToken>(sel.Result[i], settings);
                        }
                        if (error)
                            result[i] = new JValue((object)null);
                        value[i] = result[i].DeepClone();
                    }
                    for (int i = 0; i < sel.Result.Length; i++)
                    {
                        sel.CopyAction.Invoke(i, result, value);
                    }
                    for (int i = 0; i < sel.Result.Length; i++)
                    {
                        if (value[i] != null)
                            fix.Value[i] = value[i].ToString(Formatting.None);
                        else if (result[i] != null)
                            fix.Value[i] = result[i].ToString(Formatting.None);
                        else
                            fix.Value[i] = "null";
                    }
                }
            }
        }
        if (_DebugSelectTool.LoadListFromJson)
        {
            _DebugSelectTool.LoadListFromJson = false;
            var sel = _DebugSelectTool;

            if (_DebugSelectTool.JsonFile != null)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Error = (o, e) => { e.ErrorContext.Handled = true; };
                JObject json = JsonConvert.DeserializeObject<JToken>(_DebugSelectTool.JsonFile.text, settings) as JObject;
                if (json != null)
                {
                    JArray chars = json["Chars"] as JArray;
                    if (chars != null)
                    {
                        sel.Chars = new CharacterSelect[chars.Count];
                        for (int i = 0; i < chars.Count; i++)
                        {
                            sel.Chars[i] = new CharacterSelect();
                            chars[i].TryGetValue("UserID", out sel.Chars[i].UserID);
                            chars[i].TryGetValue("CharName", out sel.Chars[i].CharName);
                        }
                    }
                    json.TryGetEnum("DB_Column", out sel.DB_Column);
                    json.TryGetValue("Path", out sel.Path);
                    json.TryGetValue("Key", out sel.Key);
                }
            }
        }

    }
}
#endif
