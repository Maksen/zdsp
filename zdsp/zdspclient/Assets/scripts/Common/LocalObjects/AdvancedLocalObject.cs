using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.DebugTools;
using System;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Zealot.DebugTools
{
    public static class DebugTool
    {
        public static void Print(object value)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(value);
#else
            Console.WriteLine(value);
#endif
        }

        public static void PrintFormat(string format ,params object[] param)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogFormat(format, param);
#else
            Console.WriteLine(format, param);
#endif
        }

        public static void Error(object value)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(value);
#else
            Console.WriteLine("[Error]: "+value);
#endif
        }

        public static void ErrorFormat(string format, params object[] param)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogErrorFormat(format, param);
#else
            Console.WriteLine(format, param);
#endif
        }
    }
}

namespace Zealot.Common.Datablock
{
    public static class AdvancedLocalObjectDataExtensions
    {
        public static T GetValue<T>(this JToken node, string name)
        {
            return node[name].Value<T>();
        }

        #region avoid boxing, so write alot
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, int value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, short value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, long value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, ulong value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, string value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, float value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, double value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, char value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, DateTime value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, DateTimeOffset value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, TimeSpan value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, bool value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, decimal value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        public static void SetObjectValue(this JObject obj, AdvancedLocalObject localObj, string path, string name, object value)
        {
            obj[name] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path , name, value);
        }
        #endregion

