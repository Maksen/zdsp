using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AssetBundles
{
    [Serializable]
    public struct BundleSize
    {
        public string name;
        public long size;

        public BundleSize(string bundleName, long size)
        {
            this.name = bundleName;
            this.size = size;
        }

    }

    [Serializable]
    public class AssetBundleSizeInfo
    {
        public const string FileName = "bundleinfo.json";

        [SerializeField]
        BundleSize[] bundleSizes;

        [NonSerialized]
        Dictionary<string, long> bundleSizeMap;

        public long GetBundleSize(string bundleName)
        {
            long size = 0;
            bundleSizeMap.TryGetValue(bundleName, out size);
            return size;
        }

        void PreSerialize()
        {
            int idx = 0;
            bundleSizes = new BundleSize[bundleSizeMap.Count];

            foreach (var kvp in bundleSizeMap)
            {
                bundleSizes[idx] = new BundleSize(kvp.Key, kvp.Value);
                idx++;
            }
        }

        void PostDeserialize()
        {
            bundleSizeMap = new Dictionary<string, long>();
            if (bundleSizes != null)
            {
                for (int i = 0; i < bundleSizes.Length; i++)
                {
                    bundleSizeMap.Add(bundleSizes[i].name, bundleSizes[i].size);
                }
            }
        }

        public void AddBundleSize(string bundleName, long size)
        {
            if (bundleSizeMap == null)
                bundleSizeMap = new Dictionary<string, long>();

            if (!bundleSizeMap.ContainsKey(bundleName))
                bundleSizeMap.Add(bundleName, size);
            else
                Debug.LogErrorFormat("[AddBundleSize] {0} already exists", bundleName);
        }

        public static string Serialize(AssetBundleSizeInfo obj)
        {
            obj.PreSerialize();
            return JsonUtility.ToJson(obj);
        }

        public static AssetBundleSizeInfo Deserialize(string json)
        {
            var obj = JsonUtility.FromJson<AssetBundleSizeInfo>(json);
            obj.PostDeserialize();
            return obj;
        }
    }
}
