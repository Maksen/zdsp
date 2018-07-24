// Cinema Suite
using System.Collections;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Transition from Clear to White over time by overlaying a guiTexture.
    /// </summary>
    [CutsceneItem("Transitions", "Camera TranitionIn", CutsceneItemGenre.GlobalItem)]
    public class CameraTranitionIn : CinemaGlobalAction
    {
        public Camera shotCamera;
        [SerializeField]
        private bool transitionin = true;

        private Camera combatCamera;
        public PlayerTrackGroup playertrackgrp;
        /// <summary>
        /// Setup the effect when the script is loaded.
        /// </summary>
        void Awake()
        {
             
        }

        public override void Initialize()
        {
            base.Initialize(); 
            if(PhotonNetwork.connected)
            {
                if (combatCamera == null)
                    combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<Camera>(); 
            }
            
        }
         
        protected Quaternion originalRotation;
        protected Vector3 originalPosition;
        protected float originalFov;
        /// <summary>
        /// Enable the overlay texture and set the Color to Clear.
        /// </summary>
        public override void Trigger()
        {
            Debug.Log("-------------------CameraTransitionin Trigger--------------------------");
            if (shotCamera != null)
            { 
                originalPosition = shotCamera.transform.position;
                originalRotation = shotCamera.transform.rotation;
                originalFov = shotCamera.fieldOfView; 
            }
            if (PhotonNetwork.connected)
            {                 
                
                shotCamera.transform.position = combatCamera.transform.position;
                shotCamera.transform.rotation = combatCamera.transform.rotation;
                shotCamera.fieldOfView = combatCamera.fieldOfView;
            }
        }
        
        protected void FlyCamera(float transition)
        {
            shotCamera.transform.rotation = Quaternion.Slerp(shotCamera.transform.rotation, originalRotation, transition);
            shotCamera.transform.position = Vector3.Lerp(shotCamera.transform.position, originalPosition, transition);
            shotCamera.fieldOfView = Mathf.Lerp(shotCamera.fieldOfView, originalFov, transition);
        }

        /// <summary>
        /// Firetime is reached when playing in reverse, disable the effect.
        /// </summary>
        public override void ReverseTrigger()
        {
            End();
        }

        /// <summary>
        /// Update the effect over time, progressing the transition
        /// </summary>
        /// <param name="time">The time this action has been active</param>
        /// <param name="deltaTime">The time since the last update</param>
        public override void UpdateTime(float time, float deltaTime)
        {
            float transition = time / Duration;
            FlyCamera(transition);
        }

        /// <summary>
        /// Set the transition to an arbitrary time.
        /// </summary>
        /// <param name="time">The time of this action</param>
        /// <param name="deltaTime">the deltaTime since the last update call.</param>
        public override void SetTime(float time, float deltaTime)
        {
             
            if (PhotonNetwork.connected )
            {
                if (time >= 0 && time <= Duration)
                {
                     
                    UpdateTime(time, deltaTime);
                }
                else  
                {
                    UpdateTime(1f, 0f);

                }
            }
        }

        /// <summary>
        /// End the effect by disabling the overlay texture.
        /// </summary>
        public override void End()
        {
             
        }

        /// <summary>
        /// The end of the action has been triggered while playing the Cutscene in reverse.
        /// </summary>
        public override void ReverseEnd()
        {
             
        }

        /// <summary>
        /// Disable the overlay texture
        /// </summary>
        public override void Stop()
        {
            
        }

         
    }
}