using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
using ParameterizedThreadStart = Pathfinding.WindowsStore.ParameterizedThreadStart;
#else
using Thread = System.Threading.Thread;
using ParameterizedThreadStart = System.Threading.ParameterizedThreadStart;
#endif

/** Core Component for the A* Pathfinding System.
 * This class handles all of the pathfinding system, calculates all paths and stores the info.\n
 * This class is a singleton class, meaning there should only exist at most one active instance of it in the scene.\n
 * It might be a bit hard to use directly, usually interfacing with the pathfinding system is done through the Seeker class.
 *
 * \nosubgrouping
 * \ingroup relevant */
public class AstarPath {

	/** The version number for the A* %Pathfinding Project
	 */
	public static System.Version Version {
		get {
			return new System.Version (3,7,4);
		}
	}

	/** Information about where the package was downloaded */
	public enum AstarDistribution { WebsiteDownload, AssetStore };

	/** Used by the editor to guide the user to the correct place to download updates */
	public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;

	/** Which branch of the A* %Pathfinding Project is this release.
	 * Used when checking for updates so that
	 * users of the development versions can get notifications of development
	 * updates.
	 */
	public static readonly string Branch = "master_Free";

	/** Used by the editor to show some Pro specific stuff.
	 * Note that setting this to true will not grant you any additional features */
	public static readonly bool HasPro = false;

	/** Holds all graph data */
    public static Dictionary<string, AstarData> allAstarData = new Dictionary<string,AstarData>();
	//public AstarData astarData;

    /** Holds all pathhandler data*/
    public static Dictionary<string, PathHandler> allPathHandler = new Dictionary<string, PathHandler>();

	/** Returns the active AstarPath object in the scene.
	 * \note This is only set if the AstarPath object has been initialized (which happens in Awake).
	 */
	public static AstarPath active;

	/** Shortcut to Pathfinding.AstarData.graphs */
    //public NavGraph[] graphs {
    //    get {
    //        if (astarData == null)
    //            astarData = new AstarData ();
    //        return astarData.graphs;
    //    }
    //    set {
    //        if (astarData == null)
    //            astarData = new AstarData ();
    //        astarData.graphs = value;
    //    }
    //}

    //public NavGraph[] GetGraphs(string levelname)
    //{
        
    //}

#region InspectorSettings
	/** @name Inspector - Settings
	 * @{ */

	/** Max Nearest Node Distance.
	 * When searching for a nearest node, this is the limit (world units) for how far away it is allowed to be.
	 * \see Pathfinding.NNConstraint.constrainDistance
	 */
	public float maxNearestNodeDistance = 100;

	/** Max Nearest Node Distance Squared.
	 * \see #maxNearestNodeDistance */
	public float maxNearestNodeDistanceSqr {
		get { return maxNearestNodeDistance*maxNearestNodeDistance; }
	}

	/** Do a full GetNearest search for all graphs.
	 * Additional searches will normally only be done on the graph which in the first fast search seemed to have the closest node.
	 * With this setting on, additional searches will be done on all graphs since the first check is not always completely accurate.\n
	 * More technically: GetNearestForce on all graphs will be called if true, otherwise only on the one graph which's GetNearest search returned the best node.\n
	 * Usually faster when disabled, but higher quality searches when enabled.
	 * When using a a navmesh or recast graph, for best quality, this setting should be combined with the Pathfinding.NavMeshGraph.accurateNearestNode setting set to true.
	 * \note For the PointGraph this setting doesn't matter much as it has only one search mode.
	 */
	public bool fullGetNearestSearch = false;

	/** Prioritize graphs.
	 * Graphs will be prioritized based on their order in the inspector.
	 * The first graph which has a node closer than #prioritizeGraphsLimit will be chosen instead of searching all graphs.
	 */
	public bool prioritizeGraphs = false;

	/** Distance limit for #prioritizeGraphs.
	 * \see #prioritizeGraphs
	 */
	public float prioritizeGraphsLimit = 1F;

	/** Stored tag names.
	 * \see AstarPath.FindTagNames
	 * \see AstarPath.GetTagNames
	 */
	[SerializeField]
	protected string[] tagNames = null;

	/** The heuristic to use.
	 * The heuristic, often referred to as 'H' is the estimated cost from a node to the target.
	 * Different heuristics affect how the path picks which one to follow from multiple possible with the same length
	 * \see Pathfinding.Heuristic
	 */
	public Heuristic heuristic = Heuristic.Euclidean;

	/** The scale of the heuristic. If a smaller value than 1 is used, the pathfinder will search more nodes (slower).
	 * If 0 is used, the pathfinding will be equal to dijkstra's algorithm.
	 * If a value larger than 1 is used the pathfinding will (usually) be faster because it expands fewer nodes, but the paths might not longer be optimal
	 */
	public float heuristicScale = 1F;

	/** Number of pathfinding threads to use.
	 * Multithreading puts pathfinding in another thread, this is great for performance on 2+ core computers since the framerate will barely be affected by the pathfinding at all.
	 * - None indicates that the pathfinding is run in the Unity thread as a coroutine
	 * - Automatic will try to adjust the number of threads to the number of cores and memory on the computer.
	 * 	Less than 512mb of memory or a single core computer will make it revert to using no multithreading.
	 *
	 * It is recommended that you use one of the "Auto" settings that are available.
	 * The reason is that even if your computer might be beefy and have 8 cores.
	 * Other computers might only be quad core or dual core in which case they will not benefit from more than
	 * 1 or 3 threads respectively (you usually want to leave one core for the unity thread).
	 * If you use more threads than the number of cores on the computer it is mostly just wasting memory, it will not run any faster.
	 * The extra memory usage is not trivially small. Each thread needs to keep a small amount of data for each node in all the graphs.
	 * It is not the full graph data but it is proportional to the number of nodes.
	 * The automatic settings will inspect the machine it is running on and use that to determine the number of threads so that no memory is wasted.
	 *
	 * The exception is if you only have one (or maybe two characters) active at time. Then you should probably just go with one thread always since it is very unlikely
	 * that you will need the extra throughput given by more threads. Keep in mind that more threads primarily increases throughput by calculating different paths on different
	 * threads, it will not calculate individual paths any faster.
	 *
	 * Note that if you are modifying the pathfinding core scripts or if you are directly modifying graph data without using any of the
	 * safe wrappers (like RegisterSafeUpdate) multithreading can cause strange errors and pathfinding stopping to work if you are not careful.
	 * For basic usage (not modding the pathfinding core) it should be safe.\n
	 *
	 * \note WebGL does not support threads at all (since javascript is single-threaded)
	 *
	 * \see CalculateThreadCount
	 *
	 * \astarpro
	 */
	public ThreadCount threadCount = ThreadCount.None;

	/** Max number of milliseconds to spend each frame for pathfinding.
	 * At least 500 nodes will be searched each frame (if there are that many to search).
	 * When using multithreading this value is quite irrelevant,
	 * but do not set it too low since that could add upp to some overhead, 10ms will work good for multithreading */
	public float maxFrameTime = 1F;

	/** Defines the minimum amount of nodes in an area.
	 * If an area has less than this amount of nodes, the area will be flood filled again with the area ID GraphNode.MaxAreaIndex-1,
	 * it shouldn't affect pathfinding in any significant way.\n
	 * If you want to be able to separate areas from one another for some reason (for example to do a fast check to see if a path is at all possible)
	 * you should set this variable to 0.\n
	  * Can be found in A* Inspector-->Settings-->Min Area Size
	  *
	  * \version Since version 3.6, this variable should in most cases be set to 0 since the max number of area indices available has been greatly increased.
	  */
	public int minAreaSize = 0;

	/** Limit graph updates.
	 * If toggled, graph updates will be executed less often (specified by #maxGraphUpdateFreq)
	 */
	public bool limitGraphUpdates = true;

