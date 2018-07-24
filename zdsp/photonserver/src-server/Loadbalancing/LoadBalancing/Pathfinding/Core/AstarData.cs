using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_WINRT && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
//using MarkerMetro.Unity.WinLegacy.Reflection;
#endif

namespace Pathfinding {
	
	[System.Serializable]
	/** Stores the navigation graphs for the A* Pathfinding System.
	 * \ingroup relevant
	 * 
	 * An instance of this class is assigned to AstarPath.astarData, from it you can access all graphs loaded through the #graphs variable.\n
	 * This class also handles a lot of the high level serialization.
	 */
	public class AstarData {
		
		/** Shortcut to AstarPath.active */
        public static AstarPath active
        {
            get
            {
                return AstarPath.active;
            }
        }

        public string level;

#region Fields						

		/** All supported graph types.
		 * Populated through reflection search
		 */
		public System.Type[] graphTypes {get; private set;}
		
#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT || UNITY_WEBGL
		/** Graph types to use when building with Fast But No Exceptions for iPhone.
		 * If you add any custom graph types, you need to add them to this hard-coded list.
		 */
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
			typeof(GridGraph),
			typeof(PointGraph),
			typeof(NavMeshGraph),
		};
#endif
		
		/** All graphs this instance holds.
		 * This will be filled only after deserialization has completed.
		 * May contain null entries if graph have been removed.
		 */
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];
		
		//Serialization Settings
		
		/** Serialized data for all graphs and settings.
		 * Stored as a base64 encoded string because otherwise Unity's Undo system would sometimes corrupt the byte data (because it only stores deltas).
		 *
		 * This can be accessed as a byte array from the #data property.
		 *
		 * \since 3.6.1
		 */
		[SerializeField]
		string dataString;

		/** Data from versions from before 3.6.1.
		 * Used for handling upgrades
		 * \since 3.6.1
		 */
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("data")]
		private byte[] upgradeData;

		/** Serialized data for all graphs and settings */
		private byte[] data {
			get {
				// Handle upgrading from earlier versions than 3.6.1
				if (upgradeData != null && upgradeData.Length > 0) {
					data = upgradeData;
					upgradeData = null;
				}
				return dataString != null ? System.Convert.FromBase64String (dataString) : null;
			}
			set {
				dataString = value != null ? System.Convert.ToBase64String (value) : null;
			}
		}
		
		/** Backup data if deserialization failed.
		 */
		public byte[] data_backup;

		/** Serialized data for cached startup.
		 * If set, on start the graphs will be deserialized from this file.
		  */
		public TextAsset file_cachedStartup;

		/** Serialized data for cached startup.
		 * 
		 * \deprecated Deprecated since 3.6, AstarData.file_cachedStartup is now used instead
		 */
		public byte[] data_cachedStartup;

		/** Should graph-data be cached.
		 * Caching the startup means saving the whole graphs, not only the settings to an internal array (#data_cachedStartup) which can
		 * be loaded faster than scanning all graphs at startup. This is setup from the editor.
		 */
		[SerializeField]
		public bool cacheStartup;
		
		//End Serialization Settings
		
#endregion
		
		public byte[] GetData () {
			return data;
		}
		
		public void SetData (byte[] data) {
			this.data = data;
		}
		
		/** Loads the graphs from memory, will load cached graphs if any exists */
		public void Awake () {
			
			graphs = new NavGraph[0];
			/* End default values */
			
			DeserializeGraphs ();			
		}	
		
#region Serialization		
				
		/** Deserializes graphs from #data */
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs (data);
			}
		}
		
		/** Destroys all graphs and sets graphs to null */
		void ClearGraphs () {
			if ( graphs == null ) return;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null) graphs[i].OnDestroy ();
			}
			graphs = null;
			//UpdateShortcuts ();
		}
		
		public void OnDestroy () {
			ClearGraphs ();
		}
		
		/** Deserializes graphs from the specified byte array.
		 * If an error occured, it will try to deserialize using the old deserializer.
		 * A warning will be logged if all deserializers failed.
		  */
		public void DeserializeGraphs (byte[] bytes) {
			
			AstarPath.active.BlockUntilPathQueueBlocked();            
			
			try {
				if (bytes != null) {
                    var sr = new Pathfinding.Serialization.AstarSerializer(this);
                    if (sr.OpenDeserialize(bytes))
                    {
                        DeserializeGraphsPart(sr);

                        sr.CloseDeserialize();
                    }
                    else
                    {
                        //Debug.LogWarning("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
                    }					    					
				} else {
					throw new System.ArgumentNullException ("bytes");
				}
				active.VerifyIntegrity (this);
			} catch (System.Exception e) {
				//Debug.LogWarning ("Caught exception while deserializing data.\n"+e);
				data_backup = bytes;
			}
			
		}			
		
		/** Deserializes common info.
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 */
		public void DeserializeGraphsPart (Pathfinding.Serialization.AstarSerializer sr) {
			ClearGraphs ();
			graphs = sr.DeserializeGraphs ();

			sr.DeserializeExtraInfo();

			//Assign correct graph indices.
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) continue;
				graphs[i].GetNodes (node => {
					node.GraphIndex = (uint)i;
					return true;
				});
			}

			sr.PostDeserialization();
		}				
		
#endregion				
		
#region GraphCreation
				
		/** Creates a new graph instance of type \a type
		 * \see CreateGraph(string) */
		public NavGraph CreateGraph (System.Type type) {
            var g = System.Activator.CreateInstance (type) as NavGraph;
			g.active = active;
            g.astarData = this;
			return g;
		}
					
#endregion
		
#region GraphUtility			
		/** Gets the index of the NavGraph in the #graphs array */
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException ("graph");
			
			if ( graphs != null ) {
				for (int i=0;i<graphs.Length;i++) {
					if (graph == graphs[i]) {
						return i;
					}
				}
			}
			//Debug.LogError ("Graph doesn't exist");
			return -1;
		}
		
#endregion
	}
}