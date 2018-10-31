namespace Zealot.Client.Actions
{
    using System.Collections.Generic;
    using UnityEngine;
    using Zealot.Client.Entities;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;

    public class ACApproachWithPathFind : Action
    {
        private const float Epsilon = 0.04f;
        private const float EpsilonSqrt = 0.2f;
        private const float RotateCompleteTime = 400;     //in msec
        private const long MinimumPathRefreshTime = 2000; //in msec

        private bool mNewStart = true;
        private NetEntityGhost mTarget;
        private Vector3? mTargetPos;
        private bool mTargetposSafe;
        private bool mMoveDirectOnPathFound;
        private Vector3 mOriginalEndPoint;
        private float mRangeSq;
        private int mNextWaypointIndex;
        private List<Vector3> mWaypoints;
        private long mPathfoundElapsedTime;
        private Vector3 mLastTargetPos;
        private Vector3 mStartDir;
        private float mRotateElapsed;
        private Vector3 mDesiredForward;
        private Vector3 mNextWaypoint;

        public ACApproachWithPathFind(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            ApproachWithPathFindCommand cmd = (ApproachWithPathFindCommand)mdbCommand;

            mDesiredForward = mEntity.Forward;
            mTargetposSafe = cmd.targetposSafe;
            mMoveDirectOnPathFound = cmd.movedirectonpathfound;
            if (cmd.targetpid > 0)
            {
                mTargetPos = null;
                mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.targetpid) as NetEntityGhost;
                if (mTarget == null || mTarget.Destroyed)
                {
                    ForceIdle();
                    return;
                }
            }
            else
            {
                mTarget = null;
                mTargetPos = cmd.targetpos;
            }

            mRangeSq = cmd.range * cmd.range;

            FindPath();

            if (mWaypoints != null && mWaypoints.Count > 0)
            {
                if (mNewStart)
                {
                    NetEntityGhost ghost = (NetEntityGhost)mEntity;
                    ghost.PlayEffect(ghost.GetRunningAnimation());
                }

                MoveToNextWaypoint();
            }
        }

        public Vector3 GetTargetPos()
        {
            if (mTargetPos != null)
                return (Vector3)mTargetPos;
            else
                return mTarget.Position;
        }

        private void FindPath() //Different from server in that we wait for path to be completed
        {
            NetEntityGhost ne = mEntity as NetEntityGhost;
            mOriginalEndPoint = GetTargetPos();
            mWaypoints = null;
            mNextWaypointIndex = -1;
            mPathfoundElapsedTime = 0;
            mLastTargetPos = mOriginalEndPoint;

            //Immediately perform move to first waypoint

            List<Vector3> waypoints;
            PathFindingStates state = PathFinder.PlotPath(mEntity.Position, mOriginalEndPoint, out waypoints);
            if (state != PathFindingStates.Success || waypoints == null)
            {
                ForceIdle();
                return;
            }

            Vector3 lastPoint = waypoints[waypoints.Count - 1];
            float sqDistToReachActualDest = (lastPoint - mOriginalEndPoint).sqrMagnitude;
            if (!mTargetposSafe && sqDistToReachActualDest > 0.02f) //current cell size is 2x2m and original end point is some epsilon away from last waypoint
            {
                //Peter, If entity esp. player get stuck (keep walking on the spot) when pathfinding to a position near collider, tweak value here.
                //consider not reachable
                //GotoState("Completed"); //Peter, we allow player to move to the waypoint closest to the non-reachable point instead of not moving at all. This can happen when clicking inaccessible point in map to move to.
                //return;
            }
            else if (sqDistToReachActualDest > 0.01f) //Make sure the originalendpoint is reachable by connecting to it if too far. This can happen if grid resolution is low
                waypoints.Add(mOriginalEndPoint);

            mWaypoints = waypoints;
            if (mMoveDirectOnPathFound)
            {
                mNextWaypointIndex = mWaypoints.Count - 1;
            }
            else
            {
                mNextWaypointIndex = 0;
            }

            //Debug draw path
            //for (int i = 0; i < waypoints.Count; i++)
            //{
            //    if (i + 1 < waypoints.Count)
            //        Debug.DrawLine(waypoints[i], waypoints[i+1], Color.blue, 2f);
            //}
        }

        private double ComputePathDistance()
        {
            double total = 0;
            for (int i = 1; i < mWaypoints.Count; i++)
            {
                Vector3 first = mWaypoints[i - 1];
                Vector3 second = mWaypoints[i];
                double dist = (first - second).magnitude;
                total += dist;
            }
            return total;
        }

        private bool MoveToNextWaypoint()
        {
            if (mNextWaypointIndex >= mWaypoints.Count)
            {
                GotoState("Completed");
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

            WalkToWaypointActionCommand walkToWaypointCmd = new WalkToWaypointActionCommand();//autho client sent this commmand to non-authoclient
            walkToWaypointCmd.waypoint = mNextWaypoint;
            if (mNextWaypointIndex + 1 < mWaypoints.Count)
                walkToWaypointCmd.waypoint2 = mWaypoints[mNextWaypointIndex + 1];
            else
                walkToWaypointCmd.waypoint2 = mNextWaypoint;

            NetEntityGhost entity = (NetEntityGhost)mEntity;
            entity.SetAction(walkToWaypointCmd); //Peter: other client will do walk instead of approachwithpathfind and therefore won't play fly animation. So we use synstats
            mRotateElapsed = 0;
            return true;
            //Debug.Log("Moving to Next Waypoint!");
        }

        private void ForceIdle()
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new ClientAuthoACIdle(ghost, cmd), true);
        }

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            //Compute walk simulation to nextWaypoint
            ActorGhost ghost = (ActorGhost)mEntity;

            if (mTarget != null)
            {
                ActorGhost target = mTarget as ActorGhost;
                if (!target.IsAlive())
                {
                    ForceIdle();
                    return;
                }
            }

            //Check if reach target
            Vector3 actualTargetPos = GetTargetPos();
            Vector3 vecToTarget = actualTargetPos - ghost.Position;
            vecToTarget.y = 0;
            if (vecToTarget.sqrMagnitude <= mRangeSq + Epsilon) //target might come into reach of the entity or static position before reaching waypoint
            {
                GotoState("Completed"); //nonlocal clients and server does not know about this range. So, might offsync a bit here, while waiting for idle command to stop client monster from moving.
                return;
            }

            //Check if path is outdated, and target pos has changed quite a bit, recompute here
            //New path may be required if approaching moving entity
            if (mTarget != null)
            {
                mPathfoundElapsedTime += dt;
                if (mPathfoundElapsedTime > MinimumPathRefreshTime + GameUtils.GetRandomGenerator().Next(1000))
                {
                    if ((actualTargetPos - mLastTargetPos).sqrMagnitude > 4) //Target has moved more than 2m
                    {
                        FindPath();
                        if (mWaypoints.Count > 0)
                            MoveToNextWaypoint();
                        return;
                    }
                }
            }

            //Check if reach waypoint
            Vector3 forward = mNextWaypoint - ghost.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;

            if (distToTarget <= EpsilonSqrt || (Vector3.Dot(mStartDir, forward) < 0.0f))
            {
                ghost.Position = mNextWaypoint;
                //waypoint reached, snap it to reduce discrepancy from other clients

                mNextWaypointIndex++;
                if (MoveToNextWaypoint())
                {
                    forward = mNextWaypoint - ghost.Position;
                    forward.y = 0;
                    distToTarget = forward.magnitude;
                }
                else
                    return;
            }

            forward.Normalize();
            mRotateElapsed += dt;
            mDesiredForward = forward;
            float slerpT = Mathf.Min(mRotateElapsed / RotateCompleteTime, 1.0f);
            Vector3 interpolatedForward = Vector3.Slerp(ghost.Forward, mDesiredForward, slerpT);
            ghost.Forward = interpolatedForward;

            float moveSpeed = ghost.PlayerStats.MoveSpeed;
            Vector3 motion = ClientUtils.MoveTowards(mDesiredForward, distToTarget, moveSpeed, dt / 1000.0f);
            ghost.Move(motion);
        }

        public override bool Update(Action newAction)
        {
            ACTIONTYPE currentType = mdbCommand.GetActionType();
            ACTIONTYPE newType = newAction.mdbCommand.GetActionType();
            if (currentType == newType)
            {
                mdbCommand = newAction.mdbCommand;
                mNewStart = false;

                SetCompleteCallback(newAction.GetCompleteCallback());
                GotoState("Active");
                return true;
            }
            return false;
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            ForceIdle();//idle first
            if (mCompleteCallBack != null)
                mCompleteCallBack();
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Forward = mDesiredForward;
        }

        protected override void OnTerminatedEnter(string prevstate)
        {
            mNewStart = true;
            base.OnTerminatedEnter(prevstate);
        }

        public List<Vector3> GetWaypoints()
        {
            return mWaypoints;
        }

        public int GetNextWayPointId()
        {
            return mNextWaypointIndex;
        }

        public Vector3 GetNextWayPointPos()
        {
            return mWaypoints[mNextWaypointIndex];
        }
    }

    public class NonClientAuthoWalkWaypoint : Action
    {
        private const float Epsilon = 0.04f;
        private const float EpsilonSqrt = 0.2f;
        protected bool mNewStart = true;
        private Vector3 startDir;
        protected Vector3 mDesiredForward;
        protected const float RotateCompleteTime = 400;     //in msec
        private float mRotateElapsed;
        protected bool mMovingToFirstPoint;
        protected Vector3 mCurrWaypoint;

        public NonClientAuthoWalkWaypoint(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            mRotateElapsed = 0;
            mMovingToFirstPoint = true;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            WalkToWaypointActionCommand cmd = (WalkToWaypointActionCommand)mdbCommand;
            mCurrWaypoint = cmd.waypoint;
            //Debug.Log("Moving to first waypoint " + mCurrWaypoint);

            Vector3 forward = mCurrWaypoint - mEntity.Position;
            forward.y = 0;
            forward.Normalize();
            mDesiredForward = forward;
            startDir = forward;
            mMovingToFirstPoint = true;

            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            if (mNewStart)
                ghost.PlayEffect(ghost.GetRunningAnimation());

            //Vector3 wp1 = ((WalkToWaypointActionCommand)mdbCommand).waypoint;
            //Vector3 wp2 = ((WalkToWaypointActionCommand)mdbCommand).waypoint2;
            //wp1 += new Vector3(0, 1, 0);
            //wp2 += new Vector3(0, 1, 0);
            //Vector3 ghostpos = mEntity.Position + new Vector3(0, 1, 0);
            //Debug.DrawLine(ghostpos, wp1, Color.blue, 2f);
            //Debug.DrawLine(wp1, wp2, Color.yellow, 2f);
        }

        protected override void OnActiveUpdate(long dt)
        {
            ActorGhost ghost = (ActorGhost)mEntity;

            // Check if reach waypoint
            Vector3 forward = mCurrWaypoint - ghost.Position;
            forward.y = 0;
            float distToTarget = forward.magnitude;

            if (distToTarget <= EpsilonSqrt || (Vector3.Dot(startDir, forward) < 0.0f)) //or if turning back
            {
                //if (dotproduct < 0.0f)
                //    Debug.LogFormat("distToTarget = {0} dotproduct = {1}", distToTarget, dotproduct);

                if (mMovingToFirstPoint)  //Completed 1st waypoint
                {
                    mMovingToFirstPoint = false;
                    Vector3 nextWaypoint2 = ((WalkToWaypointActionCommand)mdbCommand).waypoint2;

                    mCurrWaypoint = nextWaypoint2; //Continue moving to second waypoint
                    forward = mCurrWaypoint - ghost.Position;
                    forward.y = 0;
                    startDir = forward;
                    distToTarget = forward.magnitude;

                    if (distToTarget <= EpsilonSqrt) //Invalid second waypoint
                    {
                        GotoState("Completed");
                        return;
                    }

                    mRotateElapsed = 0;
                    //Debug.Log("Moving to 2nd waypoint " + mCurrWaypoint);
                }
                else //Completed second waypoint, usually second waypoint will not complete as server will update client with new action
                {
                    //ghost.Position = mCurrWaypoint; //remain in its current position
                    GotoState("Completed");
                    return;
                }
            }

            forward.Normalize();
            mDesiredForward = forward;
            mRotateElapsed += dt;

            float slerpT = Mathf.Min(mRotateElapsed / RotateCompleteTime, 1.0f);
            Vector3 interpolatedForward = Vector3.Slerp(ghost.Forward, mDesiredForward, slerpT);
            ghost.Forward = interpolatedForward;

            float moveSpeed = ghost.PlayerStats.MoveSpeed;
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, moveSpeed, dt / 1000.0f);
            ghost.Move(motion);
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            //NetEntityGhost ghost = (NetEntityGhost)mEntity;
            //ghost.Forward = mDesiredForward;
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            //Debug.Log("Completed walk waypoint");
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new NonClientAuthoACIdle(ghost, cmd));

            base.OnCompleteEnter(prevstate);
        }

        public override bool Update(Action newAction)
        {
            if (newAction.mdbCommand.GetActionType() == mdbCommand.GetActionType())
            {
                mdbCommand = newAction.mdbCommand;
                mNewStart = false;
                GotoState("Active");
                return true;
            }
            return false;
        }

        protected override void OnTerminatedEnter(string prevstate)
        {
            mNewStart = true;
            base.OnTerminatedEnter(prevstate);
        }
    }
}