	/** How often should graphs be updated.
	 * If #limitGraphUpdates is true, this defines the minimum amount of seconds between each graph update.
	 */
	public float maxGraphUpdateFreq = 0.2F;

	/** @} */
#endregion

#region DebugVariables
	/** @name Debug Members
	 * @{ */

	/** How many paths has been computed this run. From application start.\n
	 * Debugging variable
	 */
	public static int PathsCompleted = 0;

	/** Debug string from the last completed path.
	 * Will be updated if #logPathResults == PathLog.InGame
	 */
	[System.NonSerialized]
	public string inGameDebugPath;

	/* @} */
#endregion

#region StatusVariables

	/** Number of parallel pathfinders.
	 * Returns the number of concurrent processes which can calculate paths at once.
	 * When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
	 * \see threadInfos
	 * \see IsUsingMultithreading
	 */
	public static int NumParallelThreads {
		get {
			return threadInfos != null ? threadInfos.Length : 0;
		}
	}

	/** Returns whether or not multithreading is used.
	 * \exception System.Exception Is thrown when it could not be decided if multithreading was used or not.
	 * This should not happen if pathfinding is set up correctly.
	 * \note This uses info about if threads are running right now, it does not use info from the settings on the A* object.
	 */
	public static bool IsUsingMultithreading {
		get {
			if (threads != null && threads.Length > 0)
				return true;
			else if (threads != null && threads.Length == 0 && threadEnumerator != null)
				return false;
			else if (Application.isPlaying)
				throw new System.Exception ("Not 'using threading' and not 'not using threading'... Are you sure pathfinding is set up correctly?\nIf scripts are reloaded in unity editor during play this could happen.\n"+
					(threads != null ? ""+threads.Length : "NULL") + " " + (threadEnumerator != null));
			else
				return false;
		}
	}

	/** Returns if any graph updates are waiting to be applied */
	public bool IsAnyGraphUpdatesQueued { get { return graphUpdateQueue != null && graphUpdateQueue.Count > 0; }}

#endregion

#region Callbacks
	/** @name Callbacks */
	 /* Callbacks to pathfinding events.
	 * These allow you to hook in to the pathfinding process.\n
	 * Callbacks can be used like this:
	 * \code
	 * public void Start () {
	 * 	AstarPath.OnPostScan += SomeFunction;
	 * }
	 *
	 * public void SomeFunction (AstarPath active) {
	 * 	//This will be called every time the graphs are scanned
	 * }
	 * \endcode
	*/
	 /** @{ */

	/** Called on Awake before anything else is done.
	  * This is called at the start of the Awake call, right after #active has been set, but this is the only thing that has been done.\n
	  * Use this when you want to set up default settings for an AstarPath component created during runtime since some settings can only be changed in Awake
	  * (such as multithreading related stuff)
	  * \code
	  * //Create a new AstarPath object on Start and apply some default settings
	  * public void Start () {
	  * 	AstarPath.OnAwakeSettings += ApplySettings;
	  * 	AstarPath astar = AddComponent<AstarPath>();
	  * }
	  *
	  * public void ApplySettings () {
	  * 	//Unregister from the delegate
	  * 	AstarPath.OnAwakeSettings -= ApplySettings;
	  *
	  * 	//For example useMultithreading should not be changed after the Awake call
	  * 	//so here's the only place to set it if you create the component during runtime
	  * 	AstarPath.active.useMultithreading = true;
	  * }
	  * \endcode
	  */
	public static System.Action OnAwakeSettings;

	/** Called for each path before searching. Be careful when using multithreading since this will be called from a different thread. */
	public static OnPathDelegate OnPathPreSearch;

	/** Called for each path after searching. Be careful when using multithreading since this will be called from a different thread. */
	public static OnPathDelegate OnPathPostSearch;

	/** Called when \a pathID overflows 65536.
	 * The Pathfinding.CleanupPath65K will be added to the queue, and directly after, this callback will be called.
	 * \note This callback will be cleared every time it is called, so if you want to register to it repeatedly, register to it directly on receiving the callback as well.
	 */
	public static System.Action On65KOverflow;

	/** Will send a callback when it is safe to update the nodes. Register to this with RegisterThreadSafeNodeUpdate
	 * When it is safe is defined as between the path searches.
	 * This callback will only be sent once and is nulled directly after the callback is sent.
	 */
	private static System.Action OnThreadSafeCallback;

	/** \deprecated */
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated;

	/** \deprecated */
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated2;

	/* @} */
#endregion

#region MemoryStructures

	/** Stack containing all waiting graph update queries. Add to this stack by using \link UpdateGraphs \endlink
	 * \see UpdateGraphs
	 */
	Queue<GraphUpdateObject> graphUpdateQueue;

	/** Holds all paths waiting to be calculated */
	ThreadControlQueue pathQueue = new ThreadControlQueue(0);

	/** References to each of the pathfinding threads */
	private static Thread[] threads;

	/** Holds info about each thread.
	 * The first item will hold information about the pathfinding coroutine when not using multithreading.
	 */
	private static PathThreadInfo[] threadInfos = new PathThreadInfo[0];

	/** When no multithreading is used, the IEnumerator is stored here.
	 * When no multithreading is used, a coroutine is used instead. It is not directly called with StartCoroutine
	 * but a separate function has just a while loop which increments the main IEnumerator.
	 * This is done so other functions can step the thread forward at any time, without having to wait for Unity to update it.
	 * \see CalculatePaths
	 * \see CalculatePathsHandler
	 */
	private static IEnumerator threadEnumerator;

	/** Holds all paths which are waiting to be flagged as completed.
	 * \see ReturnPaths
	  */
	private static Pathfinding.Util.LockFreeStack pathReturnStack = new Pathfinding.Util.LockFreeStack();

	/** Holds settings for heuristic optimization.
	 * \see heuristic-opt
	 *
	 * \astarpro
	 */
    public EuclideanEmbedding euclideanEmbedding;

	/** Holds the next node index which has not been used by any previous node.
	 * \see nodeIndexPool
	 */
	private int nextNodeIndex = 1;

	/** Holds indices for nodes that have been destroyed.
	 * To avoid trashing a lot of memory structures when nodes are
	 * frequently deleted and created, node indices are reused.
	 */
	Stack<int> nodeIndexPool = new Stack<int>();

	/** A temporary queue for paths which weren't returned due to large processing time.
	 * When some time limit is exceeded in ReturnPaths, paths are put in this queue until the next frame.
	 *
	 * Paths contain a member called 'next', so this actually forms a linked list.
	 *
	 * \see ReturnPaths
	 */
	private Path pathReturnPop;

#endregion

#region Inner structs and enums

	/** Order type for updating graphs */
	enum GraphUpdateOrder {
		GraphUpdate,
		FloodFill
	}

#endregion

	public static bool isEditor = false;

	/** The last area index which was used.
	 * Used for the \link FloodFill(GraphNode node) FloodFill \endlink function to start flood filling with an unused area.
	 * \see FloodFill(Node node)
	 */
	public uint lastUniqueAreaIndex = 0;

#region ThreadingMembers

	private static readonly System.Object safeUpdateLock = new object();

	/** \todo Should be signaled in OnDestroy */
	private System.Threading.AutoResetEvent graphUpdateAsyncEvent = new System.Threading.AutoResetEvent(false);

	private System.Threading.ManualResetEvent processingGraphUpdatesAsync = new System.Threading.ManualResetEvent(true);

#endregion

	/** The next unused Path ID.
	 * Incremented for every call to GetFromPathPool */
	private ushort nextFreePathID = 1;

