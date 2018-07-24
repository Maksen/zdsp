using System;
using System.Collections.Generic;

namespace Zealot.Common.RPC
{
    public enum RPCUsageType
    {
        Default,            //The field will be sent/received by its actual type (will not be encoded)
        RelativePosition,   //The field will be sent after the Vector3 is encoded to relative pos and decoded on received
        Direction           //The field will be sent after the Vector3 is encoded to yaw and decoded on received
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RPCFieldUsageAttribute : Attribute
    {
        public readonly RPCUsageType UsageType;

        public RPCFieldUsageAttribute()
            : this(RPCUsageType.Default)
        { }

        public RPCFieldUsageAttribute(RPCUsageType usageType)
        {
            UsageType = usageType;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RPCMethodAttribute : Attribute
    {
        public readonly RPCCategory Category;
        public readonly byte MethodID; //unique only for the RPC class it is in
        public readonly bool SuspendRPC = false;        

        public RPCMethodAttribute(RPCCategory category, byte methodid)
        {
            Category = category;
            MethodID = methodid;
        }        

        public RPCMethodAttribute(RPCCategory category, byte methodid, bool suspend)
        {
            Category = category;
            MethodID = methodid;
            SuspendRPC = suspend;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RPCUnsuspendAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RPCMethodProxyAttribute : Attribute
    {
        public readonly RPCCategory Category;
        public readonly byte MethodID; //unique only for the RPC class it is in
        public RPCMethodProxyAttribute(RPCCategory category, byte methodid)
        {
            Category = category;
            MethodID = methodid;
        }
    }
}
