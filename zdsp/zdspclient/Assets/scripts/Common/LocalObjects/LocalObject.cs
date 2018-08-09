namespace Zealot.Common.Datablock
{
    #region USING_DIRECTIVE
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Collections;
    using System.Text;        
    //using System.Collections.ObjectModel;
    //using System.Collections.Specialized;

    #endregion

    [AttributeUsage(AttributeTargets.Property)]
    public class NotSyncedAttribute : Attribute
    {
    }

    public class LocalObjectException : Exception
    {
    }

    public class LocalObjectInfo
    {
        public Dictionary<string, byte> mMethodDef = new Dictionary<string, byte>();
        public Dictionary<byte, string> mMethodDefReverse = new Dictionary<byte, string>();

        public Dictionary<string, MethodInfo>  mMethodGetterDef = new Dictionary<string, MethodInfo>();
        public Dictionary<byte, object> mListCollection = new Dictionary<byte, object>();
    }    

    public class LocalObjProxy : RealProxy
    {
        readonly object target;

        public LocalObjProxy(object target)
		: base(target.GetType())
	    {
            this.target = target;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;
            if (methodCall != null)
            {
                return HandleMethodCall(methodCall); // <- see further
            }

            return null;
        }

        public IMessage HandleMethodCall(IMethodCallMessage methodCall)
        {
            Console.WriteLine("Calling method {0}...", methodCall.MethodName);

            try
            {
                object result = null;                
                if(methodCall.MethodName.Length >= 4)
                {
                    if (methodCall.MethodName.Substring(0, 4) == "set_")
                    {
                        ((LocalObject)target).ProxyMethod(methodCall.MethodName, methodCall.InArgs);
                    }
                }              
                
                result = methodCall.MethodBase.Invoke(target, methodCall.InArgs);                
                Console.WriteLine("Calling {0}... OK", methodCall.MethodName);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (TargetInvocationException invocationException)
            {
                var exception = invocationException.InnerException;
                Console.WriteLine("Calling {0}... {1}", methodCall.MethodName, exception.GetType());
                return new ReturnMessage(exception, methodCall);
            }
        }
    }

    public class CollectionHandler<T> : ICollection<T>
    {
        private List<T> collection;
        public Dictionary<byte, T> DirtyCol;
        private LocalObject mParent;
        private byte mTfields;
        private string mFieldName = "";
        private bool mNotifyParent = true;     

        public CollectionHandler(int collectioncount)
        {
            mParent = null;
            collection = new List<T>();            
            for (int i = 0; i < collectioncount; ++i)
            {
                T val = default(T);
                collection.Add(val);
            }
            DirtyCol = new Dictionary<byte, T>();
            //collection.CollectionChanged += HandleChange;
        }

        public void SetParent(LocalObject parent, string myfieldname, byte defaultTfields = 1)
        {
            mFieldName = myfieldname;
            mTfields = defaultTfields;
            mParent = parent;
            mParent.RegisterCollection(myfieldname, this);
        }

        public void SetNotifyParent(bool val)
        {
            mNotifyParent = val;
        }

        public byte GetTfields()
        {
            return mTfields;
        }

        // shouldn't be called, since it's already been initialized at the ctor
        public void Add(T item)
        {
            collection.Add(item);
        }

        // shouldn't be called, since it's already been initialized at the ctor
        public bool Remove(T item)
        {
            return collection.Remove(item);
        }

        // shouldn't be called, since it's already been initialized at the ctor
        public void Clear()
        {
            collection.Clear();
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collection.CopyTo(array, arrayIndex);
        }

        public int Count { get { return collection.Count; } }

        public bool IsReadOnly { get { return true; } }

        public void RemoveAt(int idx)
        {
            collection.RemoveAt(idx);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public T this[int key]
        {
            get { return collection[key]; }
            set { 
                collection[key] = value;
                if (DirtyCol.ContainsKey((byte)key) == false)
                    DirtyCol.Add((byte)key, value);
                else
                    DirtyCol[(byte)key] = value;

                if (mNotifyParent)
                    mParent.ProxyMethod("set_" + this.mFieldName, new object[1]);
            }
        }

        public void ResetAll()
        {
            for (int i = 0; i < Count; ++i)
            {
                T val = default(T);
                collection[i] = val;
            }
        }

        public Dictionary<byte, T> GetDirtyDic()
        {
            return DirtyCol;
        }

        public void Reset()
        {
            DirtyCol.Clear();
        }

        public bool IsDirty()
        {
            return (DirtyCol.Count > 0);
        }
    }

    public enum LOCATEGORY : byte
    {
        EntitySyncStats,    // All entity will sync the entitysyncstats
        LocalPlayerStats,   // Only will sync to the localplayer only 
        SharedStats         // Will sync to the player who subscribe to the stats
    }

    public enum LOTYPE : byte
    {
        NPCSynStats,
        PlayerSynStats,
        SecondaryStats,
        CharacterStats, //Character stats point

        InventoryStats, // Item Inventory (100 slots each)
        InventoryStatsEnd = InventoryStats + 3, // Item Inventory, (total 300 slots)

        EquipmentStats, // Equipment Inventory
        AvatarStats, // Avatar Inventory
        ItemHotbarStats, // Item Hotbar
        RealmPartyDamageList, // Shared object, record damage list of a realm party after realm created.
        RealmStats, // Realm info that needs to be sync
        DungeonObjectiveStats, // Shared object, shared between dungeon participant
        SkillStats, // Skills
        HeroStats,
        HeroSynStats,
        StoreTransactions,

        PartyStats, // Shared object, normal party members
        GuildStats, // Shared object, shared between guild members
        LootSynStats,
		QuestSynStats,
        DestinyClueSynStats,
        LocalCombatStats,
        LocalSkillPassiveStats,
        BuffTimeStats,
        SocialStats,
        //TutorialStats,
        LotteryShopItemsTabStats,
        WelfareStats,
        SevenDaysStats,
        QuestExtraRewardsStats,
        StoreStats,
        ExchangeShopStats,

        CompensateStats,
        TongbaoCostBuff,

        PortraitDataStats,
        PowerUpStats,
    }

    public static class LocalObjectAllowType
    {                
        public static Dictionary<string, bool> AllowType;
        static LocalObjectAllowType()
        {
            AllowType = new Dictionary<string, bool>();
            AllowType.Add("System.Int64", true);
            AllowType.Add("System.Int32", true);
            AllowType.Add("System.Int16", true);
            AllowType.Add("System.String", true);
            AllowType.Add("System.Single", true);
            AllowType.Add("System.Byte", true);
            AllowType.Add("System.Double", true);
            AllowType.Add("System.String[]", true);
            AllowType.Add("System.Boolean", true);
           // AllowType.Add("Zealot.Common.Datablock.CollectionHandler", true);
        }
    }

    public class LocalObject : MarshalByRefObject
    {
        #region LocalObject InfoCache
        private static Dictionary<LOTYPE, LocalObjectInfo> LOInfos = new Dictionary<LOTYPE, LocalObjectInfo>();
        private static void InitLocalObjectInfo(Type t, LOTYPE objtype)
        {
            LocalObjectInfo info = new LocalObjectInfo();
            var propinfos = t.GetProperties();
            byte mcode = 0;
            List<string> tmpList = new List<string>();
            foreach (var propinfo in propinfos)
            {
                if (Attribute.GetCustomAttribute(propinfo, typeof(NotSyncedAttribute)) == null)
                {
                    MethodInfo msetter = propinfo.GetSetMethod();

                    info.mMethodDef.Add(msetter.Name, mcode);
                    info.mMethodDefReverse.Add(mcode, msetter.Name);

                    MethodInfo mgetter = propinfo.GetGetMethod();
                    if (LocalObjectAllowType.AllowType.ContainsKey(mgetter.ReturnType.FullName) ||
                                mgetter.ReturnType.FullName.Contains("CollectionHandler"))
                    {
                        info.mMethodGetterDef.Add("set_" + mgetter.Name.Substring(4), mgetter);
                    }

                    if (mgetter.ReturnType.FullName.Contains("CollectionHandler"))
                    {
                        tmpList.Add(mgetter.Name.Substring(4));
                    }
                    mcode++;
                }
            }
            foreach (string method in tmpList)
            {
                info.mListCollection.Add(info.mMethodDef["set_" + method], method);
            }
            LOInfos.Add(objtype, info);
        }
        #endregion

        private StringBuilder sb;

        private Dictionary<byte, object> mListCollection;
        private byte mTotalColfields = 0;
        
        private LOTYPE mLocalObjType;
        private bool mbIsDirty;
        private const byte PCODE = 0;
        protected Dictionary<byte, object> mDirtyDic;
        protected Dictionary<string, byte> mMethodDef;
        private Dictionary<byte, string> mMethodDefReverse;

        //private Dictionary<string, bool> mLazyUpdateAttributes;

        private Dictionary<string, MethodInfo> mMethodGetterDef;

        public delegate void OnValueChangedDelegate(string field, object value, object oldvalue);

        public OnValueChangedDelegate OnValueChanged { get; set; }

        public delegate void OnCollectionChangedDelegate(string field, byte idx, object value);
        
        public OnCollectionChangedDelegate OnCollectionChanged { get; set; }

        public delegate void OnCollectionChangedDelegateWithLO(LOTYPE lo,string field, byte idx, object value);

        public OnCollectionChangedDelegateWithLO OnCollectionChangedwithLO { get; set; }

        public delegate void OnNewlyAddedDelegate();
        public OnNewlyAddedDelegate OnNewlyAdded { get; set; }

        public delegate void OnLocalObjectChangedDelegate();
        public OnLocalObjectChangedDelegate OnLocalObjectChanged { get; set; }

        public bool IsNewlyAdded = true;

        public LocalObject(LOTYPE objtype)
        {
            this.OnValueChanged = null;
            mDirtyDic = new Dictionary<byte, object>();

            mbIsDirty = false;
            mLocalObjType = objtype;

            if(!LOInfos.ContainsKey(objtype))
            {
                InitLocalObjectInfo(GetType(), objtype);
            }
            var info = LOInfos[objtype];
            
            mMethodDef = info.mMethodDef;
            mMethodDefReverse = info.mMethodDefReverse;
            mMethodGetterDef = info.mMethodGetterDef;

            //mLazyUpdateAttributes = new Dictionary<string, bool>();
            mListCollection = new Dictionary<byte, object>(info.mListCollection);

            sb = new StringBuilder();
        }

        public void OnSetAttribute(string attribute, params object[] args)
        {
            sb.Length = 0;
            sb.Append("set_");
            sb.Append(attribute);
            ProxyMethod(sb.ToString(), args);
        }

        //public void AddLazyUpdateAttribute(string attri)
        //{
        //    if (mLazyUpdateAttributes.ContainsKey(attri) == false)
        //        mLazyUpdateAttributes.Add(attri, true);
        //}

        public void RegisterCollection(string fieldname, object col)
        {            
            bool found = false;
            byte foundbyte = 0;
            foreach(KeyValuePair<byte, object> kvp in mListCollection)
            {
                object obj = kvp.Value;
                if (obj.GetType() == typeof(string))
                {
                    string attrfield = (string)obj;
                    if (attrfield == fieldname)
                    {
                        found = true;
                        foundbyte = kvp.Key;
                        break;
                    }
                }
            }
            if (found)
            {
                mListCollection[foundbyte] = col;
                CollectionHandler<object> colhandler = mListCollection[foundbyte] as CollectionHandler<object>;
                int coltotal = colhandler.Count * colhandler.GetTfields();
                if (mTotalColfields + coltotal*2 > 255) //set and get
                {
                    Console.WriteLine("PLEASE CHECK YOUR LOCAL OBJECT SIZE!!!!!!!"); 
                    throw new LocalObjectException();

                }
                mTotalColfields += (byte)(coltotal*2);
            }
        }

        public virtual bool IsDirty()
        {
            return mbIsDirty;           
        }

        public virtual void SetDirty()
        {
            mbIsDirty = true;
        }

        public Dictionary<byte, object> GetDirtyDic()
        {
            return mDirtyDic;
        }

        public void Reset()
        {
            mbIsDirty = false;
            mDirtyDic.Clear();
            foreach (KeyValuePair<byte, object> kvp in mListCollection)
            {
                CollectionHandler<object> col = kvp.Value as CollectionHandler<object>;
                col.Reset();
            }
        }

        public byte GetLocalObjectType()
        {
            return (byte)mLocalObjType;
        }        
        
        public void ProxyMethod(string methodname, object[] args)
        {            
            if (mMethodDef.ContainsKey(methodname))
            {
                byte mcode = mMethodDef[methodname];
                if (mDirtyDic.ContainsKey(mcode) == false)
                    mDirtyDic.Add(mcode, args[0]);
                else
                    mDirtyDic[mcode] = args[0];
                
                SetDirty();
            }
        }        

        // multiple commands serialize
        public bool SerializeStream(byte locategory, int containerid, bool createnew, ref Dictionary<byte, object> dic)
        {                     
            byte code = (byte)dic.Count;
            if (createnew)
            {
                int colcount = mTotalColfields + mListCollection.Count * 3 + code + 5 + (mMethodGetterDef.Count - mListCollection.Count) * 2; //mMethodGetterDef.Count*2 + mListCollection.Count + mTotalColfields + code + 5;             
                if (colcount > 255)
                    return false;

                object[] args = new object[0];
                dic.Add(code++, locategory);
                dic.Add(code++, containerid);   // containerid can be persistentid or shared object id based on the local object category
                dic.Add(code++, mLocalObjType);
                dic.Add(code++, createnew);
                dic.Add(code++, (byte)mMethodGetterDef.Count);
 
                foreach (KeyValuePair<string, MethodInfo> kvp in mMethodGetterDef)
                {
                    byte mcode = mMethodDef[kvp.Key];
                    if (mListCollection.ContainsKey(mcode))
                    {
                        CollectionHandler<object> colhandler = mListCollection[mcode] as CollectionHandler<object>;
                        byte subfieldcount = colhandler.GetTfields();

                        dic.Add(code++, mcode);
                        dic.Add(code++, (byte)0); // new collection                                                
                        dic.Add(code++, (byte)colhandler.Count);
                        
                        foreach (object val in colhandler)
                        {
                            if (subfieldcount == 1)
                            {
                                dic.Add(code++, val);
                            }
                            // aiklong: next need to do multi field serialize                                
                        }
                    }
                    else
                    {                        
                        dic.Add(code++, mcode);
                        dic.Add(code++, kvp.Value.Invoke(this, args));
                    }
                }                
            }
            else
            {                
                // to use known length when created 

                int colcount = 0;
                int listcolcount = 0;
                foreach (KeyValuePair<byte, object> kvp in mDirtyDic)
                {
                    if (mListCollection.ContainsKey(kvp.Key))
                    {
                        listcolcount++;
                        CollectionHandler<object> colhandler = mListCollection[kvp.Key] as CollectionHandler<object>;
                        Dictionary<byte, object> coldirtydic = colhandler.GetDirtyDic();
                        colcount += coldirtydic.Count;
                    }
                    else
                        colcount++;
                }
                if ((colcount * 2 + listcolcount*3 + code + 5) > 255)
                    return false;

                dic.Add(code++, locategory);
                dic.Add(code++, containerid);
                dic.Add(code++, mLocalObjType);
                dic.Add(code++, createnew);
                dic.Add(code++, (byte)mDirtyDic.Count);
                                
                foreach (KeyValuePair<byte, object> attr in mDirtyDic)
                {
                    if (mListCollection.ContainsKey(attr.Key))
                    {                            
                        CollectionHandler<object> colhandler = mListCollection[attr.Key] as CollectionHandler<object>;
                        Dictionary<byte, object> coldirtydic = colhandler.GetDirtyDic();
                        dic.Add(code++, attr.Key);
                        dic.Add(code++, (byte)1);  // modify
                        dic.Add(code++, (byte)coldirtydic.Count);
                        foreach (KeyValuePair<byte, object> coldirtykvp in coldirtydic)
                        {
                            dic.Add(code++, coldirtykvp.Key);
                            // aiklong: now only cater for single value subfield
                            dic.Add(code++, coldirtykvp.Value);
                        }
                    }
                    else
                    {
                        dic.Add(code++, attr.Key);
                        dic.Add(code++, attr.Value);
                    }
                }                
            }            
            return true;
        }

        // single action command serialize
        public Dictionary<byte, object> Serialize(byte locategory, int containerid, bool createnew)
        {                        
            byte pcode = PCODE;
            Dictionary<byte, object> dic = new Dictionary<byte, object>();
            dic.Add(pcode++, 0);
            bool ret = SerializeStream(locategory, containerid, createnew, ref dic);            
            return dic;
        }
        
        public void Deserialize(Dictionary<byte, object> dic, bool createnew, LOTYPE objtype, ref byte pcode, out string methodname)
        {                        
            byte count = (byte)dic[pcode++];
            Type t = GetType();
            methodname = "";
            object[] argsgetter = new object[0];

            for (int i = 0; i < count; ++i)
            {
                byte mcode = (byte)dic[pcode++];
                methodname = mMethodDefReverse[mcode];
                object[] args = new object[1];
                if (mListCollection.ContainsKey(mcode))
                {
                    CollectionHandler<object> colhandler = mListCollection[mcode] as CollectionHandler<object>;
                    byte newormodify = (byte)dic[pcode++];
                    byte coldirtycount = (byte)dic[pcode++];
                    
                    for(byte j=0; j < coldirtycount; j++)
                    {
                        byte idx = 0;
                        object val = null;
                        if (newormodify == 0) // new collection
                        {
                            idx = j;
                            val = dic[pcode++];
                            colhandler[idx] = val;
                        }
                        else
                        {
                            idx = (byte)dic[pcode++];
                            val = dic[pcode++];
                            colhandler[idx] = val;
                        }
                        if (OnCollectionChanged != null)
                        {
                            OnCollectionChanged(methodname.Substring(4), idx, val);
                        }

                        if (OnCollectionChangedwithLO != null)
                        {
                            OnCollectionChangedwithLO(objtype, methodname.Substring(4), idx, val);
                        }
                    }
                }
                else
                {
                    MethodInfo mgetter = mMethodGetterDef[methodname];
                    object oldvalue = mgetter.Invoke(this, argsgetter);

                    MethodInfo m = t.GetMethod(methodname);
                    args[0] = dic[pcode++];
                    m.Invoke(this, args);
                  
                    if (OnValueChanged != null)
                    {
                        OnValueChanged(methodname.Substring(4), args[0], oldvalue);
                    }
                }           
            }

            if (OnLocalObjectChanged != null)
                OnLocalObjectChanged();
            if (createnew)
            {
                IsNewlyAdded = false;
                if (OnNewlyAdded != null)
                    OnNewlyAdded();
            }
        }

        public string ToFormattedString()
        {
            StringBuilder sb = new StringBuilder();            
            foreach (KeyValuePair<string, MethodInfo> kvp in mMethodGetterDef)
            {
                byte mcode = mMethodDef[kvp.Key];
                if (mListCollection.ContainsKey(mcode))
                {
                    sb.AppendLine("<<List>>");
                    CollectionHandler<object> colhandler = mListCollection[mcode] as CollectionHandler<object>;
                    byte subfieldcount = colhandler.GetTfields();

                    int index = 0;
                    foreach (object val in colhandler)
                    {
                        if (subfieldcount == 1)
                        {
                            if (val != null)
                                sb.AppendLine( "Index = " + index + ", value = " + val.ToString() );
                        }
                        index++;
                    }
                }
                else
                {
                    object[] args = new object[0];
                    sb.AppendLine(kvp.Key.Substring(4) + ":" + kvp.Value.Invoke(this, args)); 
                }
            }
            return sb.ToString();
        }
    }
}
