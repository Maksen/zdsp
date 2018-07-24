using System;
using System.Collections.Generic;
using Zealot.Client.Entities;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Common.Actions;
using UnityEngine;

public class LocalEntityState
{
	public NetEntityGhost ne;	
	public Vector3 lastPos;
    public Vector3 lastForward;
	
	public LocalEntityState(NetEntityGhost ne)
	{
		this.ne = ne;	
		lastPos = ne.Position;
        lastForward = ne.Forward;
	}
}

//Responsible for handling networking of entities at client
public class NetClient
{
    private static readonly long ACTION_UPDATE_RATE = 200; //has to be fast due to walk and skill actions
    private static readonly long SNAPSHOT_UPDATE_RATE = 200; //slower: fewer jumps but further jump
    protected Dictionary<int, LocalEntityState> mLocalEntities;
	protected Dictionary<int, NetEntityGhost> mSpawnedObjects; 
	protected ClientEntitySystem mEntitySystem;
    private long mTimeElapsedAction;
    private long mTimeElapsedSnapshot;

    public NetClient (ClientEntitySystem entitySystem)
	{
		mLocalEntities = new Dictionary<int, LocalEntityState>();
		mSpawnedObjects = new Dictionary<int, NetEntityGhost>();		
		mEntitySystem = entitySystem;
        mTimeElapsedAction = 0;
        mTimeElapsedSnapshot = 0;
    }
	
	public void CleanUp()
	{
		mLocalEntities.Clear();
		mSpawnedObjects.Clear();
		mEntitySystem = null;
	}
	
	public void AddLocalEntity(NetEntityGhost ne)
	{
		LocalEntityState les = new LocalEntityState(ne);
		mLocalEntities.Add(ne.GetPersistentID(), les);
	}
	
	public void Update(long dt)
	{
        mTimeElapsedAction += dt;
        mTimeElapsedSnapshot += dt;

        bool updateAction = false, updateSnapshot = false;
        if (mTimeElapsedAction >= ACTION_UPDATE_RATE)
        {
            mTimeElapsedAction = 0;
            updateAction = true;
        }

        if (mTimeElapsedSnapshot >= SNAPSHOT_UPDATE_RATE)
        {
            mTimeElapsedSnapshot = 0;
            updateSnapshot = true;
        }

        if (!updateAction && !updateSnapshot)
            return;
        
        foreach (KeyValuePair<int, LocalEntityState> kvp in mLocalEntities)
		{
			int pid = kvp.Key;
			LocalEntityState les = kvp.Value;
			NetEntityGhost ne = les.ne;
			if (ne.Destroyed)
				continue;

            //Check need to send snapshot
            if (updateSnapshot)
            {
                float distMoved = (les.lastPos - ne.Position).sqrMagnitude;
                float dp = Vector3.Dot(les.lastForward, ne.Forward);
                bool sendSnapShot = false;
                if (ne.IsPlayer())
                    sendSnapShot = distMoved > 0.5f || dp < 0.95f; //moved more than 0.5m or angle change of more than 18 deg
                if (sendSnapShot) 
                {
                    SnapshotUpdateToServer(ne.GetPersistentID(), ne.Position, ne.Forward); //Peter, TODO: pack into bigger packet if more than 1 local entity?
                    les.lastPos = ne.Position;
                    les.lastForward = ne.Forward;
                }
            }

            //Check need to send action command
            if (updateAction)
            {
                if (!ne.mActionSent)
                {
                    ActionCommand cmd = ne.GetActionCmd();
                    if (cmd != null)
                    {
                        ActionCmdUpdateToServer(ne.GetPersistentID(), cmd);
                    }
                    ne.mActionSent = true;
                }
            }
		}
	}	
	
	private void SnapshotUpdateToServer(int pid, Vector3 pos, Vector3 forward)
	{
		SnapShotUpdateCommand cmd = new SnapShotUpdateCommand ();
		cmd.pos = pos;
		cmd.forward = forward;
		RPCFactory.ActionRPC.SendAction (cmd.Serialize (pid));
	}
	
	private void ActionCmdUpdateToServer(int pid, ActionCommand cmd)
	{
		RPCFactory.ActionRPC.SendAction (cmd.Serialize(pid));
    }

    public List<Entity> GetSpawnedPlayers(Vector3 pos, float radius)
    {
        List<Entity> spawnedplayers = mEntitySystem.QueryEntitiesInSphere(pos, radius, (queriedEntity) =>
        {
            PlayerGhost spawnedplayer = queriedEntity as PlayerGhost;
            return (spawnedplayer != null && spawnedplayer != GameInfo.gLocalPlayer);
        });
        return spawnedplayers;
    }
}

