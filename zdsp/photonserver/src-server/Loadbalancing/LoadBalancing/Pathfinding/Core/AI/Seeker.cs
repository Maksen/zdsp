using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using System.Diagnostics;

[AddComponentMenu ("Pathfinding/Seeker")]
/** Handles path calls for a single unit.
 * \ingroup relevant
 * This is a component which is meant to be attached to a single unit (AI, Robot, Player, whatever) to handle it's pathfinding calls.
 * It also handles post-processing of paths using modifiers.
 * \see \ref calling-pathfinding
 */
public class Seeker : ISerializationCallbackReceiver {
	//====== SETTINGS ======

	/** Path modifier which tweaks the start and end points of a path */
	//public StartEndModifier startEndModifier = new StartEndModifier ();

	/** The tags which the Seeker can traverse.
	 *
	 * \note This field is a bitmask.
	 * \see https://en.wikipedia.org/wiki/Mask_(computing)
	 */
	[HideInInspector]
	//public int traversableTags = -1;
    //layer 30: nav_collision 29: nav_restricted 28: nav_walkable 4: water
    public int traversableTags = unchecked((int)0xBFFFFFEF);

	/** Required for serialization backwards compatibility.
	 * \since 3.6.8
	 */
	[UnityEngine.Serialization.FormerlySerializedAs("traversableTags")]
	[SerializeField]
	[HideInInspector]
	protected TagMask traversableTagsCompatibility = new TagMask(-1,-1);

	/** Penalties for each tag.
	 * Tag 0 which is the default tag, will have added a penalty of tagPenalties[0].
	 * These should only be positive values since the A* algorithm cannot handle negative penalties.
	 *
	 * \note This array should always have a length of 32 otherwise the system will ignore it.
	 *
	 * \see Pathfinding.Path.tagPenalties
	 */
	public int[] tagPenalties = new int[32];

	//====== SETTINGS ======

	/** Callback for when a path is completed.
	 * Movement scripts should register to this delegate.\n
	 * A temporary callback can also be set when calling StartPath, but that delegate will only be called for that path
	 */
	public OnPathDelegate pathCallback;

	/** Called before pathfinding is started */
	public OnPathDelegate preProcessPath;

	/** Anything which only modifies the positions (Vector3[]) */
	public OnPathDelegate postProcessPath;

	// DEBUG
	[System.NonSerialized]
	List<Vector3> lastCompletedVectorPath;
	[System.NonSerialized]
	List<GraphNode> lastCompletedNodePath;

	// END DEBUG

	/** The current path */
	[System.NonSerialized]
	protected Path path;

	/** Returns #path.
	 * You should rarely have to use this. Instead get the path when the path callback is called.
	 *
	 * \see pathCallback
	 */
	public Path GetCurrentPath () {
		return path;
	}

	/** Cached delegate to avoid allocating one every time a path is started */
	private OnPathDelegate onPathDelegate;

	/** Temporary callback only called for the current path. This value is set by the StartPath functions */
	private OnPathDelegate tmpPathCallback;

	/** The path ID of the last path queried */
	protected uint lastPathID;

    private string level;

    public Seeker(string level)
    {
        this.level = level;
    }

	/** Initializes a few variables */
	public void Init () {
		onPathDelegate = OnPathComplete;

		//startEndModifier.Awake (this);
	}

	/** Cleans up some variables.
	 * Releases any eventually claimed paths.
	 * Calls OnDestroy on the #startEndModifier.
	 *
	 * \see ReleaseClaimedPath
	 * \see startEndModifier
	 */
	public void OnDestroy () {
		//startEndModifier.OnDestroy (this);
	}

    /** Internal list of all modifiers */
    //List<IPathModifier> modifiers = new List<IPathModifier>();
    IPathModifier themodifier;
    IPathModifier themodifier2;

    /** Called by modifiers to register themselves */
    public void RegisterModifier(IPathModifier mod)
    {
        //if (modifiers == null)
        //{
        //    modifiers = new List<IPathModifier>(1);
        //}

        //modifiers.Add(mod);
        themodifier = mod;
    }

    public void RegisterModifier2(IPathModifier mod)
    {        
        themodifier2 = mod;
    }

    /** Called by modifiers when they are disabled or destroyed */
    public void DeregisterModifier(IPathModifier mod)
    {
        //if (modifiers == null)
        //{
        //    return;
        //}
        //modifiers.Remove(mod);
        themodifier = null;
    }

	public enum ModifierPass {
		PreProcess,
		// An obsolete item occupied index 1 previously
		PostProcess = 2,
	}