        #region avoid boxing, so write alot

        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, int value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, short value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, long value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, ulong value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, string value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, float value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, double value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, char value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, DateTime value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, DateTimeOffset value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, TimeSpan value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, bool value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, decimal value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        public static void SetArrayValue(this JArray array, AdvancedLocalObject localObj, string path, int index, object value)
        {
            array[index] = new JValue(value);
            if (localObj != null)
                localObj.PatchAddValue(path ,index.ToString(), value);
        }
        #endregion
    }

    public class JsonBuildUtilities
    {
        private StringBuilder m_Builder;
        public JsonBuildUtilities(int capacity)
        {
            m_Builder = new StringBuilder(capacity);
        }

        public string BuildJsonStringSpecificed(JObject obj, string[][] specificedKeyGroups)
        {
            bool first = true;
            m_Builder.Length = 0;
            m_Builder.Append('{');

            foreach (var group in specificedKeyGroups)
            {
                foreach (var key in group)
                {
                    JToken ch;
                    if (!obj.TryGetValue(key, out ch))
                        continue;

                    if (first)
                        first = false;
                    else
                        m_Builder.Append(',');

                    m_Builder.Append('\"');
                    m_Builder.Append(key);
                    m_Builder.Append("\":");
                    m_Builder.Append(ch.ToString(Formatting.None));
                }
            }
            m_Builder.Append('}');

            return m_Builder.ToString();
        }
    }

    /// <summary>
    /// AdvancedLocalObject使用的資料
    /// 請創建一個class並繼承此類別，該class的Root代表整塊json
    /// </summary>
    public abstract class AdvancedLocalObjectData
    {
        protected AdvancedLocalObject m_Obj;
        protected JToken m_Root;
        protected string[] m_Records;
        protected string[] m_States;
        protected string[] m_ServerRecords;

        private JsonBuildUtilities builder=new JsonBuildUtilities(256);

        public string BuildRecordsString()
        {
            if (m_Root.Type == JTokenType.Object)
                return builder.BuildJsonStringSpecificed(m_Root as JObject, new string[][] { m_Records, m_ServerRecords });
            else
                return m_Root.ToString(Formatting.None);
        }

        public void PatchToClient()
        {
            if (m_Obj == null)
                return;

            if(m_Obj.Root.Type != JTokenType.Object)
            {
                m_Obj.PatchPath(string.Empty);
                return;
            }
            if (m_Records != null)
                foreach (var item in m_Records)
                    m_Obj.PatchKey(item);

            if (m_States != null)
                foreach (var item in m_States)
                    m_Obj.PatchKey(item);
        }

        protected AdvancedLocalObjectData(AdvancedLocalObject obj)
        {
            AffectEntities = new List<AdvancedLocalObjectData>(2);
            m_Obj = obj;
            m_Root = obj.Root;
            OnSetDataUsage();
        }
        protected AdvancedLocalObjectData(AdvancedLocalObject obj, JToken root)
        {
            AffectEntities = new List<AdvancedLocalObjectData>(2);
            m_Obj = obj;
            m_Root = root;
            OnSetDataUsage();
        }

        protected abstract void OnSetDataUsage();

        public JToken Root { get { return m_Root; } }
        public AdvancedLocalObject Object { get { return m_Obj; } }

        /// <summary>
        /// 會一併更新root的AdvancedLocalObjectData
        /// </summary>
        public List<AdvancedLocalObjectData> AffectEntities { get; private set; }

        public event Action OnUpdateRootValue;

        public void UpdateRootValue(JToken root=null)
        {
            if (root != null)
                m_Root = root;
            OnUpdateRootValue();
        }

        public virtual void OnUpdateNewRoot(JToken newRoot)
        {
            m_Root = newRoot;
            OnUpdateRootValue();
            foreach (var item in AffectEntities)
                item.OnUpdateNewRoot(newRoot);
        }

        public static JArray NewArray()
        {
            return new JArray();
        }

        public static JObject NewObject()
        {
            return new JObject();
        }

        /// <summary>
        /// 取得路徑上的值
        /// </summary>
        public T GetPathValue<T>(string path)
        {
            return m_Root.SelectToken(path).Value<T>();
        }


        /// <summary>
        /// 設定路徑上的值. 必須要server與client 都有預設此路徑，要不然無法設定給client. 注意路徑無法為空值
        /// </summary>
        public void SetPathValue(string path, object value)
        {
            if (!string.IsNullOrEmpty(path))
            {
                JValue node = m_Root.SelectToken(path) as JValue;
                if (node != null)
                {
                    node.Value = value;
                    if (m_Obj != null)
                        m_Obj.PatchUpdateValue(path, value);
                }
                else
                {
                    DebugTool.Error("[AdvancedLocalObjectDataExtensions] method:'SetPathValue' reason:'node of path is not JValue'");
                }
            }
            else
                DebugTool.Error("[AdvancedLocalObjectDataExtensions] method:'SetPathValue' reason:'path can't be empty.'");
        }

        /// <summary>
        /// 設定路徑上的值. 但不更新給client
        /// </summary>
        public void SetPathValueNoPatch(string path, object value)
        {
            if (!string.IsNullOrEmpty(path))
            {
                JValue node = m_Root.SelectToken(path) as JValue;
                if (node != null)
                    node.Value = value;
                else
                {
                    DebugTool.Error("[AdvancedLocalObjectDataExtensions] method:'SetPathValueNoPatch' reason:'node of path is not JValue'");
                }
            }
            else
                DebugTool.Error("[AdvancedLocalObjectDataExtensions] method:'SetPathValueNoPatch' reason:'path can't be empty.'");
        }

        public T GetValue<T>(string name)
        {
            return m_Root[name].Value<T>();
        }
        #region Hide
        //int
        //short
        //long
        //ulong
        //string
        //float
        //double
        //char
        //DateTime
        //DateTimeOffset
        //TimeSpan
        //bool
        //decimal
        //object
        #endregion

        #region avoid boxing, so write alot
        public void SetValueNoPatch(string name, int value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, short value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, long value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, ulong value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, string value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, float value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, double value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, char value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, DateTime value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, DateTimeOffset value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, TimeSpan value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, bool value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, decimal value)
        {
            m_Root[name] = new JValue(value);
        }
        public void SetValueNoPatch(string name, object value)
        {
            m_Root[name] = new JValue(value);
        }
        #endregion

        #region avoid boxing, so write alot
        public void SetValue(string name, int value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, short value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, long value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, ulong value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, string value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, float value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, double value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }

        public void SetValue(string name, char value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }

        public void SetValue(string name, DateTime value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }

        public void SetValue(string name, DateTimeOffset value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, TimeSpan value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, bool value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, decimal value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        public void SetValue(string name, object value)
        {
            m_Root[name] = new JValue(value);
            if (m_Obj != null)
                m_Obj.PatchAddValue(string.Empty, name, value);
        }
        #endregion



        /// <summary>
        /// 抓取物件變數的時候如果沒有該屬性則產生一個新的
        /// </summary>
        /// <typeparam name="T">抓取屬性的類型</typeparam>
        /// <param name="obj">被抓取的對象</param>
        /// <param name="name"></param>
        /// <param name="createFunc">產生新的變數的func</param>
        /// <returns></returns>
        public static T NewIfNotExist<T>(JToken obj, string name,Func<T> createFunc)
            where T:JToken
        {
            T item = obj[name] as T;
            if (item == null)
            {
                item = createFunc();
                obj[name] = item;
            }
            return item;
        }


        public static void PathCapture_ElementOfArray(string path, Action<string, int> array_update, Action<string> other_update)
        {
            int l = path.LastIndexOf('[');
            int r = path.LastIndexOf(']');
            if (r == path.Length - 1 && l >= 0 && l < r)
            {
                int index;
                if (int.TryParse(path.Substring(l + 1, r - l - 1), out index) && index >= 0)
                {
                    string name = path.Substring(0, l);
                    if (array_update != null)
                        array_update(name, index);
                }
            }
            else if (other_update != null)
                other_update(path);
        }
    }

    /// <summary>
    /// 能容納更大容量的LocalObject，透過msg欄位來傳達指令
    /// 這邊使用的名稱  '陣列'代表JArray  '物件'代表JObject  一般數值都是使用JValue
    /// '另一個端點'代表client或者server，不過目前因為只有server能傳送資料，所以目前指的就是client客戶端
    /// </summary>
    public class AdvancedLocalObject : LocalObject
    {
        /// <summary>
        /// 資料更新指令，使用第一個char作為指令
        /// </summary>
        public enum MessageType : ushort
        {
            /// <summary>
            /// 如果路徑上的節點是陣列或者物件，新增資料至路徑上的節點，如果已經有該索引值，則更新資料
            /// </summary>
            Add = 'a',
            /// <summary>
            /// 更新路徑上的節點
            /// </summary>
            Update = 'u',
            /// <summary>
            /// 移除路徑上的節點
            /// </summary>
            Remove = 'r',
            /// <summary>
            /// 如果路徑上的節點是陣列或者物件，陣列清空變長度0或者物件的所有子節點清空
            /// </summary>
            Clear = 'c',
            /// <summary>
            /// 根據清單內的資料移除路徑上的節點
            /// </summary>
            RemoveList = 'l',
            /// <summary>
            /// 傳送事件給另一個端點，目前只能使用在server傳送訊息給client
            /// </summary>
            Event = 'e',
        }

        public static JToken Validate<T>(JToken data)
            where T : class, new()
        {
            string json = data.ToString(Formatting.None);
            T validated = JsonConvert.DeserializeObject<T>(json) as T;
            if (validated == null)
                validated = new T();
            return JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(validated));
        }

        protected string m_Tag=string.Empty;
        protected bool m_IsServer;
        protected bool m_DebugMode = false;


        private List<MethodInfo> m_RefreshPropList = new List<MethodInfo>();
        protected Dictionary<string, NoticeRefreshNode> m_RefreshList = new Dictionary<string, NoticeRefreshNode>();

        protected virtual object OnGetRootData() { return this; }
        protected virtual Type OnGetRootDataType() { return this.GetType(); }

        private void GetRefreshList()
        {
            Type type = OnGetRootDataType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (typeof(NoticeRefreshNode).IsAssignableFrom(prop.PropertyType))
                    m_RefreshPropList.Add(prop.GetGetMethod());
            }

        }

        private void RefreshUpdatedNode(string path, JToken newNode)
        {
            if (m_RefreshPropList.Count > 0)
            {
                List<MethodInfo> toSave = new List<MethodInfo>(m_RefreshPropList.Count);
                for (int i = 0; i < m_RefreshPropList.Count; i++)
                {
                    NoticeRefreshNode node = (NoticeRefreshNode)m_RefreshPropList[i].Invoke(OnGetRootData(), null);
                    if (node != null)
                    {
                        if(m_DebugMode)
                            DebugTool.PrintFormat("[AdvancedLocalObject] method:'RefreshUpdatedNode' section:'m_RefreshList.Add' node.Path:'{0}'", node.Path);
                        m_RefreshList.Add(node.Path, node);
                    }
                    else
                        toSave.Add(m_RefreshPropList[i]);
                }
                m_RefreshPropList.Clear();
                m_RefreshPropList.AddRange(toSave);
            }
            {
                NoticeRefreshNode node;
                if (m_RefreshList.TryGetValue(path, out node))
                {
                    if (m_DebugMode)
                        DebugTool.PrintFormat("[AdvancedLocalObject] method:'RefreshUpdatedNode' section:'OnRefresh' node.Path:'{0}'", node.Path);
                    node.OnRefresh(newNode);
                }
            }
        }


        public static bool Parse(string time, out DateTime t, string format= DateTimeFormat_Day)
        {
            return DateTime.TryParseExact(time, format, null, System.Globalization.DateTimeStyles.None, out t);
        }
        public static bool Parse(string time, out DateTimeOffset t, string format= DateTimeFormat_Day)
        {
            return DateTimeOffset.TryParseExact(time, format, null, System.Globalization.DateTimeStyles.None, out t);
        }
        //目前Unity3d C#版本不支援
        //public static bool Parse(string time, out TimeSpan t, string format= TimeSpanFormat_Second)
        //{
        //    return TimeSpan.TryParseExact(time, format, null, System.Globalization.TimeSpanStyles.None, out t);
        //}

        /// <summary>
        /// 啟用自訂時間格式，注意啟用時有boxing的負擔
        /// </summary>
        [NotSynced]
        public bool UseCustomTimeFormat { get; set; }


        public const string DateTimeFormat_Second = "yyyy/MM/dd HH:mm:ss";
        public const string DateTimeFormat_Minute = "yyyy/MM/dd HH:mm";
        public const string DateTimeFormat_Hours = "yyyy/MM/dd HH";
        public const string DateTimeFormat_Day = "yyyy/MM/dd";
        public const string DateTimeFormat_Default = DateTimeFormat_Day;

        /// <summary>
        /// 自訂時間格式，要使用時請UseCustomTimeFormat=true，可使用預設格式DateTimeFormat_*，詳細請參考微軟官網
        /// </summary>
        [NotSynced]
        public string DateTimeFormat { get; set; }


        //目前Unity3d C#版本不支援
        //public const string TimeSpanFormat_Millisecond = @"d\ hh\:mm\:ss\.fff";
        //public const string TimeSpanFormat_Second = @"d\ hh\:mm\:ss";
        //public const string TimeSpanFormat_Minute = @"d\ hh\:mm";
        //public const string TimeSpanFormat_Hours = @"d\ hh";
        //public const string TimeSpanFormat_Day = "dd";
        //public const string TimeSpanFormat_Default = TimeSpanFormat_Second;

        ///// <summary>
        ///// 自訂TimeSpan格式，要使用時請UseCustomTimeFormat=true，可使用預設格式TimeSpanFormat_*，詳細請參考微軟官網
        ///// </summary>
        //[NotSynced]
        //public string TimeSpanFormat { get; set; }


        public AdvancedLocalObject(LOTYPE type,bool isServer) : base(type)
        {

            UseCustomTimeFormat = false;
            DateTimeFormat = DateTimeFormat_Default;
            //目前Unity3d C#版本不支援
            //TimeSpanFormat = TimeSpanFormat_Default;

            this.m_IsServer = isServer;
            Root = new JObject();
            pack = new StringBuilder();
            wrapBuilder = new StringBuilder();

            GetRefreshList();
        }

        public static int[] List_S2I(string[] indices)
        {
            List<int> i_indices = new List<int>(indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                int index;
                if (int.TryParse(indices[i], out index))
                    i_indices.Add(index);
            }
            return i_indices.ToArray();
        }
        public static string[] List_I2S(int[] indices)
        {
            string[] s_indices = new string[indices.Length];
            for (int i = 0; i < s_indices.Length; i++)
                s_indices[i] = indices[i].ToString();
            return s_indices;
        }

        public static void RemoveArrayNodesFromList(JArray array, int[] indices)
        {
            List<JToken> toSave = new List<JToken>(array.Count);
            Array.Sort(indices);
            for (int i = 0; i < array.Count; i++)
            {
                int index = Array.BinarySearch(indices, i);
                if (index < 0)
                    toSave.Add(array[i]);
            }

            array.Clear();
            for (int i = 0; i < toSave.Count; i++)
                array.Add(toSave[i]);
        }


        public static JToken JsonFilter(JToken root, string[] except)
        {
            if (except != null && except.Length > 0 && root.Type == JTokenType.Object)
            {
                JObject obj = root.DeepClone() as JObject;
                foreach (var item in except)
                    obj.Remove(item);
                root = obj;
            }
            return root;
        }

        /// <summary>
        /// 從根結點更新整塊資料
        /// </summary>
        public void PatchFull()
        {
            PatchUpdateValue("", Root);
        }

        /// <summary>
        /// 從路徑節點更新資料
        /// 注意,此方式必須對方資料也有該節點
        /// </summary>
        public void PatchPath(string path)
        {
            PatchUpdateValue(path, Root.SelectToken(path));
        }

        /// <summary>
        /// 從第一層的子節點更新資料
        /// </summary>
        public void PatchKey(string key)
        {
            PatchAddValue(string.Empty, key, ((JObject)Root)[key]);
        }


        /// <summary>
        /// 取得節點資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑</param>
        /// <returns>路徑抓取回傳的資料</returns>
        public T GetPathNode<T>(string path)
        {
            JToken token = Root.SelectToken(path);
            if (token != null)
                return token.Value<T>();
            else
                return default(T);
        }
        /// <summary>
        /// 取得節點資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑</param>
        /// <param name="result">是否抓取成功</param>
        /// <returns></returns>
        public bool TryGetPathNode<T>(string path, out T result)
        {
            JToken token = Root.SelectToken(path);
            if (token != null)
            {
                result = token.Value<T>();
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }


        void ToStringAndType<T>(T value,out string str,out string typeName)
        {
            typeName = value.GetType().Name;
            if (value is JToken)
            {
                str= (value as JToken).ToString(Formatting.None);
                return;
            }
            if(UseCustomTimeFormat)
            {
                switch (typeName)
                {
                    default:
                        str = value.ToString();
                        break;
                    case "DateTime":
                        str = ((DateTime)(object)value).ToString(DateTimeFormat);
                        break;
                    case "DateTimeOffset":
                        str = ((DateTimeOffset)(object)value).ToString(DateTimeFormat);
                        break;
                    //目前Unity3d C#版本不支援
                    //case "TimeSpan":
                    //    str = ((TimeSpan)(object)value).ToString(TimeSpanFormat);
                    //    break;
                }
            }
            else
                str = value.ToString();
        }

        /// <summary>
        /// 移除清單
        /// </summary>
        /// <param name="path">路徑</param>
        /// <param name="patch">是否更新至另一個端點</param>
        public void PathRemoveList(string path, string[] indices, bool patch)
        {
            RemoveList(path, indices);
            if (patch)
                PatchRemoveList(path, indices);
        }
        /// <summary>
        /// 移除節點資料
        /// </summary>
        /// <param name="path">路徑</param>
        /// <param name="patch">是否更新至另一個端點</param>
        public void PathRemoveNode(string path, bool patch)
        {
            UpdatePath(path, null, null, null);
            if (patch)
                PatchRemoveValue(path);
        }
        /// <summary>
        /// 更新已存在的節點資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑</param>
        /// <param name="value">新的資料</param>
        /// <param name="patch">是否更新至另一個端點</param>
        public void PathUpdateNode<T>(string path, T value, bool patch)
        {
            if (value != null)
            {
                string str, type; ToStringAndType(value, out str, out type);
                UpdatePath(path, str, type, null);
                if (patch)
                    PatchUpdateValue(path, value);
            }
        }

        /// <summary>
        /// 新增節點資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑，必須為陣列或物件</param>
        /// <param name="key">陣列的索引或者物件的屬性名稱，如果要新增在陣列最後一個，可以指定空字串給他(但請不要指定null)</param>
        /// <param name="value">新的資料</param>
        /// <param name="patch">是否更新至另一個端點</param>
        public void PathAddNode<T>(string path, string key, T value, bool patch)
        {
            if (value != null && !string.IsNullOrEmpty(key))
            {
                string str, type; ToStringAndType(value, out str, out type);
                UpdatePath(path, str, type, key);
                if (patch)
                    PatchAddValue(path, key, value);
            }
        }

        /// <summary>
        /// 清除節點資料
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑，必須為陣列或物件</param>
        /// <param name="patch">是否更新至另一個端點</param>
        public void PathClearNode<T>(string path, bool patch)
        {
            ClearPath(path);
            if (patch)
                PatchClearValue(path);
        }

        /// <summary>
        /// 傳送新增資料指令至另一個端點
        /// </summary>
        /// <typeparam name="T">資料型別</typeparam>
        /// <param name="path">路徑，必須為陣列或物件</param>
        /// <param name="key">陣列的索引或者物件的屬性名稱，如果要新增在陣列最後一個，可以指定空字串給他(但請不要指定null)</param>
        /// <param name="value">新的資料</param>
        public void PatchAddValue<T>(string path, string key, T value)
        {
            if (!m_IsServer)
                return;
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }
            string str, type; ToStringAndType(value, out str, out type);
            pack.AppendLine("a|" + WrapValue(path) + "|" + str + "|" + type + "|" + WrapValue(key));
        }
        /// <summary>
        /// 傳送更新資料指令至另一個端點
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public void PatchUpdateValue<T>(string path, T value)
        {
            if (!m_IsServer)
                return;
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }
            string str, type; ToStringAndType(value, out str, out type);
            pack.AppendLine("u|" + WrapValue(path) + "|" + str + "|" + type);
        }
        /// <summary>
        /// 傳送移除資料指令至另一個端點
        /// </summary>
        /// <param name="path"></param>
        public void PatchRemoveValue(string path)
        {
            if (!m_IsServer)
                return;
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }
            pack.AppendLine("r|" + WrapValue(path));
        }

        /// <summary>
        /// 傳送移除清單資料指令至另一個端點
        /// </summary>
        /// <param name="path"></param>
        public void PatchRemoveList(string path,string[] properties)
        {
            if (!m_IsServer)
                return;
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }

            StringBuilder sb = new StringBuilder(128);
            for (int i = 0; i < properties.Length; i++)
            {
                if (i > 0)
                    sb.Append("," + properties[i]);
                else
                    sb.Append(properties[0]);
            }

            pack.AppendLine("l|" + WrapValue(path) + "|" + WrapValue(sb.ToString()));
        }

        /// <summary>
        /// 傳送清除資料指令至另一個端點
        /// </summary>
        /// <param name="path"></param>
        public void PatchClearValue(string path)
        {
            if (!m_IsServer)
                return;
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }
            pack.AppendLine("c|" + WrapValue(path));
        }

        /// <summary>
        /// 傳遞事件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="eventName"></param>
        /// <param name="param"></param>
        public void PatchEvent(string path,string eventName, string param)
        {
            if (!bMyDirty)
            {
                pack.Length = 0;
                bMyDirty = true;
                SetDirty();
            }
            pack.AppendLine("e|" + WrapValue(path) + "|" + WrapValue(eventName) + "|" + WrapValue(param));
        }

        private void ClearPath(string path)
        {
            JToken token = Root.SelectToken(path);
            JArray a = token as JArray;
            if (a != null)
            {
                a.Clear();
                return;
            }
            JObject o = token as JObject;
            if(o!=null)
            {
                o.RemoveAll();
            }
            
        }

        private void RemoveList(string path, string[] indices)
        {
            JToken token = Root.SelectToken(path);
            if (token != null)
            {
                if (token.Type == JTokenType.Object)
                {
                    JObject o = (token as JObject);
                    for (int i = 0; i < indices.Length; i++)
                        o.Remove(indices[i]);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray arr = (token as JArray);
                    RemoveArrayNodesFromList(arr, List_S2I(indices));
                }
            }
        }

        private void UpdatePath(string path, string value, string type, string key)
        {
            JToken token = Root.SelectToken(path);
            if (token != null)
            {
                //Remove
                if (type==null)
                {
                    var parent = token.Parent;
                    if (parent != null)
                    {
                        if (parent.Type == JTokenType.Array)
                            token.Remove();
                        else if (parent.Type == JTokenType.Property)
                            parent.Remove();//remove must from JProperty
                        else if (parent.Type == JTokenType.Object)
                            token.Remove();
                        else
                            DebugTool.ErrorFormat("[AdvancedLocalObject] msg:'Remove failed' path:'{0}' reason:'parent type is {1}'",path, parent.Type);
                    }
                    else
                        DebugTool.ErrorFormat("[AdvancedLocalObject] msg:'Remove failed' path:'{0}' reason:'parent is null'", path);
                    return;
                }

                JToken node = null;
                //deserialize
                switch (type)
                {
                    case "String":
                        {
                            node = new JValue(value);
                        }
                        break;
                    case "Char":
                        {
                            char v='?';
                            if(value.Length>0)
                                v = value[0];
                            node = new JValue(v);
                        }
                        break;
                    case "Boolean":
                        {
                            Boolean v; Boolean.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "Int32":
                        {
                            Int32 v; Int32.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "Single":
                        {
                            Single v; Single.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "Int64":
                        {
                            Int64 v; Int64.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "UInt64":
                        {
                            UInt64 v; UInt64.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "Double":
                        {
                            Double v; Double.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;

                    case "JObject":
                    case "JValue":
                    case "JArray":
                        {
                            JToken v = JsonConvert.DeserializeObject<JToken>(value);
                            node = v;
                        }
                        break;


                    case "DateTime":
                        {
                            DateTime v;
                            if(UseCustomTimeFormat)
                                DateTime.TryParseExact(value, DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out v);
                            else
                                DateTime.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;
                    case "DateTimeOffset":
                        {
                            DateTimeOffset v;
                            if (UseCustomTimeFormat)
                                DateTimeOffset.TryParseExact(value, DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out v);
                            else
                                DateTimeOffset.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;

                    case "TimeSpan":
                        {
                            TimeSpan v;
                            //目前Unity3d C#版本不支援
                            //if (UseCustomTimeFormat)
                            //    TimeSpan.TryParseExact(value, TimeSpanFormat, null, System.Globalization.TimeSpanStyles.None, out v);
                            //else
                                TimeSpan.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;

                    case "Decimal":
                        {
                            Decimal v; Decimal.TryParse(value, out v);
                            node = new JValue(v);
                        }
                        break;

                }

                if (node != null)
                {
                    //replace
                    if (key==null)
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            Root = node;
                            RefreshUpdatedNode(string.Empty, node);
                        }
                        else
                        {
                            token.Replace(node);
                            RefreshUpdatedNode(path, Root.SelectToken(path));
                        }
                    }
                    //object[key]=value
                    else if (token.Type == JTokenType.Object)
                    {
                        (token as JObject)[key] = node;
                    }
                    //array[index]=value
                    else if (token.Type == JTokenType.Array)
                    {
                        JArray arr = token as JArray;
                        int index;
                        if (int.TryParse(key, out index))
                        {
                            if (index >= 0 && index < arr.Count)
                                arr[key] = node;
                            else
                                arr.Add(node);
                        }
                        else
                            arr.Add(node);
                    }

                }
            }

        }

        /// <summary>
        /// 根結點資料，基本用法為跟AdvancedLocalObjectData的Root reference相同值
        /// </summary>
        [NotSynced]
        public JToken Root { get; set; }

        StringBuilder pack;
        StringBuilder wrapBuilder;

        readonly static char[] sp = new char[] { '|' };
        readonly static char[] sp2 = new char[] { ',' };
        void unpack(string msgs)
        {
            int index = 0;

            int i = 0;
            for (; ; )
            {
                int nextIndex = msgs.IndexOf('\n', index);
                if (nextIndex >= 0)
                {
                    string item = msgs.Substring(index, nextIndex - index);

                    string[] items = item.Split(sp);

                    MessageType op = (MessageType)items[0][0];
                    string path = UnwrapValue(items[1]);
                    switch (op)
                    {
                        case  MessageType.Add:
                            string key = UnwrapValue(items[4]);
                            BeforePacth(i, op, path, key);
                            UpdatePath(path, UnwrapValue(items[2]), UnwrapValue(items[3]), key);
                            AfterPacth(i, op, path, key);
                            break;
                        case MessageType.Update:
                            BeforePacth(i, op, path,null);
                            UpdatePath(path, UnwrapValue(items[2]), UnwrapValue(items[3]), null);
                            AfterPacth(i, op, path, null);
                            break;
                        case MessageType.Remove:
                            BeforePacth(i, op, path,null);
                            UpdatePath(path, null, null, null);
                            AfterPacth(i, op, path, null);
                            break;
                        case MessageType.Clear:
                            BeforePacth(i, op, path,null);
                            ClearPath(path);
                            AfterPacth(i, op, path, null);
                            break;
                        case MessageType.RemoveList:
                            string[] indices = UnwrapValue(items[2]).Split(sp2, StringSplitOptions.RemoveEmptyEntries);
                            BeforePacth(i, op, path, indices);
                            RemoveList(path, indices);
                            AfterPacth(i, op, path, indices);
                            break;
                        case MessageType.Event:
                            OnEvent(i, path, UnwrapValue(items[2]), UnwrapValue(items[3]));
                            break;
                    }




                }
                else
                    break;
                index = nextIndex + 1;

                i++;
            }
        }
        /// <summary>
        /// 過濾特殊字元，無特殊用途請不要使用此method
        /// </summary>
        public string WrapValue(string value)
        {
            wrapBuilder.Length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\\':
                        wrapBuilder.Append("\\0");
                        break;
                    case '|':
                        wrapBuilder.Append("\\1");
                        break;
                    case '\r':
                        wrapBuilder.Append("\\2");
                        break;
                    case '\n':
                        wrapBuilder.Append("\\3");
                        break;
                    default:
                        wrapBuilder.Append(c);
                        break;
                }
            }
            return wrapBuilder.ToString();
        }
        /// <summary>
        /// 從過濾特殊字元後的資料解包，無特殊用途請不要使用此method
        /// </summary>
        public string UnwrapValue(string value)
        {
            wrapBuilder.Length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                        break;
                    case '\\':
                        if (value.Length > i + 1)
                        {
                            i++;
                            switch (value[i])
                            {
                                case '0':
                                    wrapBuilder.Append('\\');
                                    break;
                                case '1':
                                    wrapBuilder.Append('|');
                                    break;
                                case '2':
                                    wrapBuilder.Append('\r');
                                    break;
                                case '3':
                                    wrapBuilder.Append('\n');
                                    break;
                                default:
                                    wrapBuilder.Append('\\');
                                    wrapBuilder.Append(value[i]);
                                    break;
                            }
                        }
                        else
                            wrapBuilder.Append('\\');
                        break;
                    default:
                        wrapBuilder.Append(c);
                        break;
                }
            }
            return wrapBuilder.ToString();
        }

        /// <summary>
        /// 發生在從另一個端點接收資料資料更新之前
        /// 請override此method來設定資料
        /// </summary>
        /// <param name="cmdIndex">第幾個更新指令</param>
        /// <param name="op">指令的類型</param>
        /// <param name="path">資料的路徑</param>
        public virtual void BeforePacth(int cmdIndex, MessageType op, string path,object key)
        {

        }
        /// <summary>
        /// 發生在從另一個端點接收資料資料更新之後
        /// 請override此method來設定資料
        /// </summary>
        /// <param name="cmdIndex">第幾個更新指令</param>
        /// <param name="op">指令的類型</param>
        /// <param name="path">資料的路徑</param>
        public virtual void AfterPacth(int cmdIndex, MessageType op, string path,object key)
        {

        }

        /// <summary>
        /// 從另一個端點發送事件訊息時取得的資料
        /// </summary>
        /// <param name="index">第幾個更新指令</param>
        /// <param name="path">資料的路徑</param>
        /// <param name="param">事件參數</param>
        public virtual void OnEvent(int index, string path,string eventName, string param)
        {
        }

        public string msg
        {
            get { return pack.ToString(); }
            set
            {
                if(m_DebugMode)
                    DebugTool.Print("["+ m_Tag + "] msg: "+value);
                unpack(value);
                if (m_DebugMode)
                    DebugTool.Print("[" + m_Tag + "] Updated:\n" + Root.ToString());
            }
        }

        protected override void OnSerializeStream()
        {
            if (bMyDirty)
                OnSetAttribute("msg", pack.ToString());
        }
        protected override void OnReset()
        {
            if (bMyDirty)
            {
                bMyDirty = false;
                pack.Length = 0;
            }
        }

        bool bMyDirty = false;

    }

    public interface NoticeRefreshNode
    {
        string Path { get; }
        void OnRefresh(JToken newNode);
    }

    public abstract class JNode: NoticeRefreshNode
    {
        protected AdvancedLocalObject save_obj;
        protected AdvancedLocalObject obj;
        public AdvancedLocalObject LocalObject { get { return obj; } }

        protected string path;
        public virtual string Path { get { return path; } }
        public abstract void OnRefresh(JToken newNode);

        public void StopPatch() { obj = null; }
        public void ResumePatch() { obj = save_obj; }
        public void PatchFull()
        {
            if (obj != null)
                obj.PatchPath(Path);
        }
    }

    public abstract class JDictionaryNode<T> : JNode, IDictionary<string, T>
    {
        protected JObject dic;
        public override void OnRefresh(JToken newNode) { dic = newNode as JObject; }

        protected abstract JToken GetToken(T node);
        protected abstract T GetNode(JToken token);

        public JDictionaryNode(JObject dic, string path, AdvancedLocalObject obj)
        {
            this.dic = dic;
            this.path = path;
            this.obj = obj;
            this.save_obj = obj;
        }
       

        public T this[string key]
        {
            get { return GetNode(dic[key]); }
            set {
                var token = GetToken(value);
                dic[key] = token;
                if (obj != null)
                    obj.PatchAddValue(path, key, token);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                string[] keys = new string[dic.Count];
                int i = 0;
                foreach (var item in dic)
                    keys[i++] = item.Key;
                return keys;
            }
        }

        public ICollection<T> Values
        {
            get
            {
                T[] values = new T[dic.Count];
                int i = 0;
                foreach (var item in dic)
                    values[i++] = GetNode( item.Value);
                return values;
            }
        }

        public int Count { get { return dic.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(string key, T value)
        {
            var token = GetToken(value);
            dic.Add(key, token);
            if (obj != null)
                obj.PatchAddValue(path, key, token);
        }

        public void Add(KeyValuePair<string, T> item)
        {
            var token = GetToken(item.Value);
            dic.Add(item.Key, token);
            if (obj != null)
                obj.PatchAddValue(path, item.Key, token);
        }

        public void Clear()
        {
            dic.RemoveAll();
            if (obj != null)
                obj.PatchClearValue(path);
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            JToken token;
            if(dic.TryGetValue(item.Key, out token))
            {
                if (GetToken(item.Value) == token)
                    return true;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            JToken token;
            return dic.TryGetValue(key, out token);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            foreach(var item in dic)
            {
                array[arrayIndex] = new KeyValuePair<string, T>(item.Key, GetNode(item.Value));
                arrayIndex++;
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            foreach (var item in dic)
                yield return new KeyValuePair<string, T>(item.Key, GetNode(item.Value));
        }

        public bool Remove(string key)
        {
            bool b= dic.Remove(key);
            if (b && obj != null)
                obj.PatchRemoveValue(path + "." + key);
            return b;
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            JToken token;
            if(dic.TryGetValue(item.Key,out token))
            {
                if (GetToken(item.Value) == token)
                {
                    dic.Remove(item.Key);

                    if (obj != null)
                        obj.PatchRemoveValue(path + "." + item.Key);

                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(string key, out T value)
        {
            JToken token;
            if (dic.TryGetValue(key, out token))
            {
                value = GetNode(token);
                return true;
            }
            value = default(T);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class JArrayNode<T>: JNode, IEnumerable<T>, NoticeRefreshNode
    {
        private JArray array;
        public override void OnRefresh(JToken newNode) { array = newNode as JArray; }

        protected abstract JToken GetToken(T node);
        protected abstract T GetNode(JToken token);

        public JArrayNode(JArray array, string path, AdvancedLocalObject obj)
        {
            this.array = array;
            this.path = path;
            this.obj = obj;
            this.save_obj = obj;
        }

        public void Clear()
        {
            array.Clear();
            if (obj != null)
                obj.PatchClearValue(path);
        }

        public void Add(T node)
        {
            var token = GetToken(node);
            array.Add(token);
            if (obj != null)
                obj.PatchAddValue(path, string.Empty, token);
        }

        public void RemoveAt(int index)
        {
            array.RemoveAt(index);
            if (obj != null)
                obj.PatchRemoveValue(path + "[" + index + "]");
        }

        public void RemoveList(int[] indices)
        {
            AdvancedLocalObject.RemoveArrayNodesFromList(array,indices);

            if (obj != null)
                obj.PatchRemoveList(path,AdvancedLocalObject.List_I2S(indices));
        }

        public int Count { get { return array.Count; } }

        public T this[int index]
        {
            get { return GetNode(array[index]); }
            set {
                JToken token = GetToken(value);
                array[index] = token;
                if (obj != null)
                    obj.PatchUpdateValue(path + "[" + index + "]", token);
            }
        }

        public T GetFirst()
        {
            return GetNode(array[0]);
        }
        public T GetTail()
        {
            return GetNode(array[array.Count - 1]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            int c = array.Count;
            for (int i = 0; i < c; i++)
                yield return GetNode(array[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


}

namespace Zealot.Common
{
    public interface IStats
    {
        void LoadFromInventoryData(IInventoryData data);
        void SaveToInventoryData(IInventoryData data);
    }
    public interface IInventoryData
    {
    }

    public interface IJsonInventoryData: IInventoryData
    {
        void LoadDataFromJsonString(string data);
        string SaveDataToJsonString();
    }
}