	/** Returns tag names.
	 * Makes sure that the tag names array is not null and of length 32.
	 * If it is null or not of length 32, it creates a new array and fills it with 0,1,2,3,4 etc...
	 * \see AstarPath.FindTagNames
	 */
	public string[] GetTagNames () {

		if (tagNames == null || tagNames.Length	!= 32) {
			tagNames = new string[32];
			for (int i=0;i<tagNames.Length;i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}

	/** Returns the next free path ID. If the next free path ID overflows 65535, a cleanup operation is queued
	 * \see Pathfinding.CleanupPath65K
	 */
	public ushort GetNextPathID ()
	{
		if (nextFreePathID == 0) {
			nextFreePathID++;

			//Queue a cleanup operation to zero all path IDs
			//StartPath (new CleanupPath65K ());
			//Debug.Log ("65K cleanup");

			//ushort toBeReturned = nextFreePathID;

			if (On65KOverflow != null) {
				System.Action tmp = On65KOverflow;
				On65KOverflow = null;
				tmp ();
			}

			//return nextFreePathID++;
		}
		return nextFreePathID++;
	}	

	public struct AstarWorkItem {
		/** Init function.
		 * May be null if no initialization is needed.
		 * Will be called once, right before the first call to #update.
		 */
		public System.Action init;

		/** Update function, called once per frame when the work item executes.
		 * Takes a param \a force. If that is true, the work item should try to complete the whole item in one go instead
		 * of spreading it out over multiple frames.
		 * \returns True when the work item is completed.
		 */
		public System.Func<bool, bool> update;

		public AstarWorkItem (System.Func<bool, bool> update) {
			init = null;
			this.update = update;
		}

		public AstarWorkItem (System.Action init, System.Func<bool, bool> update) {
			this.init = init;
			this.update = update;
		}
	}

	private Queue<AstarWorkItem> workItems = new Queue<AstarWorkItem>();

	/* Checks if the OnThreadSafeCallback callback needs to be (and can) be called and if so, does it.
	 * Unpauses pathfinding threads after that.
	 * \see CallThreadSafeCallbacks
	 */
	public void Update () {
		PerformBlockingActions();

		//Process paths
		if (threadEnumerator != null) {
			try {
				threadEnumerator.MoveNext ();
			} catch (System.Exception e) {
				//This will kill pathfinding
				threadEnumerator = null;

				// Queue termination exceptions should be ignored, they are supposed to kill the thread
				if (!(e is ThreadControlQueue.QueueTerminationException)) {

					//Debug.LogException (e);
					//Debug.LogError ("Unhandled exception during pathfinding. Terminating.");
					pathQueue.TerminateReceivers();

					//This will throw an exception supposed to kill the thread
					try {
						pathQueue.PopNoBlock(false);
					} catch {}
				}
			}
		}

		//Return calculated paths
		ReturnPaths(true);
	}

	private void PerformBlockingActions (bool force = false, bool unblockOnComplete = true) {
		if (pathQueue.AllReceiversBlocked) {
			// Return all paths before starting blocking actions (these might change the graph and make returned paths invalid (at least the nodes))
			ReturnPaths (false);

			//This must be called before since otherwise ProcessWorkItems might start pathfinding again
			//if no work items are left to be processed resulting in thread safe callbacks never being called
			if (OnThreadSafeCallback != null) {
				System.Action tmp = OnThreadSafeCallback;
				OnThreadSafeCallback = null;
				tmp ();
			}

			//if (ProcessWorkItems (force) == 2) {
				//At this stage there are no more work items, restart pathfinding threads
				if (unblockOnComplete) {

					// Recalculate
					if ( euclideanEmbedding.dirty ) {
						euclideanEmbedding.RecalculateCosts ();
					}

					pathQueue.Unblock();
				}
			//}
		}

	}

	/** Forces thread safe callbacks to run.
	 * This will force all thread safe callbacks to run immidiately. Or rather, it will block the Unity main thread until callbacks can be called and then issue them.
	 * This will force the pathfinding threads to finish calculate the path they are currently calculating (if any) and then pause.
	 * When all threads have paused, thread safe callbacks will be called (which can be e.g graph updates).
	 *
	 * \warning Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	 * But you probably wont have to worry about that
	 *
	 * \note This is almost (note almost) identical to FlushGraphUpdates, but added for more appropriate name.
	 */
	public void FlushThreadSafeCallbacks () {

		//No callbacks? why wait?
		if (OnThreadSafeCallback == null) {
			return;
		}

		BlockUntilPathQueueBlocked();
		PerformBlockingActions();
	}


	/** Calculates number of threads to use.
	 * If \a count is not Automatic, simply returns \a count casted to an int.
	 * \returns An int specifying how many threads to use, 0 means a coroutine should be used for pathfinding instead of a separate thread.
	 *
	 * If \a count is set to Automatic it will return a value based on the number of processors and memory for the current system.
	 * If memory is <= 512MB or logical cores are <= 1, it will return 0. If memory is <= 1024 it will clamp threads to max 2.
	 * Otherwise it will return the number of logical cores clamped to 6.
	 *
	 * When running on WebGL this method always returns 0
	 */
	public static int CalculateThreadCount (ThreadCount count) {
#if UNITY_WEBGL
		return 0;
#else
		if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad) {
			int logicalCores = Mathf.Max (1,SystemInfo.processorCount);
			int memory = SystemInfo.systemMemorySize;

			if ( memory <= 0 ) {
				//Debug.LogError ("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
				memory = 1024;
			}

			if ( logicalCores <= 1) return 0;
			if ( memory <= 512) return 0;

			return 1;
		} else {
			return (int)count > 0 ? 1 : 0;
		}
#endif
	}

	/** Sets up all needed variables and scans the graphs.
	 * Calls Initialize, starts the ReturnPaths coroutine and scans all graphs.
	 * Also starts threads if using multithreading
	 * \see #OnAwakeSettings
	 */
	public void Init () {
		//Very important to set this. Ensures the singleton pattern holds
		active = this;
        euclideanEmbedding = new EuclideanEmbedding();
		if (OnAwakeSettings != null) {
			OnAwakeSettings ();
		}

		//To make sure all graph modifiers have been enabled before scan (to avoid script run order issues)
		//GraphModifier.FindAllModifiers ();
		//RelevantGraphSurface.FindAllGraphSurfaces ();

		//int numThreads = CalculateThreadCount (threadCount);
        //int numThreads = 1;
        //int numThreads = 0;

		// Trying to prevent simple modding to add support for more than one thread
        //if ( numThreads > 1 ) {
        //    threadCount = ThreadCount.One;
        //    numThreads = 1;
        //}

        //threads = new Thread[numThreads];

        ////Thread info, will contain at least one item since the coroutine "thread" is thought of as a real thread in this case
        //threadInfos = new PathThreadInfo[System.Math.Max(numThreads,1)];

        ////Set up path queue with the specified number of receivers
        //pathQueue = new ThreadControlQueue(threadInfos.Length);

        //for (int i=0;i<threadInfos.Length;i++) {
        //    //threadInfos[i] = new PathThreadInfo(i,this,new PathHandler(i, threadInfos.Length));
        //    threadInfos[i] = new PathThreadInfo(i, this);
        //}

        ////Start coroutine if not using multithreading
        //if (numThreads == 0) {
        //    threadEnumerator = CalculatePaths (threadInfos[0]);
        //} else {
        //    threadEnumerator = null;
        //}

        threads = new Thread[1];
        threadInfos = new PathThreadInfo[1];
        pathQueue = new ThreadControlQueue(1);
        threadInfos[0] = new PathThreadInfo(0, this);
        threadEnumerator = null;

//#if !UNITY_WEBGL
        threads[0] = new Thread(new ParameterizedThreadStart(CalculatePathsThreaded));
        threads[0].Name = "Pathfinding Thread 0";
        threads[0].IsBackground = true;
        threads[0].Start(threadInfos[0]);

        //for (int i=0;i<threads.Length;i++) {
        //    threads[i] = new Thread (new ParameterizedThreadStart (CalculatePathsThreaded));
        //    threads[i].Name = "Pathfinding Thread " + i;
        //    threads[i].IsBackground = true;
        //}

        ////Start pathfinding threads
        //for (int i=0;i<threads.Length;i++) {
        //    threads[i].Start (threadInfos[i]);
        //}

//        if ( numThreads != 0 ) {
//            graphUpdateThread = new Thread (new ParameterizedThreadStart(ProcessGraphUpdatesAsync));
//            graphUpdateThread.IsBackground = true;

//            // Set the thread priority for graph updates
//            // Unless compiling for windows store or windows phone which does not support it
//#if !UNITY_WINRT
//            graphUpdateThread.Priority = System.Threading.ThreadPriority.Lowest;
//#endif
//            graphUpdateThread.Start (this);
//        }
//#endif

		Initialize ();


		// Flush work items, possibly added in initialize to load graph data
		//FlushWorkItems();

		euclideanEmbedding.dirty = true;

	}

    //public void SetActiveAstar()
    //{
    //    //astarData = allAstarData[level];
    //    active = this;
    //}

    //testing func, to be removed
    //public void TestComputePath(Path p)
    //{
    //    //CalculatePathsThreaded(threadInfos[0]);
    //    PathThreadInfo threadInfo = threadInfos[0];
    //    AstarPath astar = threadInfo.astar;

    //    //Initialize memory for this thread
    //    PathHandler runData = threadInfo.runData;

    //    if (runData.nodes == null)
    //        throw new System.NullReferenceException ("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");

    //    //Max number of ticks before yielding/sleeping
    //    long maxTicks = (long)(astar.maxFrameTime*10000);
    //    long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;



    //        //The path we are currently calculating
    //        //Path p = astar.pathQueue.Pop();

    //        //Max number of ticks we are allowed to continue working in one run
    //        //One tick is 1/10000 of a millisecond
    //        //maxTicks = (long)(astar.maxFrameTime*10000);

    //            //Trying to prevent simple modding to allow more than one thread
    //            //if ( threadInfo.threadIndex > 0 ) {
    //            //    throw new System.Exception ("Thread Error");
    //            //}

    //            p.PrepareBase (runData);

    //            //Now processing the path
    //            //Will advance to Processing
    //            p.AdvanceState (PathState.Processing);

    //            //Prepare the path
    //            p.Prepare ();

    //            if (!p.IsDone()) {

    //                //Initialize the path, now ready to begin search
    //                p.Initialize ();

    //                //The error can turn up in the Init function
    //                while (!p.IsDone ()) {
    //                    p.CalculateStep (targetTick);
    //                    p.searchIterations++;

    //                    //If the path has finished calculation, we can break here directly instead of sleeping
    //                    if (p.IsDone ()) break;

    //                    //Cancel function (and thus the thread) if no more paths should be accepted.
    //                    //This is done when the A* object is about to be destroyed
    //                    //The path is returned and then this function will be terminated
    //                    if (astar.pathQueue.IsTerminating) {
    //                        p.Error ();
    //                    }
    //                }
    //            }

    //            // Cleans up node tagging and other things
    //            p.Cleanup ();


    //            if ( p.immediateCallback != null ) p.immediateCallback (p);

    //            if (OnPathPostSearch != null) {
    //                OnPathPostSearch (p);
    //            }

    //            //Push the path onto the return stack
    //            //It will be detected by the main Unity thread and returned as fast as possible (the next late update hopefully)
    //            pathReturnStack.Push (p);

    //            //Will advance to ReturnQueue
    //            p.AdvanceState (PathState.ReturnQueue);
			
		
    //}


    public void LoadGraphData(string level, string path)
    {
        if (!allAstarData.ContainsKey(level))
        {
            allAstarData.Add(level, new AstarData());            
        }
        allAstarData[level].level = level;
        if (!allPathHandler.ContainsKey(level))
        {
            allPathHandler.Add(level, new PathHandler(0, 1));
        }
        if (path != "")
        {
            byte[] bytes;
            try
            {
                bytes = Pathfinding.Serialization.AstarSerializer.LoadFromFile(path);
            }
            catch (System.Exception e)
            {
                //Debug.LogError("Could not load from file at '" + path + "'\n" + e);
                bytes = null;
            }

            if (bytes != null)
            {                
                //astarData.DeserializeGraphs(bytes);
                allAstarData[level].DeserializeGraphs(bytes);
            }
        }
        //allAstarData.Add(level, astarData);
    }

	/** Does simple error checking.
	 */
	internal void VerifyIntegrity (AstarData ad) {

        if (active != this)
        {
            throw new System.Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
        }

		if (ad == null) {
			throw new System.NullReferenceException ("AstarData is null... Astar not set up correctly?");
		}

		if (ad.graphs == null) {
			ad.graphs = new NavGraph[0];
		}

		if (pathQueue == null && !Application.isPlaying) {
			pathQueue = new ThreadControlQueue(0);
		}
		if (threadInfos == null && !Application.isPlaying) {
			threadInfos = new PathThreadInfo[0];
		}

		//Dummy if, the getter does some error checking
		if (IsUsingMultithreading) {
		}
	}

	/** Internal method to make sure #active is set to this object and that #astarData is not null.
	 * Also calls OnEnable for the #colorSettings and initializes astarData.userConnections if it wasn't initialized before
	 *
	 * \note This is mostly for use internally by the system.
	 */
	public void SetUpReferences () {
		active = this;
        //if (astarData == null) {
        //    astarData = new AstarData ();
        //}
	}

	/** Initializes various variables.
	 * \link SetUpReferences Sets up references \endlink,
	 * Searches for graph types, calls Awake on #astarData and on all graphs
	 *
	 * \see AstarData.FindGraphTypes
	 * \see SetUpReferences
	 */
	void Initialize () {

        //AstarProfiler.InitializeFastProfile (new string [14] {
        //    "Prepare", 			//0
        //    "Initialize",		//1
        //    "CalculateStep",	//2
        //    "Trace",			//3
        //    "Open",				//4
        //    "UpdateAllG",		//5
        //    "Add",				//6
        //    "Remove",			//7
        //    "PreProcessing",	//8
        //    "Callback",			//9
        //    "Overhead",			//10
        //    "Log",				//11
        //    "ReturnPaths",		//12
        //    "PostPathCallback"	//13
        //});

		SetUpReferences ();		
	}

	/** Clears up variables and other stuff, destroys graphs.
	 * Note that when destroying an AstarPath object, all static variables such as callbacks will be cleared.
	 */
	void OnDestroy () {
		if ( active != this ) return;

		BlockUntilPathQueueBlocked();

		euclideanEmbedding.dirty = false;
		//FlushWorkItems (false, true);

		//Don't accept any more path calls to this AstarPath instance.
		//This will cause all eventual multithreading threads to exit
		pathQueue.TerminateReceivers();

		//Resume graph update thread, will cause it to terminate
		graphUpdateAsyncEvent.Set();

		//Try to join pathfinding threads
		if (threads != null) {
			for (int i=0;i<threads.Length;i++) {
				if (!threads[i].Join (50)) {
					threads[i].Abort ();
				}
			}
		}

		//Return all paths
		ReturnPaths (false);
		//Just in case someone happened to request a path in ReturnPath() (even though they should get canceled)
		pathReturnStack.PopAll ();

		//Clean graphs up
		//astarData.OnDestroy ();
        foreach (KeyValuePair<string, AstarData> kv in allAstarData)
        {
            kv.Value.OnDestroy();
        }

		graphUpdateQueue = null;

		//Clear all callbacks		
		OnAwakeSettings			= null;
		OnPathPreSearch			= null;
		OnPathPostSearch		= null;
		On65KOverflow			= null;
		OnThreadSafeCallback	= null;

		threads = null;
		threadInfos = null;

		PathsCompleted = 0;

		active = null;

	}

#region ScanMethods

    ///** Floodfills starting from the specified node */
    //public void FloodFill (GraphNode seed) {
    //    FloodFill (seed, lastUniqueAreaIndex+1);
    //    lastUniqueAreaIndex++;
    //}

    ///** Floodfills starting from 'seed' using the specified area */
    //public void FloodFill (GraphNode seed, uint area) {

    //    if (area > GraphNode.MaxAreaIndex) {
    //        //Debug.LogError ("Too high area index - The maximum area index is " + GraphNode.MaxAreaIndex);
    //        return;
    //    }

    //    if (area < 0) {
    //        //Debug.LogError ("Too low area index - The minimum area index is 0");
    //        return;
    //    }

    //    if (floodStack == null) {
    //        floodStack = new Stack<GraphNode> (1024);
    //    }

    //    Stack<GraphNode> stack = floodStack;

    //    stack.Clear ();

    //    stack.Push (seed);
    //    seed.Area = (uint)area;

    //    while (stack.Count > 0) {
    //        stack.Pop ().FloodFill (stack,(uint)area);
    //    }

    //}

    ///** Floodfills all graphs and updates areas for every node.
    // * The different colored areas that you see in the scene view when looking at graphs
    // * are called just 'areas', this method calculates which nodes are in what areas.
    // * \see Pathfinding.Node.area
    // */
    //[ContextMenu("Flood Fill Graphs")]
    //public void FloodFill () {
    //    queuedWorkItemFloodFill = false;


    //    if (astarData.graphs == null) {
    //        return;
    //    }

    //    uint area = 0;

    //    lastUniqueAreaIndex = 0;

    //    if (floodStack == null) {
    //        floodStack = new Stack<GraphNode> (1024);
    //    }

    //    Stack<GraphNode> stack = floodStack;

    //    for (int i=0;i<graphs.Length;i++) {
    //        NavGraph graph = graphs[i];

    //        if (graph != null) {
    //            graph.GetNodes (delegate (GraphNode node) {
    //                node.Area = 0;
    //                return true;
    //            });
    //        }
    //    }

    //    int smallAreasDetected = 0;

    //    bool warnAboutAreas = false;

    //    List<GraphNode> smallAreaList = Pathfinding.Util.ListPool<GraphNode>.Claim();//new List<GraphNode>();

    //    for (int i=0;i<graphs.Length;i++) {

    //        NavGraph graph = graphs[i];

    //        if (graph == null) continue;

    //        //for (int j=0;j<graph.nodes.Length;j++)
    //        GraphNodeDelegateCancelable del = delegate (GraphNode node) {
    //            if (node.Walkable && node.Area == 0) {

    //                area++;

    //                uint thisArea = area;

    //                if (area > GraphNode.MaxAreaIndex) {
    //                    if ( smallAreaList.Count > 0 ) {
    //                        GraphNode smallOne = smallAreaList[smallAreaList.Count-1];
    //                        thisArea = smallOne.Area;
    //                        smallAreaList.RemoveAt (smallAreaList.Count-1);

    //                        //Flood fill the area again with area ID GraphNode.MaxAreaIndex-1, this identifies a small area
    //                        stack.Clear ();

    //                        stack.Push (smallOne);
    //                        smallOne.Area = GraphNode.MaxAreaIndex;

    //                        while (stack.Count > 0) {
    //                            stack.Pop ().FloodFill (stack,GraphNode.MaxAreaIndex);
    //                        }

    //                        smallAreasDetected++;
    //                    } else {
    //                        // Forced to consider this a small area
    //                        area--;
    //                        thisArea = area;
    //                        warnAboutAreas = true;
    //                    }
    //                }

    //                stack.Clear ();

    //                stack.Push (node);

    //                int counter = 1;

    //                node.Area = thisArea;

    //                while (stack.Count > 0) {
    //                    counter++;
    //                    stack.Pop ().FloodFill (stack,thisArea);
    //                }

    //                if (counter < minAreaSize) {
    //                    smallAreaList.Add ( node );
    //                }
    //            }
    //            return true;
    //        };

    //        graph.GetNodes (del);
    //    }

    //    lastUniqueAreaIndex = area;

    //    if (warnAboutAreas) {
    //        //Debug.LogError ("Too many areas - The maximum number of areas is " + GraphNode.MaxAreaIndex +". Try raising the A* Inspector -> Settings -> Min Area Size value. Enable the optimization ASTAR_MORE_AREAS under the Optimizations tab.");
    //    }

    //    Pathfinding.Util.ListPool<GraphNode>.Release ( smallAreaList );

    //}

	/** Returns a new global node index.
	 * \warning This method should not be called directly. It is used by the GraphNode constructor.
	 */
	public int GetNewNodeIndex () {
		if (nodeIndexPool.Count > 0) return nodeIndexPool.Pop();
		return nextNodeIndex++;
	}

	/** Initializes temporary path data for a node.
	 * \warning This method should not be called directly. It is used by the GraphNode constructor.
	 */
	public void InitializeNode (GraphNode node, string level) {
		if (!pathQueue.AllReceiversBlocked) throw new System.Exception ("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update");
        if (!allPathHandler.ContainsKey(level)) throw new System.Exception("InitializeNode: Unkown level:" + level);
		if (threadInfos == null) threadInfos = new PathThreadInfo[0];

        allPathHandler[level].InitializeNode(node);        
		//for (int i=0;i<threadInfos.Length;i++) {
			//threadInfos[i].runData.InitializeNode (node);            
		//}
	}

	/** Destroyes the given node.
	 * This is to be called after the node has been disconnected from the graph so that it cannot be reached from any other nodes.
	 * It should only be called during graph updates, that is when the pathfinding threads are either not running or paused.
	 *
	 * \warning This method should not be called by user code. It is used internally by the system.
	 */
	public void DestroyNode (GraphNode node) {
		if (node.NodeIndex == -1) return;
        string level = node.level;
        if (!allPathHandler.ContainsKey(level)) return;
		nodeIndexPool.Push(node.NodeIndex);

		if (threadInfos == null) threadInfos = new PathThreadInfo[0];

        allPathHandler[level].DestroyNode(node);
        //for (int i=0;i<threadInfos.Length;i++) {
        //    threadInfos[i].runData.DestroyNode (node);
        //}
	}

	/** Blocks until all pathfinding threads are paused and blocked.
	 * A call to pathQueue.Unblock is required to resume pathfinding calculations. However in
	 * most cases you should never unblock the path queue, instead let the pathfinding scripts do that in the next update.
	 * Unblocking the queue when other tasks (e.g graph updates) are running can interfere and cause invalid graphs.
	 *
	 * \note In most cases this should not be called from user code.
	 */
	public void BlockUntilPathQueueBlocked () {
		if (pathQueue == null) return;

		pathQueue.Block();

		while (!pathQueue.AllReceiversBlocked) {
			if (IsUsingMultithreading) {
				Thread.Sleep(1);
			} else {
				threadEnumerator.MoveNext ();
			}
		}
	}
#endregion

	/** Wait for the specified path to be calculated.
	 * Normally it takes a few frames for a path to get calculated and returned.
	 * This function will ensure that the path will be calculated when this function returns
	 * and that the callback for that path has been called.
	 *
	 * \note Do not confuse this with Pathfinding.Path.WaitForPath. This one will halt all operations until the path has been calculated
	 * while Pathfinding.Path.WaitForPath will wait using yield until it has been calculated.
	 *
	 * If requesting a lot of paths in one go and waiting for the last one to complete,
	 * it will calculate most of the paths in the queue (only most if using multithreading, all if not using multithreading).
	 *
	 * Use this function only if you really need to.
	 * There is a point to spreading path calculations out over several frames.
	 * It smoothes out the framerate and makes sure requesting a large
	 * number of paths at the same time does not cause lag.
	 *
	 * \note Graph updates and other callbacks might get called during the execution of this function.
	 *
	 * When the pathfinder is shutting down. I.e in OnDestroy, this function will not do anything.
	 *
	 * \param p The path to wait for. The path must be started, otherwise an exception will be thrown.
	 *
	 * \throws Exception if pathfinding is not initialized properly for this scene (most likely no AstarPath object exists)
	 * or if the path has not been started yet.
	 * Also throws an exception if critical errors occur such as when the pathfinding threads have crashed (which should not happen in normal cases).
	 * This prevents an infinite loop while waiting for the path.
	 *
	 * \see Pathfinding.Path.WaitForPath
	 */
    //public static void WaitForPath (Path p) {

    //    if (active == null)
    //        throw new System.Exception ("Pathfinding is not correctly initialized in this scene (yet?). " +
    //            "AstarPath.active is null.\nDo not call this function in Awake");

    //    if (p == null) throw new System.ArgumentNullException ("Path must not be null");

    //    if (active.pathQueue.IsTerminating) return;

    //    if (p.GetState () == PathState.Created){
    //        throw new System.Exception ("The specified path has not been started yet.");
    //    }

    //    waitForPathDepth++;

    //    if (waitForPathDepth == 5) {
    //        //Debug.LogError ("You are calling the WaitForPath function recursively (maybe from a path callback). Please don't do this.");
    //    }

    //    if (p.GetState() < PathState.ReturnQueue) {
    //        if (IsUsingMultithreading) {

    //            while (p.GetState() < PathState.ReturnQueue) {
    //                if (active.pathQueue.IsTerminating) {
    //                    waitForPathDepth--;
    //                    throw new System.Exception ("Pathfinding Threads seems to have crashed.");
    //                }

    //                //Wait for threads to calculate paths
    //                Thread.Sleep(1);
    //                active.PerformBlockingActions();
    //            }
    //        } else {
    //            while (p.GetState() < PathState.ReturnQueue) {
    //                if (active.pathQueue.IsEmpty && p.GetState () != PathState.Processing) {
    //                    waitForPathDepth--;
    //                    throw new System.Exception ("Critical error. Path Queue is empty but the path state is '" + p.GetState() + "'");
    //                }

    //                //Calculate some paths
    //                threadEnumerator.MoveNext ();
    //                active.PerformBlockingActions();
    //            }
    //        }
    //    }

    //    active.ReturnPaths (false);

    //    waitForPathDepth--;
    //}

	/** Will send a callback when it is safe to update nodes. This is defined as between the path searches.
	  * This callback will only be sent once and is nulled directly after the callback has been sent.
	  * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	  * Not performance as in it will use much CPU power,
	  * but performance as in the number of paths per second will probably go down (though your framerate might actually increase a tiny bit)
	  *
	  * You should only call this function from the main unity thread (i.e normal game code).
	  *
	  * \note The threadSafe parameter has been deprecated
	  * \deprecated
	  */
    //[System.Obsolete ("The threadSafe parameter has been deprecated")]
    //public static void RegisterSafeUpdate (System.Action callback, bool threadSafe) {
    //    RegisterSafeUpdate ( callback );
    //}

	/** Will send a callback when it is safe to update nodes. This is defined as between the path searches.
	  * This callback will only be sent once and is nulled directly after the callback has been sent.
	  * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	  * Not performance as in it will use much CPU power,
	  * but performance as in the number of paths per second will probably go down (though your framerate might actually increase a tiny bit)
	  *
	  * You should only call this function from the main unity thread (i.e normal game code).
	  *
	  * \code
Node node = AstarPath.active.GetNearest (transform.position).node;
AstarPath.RegisterSafeUpdate (delegate () {
	node.walkable = false;
});
\endcode

\code
Node node = AstarPath.active.GetNearest (transform.position).node;
AstarPath.RegisterSafeUpdate (delegate () {
	node.position = (Int3)transform.position;
});
\endcode
	  *
	  *
	  */
    //public static void RegisterSafeUpdate (System.Action callback) {
    //    if (callback == null || !Application.isPlaying) {
    //        return;
    //    }

    //    // If all pathfinding threads are already blocked
    //    // we might as well just call the callback immediately
    //    if (active.pathQueue.AllReceiversBlocked) {
    //        // We need to lock here since we cannot be sure that this is the Unity Thread
    //        // and therefore we cannot be sure that some other thread will not unblock the queue while we are processing the callback
    //        active.pathQueue.Lock();
    //        try {
    //            // Check again
    //            if (active.pathQueue.AllReceiversBlocked) {
    //                callback ();
    //                return;
    //            }
    //            // If that check failed, it will fall back to the code below
    //        } finally {
    //            active.pathQueue.Unlock();
    //        }
    //    }

    //    // Lock while modifying the callback
    //    lock (safeUpdateLock) {
    //        // OnSafeCallback has been deprecated

    //        OnThreadSafeCallback += callback;
    //    }

    //    // Block path queue so that the above callbacks may be called
    //    active.pathQueue.Block();
    //}

	/** Blocks the path queue so that e.g work items can be performed */
	void InterruptPathfinding () {
		pathQueue.Block();
	}

	/** Puts the Path in queue for calculation.
	  * The callback specified when constructing the path will be called when the path has been calculated.
	  * Usually you should use the Seeker component instead of calling this function directly.
	  *
	  * \param p The path that should be put in queue for calculation
	  * \param pushToFront If true, the path will be pushed to the front of the queue, bypassing all waiting paths and making it the next path to be calculated.
	  * This can be useful if you have a path which you want to prioritize over all others. Be careful to not overuse it though.
	  * If too many paths are put in the front of the queue often, this can lead to normal paths having to wait a very long time before being calculated.
	  */
    public static void StartPath(string level, Path p, bool pushToFront = false)
    {
        if (!allAstarData.ContainsKey(level)) return;
        AstarData adata = allAstarData[level];
        if (System.Object.ReferenceEquals(active, null))
        {
            return;
        }

        if (p.GetState() != PathState.Created)
        {
            throw new System.Exception("The path has an invalid state. Expected " + PathState.Created + " found " + p.GetState() + "\n" +
                "Make sure you are not requesting the same path twice");
        }

        if (active.pathQueue.IsTerminating)
        {
            p.Error();
            return;
        }

        if (adata.graphs == null || adata.graphs.Length == 0)
        {
            //Debug.LogError ("There are no graphs in the scene");
            p.Error();
            //Debug.LogError (p.errorLog);
            return;
        }

        p.Claim(active);


        //Will increment to PathQueue
        p.AdvanceState(PathState.PathQueue);
        if (pushToFront)
        {
            active.pathQueue.PushFront(p);
        }
        else
        {
            active.pathQueue.Push(p);
        }
    }

    //public void StartPath(Path p, bool pushToFront = false)
    //{

    //    if (p.GetState() != PathState.Created)
    //    {
    //        throw new System.Exception("The path has an invalid state. Expected " + PathState.Created + " found " + p.GetState() + "\n" +
    //            "Make sure you are not requesting the same path twice");
    //    }

    //    if (pathQueue.IsTerminating)
    //    {
    //        p.Error();
    //        return;
    //    }

    //    if (graphs == null || graphs.Length == 0)
    //    {
    //        //Debug.LogError ("There are no graphs in the scene");
    //        p.Error();
    //        //Debug.LogError (p.errorLog);
    //        return;
    //    }

    //    p.Claim(this);


    //    //Will increment to PathQueue
    //    p.AdvanceState(PathState.PathQueue);
    //    if (pushToFront)
    //    {
    //        pathQueue.PushFront(p);
    //    }
    //    else
    //    {
    //        pathQueue.Push(p);
    //    }
    //}


	/** Terminates pathfinding threads when the application quits.
	 */
	public void OnApplicationQuit () {

		OnDestroy ();

#if !UNITY_WEBPLAYER
		if (threads == null) return;
		//Unity webplayer does not support Abort (even though it supports starting threads). Hope that UnityPlayer aborts the threads
		for (int i=0;i<threads.Length;i++) {
			if ( threads[i] != null && threads[i].IsAlive ) threads[i].Abort ();
		}
#endif
	}

#region MainThreads

	/** Returns all paths in the return stack.
	  * Paths which have been processed are put in the return stack.
	  * This function will pop all items from the stack and return them to e.g the Seeker requesting them.
	  *
	  * \param timeSlice Do not return all paths at once if it takes a long time, instead return some and wait until the next call.
	  */
	public void ReturnPaths (bool timeSlice) {

		//Pop all items from the stack
		Path p = pathReturnStack.PopAll ();

		if(pathReturnPop == null) {
			pathReturnPop = p;
		} else {
			Path tail = pathReturnPop;
			while (tail.next != null) tail = tail.next;
			tail.next = p;
		}

		//Hard coded limit on 1.0 ms
		long targetTick = timeSlice ? System.DateTime.UtcNow.Ticks + 1 * 10000 : 0;

		int counter = 0;
		//Loop through the linked list and return all paths
		while (pathReturnPop != null) {

			//Move to the next path
			Path prev = pathReturnPop;
			pathReturnPop = pathReturnPop.next;

			/* Remove the reference to prevent possible memory leaks
			If for example the first path computed was stored somewhere,
			it would through the linked list contain references to all comming paths to be computed,
			and thus the nodes those paths searched.
			That adds up to a lot of memory not being released */
			prev.next = null;

			//Return the path
			prev.ReturnPath ();

			//Will increment to Returned
			//However since multithreading is annoying, it might be set to ReturnQueue for a small time until the pathfinding calculation
			//thread advanced the state as well
			prev.AdvanceState (PathState.Returned);

			prev.ReleaseSilent (this);

			counter++;
			//At least 5 paths will be returned, even if timeSlice is enabled
			if (counter > 5 && timeSlice) {
				counter = 0;
				if (System.DateTime.UtcNow.Ticks >= targetTick) {
					return;
				}
			}
		}
	}

	/** Main pathfinding method (multithreaded).
	 * This method will calculate the paths in the pathfinding queue when multithreading is enabled.
	 *
	 * \see CalculatePaths
	 * \see StartPath
	 *
	 * \astarpro
	 */
	private static void CalculatePathsThreaded (System.Object _threadInfo) {

		PathThreadInfo threadInfo;

		try {
			threadInfo = (PathThreadInfo)_threadInfo;
		} catch (System.Exception e) {
			//Debug.LogError ("Arguments to pathfinding threads must be of type ThreadStartInfo\n"+e);
			throw new System.ArgumentException ("Argument must be of type ThreadStartInfo",e);
		}

		AstarPath astar = threadInfo.astar;

		try {

			//Initialize memory for this thread
            //PathHandler runData = threadInfo.runData;

            //if (runData.nodes == null)
            //    throw new System.NullReferenceException ("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");

			//Max number of ticks before yielding/sleeping
			long maxTicks = (long)(astar.maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
            PathHandler runData;
			while (true) {

				//The path we are currently calculating
				Path p = astar.pathQueue.Pop();
                ABPath abp = p as ABPath;
                if(abp == null) throw new System.Exception("Only ABPath is allowed!! why got other type??");
				//Max number of ticks we are allowed to continue working in one run
				//One tick is 1/10000 of a millisecond
				maxTicks = (long)(astar.maxFrameTime*10000);

				//Trying to prevent simple modding to allow more than one thread
				if ( threadInfo.threadIndex > 0 ) {
					throw new System.Exception ("Thread Error");
				}

				//AstarProfiler.StartFastProfile (0);
                if(!allPathHandler.ContainsKey(abp.level)) throw new System.Exception("Unknown level:["+abp.level+"]");
                runData = allPathHandler[abp.level];
				p.PrepareBase (runData);

				//Now processing the path
				//Will advance to Processing
				p.AdvanceState (PathState.Processing);

				//Call some callbacks
				if (OnPathPreSearch != null) {
					OnPathPreSearch (p);
				}

				//Tick for when the path started, used for calculating how long time the calculation took
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;

				//Prepare the path
				p.Prepare ();

				//AstarProfiler.EndFastProfile (0);

				if (!p.IsDone()) {

					//AstarProfiler.StartFastProfile (1);

					//Initialize the path, now ready to begin search
					p.Initialize ();

					//AstarProfiler.EndFastProfile (1);

					//The error can turn up in the Init function
					while (!p.IsDone ()) {
						//Do some work on the path calculation.
						//The function will return when it has taken too much time
						//or when it has finished calculation
						//AstarProfiler.StartFastProfile (2);
						p.CalculateStep (targetTick);
						p.searchIterations++;

						//AstarProfiler.EndFastProfile (2);

						//If the path has finished calculation, we can break here directly instead of sleeping
						if (p.IsDone ()) 
                            break;

						//Yield/sleep so other threads can work
						totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
						Thread.Sleep(0);
						startTicks = System.DateTime.UtcNow.Ticks;

						targetTick = startTicks + maxTicks;

						//Cancel function (and thus the thread) if no more paths should be accepted.
						//This is done when the A* object is about to be destroyed
						//The path is returned and then this function will be terminated
						if (astar.pathQueue.IsTerminating) {
							p.Error ();
						}
					}

					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;

				}

				// Cleans up node tagging and other things
				p.Cleanup ();

				//AstarProfiler.StartFastProfile (9);


				if ( p.immediateCallback != null ) p.immediateCallback (p);

				if (OnPathPostSearch != null) {
					OnPathPostSearch (p);
				}

				//Push the path onto the return stack
				//It will be detected by the main Unity thread and returned as fast as possible (the next late update hopefully)
				//pathReturnStack.Push (p);

                p.ReturnPath();

				//Will advance to ReturnQueue
				p.AdvanceState (PathState.ReturnQueue);

				//AstarProfiler.EndFastProfile (9);

				//Wait a bit if we have calculated a lot of paths
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					Thread.Sleep(1);
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		} catch (System.Exception e) {
#if !NETFX_CORE
			if (e is System.Threading.ThreadAbortException || e is ThreadControlQueue.QueueTerminationException)
#else
			if (e is ThreadControlQueue.QueueTerminationException)
#endif
			{
				return;
			}
			//Debug.LogException (e);
			//Debug.LogError ("Unhandled exception during pathfinding. Terminating.");
			//Unhandled exception, kill pathfinding
			astar.pathQueue.TerminateReceivers();
		}

		//Debug.LogError ("Error : This part should never be reached.");
		astar.pathQueue.ReceiverTerminated ();
	}

	/** Main pathfinding method.
	 * This method will calculate the paths in the pathfinding queue.
	 *
	 * \see CalculatePathsThreaded
	 * \see StartPath
	 */
	private static IEnumerator CalculatePaths (System.Object _threadInfo) {
        return null; //this func not used
        //PathThreadInfo threadInfo;
        //try {
        //    threadInfo = (PathThreadInfo)_threadInfo;
        //} catch (System.Exception e) {
        //    //Debug.LogError ("Arguments to pathfinding threads must be of type ThreadStartInfo\n"+e);
        //    throw new System.ArgumentException ("Argument must be of type ThreadStartInfo",e);
        //}

        //int numPaths = 0;

        ////Initialize memory for this thread
        //PathHandler runData = threadInfo.runData;

        //AstarPath astar = threadInfo.astar;

        //if (runData.nodes == null)
        //    throw new System.NullReferenceException ("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\n" +
        //        "threadInfo is an argument to the thread functions");

        ////Max number of ticks before yielding/sleeping
        //long maxTicks = (long)(active.maxFrameTime*10000);
        ////long maxTicks = (long)(maxFrameTime * 10000);
        //long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;

        //while (true) {

        //    //The path we are currently calculating
        //    Path p = null;

        //    AstarProfiler.StartProfile ("Path Queue");

        //    //Try to get the next path to be calculated
        //    bool blockedBefore = false;
        //    while (p == null) {
        //        try {
        //            p = astar.pathQueue.PopNoBlock(blockedBefore);
        //            if (p == null) {
        //                blockedBefore = true;
        //            }
        //        } catch (ThreadControlQueue.QueueTerminationException) {
        //            yield break;
        //        }

        //        if (p == null) {
        //            AstarProfiler.EndProfile ();
        //            yield return null;
        //            AstarProfiler.StartProfile ("Path Queue");
        //        }
        //    }


        //    AstarProfiler.EndProfile ();

        //    AstarProfiler.StartProfile ("Path Calc");

        //    //Max number of ticks we are allowed to continue working in one run
        //    //One tick is 1/10000 of a millisecond
        //    maxTicks = (long)(active.maxFrameTime*10000);
        //    //maxTicks = (long)(maxFrameTime * 10000);

        //    p.PrepareBase (runData);

        //    //Now processing the path
        //    //Will advance to Processing
        //    p.AdvanceState (PathState.Processing);

        //    // Call some callbacks
        //    // It needs to be stored in a local variable to avoid race conditions
        //    var tmpOnPathPreSearch = OnPathPreSearch;
        //    if (tmpOnPathPreSearch != null) tmpOnPathPreSearch (p);

        //    numPaths++;

        //    //Tick for when the path started, used for calculating how long time the calculation took
        //    long startTicks = System.DateTime.UtcNow.Ticks;
        //    long totalTicks = 0;

        //    AstarProfiler.StartFastProfile(8);

        //    AstarProfiler.StartFastProfile(0);
        //    //Prepare the path
        //    AstarProfiler.StartProfile ("Path Prepare");
        //    p.Prepare ();
        //    AstarProfiler.EndProfile ("Path Prepare");
        //    AstarProfiler.EndFastProfile (0);

        //    // Check if the Prepare call caused the path to complete
        //    // If this happens the path usually failed
        //    if (!p.IsDone()) {

        //        //Initialize the path, now ready to begin search
        //        AstarProfiler.StartProfile ("Path Initialize");
        //        p.Initialize ();
        //        AstarProfiler.EndProfile ();

        //        //The error can turn up in the Init function
        //        while (!p.IsDone ()) {
        //            // Do some work on the path calculation.
        //            // The function will return when it has taken too much time
        //            // or when it has finished calculation
        //            AstarProfiler.StartFastProfile(2);

        //            AstarProfiler.StartProfile ("Path Calc Step");
        //            p.CalculateStep (targetTick);
        //            AstarProfiler.EndFastProfile(2);
        //            p.searchIterations++;

        //            AstarProfiler.EndProfile ();

        //            // If the path has finished calculation, we can break here directly instead of sleeping
        //            // Improves latency
        //            if (p.IsDone ()) break;

        //            AstarProfiler.EndFastProfile(8);
        //            totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
        //            // Yield/sleep so other threads can work

        //            AstarProfiler.EndProfile ();
        //            yield return null;
        //            AstarProfiler.StartProfile ("Path Calc");

        //            startTicks = System.DateTime.UtcNow.Ticks;
        //            AstarProfiler.StartFastProfile(8);

        //            //Cancel function (and thus the thread) if no more paths should be accepted.
        //            //This is done when the A* object is about to be destroyed
        //            //The path is returned and then this function will be terminated (see similar IF statement higher up in the function)
        //            if (astar.pathQueue.IsTerminating) {
        //                p.Error ();
        //            }

        //            targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
        //        }

        //        totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
        //        p.duration = totalTicks*0.0001F;

        //    }

        //    // Cleans up node tagging and other things
        //    p.Cleanup ();

        //    //Log path results
        //    AstarProfiler.StartProfile ("Log Path Results");
        //    AstarProfiler.EndProfile ();

        //    AstarProfiler.EndFastProfile(8);

        //    // Call the immediate callback
        //    // It needs to be stored in a local variable to avoid race conditions
        //    var tmpImmediateCallback = p.immediateCallback;
        //    if ( tmpImmediateCallback != null ) tmpImmediateCallback (p);

        //    AstarProfiler.StartFastProfile(13);

        //    // It needs to be stored in a local variable to avoid race conditions
        //    var tmpOnPathPostSearch = OnPathPostSearch;
        //    if (tmpOnPathPostSearch != null) tmpOnPathPostSearch (p);

        //    AstarProfiler.EndFastProfile(13);

        //    //Push the path onto the return stack
        //    //It will be detected by the main Unity thread and returned as fast as possible (the next late update)
        //    pathReturnStack.Push (p);

        //    p.AdvanceState (PathState.ReturnQueue);

        //    AstarProfiler.EndProfile ();

        //    //Wait a bit if we have calculated a lot of paths
        //    if (System.DateTime.UtcNow.Ticks > targetTick) {
        //        yield return null;
        //        targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
        //        numPaths = 0;
        //    }
        //}

        ////Debug.LogError ("Error : This part should never be reached");
	}
#endregion


	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.\n
	 Using the NNConstraint.None constraint.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (string level, Vector3 position) {
		return GetNearest(level, position,NNConstraint.None);
	}

	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (string level, Vector3 position, NNConstraint constraint) {
		return GetNearest(level, position,constraint,null, null);
	}

	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (string level, Vector3 position, NNConstraint constraint, GraphNode hint, Vector3? preferredDir) 
    {
        if (!allAstarData.ContainsKey(level)) return new NNInfo();
        NavGraph[] graphs = allAstarData[level].graphs;
		if (graphs == null) { return new NNInfo(); }

		float minDist = float.PositiveInfinity;//Math.Infinity;
		NNInfo nearestNode = new NNInfo ();
		int nearestGraph = -1;

		for (int i=0;i<graphs.Length;i++) {

			NavGraph graph = graphs[i];

			if (graph == null) continue;

			//Check if this graph should be searched
			if (!constraint.SuitableGraph (i,graph)) {
				continue;
			}

			NNInfo nnInfo;
			if (fullGetNearestSearch) {
				nnInfo = graph.GetNearestForce (position, constraint, preferredDir);
			} else {
				nnInfo = graph.GetNearest (position, constraint, preferredDir);
			}

			GraphNode node = nnInfo.node;

			if (node == null) {
				continue;
			}

			float dist = ((Vector3)nnInfo.clampedPosition-position).magnitude;            

			if (prioritizeGraphs && dist < prioritizeGraphsLimit) {
				//The node is close enough, choose this graph and discard all others
				minDist = dist;
				nearestNode = nnInfo;
				nearestGraph = i;
				break;
			} else {
				if (dist < minDist) {
					minDist = dist;
					nearestNode = nnInfo;
					nearestGraph = i;
				}
			}
		}

		//No matches found
		if (nearestGraph == -1) {
			return nearestNode;
		}

		//Check if a constrained node has already been set
		if (nearestNode.constrainedNode != null) {
			nearestNode.node = nearestNode.constrainedNode;
			nearestNode.clampedPosition = nearestNode.constClampedPosition;
		}

		if (!fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable (nearestNode.node)) {

			//Otherwise, perform a check to force the graphs to check for a suitable node
			NNInfo nnInfo = graphs[nearestGraph].GetNearestForce (position, constraint, preferredDir);

			if (nnInfo.node != null) {
				nearestNode = nnInfo;
			}
		}

		if (!constraint.Suitable (nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr)) {
			return new NNInfo();
		}

		return nearestNode;
	}

	/** Returns the node closest to the ray (slow).
	  * \warning This function is brute-force and very slow, it can barely be used once per frame
	  */
	public GraphNode GetNearest (string level, Ray ray) {
        if (!allAstarData.ContainsKey(level)) return null;
        NavGraph[] graphs = allAstarData[level].graphs;
		if (graphs == null) { return null; }

		float minDist = Mathf.Infinity;
		GraphNode nearestNode = null;

		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;

		for (int i=0;i<graphs.Length;i++) {

			NavGraph graph = graphs[i];

			graph.GetNodes (delegate (GraphNode node) {
	        	Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin,lineDirection)*lineDirection);

				float tmp = Mathf.Abs (p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) return true;

				tmp = Mathf.Abs (p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) return true;

				float dist = (p-pos).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
				return true;
			});

		}

		return nearestNode;
	}
}
