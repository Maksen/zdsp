namespace Zealot.Server.Actions
{
    using Photon.LoadBalancing.GameServer;
    using System.Collections.Generic;
    using UnityEngine;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    public class ASApproachWithPathFind : Action
    {
        private const float Epsilon = 0.04f;
        private const float RotateCompleteTime = 400;     //in msec
        private const long MinimumPathRefreshTime = 2000; //in msec

        private NetEntity mTarget;
        private Vector3? mTargetPos;
        private bool mTargetposSafe;
        private bool mMoveDirectOnPathFound;
        private Vector3 mOriginalEndPoint;
        private float mRangeSqr;
        private int mNextWaypointIndex;
        private List<Vector3> mWaypoints;
        private long mPathfoundElapsedTime;
        private Vector3 mLastTargetPos;
        private Vector3 mStartDir;
        private Vector3 mNextWaypoint;

        public ASApproachWithPathFind(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        public override void Start()
        {
            ApproachWithPathFindCommand cmd = (ApproachWithPathFindCommand)mdbCommand;

            mNextWaypointIndex = -1;
            mTargetposSafe = cmd.targetposSafe;
            mMoveDirectOnPathFound = cmd.movedirectonpathfound;
            if (cmd.targetpid != -1)
            {
                mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.targetpid) as NetEntity;
                if (mTarget == null || mTarget.Destroyed)
                {
                    GotoState("Completed");
                    return;
                }
            }
            else
            {
                mTarget = null;
                mTargetPos = cmd.targetpos;
            }

            mRangeSqr = cmd.range * cmd.range;
            base.Start();

            FindPath();
        }

        public Vector3 GetTargetPos()
        {
            if (mTargetPos != null)
                return (Vector3)mTargetPos;
            else
                return mTarget.Position;
        }

        //Do not setaction. Client side do not perform this action.

        private void FindPath()
        {
            NetEntity ne = mEntity as NetEntity;
            mOriginalEndPoint = GetTargetPos();
            PathManager.SeekPath(ne.mInstance.mRoom.Name, ne.mInstance.mRoom.Guid,
                                 ne.GetPersistentID(), ne.Position, mOriginalEndPoint, PathPostProcess.StripStraightPoints);
            mWaypoints = null;
            mNextWaypointIndex = -1;
            mPathfoundElapsedTime = 0;
            mLastTargetPos = mOriginalEndPoint;
            ne.SetAction(new IdleActionCommand()); //Let monster idle at client while waiting for path find result
        }

        private bool MoveToNextWaypoint()
        {
            if (mNextWaypointIndex >= mWaypoints.Count)
            {
                GotoState("Completed"); //This mean path completed without reaching  entity. Let AI handle.
                return false;
            }

            //As the original target end point may be added as the last waypoint, it may be closer than 2nd last waypoint
            //So skip the 2nd last and go straight to last waypoint if it is closer
            Vector3 nextWaypoint = mWaypoints[mNextWaypointIndex];
            if (mNextWaypointIndex == mWaypoints.Count - 2)  // next waypoint is 2nd last
            {
                Vector3 lastWaypoint = mWaypoints[mNextWaypointIndex + 1];
                float sqrDistToNextWaypoint = (nextWaypoint - mEntity.Position).sqrMagnitude;
                float sqrDistToLastWaypoint = (lastWaypoint - mEntity.Position).sqrMagnitude;
                if (sqrDistToLastWaypoint < sqrDistToNextWaypoint)
                {
                    mNextWaypointIndex++;
                    nextWaypoint = lastWaypoint;
                }
            }

            mNextWaypoint = nextWaypoint;
            mStartDir = mNextWaypoint - mEntity.Position;
            mStartDir.y = 0;
            mStartDir.Normalize();

            //This send 1 more waypoint but is smoother
            WalkToWaypointActionCommand cmd = new WalkToWaypointActionCommand();
            cmd.waypoint = mNextWaypoint;
            if (mNextWaypointIndex + 1 < mWaypoints.Count)
                cmd.waypoint2 = mWaypoints[mNextWaypointIndex + 1];
            else
                cmd.waypoint2 = mNextWaypoint;

            ((NetEntity)mEntity).SetAction(cmd);
            if ((mEntity.IsMonster()))
            {
                ((Monster)mEntity).HasMoved = true;
            }
            return true;
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            if (mNextWaypointIndex == -1)
            {
                //i.e. Waiting for path result
                NetEntity ne = mEntity as NetEntity;
                PathFindState pathfindState;
                List<Vector3> waypoints = PathManager.GetWaypoints(ne.mInstance.mRoom.Guid, ne.GetPersistentID(), out pathfindState);

                switch (pathfindState)
                {
                    case PathFindState.NotReady:
                        return; //wait for next update to check again
                    case PathFindState.Success:
                        //if targetpos is assumed to be valid, we can add it to the path if the final waypoint is still a bit off
                        Vector3 lastPoint = waypoints[waypoints.Count - 1];
                        float sqDistToReachActualDest = (lastPoint - mOriginalEndPoint).sqrMagnitude;
                        if (!mTargetposSafe && sqDistToReachActualDest > 1.44f) //current cell size is 2x2m and original end point is 1.2m away from last waypoint
                        {
                            //consider not reachable
                            GotoState("Completed");
                            break;
                        }
                        else if (sqDistToReachActualDest > 0.25f) //Make sure the originalendpoint is reachable by connecting to it if too far. This can happen if grid resolution is low
                            waypoints.Add(mOriginalEndPoint);

                        //set waypoint and walk in the next update
                        mWaypoints = new List<Vector3>(waypoints);
                        if (mMoveDirectOnPathFound)
                        {
                            mNextWaypointIndex = mWaypoints.Count - 1;
                        }
                        else
                        {
                            mNextWaypointIndex = 0;
                        }
                        MoveToNextWaypoint();//this may switch to complete state. so no more code below

                        break;

                    case PathFindState.Partial:
                        Vector3 printPos = mTargetPos == null ? mTarget.Position : (Vector3)mTargetPos;
                        //log.Info("Only partial path from " + ne.Position + " to " + printPos);

                        GotoState("Completed");
                        break;

                    case PathFindState.Error:
                        Vector3 printPos2 = mTargetPos == null ? mTarget.Position : (Vector3)mTargetPos;
                        //log.Info("Unable to find path from " + ne.Position + " to " + printPos2);
                        GotoState("Completed");
                        break;
                }
            }
            else //Compute walk simulation to nextWaypoint
            {
                Actor entity = (Actor)mEntity;
                if (mTarget != null)
                {
                    Actor target = mTarget as Actor;
                    if (target.Destroyed)
                    {
                        GotoState("Completed");
                        return;
                    }
                }

                //Check if reach target
                Vector3 actualTargetPos = GetTargetPos();
                Vector3 vecToTarget = actualTargetPos - entity.Position;
                vecToTarget.y = 0;
                if (vecToTarget.sqrMagnitude <= mRangeSqr + Epsilon) //target might come into reach of the entity or static position before reaching waypoint
                {
                    GotoState("Completed"); //Client does not know about this range. So, might offsync a bit here, while waiting for idle command to stop client monster from moving.
                    return;
                }

                //Check if path is outdated, and target pos has changed quite a bit, recompute here
                //New path may be required if approaching moving entity
                //if (mTarget != null)
                //{
                //    mPathfoundElapsedTime += dt;
                //    if (mPathfoundElapsedTime > MinimumPathRefreshTime + GameUtils.GetRandomGenerator().Next(1000))
                //    {
                //        if ((actualTargetPos - mLastTargetPos).sqrMagnitude > 4) //Target has moved more than 2m
                //        {
                //            FindPath();
                //            return;
                //        }
                //    }
                //} //Peter, may not be required to save performance cost

                //Check if reach waypoint
                Vector3 forward = mNextWaypoint - entity.Position;
                forward.y = 0;

                if (forward.sqrMagnitude <= Epsilon || (Vector3.Dot(mStartDir, forward) < 0.0f))
                {
                    entity.Position = mNextWaypoint;
                    //waypoint reached, snap it to reduce discrepancy from client

                    mNextWaypointIndex++;

                    if (MoveToNextWaypoint())
                    {
                        forward = mNextWaypoint - entity.Position;
                        forward.y = 0;
                    }
                    else
                        return;
                }

                if (entity.IsGettingHit())
                    return;

                forward.Normalize();
                entity.Forward = forward; //server just set the direction immediately while client will do the slerp for better visual
                float moveSpeed = entity.PlayerStats.MoveSpeed;
                entity.Position = Vector3.MoveTowards(entity.Position, mNextWaypoint, moveSpeed * dt / 1000.0f);
            }
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

            //clear pathmanager seeker when this action has stopped
            NetEntity ne = mEntity as NetEntity;
            PathManager.RemoveSeeker(ne.mInstance.mRoom.Guid, ne.GetPersistentID());
        }
    }
}