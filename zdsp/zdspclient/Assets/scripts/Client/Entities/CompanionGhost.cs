using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;

namespace Zealot.Client.Entities
{
    public class CompanionGhost : BaseClientEntity
    {
        protected string mModelPath;
        private StaticNPCJson mNPCJson;
        private PlayerGhost mParent;
        private Vector3 mTargetPos;
        private List<Vector3> mWaypoints = null;
        private int mCurrentWaypointIndex = -1;
        private Vector3 mNextWaypointPosition;
        private bool bIsIdle = true;

        public CompanionGhost()
        {
            EntityType = EntityType.CompanionGhost;
        }

        public void Init(StaticNPCJson npcjson, Vector3 pos, Vector3 forward, PlayerGhost player)
        {
            mNPCJson = npcjson;
            mModelPath = npcjson.containerprefabpath;
            mParent = player;
            Position = pos;
            Forward = forward;
            mTargetPos = Vector3.zero;

            base.Init();
            OnAnimObjLoaded(AssetManager.LoadAsset<GameObject>(mModelPath));
        }

        public override void OnAnimObjLoaded(UnityEngine.Object asset)
        {
            if (asset != null)
                AnimObj = (GameObject)UnityEngine.Object.Instantiate(asset);
            InitAnimObj();
        }

        public void InitAnimObj()
        {
            InitEntityComponents();

            AnimObj.transform.position = Position;
            AnimObj.transform.forward = Forward;
            AnimObj.tag = "NPC";
            AnimObj.name = mNPCJson.archetype;

            ClientUtils.SetLayerRecursively(AnimObj, LayerMask.NameToLayer("Entities"));

            bIsIdle = false;

            Show(true);
            ShowEffect(true);
            mShadow.SetActive(true);
        }

        public int GetNpcId()
        {
            return mNPCJson.id;
        }

        public void UpdatePosition(Vector3 pos, Vector3 forward)
        {
            if (AnimObj != null)
            {
                AnimObj.transform.position = Position = pos;
                AnimObj.transform.forward = Forward = forward;
            }
        }

        public override void Update(long dt)
        {
            if (GameInfo.gLocalPlayer == null)
            {
                return;
            }

            Vector3 targetpos = GameInfo.gLocalPlayer.Position;
            Vector3 position = mTargetPos == Vector3.zero ? Position : mTargetPos;
            if (Vector3.Distance(targetpos, position) > 1.5f)
            {
                mTargetPos = GameUtils.RandomPosWithRadiusRange(targetpos, 1.0f, 1.5f);
                PathFindingToPlayer(position, mTargetPos);
            }
            else
            {
                if (mWaypoints == null)
                {
                    ForceIdle();
                }
            }

            if (mNextWaypointPosition != Vector3.zero)
            {
                Vector3 forward = mNextWaypointPosition - Position;
                forward.y = 0;
                forward.Normalize();
                float distToTarget = forward.magnitude;

                if (distToTarget > 0)
                {
                    AnimObj.transform.forward = Forward = forward;
                    float moveSpeed = 5 * dt / 1000.0f;
                    AnimObj.transform.position = Position = Vector3.MoveTowards(Position, mNextWaypointPosition, moveSpeed);
                    PlayRunEffect();
                }
                else
                {
                    mCurrentWaypointIndex = -1;
                    mNextWaypointPosition = Vector3.zero;
                    mWaypoints = null;
                    ForceIdle();
                }
            }
        }

        private void PathFindingToPlayer(Vector3 currentpos, Vector3 targetpos)
        {
            PathFindingStates state = PathFinder.PlotPath(currentpos, targetpos, out mWaypoints);
            if (state != PathFindingStates.Success || mWaypoints == null)
            {
                mCurrentWaypointIndex = -1;
                mNextWaypointPosition = Vector3.zero;
                ForceIdle();
                return;
            }
            else
            {
                mWaypoints[mWaypoints.Count - 1] = mTargetPos;
                mCurrentWaypointIndex = 0;
                mNextWaypointPosition = mWaypoints[mCurrentWaypointIndex];
            }
        }

        private void ForceIdle()
        {
            if (!bIsIdle)
            {
                bIsIdle = true;
                PlayEffect("standby");
            }
        }

        private void PlayRunEffect()
        {
            if (bIsIdle)
            {
                bIsIdle = false;
                PlayEffect("run");
            }
        }
    }
}
