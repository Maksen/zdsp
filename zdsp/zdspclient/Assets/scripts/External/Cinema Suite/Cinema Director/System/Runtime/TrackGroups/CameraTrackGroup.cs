using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The ActorTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("Camera Track Group", TimelineTrackGenre.ActorTrack)]
    public class CameraTrackGroup : ActorTrackGroup
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


        Camera combatCamera;
        Camera cameraActor;
        public override void Initialize()
        {
            base.Initialize();            
            if (PhotonNetwork.connected )
            {
                combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<Camera>();
                cameraActor = Actor.GetComponent<Camera>();

            }
        }

        public override void UpdateTrackGroup(float time, float deltaTime)
        {
            base.UpdateTrackGroup(time, deltaTime);
            if (PhotonNetwork.connected)
            {
                //currently the transition out of camera isfrom actor camera to game camera. so can not change this. 
                //if going to change the way , this may be necessary.
                //GameInfo.gCombat.PlayerCamera.transform.position = Actor.position;
                //GameInfo.gCombat.PlayerCamera.transform.rotation = Actor.rotation;
                //combatCamera.fieldOfView = cameraActor.fieldOfView;
            }
        }
    }
}