	/** Post Processes the path.
	 * This will run any modifiers attached to this GameObject on the path.
	 * This is identical to calling RunModifiers(ModifierPass.PostProcess, path)
	 * \see RunModifiers
	 * \since Added in 3.2
	 */
	public void PostProcess (Path p) {
		RunModifiers (ModifierPass.PostProcess,p);
	}

	/** Runs modifiers on path \a p */
    public void RunModifiers(ModifierPass pass, Path p)
    {

        // Sort the modifiers based on priority
        // Bubble sort works because it is a small list and it is always
        // going to be sorted anyway since the same list is
        // re-sorted every time this method is executed
        //bool changed = true;
        //while (changed)
        //{
        //    changed = false;
        //    for (int i = 0; i < modifiers.Count - 1; i++)
        //    {
        //        if (modifiers[i].Priority < modifiers[i + 1].Priority)
        //        {
        //            IPathModifier tmp = modifiers[i];
        //            modifiers[i] = modifiers[i + 1];
        //            modifiers[i + 1] = tmp;
        //            changed = true;
        //        }
        //    }
        //}

        // Call delegates if they exist
        switch (pass)
        {
            case ModifierPass.PreProcess:
                if (preProcessPath != null) preProcessPath(p);
                break;
            case ModifierPass.PostProcess:
                if (postProcessPath != null) postProcessPath(p);
                break;
        }

        // No modifiers, then exit here
        //if (modifiers.Count == 0) return;
        if (themodifier == null) return;

        ModifierData prevOutput = ModifierData.All;
        //IPathModifier prevMod = modifiers[0];

        //// Loop through all modifiers and apply post processing
        //for (int i = 0; i < modifiers.Count; i++)
        //{
        //    // Cast to MonoModifier, i.e modifiers attached as scripts to the game object
        //    var mMod = modifiers[i] as MonoModifier;

        //    // Ignore modifiers which are not enabled
        //    if (mMod != null && !mMod.enabled) continue;

        //    switch (pass)
        //    {
        //        case ModifierPass.PreProcess:
        //            modifiers[i].PreProcess(p);
        //            break;
        //        case ModifierPass.PostProcess:

        //            // Convert the path if necessary to match the required input for the modifier
        //            ModifierData newInput = ModifierConverter.Convert(p, prevOutput, modifiers[i].input);

        //            if (newInput != ModifierData.None)
        //            {
        //                modifiers[i].Apply(p, newInput);
        //                prevOutput = modifiers[i].output;
        //            }
        //            else
        //            {

        //                UnityEngine.Debug.Log("Error converting " + (i > 0 ? prevMod.GetType().Name : "original") + "'s output to " + (modifiers[i].GetType().Name) + "'s input.\nTry rearranging the modifier priorities on the Seeker.");

        //                prevOutput = ModifierData.None;
        //            }

        //            prevMod = modifiers[i];
        //            break;
        //    }

        //    if (prevOutput == ModifierData.None)
        //    {
        //        break;
        //    }

        //}

        switch (pass)
        {
            case ModifierPass.PreProcess:
                themodifier.PreProcess(p);
                break;
            case ModifierPass.PostProcess:

                // Convert the path if necessary to match the required input for the modifier
                ModifierData newInput = ModifierConverter.Convert(p, prevOutput, themodifier.input);

                if (newInput != ModifierData.None)
                {
                    themodifier.Apply(p, newInput);                    
                    prevOutput = themodifier.output;                    
                }
                else
                {
                    //error!
                    prevOutput = ModifierData.None;
                }

                //2nd modifier test
                if (prevOutput != ModifierData.None && themodifier2 != null)
                {
                    ModifierData input2 = ModifierConverter.Convert(p, prevOutput, themodifier2.input);
                    if (input2 != ModifierData.None)
                    {
                        themodifier2.Apply(p, input2);
                        prevOutput = themodifier2.output;
                    }
                }
                break;
        }
    }

	/** Is the current path done calculating.
	 * Returns true if the current #path has been returned or if the #path is null.
	 *
	 * \note Do not confuse this with Pathfinding.Path.IsDone. They usually return the same value, but not always
	 * since the path might be completely calculated, but it has not yet been processed by the Seeker.
	 *
	 * \since Added in 3.0.8
	 * \version Behaviour changed in 3.2
	 */
	public bool IsDone () {
		return path == null || path.GetState() >= PathState.Returned;
	}

