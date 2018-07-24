using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;

/*  The AssetBundle Manager provides a High-Level API for working with AssetBundles. 
    The AssetBundle Manager will take care of loading AssetBundles and their associated 
    Asset Dependencies.
        Initialize()
            Initializes the AssetBundle manifest object.
        LoadAssetAsync()
            Loads a given asset from a given AssetBundle and handles all the dependencies.
        LoadLevelAsync()
            Loads a given scene from a given AssetBundle and handles all the dependencies.
        LoadDependencies()
            Loads all the dependent AssetBundles for a given AssetBundle.
        BaseDownloadingURL
            Sets the base downloading url which is used for automatic downloading dependencies.
        SimulateAssetBundleInEditor
            Sets Simulation Mode in the Editor.
        Variants
            Sets the active variant.
        RemapVariantName()
            Resolves the correct AssetBundle according to the active variant.
*/

namespace AssetBundles
{
    /// <summary>
    /// Loaded assetBundle contains the references count which can be used to
    /// unload dependent assetBundles automatically.
    /// </summary>
    public class LoadedAssetBundle
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        internal event Action unload;

        internal void OnUnload()
        {
            m_AssetBundle.Unload(false);
            if (unload != null)
                unload();
        }

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }
    		
    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        public enum LogMode { All, JustErrors };
        public enum LogType { Info, Warning, Error };

        static LogMode m_LogMode = LogMode.All;
        static string m_BaseDownloadingURL = "";
        static string[] m_ActiveVariants = { };
        static AssetBundleManifest m_AssetBundleManifest = null;


        static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();
        static Dictionary<string, int> m_DownloadingBundles = new Dictionary<string, int> ();
        static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();
        static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

        public static LogMode logMode
        {
            get { return m_LogMode; }
            set { m_LogMode = value; }
        }

        /// <summary>
        /// The base downloading url which is used to generate the full
        /// downloading url with the assetBundle names.
        /// </summary>
        public static string BaseDownloadingURL
        {
            get { return m_BaseDownloadingURL; }
            set { m_BaseDownloadingURL = value; }
        }

        public delegate string OverrideBaseDownloadingURLDelegate(string bundleName);

        /// <summary>
        /// Implements per-bundle base downloading URL override.
        /// The subscribers must return null values for unknown bundle names;
        /// </summary>
        public static event OverrideBaseDownloadingURLDelegate overrideBaseDownloadingURL;

        /// <summary>
        /// Variants which is used to define the active variants.
        /// </summary>
        public static string[] ActiveVariants
        {
            get { return m_ActiveVariants; }
            set { m_ActiveVariants = value; }
        }

        /// <summary>
        /// AssetBundleManifest object which can be used to load the dependecies
        /// and check suitable assetBundle variants.
        /// </summary>
        public static AssetBundleManifest AssetBundleManifestObject
        {
            set { m_AssetBundleManifest = value; }
        }

        private static void Log(LogType logType, string text)
        {
            if (logType == LogType.Error)
                Debug.LogError("[AssetBundleManager] " + text);
            else if (m_LogMode == LogMode.All && logType == LogType.Warning)
                Debug.LogWarning("[AssetBundleManager] " + text);
            else if (m_LogMode == LogMode.All)
                Debug.Log("[AssetBundleManager] " + text);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulateAssetBundleInEditor
        {
            get
            {
                return false;
            }
        }
#endif

        public static string GetStreamingAssetsPath()
        {
            if (Application.isEditor)
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            //else if (Application.isWebPlayer)
            //    return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
            {
#if UNITY_IOS
                return "file://" + Application.streamingAssetsPath;
#endif
                return Application.streamingAssetsPath;
            }
            else // For standalone player.
                return "file://" + Application.streamingAssetsPath;
        }

        /// <summary>
        /// Sets base downloading URL to a directory relative to the streaming assets directory.
        /// Asset bundles are loaded from a local directory.
        /// </summary>
        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = GetStreamingAssetsPath() + relativePath;
        }

        /// <summary>
        /// Sets base downloading URL to a web URL. The directory pointed to by this URL
        /// on the web-server should have the same structure as the AssetBundles directory
        /// in the demo project root.
        /// </summary>
        /// <example>For example, AssetBundles/iOS/xyz-scene must map to
        /// absolutePath/iOS/xyz-scene.
        /// <example>
        public static void SetSourceAssetBundleURL(string absolutePath)
        {
            if (!absolutePath.EndsWith("/"))
            {
                absolutePath += "/";
            }

            var platformName = Utility.GetPlatformName();

#if MYCARD
            platformName = "MyCard/" + platformName;
#endif

            BaseDownloadingURL = absolutePath + platformName + "/" + platformName + "/";
        }

        /// <summary>
        /// Retrieves an asset bundle that has previously been requested via LoadAssetBundle.
        /// Returns null if the asset bundle or one of its dependencies have not been downloaded yet.
        /// </summary>
        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error, bool log=false)
        {
            if (m_DownloadingErrors.TryGetValue (assetBundleName, out error)) {
	            //if (log)
	            //	Debug.LogFormat ("m_LoadedAssetBundles download error {0}", assetBundleName);
	            return null;
            }
            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null) {
	            //if (log)
	            //	Debug.LogFormat ("m_LoadedAssetBundles doesn't contain {0}", assetBundleName);
	            return null;
            }

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                if (m_DownloadingErrors.TryGetValue (dependency, out error)) {
	                //if (log)
	                //	Debug.LogFormat ("m_LoadedAssetBundles download error {0}, dependency {1}", assetBundleName, dependency);
	                return null;
                }
                // Wait all the dependent assetBundles being loaded.
                LoadedAssetBundle dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) {
	                //if (log)
	                //	Debug.LogFormat ("m_LoadedAssetBundles doesn't contain {0}, dependency {1}", assetBundleName, dependency);
	                return null;
                }
            }

            return bundle;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been downloaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        static public bool IsAssetBundleDownloaded(string assetBundleName, out string error)
        {
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                return false;

            return m_LoadedAssetBundles.ContainsKey(assetBundleName);
        }

        /// <summary>
        /// Initializes asset bundle manager
        /// </summary>
        static public void Initialize()
        {
            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);
        }

        /// <summary>
        /// Starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation LoadManifest()
        {
            string manifestAssetBundleName = Utility.GetPlatformName();
            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            m_InProgressOperations.Add(operation);
            return operation;
        }

        // Temporarily work around a il2cpp bug
        static protected void LoadAssetBundle(string assetBundleName)
        {
            LoadAssetBundle(assetBundleName, false);
        }

        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if (SimulateAssetBundleInEditor)
                return;
