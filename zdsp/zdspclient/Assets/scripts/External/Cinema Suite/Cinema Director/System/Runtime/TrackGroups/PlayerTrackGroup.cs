using System.Collections;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The ActorTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("Player Track Group", TimelineTrackGenre.ActorTrack)]
    public class PlayerTrackGroup : ActorTrackGroup
    {
        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public override Transform Actor
        {
            get {
                return base.Actor;
            }
            set { base.Actor = value; }
        }
        
        private Transform oldactor;
        private Transform effectRoot;
        private ZDSPCamera combatCamera;
        public override void Initialize()
        {
            base.Initialize();
            Debug.LogWarning("player track group Initialize");
            if (PhotonNetwork.connected )
            {
                oldactor = Actor;                
                
                combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
                combatCamera.targetObject = null;
                GameInfo.gLocalPlayer.ShowModelOnly(true);
                GameInfo.gLocalPlayer.AnimObj.GetComponent<ActorNameTagController>().enabled = false;
                
                Actor = GameInfo.gLocalPlayer.AnimObj.transform;
                if(oldactor != null)
                {
                    effectRoot = oldactor.Find("effects");
                    oldactor.gameObject.SetActive(false);
                    GameInfo.gLocalPlayer.AnimObj.transform.position = oldactor.position;
                    GameInfo.gLocalPlayer.AnimObj.transform.forward = oldactor.forward;
                }                
                if (effectRoot != null)
                {
                    effectRoot.SetParent(Actor, false); 
                }

            } 
        }

         

        public override void UpdateTrackGroup(float time, float deltaTime)
        {
            base.UpdateTrackGroup(time, deltaTime);
            if (PhotonNetwork.connected)
            {
                //Debug.LogFormat("setting player position {0}  at pos: {1} " , time, Actor.position );
                GameInfo.gLocalPlayer.Position = Actor.position;
                GameInfo.gLocalPlayer.Forward = Actor.forward;
            }
        }

        public override void Stop()
        {
            base.Stop();
            Debug.LogWarning("---------player track group stop---------");
            if (PhotonNetwork.connected)
            {
                GameInfo.gLocalPlayer.ShowModelOnly(true);
                GameInfo.gLocalPlayer.AnimObj.GetComponent<ActorNameTagController>().enabled = true;
                Animation anim = GameInfo.gLocalPlayer.AnimObj.GetComponent<Animation>();
                if(anim.wrapMode != WrapMode.Loop)
                {
                    Debug.LogWarning("------reverting Player Animation component WrapMode");
                    anim.wrapMode = WrapMode.Loop;
                }
                GameInfo.gLocalPlayer.ForceIdle();
                if (effectRoot != null)
                {
                    effectRoot.SetParent(oldactor, false);
                }
            }
        }
    }
}