	/** Called when a path has completed.
	 * This should have been implemented as optional parameter values, but that didn't seem to work very well with delegates (the values weren't the default ones)
	 * \see OnPathComplete(Path,bool,bool)
	 */
	void OnPathComplete (Path p) {
		OnPathComplete (p,true,true);
	}

	/** Called when a path has completed.
	 * Will post process it and return it by calling #tmpPathCallback and #pathCallback
	 */
	void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {

		//AstarProfiler.StartProfile ("Seeker OnPathComplete");

		if (p != null && p != path && sendCallbacks) {
			return;
		}


		if (this == null || p == null || p != path)
			return;

		if (!path.error && runModifiers) {
			//AstarProfiler.StartProfile ("Seeker Modifiers");

			// This will send the path for post processing to modifiers attached to this Seeker
			RunModifiers (ModifierPass.PostProcess, path);

			//AstarProfiler.EndProfile ();
		}

		if (sendCallbacks) {

			p.Claim (this);

			//AstarProfiler.StartProfile ("Seeker Callbacks");

			lastCompletedNodePath = p.path;
			lastCompletedVectorPath = p.vectorPath;

			// This will send the path to the callback (if any) specified when calling StartPath
			if (tmpPathCallback != null) {
				tmpPathCallback (p);
			}

			// This will send the path to any script which has registered to the callback
			if (pathCallback != null) {
				pathCallback (p);
			}

			//AstarProfiler.EndProfile();
		}

		//AstarProfiler.EndProfile ();
	}


	/** Returns a new path instance.
	 * The path will be taken from the path pool if path recycling is turned on.\n
	 * This path can be sent to #StartPath(Path,OnPathDelegate,int) with no change, but if no change is required #StartPath(Vector3,Vector3,OnPathDelegate) does just that.
	 * \code var seeker = GetComponent<Seeker>();
	 * Path p = seeker.GetNewPath (transform.position, transform.position+transform.forward*100);
	 * // Disable heuristics on just this path for example
	 * p.heuristic = Heuristic.None;
	 * seeker.StartPath (p, OnPathComplete);
	 * \endcode
	 */
	public ABPath GetNewPath (Vector3 start, Vector3 end) {
		// Construct a path with start and end points
		ABPath p = ABPath.Construct (level, start, end, null);

		return p;
	}

	/** Call this function to start calculating a path.
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 */
	public Path StartPath (Vector3 start, Vector3 end) {
		return StartPath (start,end,null,-1);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 * \param callback	The function to call when the path has been calculated
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
	public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
		return StartPath (start,end,callback,-1);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param start		The start point of the path
	 * \param end		The end point of the path
	 * \param callback	The function to call when the path has been calculated
	 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask.
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed) */
	public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask) {
		Path p = GetNewPath (start,end);
		return StartPath (p, callback, graphMask);
	}

	/** Call this function to start calculating a path.
	 *
	 * \param p			The path to start calculating
	 * \param callback	The function to call when the path has been calculated
	 * \param graphMask	Mask used to specify which graphs should be searched for close nodes. See Pathfinding.NNConstraint.graphMask.
	 *
	 * \a callback will be called when the path has completed.
	 * \a Callback will not be called if the path is canceled (e.g when a new path is requested before the previous one has completed)
	 */
	public Path StartPath (Path p, OnPathDelegate callback = null, int graphMask = -1) {
		p.enabledTags = traversableTags;
		p.tagPenalties = tagPenalties;

		// Cancel a previously requested path is it has not been processed yet and also make sure that it has not been recycled and used somewhere else
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID) {
			path.Error();
		}

		path = p;
		path.callback += onPathDelegate;

		path.nnConstraint.graphMask = graphMask;

		tmpPathCallback = callback;

		// Save the path id so we can make sure that if we cancel a path (see above) it should not have been recycled yet.
		lastPathID = path.pathID;

		// Pre process the path
		//RunModifiers (ModifierPass.PreProcess, path);

		// Send the request to the pathfinder
		AstarPath.StartPath (level, path);

		return path;
	}	

	/** Handle serialization backwards compatibility */
	void ISerializationCallbackReceiver.OnBeforeSerialize () {
	}

	/** Handle serialization backwards compatibility */
	void ISerializationCallbackReceiver.OnAfterDeserialize () {
		if (traversableTagsCompatibility != null && traversableTagsCompatibility.tagsChange != -1) {
			traversableTags = traversableTagsCompatibility.tagsChange;
			traversableTagsCompatibility = new TagMask(-1,-1);
		}
	}
}