#endif

            if (!isLoadingAssetBundleManifest)
            {
                if (m_AssetBundleManifest == null)
                {
                    Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }
            LoadAssetBundleInternal (assetBundleName, isLoadingAssetBundleManifest, (isAlreadyProcessed) => LoadAssetBundleCB (assetBundleName, isLoadingAssetBundleManifest, isAlreadyProcessed));
        }

        private static void LoadAssetBundleCB(string assetBundleName, bool isLoadingAssetBundleManifest, bool isAlreadyProcessed)
        {
            // Check if the assetBundle has already been processed.
            // Load dependencies.
            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
	            LoadDependencies (assetBundleName);	
        }

        // Returns base downloading URL for the given asset bundle.
        // This URL may be overridden on per-bundle basis via overrideBaseDownloadingURL event.
        protected static string GetAssetBundleBaseDownloadingURL(string bundleName)
        {
            if (overrideBaseDownloadingURL != null)
            {
                foreach (OverrideBaseDownloadingURLDelegate method in overrideBaseDownloadingURL.GetInvocationList())
                {
                    string res = method(bundleName);
                    if (res != null)
                        return res;
                }
            }
            return m_BaseDownloadingURL;
        }

        // Checks who is responsible for determination of the correct asset bundle variant
        // that should be loaded on this platform. 
        //
        // On most platforms, this is done by the AssetBundleManager itself. However, on
        // certain platforms (iOS at the moment) it's possible that an external asset bundle
        // variant resolution mechanism is used. In these cases, we use base asset bundle 
        // name (without the variant tag) as the bundle identifier. The platform-specific 
        // code is responsible for correctly loading the bundle.
        static protected bool UsesExternalBundleVariantResolutionMechanism(string baseAssetBundleName)
        {
#if ENABLE_IOS_APP_SLICING
            var url = GetAssetBundleBaseDownloadingURL(baseAssetBundleName);
            if (url.ToLower().StartsWith("res://") ||
                url.ToLower().StartsWith("odr://"))
                return true;
#endif
            return false;
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        static protected string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

            // Get base bundle name
            string baseName = assetBundleName.Split('.')[0];

            if (UsesExternalBundleVariantResolutionMechanism(baseName))
                return baseName;

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                string curBaseName = curSplit[0];
                string curVariant = curSplit[1];

                if (curBaseName != baseName)
                    continue;

                int found = System.Array.IndexOf(m_ActiveVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFit == int.MaxValue - 1)
            {
                Log(LogType.Warning, "Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex]);
            }

            if (bestFitIndex != -1)
            {
                return bundlesWithVariant[bestFitIndex];
            }
            else
            {
                return assetBundleName;
            }
        }

        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static protected void LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest, System.Action<bool> callback)
        {
            //Debug.LogFormat ("LoadAssetBundle {0}", assetBundleName);
            // Already loaded.
            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                bundle.m_ReferencedCount++;
                //Debug.LogFormat ("LoadAssetBundle {0}, ref={1}", assetBundleName, bundle.m_ReferencedCount);
                if (callback != null)
	                callback(true);
                return;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
			if (m_DownloadingBundles.ContainsKey (assetBundleName)) {
                m_DownloadingBundles [assetBundleName]++;
                if (callback != null)
	                callback(true);
				return;
			}

            string bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if (bundleBaseDownloadingURL.ToLower().StartsWith("odr://"))
            {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through ODR");
                m_InProgressOperations.Add(new AssetBundleDownloadFromODROperation(assetBundleName));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through ODR: this Unity version or build target doesn't support it.");
#endif
            }
            else if (bundleBaseDownloadingURL.ToLower().StartsWith("res://"))
            {
#if ENABLE_IOS_APP_SLICING
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through asset catalog");
                m_InProgressOperations.Add(new AssetBundleOpenFromAssetCatalogOperation(assetBundleName));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through asset catalog: this Unity version or build target doesn't support it.");
#endif
            }
            else
            {
                WWW download = null;

                if (!bundleBaseDownloadingURL.EndsWith("/"))
                {
                    bundleBaseDownloadingURL += "/";
                }

                string url = bundleBaseDownloadingURL + assetBundleName;

                // For manifest assetbundle, always download it as we don't have hash for it.
                if (isLoadingAssetBundleManifest)
	                download = new WWW (url);
                else {
	                if (m_DownloadingBundles.Count < 25)
		                download = WWW.LoadFromCacheOrDownload (url, m_AssetBundleManifest.GetAssetBundleHash (assetBundleName), 0);
	                else
	                {
		                //Debug.LogFormat("LoadAssetBundleInternal exceed limit {0}", assetBundleName);						
		                AssetLoader.Instance.StartCoroutine (LoadAssetBundleFromCache (url, assetBundleName, callback));
		                return;
	                }
                }

                m_InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, download));
            }
            m_DownloadingBundles.Add(assetBundleName, 1);
            //Debug.LogFormat("LoadAssetBundleInternal {0}", assetBundleName);
            if (callback != null)
	            callback(false);
        }

        private static IEnumerator LoadAssetBundleFromCache(string url, string assetBundleName, System.Action<bool> callback)
        {
	        do {
		        yield return null;
	        } while (m_DownloadingBundles.Count >= 25);
				
	        if (m_DownloadingBundles.ContainsKey (assetBundleName)) {
		        m_DownloadingBundles [assetBundleName]++;
		        //Debug.LogFormat("LoadAssetBundleFromCache exist {0}, {1}", assetBundleName, m_DownloadingBundles [assetBundleName]);
		        if (callback != null)
			        callback(true);
		        yield break;
	        }

	        WWW download = WWW.LoadFromCacheOrDownload (url, m_AssetBundleManifest.GetAssetBundleHash (assetBundleName), 0);
	        m_InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, download));
	        m_DownloadingBundles.Add(assetBundleName, 1);
	        //Debug.LogFormat("LoadAssetBundleFromCache new {0}", assetBundleName);
	        if (callback != null)
		        callback(false);
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName)
        {
            if (m_AssetBundleManifest == null)
            {
                Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++) {
	            //Debug.LogFormat ("LoadDependencies {0}", dependencies [i]);
	            LoadAssetBundleInternal (dependencies [i], false, null);				
            }
        }

        static public void UnloadAssetBundleManifest()
        {
            UnloadAssetBundle(Utility.GetPlatformName());
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        static public void UnloadAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
                return;
#endif
            assetBundleName = RemapVariantName(assetBundleName);

            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
                return;

            if (--bundle.m_ReferencedCount == 0)
            {
                bundle.OnUnload();
                m_LoadedAssetBundles.Remove(assetBundleName);

                Log(LogType.Info, assetBundleName + " has been unloaded successfully");
            }
        }

        void Update()
        {
            // Update all in progress operations
            for (int i = 0; i < m_InProgressOperations.Count;)
            {
                var operation = m_InProgressOperations[i];
                if (operation.Update())
                {
                    i++;
                }
                else
                {
                    m_InProgressOperations.RemoveAt(i);
                    ProcessFinishedOperation(operation);
                }
            }
        }

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            AssetBundleDownloadOperation download = operation as AssetBundleDownloadOperation;
            if (download == null)
                return;

            if (download.error == null)
            {
                m_LoadedAssetBundles.Add(download.assetBundleName, download.assetBundle);
                if (m_DownloadingBundles.ContainsKey(download.assetBundleName))
	                download.assetBundle.m_ReferencedCount = m_DownloadingBundles [download.assetBundleName];
                m_DownloadingErrors.Remove(download.assetBundleName);
            }
            else
            {
                string msg = string.Format("Failed downloading bundle {0} from {1}: {2}",
                        download.assetBundleName, download.GetSourceURL(), download.error);
                m_DownloadingErrors[download.assetBundleName] = msg;
            }

            m_DownloadingBundles.Remove(download.assetBundleName);
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

            AssetBundleLoadAssetOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if (assetPaths.Length == 0)
                {
                    Log(LogType.Error, "There is no asset with name \"" + assetName + "\" in " + assetBundleName);
                    return null;
                }

                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                UnityEngine.Object target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                operation = new AssetBundleLoadAssetOperationSimulation(target);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

                m_InProgressOperations.Add(operation);
            }

            return operation;
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadLevelOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

            AssetBundleLoadLevelOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                operation = new AssetBundleLoadLevelSimulationOperation(assetBundleName, levelName, isAdditive);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

                m_InProgressOperations.Add(operation);
            }

            return operation;
        }

        static public AssetBundleLoadAssetOperation LoadAllAssetAsync(string assetBundleName, System.Type type)
        {
            Log(LogType.Info, "Loading all assets from " + assetBundleName + " bundle");

            AssetBundleLoadAssetOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                //string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if (assetPaths.Length == 0)
                {
                    Log(LogType.Error, "There is no asset with name in " + assetBundleName);
                    return null;
                }

                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                UnityEngine.Object target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                operation = new AssetBundleLoadAssetOperationSimulation(target);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadAllAssetOperationFull(assetBundleName, type);

                m_InProgressOperations.Add(operation);
            }

            return operation;
        }


        #region Patching
        static private bool CanDownloadAssetBundle(string bundleName)
        {
            Hash128 levelhash = m_AssetBundleManifest.GetAssetBundleHash(bundleName);
            if (levelhash.isValid)
            {
                string url = AssetBundleManager.BaseDownloadingURL + bundleName;
                return !Caching.IsVersionCached(url, levelhash);
            }
            return false;
        }

        public static void MarkAsUsed(string bundleName)
        {
            Hash128 levelhash = m_AssetBundleManifest.GetAssetBundleHash(bundleName);
            if (levelhash.isValid)
            {
                string url = AssetBundleManager.BaseDownloadingURL + bundleName;
                if (Caching.MarkAsUsed(url, levelhash) == false)
                    Debug.LogError("MarkAsUsed Error : " + bundleName);
            }
        }

        public static void MarkAllBundleAsUsed()
        {
            var bundles = m_AssetBundleManifest.GetAllAssetBundles();
            foreach (string bundleName in bundles)
            {
                var levelhash = m_AssetBundleManifest.GetAssetBundleHash(bundleName);
                if (levelhash.isValid)
                {
                    string url = AssetBundleManager.BaseDownloadingURL + bundleName;
                    if (Caching.MarkAsUsed(url, levelhash) == false)
                        Debug.LogError("MarkAsUsed Error : " + bundleName);
                }
            }
        }

        public static HashSet<string> GetDownloadList()
        {
            HashSet<string> downloadList = new HashSet<string>();

            var bundles = m_AssetBundleManifest.GetAllAssetBundles();
            foreach (string bundleName in bundles)
            {
                if (CanDownloadAssetBundle(bundleName))
                {
                    downloadList.Add(bundleName);
                }
                else
                {
                    MarkAsUsed(bundleName);
                }
            }

            return downloadList;
        }

        static public AssetBundleLoadDownloadPatchOperation PatchBundle(string assetBundleName)
        {
            Log(LogType.Info, "Patching " + assetBundleName);

            AssetBundleLoadDownloadPatchOperation operation = null;

            //Mirror LoadAssetBundleInternal
            assetBundleName = RemapVariantName(assetBundleName);
                        
            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                throw new Exception(string.Format("Bundle [{0}] still loaded while patching", assetBundleName));
            }

            if (m_DownloadingBundles.ContainsKey (assetBundleName)) {
	            m_DownloadingBundles [assetBundleName]++;
	            return null;
            }

            string bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if (!bundleBaseDownloadingURL.EndsWith("/"))
            {
                bundleBaseDownloadingURL += "/";
            }

            string url = bundleBaseDownloadingURL + assetBundleName;

            WWW download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

            operation = new AssetBundleLoadDownloadPatchOperation(assetBundleName, download);

            m_InProgressOperations.Add(operation);
            m_DownloadingBundles.Add(assetBundleName, 1);

            return operation;
        }
#endregion
    } // End of AssetBundleManager.
}