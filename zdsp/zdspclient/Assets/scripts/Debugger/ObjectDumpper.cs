#if UNITY_EDITOR
using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDumpper : MonoBehaviour {

    [Header("Execute")]
    public bool TrueToDump;
    public bool DumpOnce=true;
    [Header("Options")]
    [Tooltip("顯示的目標Unity根物件")]
    public UnityEngine.Object DumpTarget;
    [Tooltip("自動使用GetComponent物件的Type")]
    public string AutoGetComponentType;
    [Tooltip("顯示的目標的路徑")]
    public string DumpPath;

    [Tooltip("路徑最後的抓取值需要轉換的Type")]
    public string CastType;

    public bool IsDontDestroyOnLoad = false;


    [Tooltip("包含or排除的清單")]
    public string[] NameFilter;
    [Tooltip("包含or排除")]
    public bool NameFilterInclude = false;
    [Tooltip("名稱以值開始")]
    public string[] StartWith;
    [Tooltip("是否列出父class的成員")]
    public bool FlattenHierarchy;
    [Tooltip("是否列出private的成員")]
    public bool Private;
    [Tooltip("是否只顯示成員名稱而不執行取值")]
    public bool DumpNameOnly = true;
    [Tooltip("特殊字元排除")]
    public string IgnoreSpecialChar;
    public bool DumpMethods;
    public bool DumpFields = true;
    public bool DumpProperties;

    [Header("Outputs")]
    [TextArea(3, 100)]
    public string DumpResults;

    private void Awake()
    {
        if (IsDontDestroyOnLoad)
            DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        DoGetCom();
        if (TrueToDump)
        {
            if (DumpOnce)
                TrueToDump = false;
            DoDump();
        }
    }

    static Type GetTypeByName(string name)
    {
        Type type = null;
        if (!string.IsNullOrEmpty(name))
        {
            
            try
            {
                type = Type.GetType(name);
            }
            catch (Exception)
            {
            }
            if (type == null)
            {
                try
                {
                    type = Type.GetType(name + ", " + typeof(ObjectDumpper).Assembly.FullName);
                }
                catch (Exception)
                {
                }
            }
        }
        return type;
    }

    void DoGetCom()
    {
        if (DumpTarget == null)
        {
            Type type = GetTypeByName(AutoGetComponentType);
            if (type != null)
                DumpTarget = GetComponent(type);
        }
    }

    void DoDump()
    {
        StringBuilder sb = new StringBuilder();
        object o = GetMemberByPath(DumpTarget, DumpPath);
        if (o != null)
        {

            HashSet<string> tb = new HashSet<string>(); ;
            if (NameFilter != null)
                foreach (var item in NameFilter)
                    tb.Add(item);
            if (StartWith == null)
                StartWith = new string[0];

            try
            {
                Type type = o.GetType();

                Type t_type = GetTypeByName(CastType);
                if (t_type != null)
                {
                    type = t_type;
                    o = Convert.ChangeType(o,t_type);
                }

                if (o == null)
                {
                    DumpResults = string.Empty;
                    return;
                }

                sb.AppendLine("TargetValue: " + o.ToString());

                var flags =
                    (Private ? BindingFlags.Public | BindingFlags.NonPublic : BindingFlags.Public ) |
                    BindingFlags.Instance |
                    (FlattenHierarchy ? BindingFlags.FlattenHierarchy : BindingFlags.DeclaredOnly);

                if (DumpMethods)
                {
                    sb.AppendLine("Methods:");
                    foreach (var prop in type.GetMethods(flags))
                    {
                        if (Filter(prop.Name, NameFilterInclude, tb) &&
                            CheckSpecialChar(prop.Name, IgnoreSpecialChar) &&
                            CheckStartWith(prop.Name, StartWith))
                        {
                            if (!prop.IsGenericMethod &&
                                prop.GetParameters().Length == 0 &&
                                !prop.IsSpecialName)

                            {
                                if (DumpNameOnly)
                                    sb.AppendLine(prop.ToString());
                                else
                                    TryCatchGet(prop.Name, () => prop.Invoke(o, null),
                                        (name, value) => sb.AppendLine(name + ": " + value),
                                        (name, msg) => sb.AppendLine("[failed]" + name + " msg:" + msg));
                            }
                            else
                            {
                                sb.AppendLine(prop.ToString());
                            }
                        }
                    }
                }

                if (DumpProperties)
                {
                    sb.AppendLine("Properties:");
                    foreach (var prop in type.GetProperties(flags))
                    {
                        if (Filter(prop.Name, NameFilterInclude, tb) &&
                            prop.GetIndexParameters().Length == 0 &&
                            CheckSpecialChar(prop.Name, IgnoreSpecialChar) &&
                            CheckStartWith(prop.Name, StartWith))
                        {
                            if (DumpNameOnly)
                                sb.AppendLine(prop.Name + "; type:"+prop.PropertyType.FullName);
                            else
                                TryCatchGet(prop.Name, () => prop.GetValue(o, null),
                                (name, value) => sb.AppendLine(name + ": " + value),
                                (name, msg) => sb.AppendLine("[failed]" + name + " msg:" + msg));
                        }
                    }

                }
                if (DumpFields)
                {
                    sb.AppendLine("Fields:");
                    foreach (var prop in type.GetFields(flags))
                    {
                        if (Filter(prop.Name, NameFilterInclude, tb) &&
                            !prop.IsSpecialName &&
                            CheckSpecialChar(prop.Name,IgnoreSpecialChar) &&
                            CheckStartWith(prop.Name,StartWith))
                        {
                            if (DumpNameOnly)
                                sb.AppendLine(prop.Name + "; type:" + prop.FieldType.FullName);
                            else
                                TryCatchGet(prop.Name, () => prop.GetValue(o),
                                (name, value) => sb.AppendLine(name + ": " + value),
                                (name, msg) => sb.AppendLine("[failed]" +name + " msg:" + msg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
            }

            DumpResults = sb.ToString();
        }
    }

    static bool CheckSpecialChar(string name,string s)
    {
        if(!string.IsNullOrEmpty(s))
        {
            for (int i = 0; i < s.Length; i++)
                if (name.IndexOf(s[i]) >= 0)
                    return false;
        }
        return true;
    }

    static bool CheckStartWith(string name,string[] startWith)
    {
        if (startWith.Length == 0)
            return true;
        foreach (var st in startWith)
            if (name.StartsWith(st))
                return true;
        return false;
    }
    static object GetMemberByPath(object o, string path)
    {
        string[] names = path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        try
        {
            object cur = o;
            for (int i = 0; i < names.Length; i++)
            {
                if (cur == null)
                    return null;
                string name = names[i];
                Type type = cur.GetType();
                var infos = type.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                if (infos != null && infos.Length > 0)
                {
                    bool br = false;
                    for (int j = 0; j < infos.Length; j++)
                    {
                        switch (infos[j].MemberType)
                        {
                            case MemberTypes.Field:
                                FieldInfo fi = infos[j] as FieldInfo;
                                br = true;
                                cur = fi.GetValue(cur);
                                break;
                            case MemberTypes.Property:
                                PropertyInfo pi = infos[j] as PropertyInfo;
                                if (pi.GetIndexParameters().Length == 0)
                                {
                                    br = true;
                                    cur = pi.GetValue(cur, null);
                                }
                                break;
                            case MemberTypes.Method:
                                MethodInfo mi = infos[j] as MethodInfo;
                                if (!mi.IsGenericMethod && mi.GetParameters().Length == 0)
                                {
                                    br = true;
                                    cur = mi.Invoke(cur, null);
                                }
                                break;
                        }
                        if (br)
                            break;
                    }

                    if (br == false)
                        return null;
                }
                else
                    return null;

            }
            return cur;
        }
        catch (Exception)
        {
            return null;
        }
    }

    static bool Filter(string name, bool include, HashSet<string> tb)
    {
        if (include)
        {
            return tb.Contains(name);
        }
        else
        {
            return !tb.Contains(name);
        }
    }

    static void TryCatchGet(string name, Func<object> getFunc, Action<string, string> success, Action<string, string> fail)
    {
        try
        {
            object o = getFunc();
            if (o != null)
                success(name, o.ToString());
            else
                success(name, "(null)");
        }
        catch (Exception ex)
        {
            fail(name, ex.Message);
        }
    }
}
#endif