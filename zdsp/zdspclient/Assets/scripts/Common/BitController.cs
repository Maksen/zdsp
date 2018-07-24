namespace Zealot.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class BitAttribute : Attribute
    {
        public int BitCount { get; private set; }
        public BitAttribute()
        {
            BitCount = 8;//1 byte
        }
        public BitAttribute(int bitCount)
        {
            BitCount = bitCount;
        }
    }

    public abstract class BitController
    {
        Dictionary<PropertyInfo, BitAttribute> mMethod;
        public BitController()
        {
            InitReflection();
        }

        public abstract void InitReflection();

        protected void SetReflectionProperty(Dictionary<PropertyInfo, BitAttribute> method)
        {
            mMethod = method;
        }
        protected void Init(ref Dictionary<PropertyInfo, BitAttribute> method)
        {
            method = new Dictionary<PropertyInfo, BitAttribute>();
            var propinfos = GetType().GetProperties();
            foreach (var propinfo in propinfos)
            {
                object[] attrs = propinfo.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    BitAttribute authAttr = attr as BitAttribute;
                    if (authAttr != null)
                    {
                        method.Add(propinfo, authAttr);
                    }
                }
            }
            SetReflectionProperty(method);
        }
        public int EncodeBit()
        {
            if (mMethod == null)
                return -1;
            //BitField = 0;//reset
            int BitField = 0;

            object[] args = new object[0];
            foreach (var method in mMethod)
            {
                int value = Convert.ToInt32(method.Key.GetGetMethod().Invoke(this, args));
                BitField <<= method.Value.BitCount;
                BitField |= value;
            }

            return BitField;
        }

        public void DecodeBit(int BitField)
        {
            if (mMethod == null)
                return;

            object[] args = new object[1];
            for (int i = mMethod.Count - 1; i >= 0; i--)
            {
                var element = mMethod.ElementAt(i);
                int bitcount = element.Value.BitCount;
                int value = BitField & ((1 << bitcount) - 1);
                BitField >>= bitcount;

                if (element.Key.GetGetMethod().ReturnType.FullName == "System.Boolean")
                {
                    args[0] = value > 0 ? true : false;
                    element.Key.GetSetMethod().Invoke(this, args);
                }
                else
                {
                    args[0] = value;
                    element.Key.GetSetMethod().Invoke(this, args);
                }
            }
        }
    }
}