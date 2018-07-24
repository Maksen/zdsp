// Cinema Suite
using System.Collections;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// Transition from Clear to White over time by overlaying a guiTexture.
    /// </summary>
    [CutsceneItem("Transitions", "Camera TranitionOut", CutsceneItemGenre.GlobalItem)]
    public class CameraTranitionOut : CinemaGlobalAction
    {
        public Camera shotCamera; 
        private ZDSPCamera combatCamera;
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
            originalPosition = shotCamera.transform.position;
            originalRotation = shotCamera.transform.rotation;
            if(PhotonNetwork.connected)
            {
                if (combatCamera == null)
                    combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>(); 
            }
            
        }
        protected Quaternion originalRotation;
        protected Vector3 originalPosition;
        protected Quaternion targetRotation;
        protected Vector3 targetPosition;
        protected float targetFov;
        /// <summary>
        /// Enable the overlay texture and set the Color to Clear.
        /// </summary>
        public override void Trigger()
        {
            Debug.Log("-------------------CameraTransitionout Trigger--------------------------");
            if (PhotonNetwork.connected)
            {
                if(combatCamera == null)
                    combatCamera = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
                 
				if (shotCamera != null)
				{
                    GameObject go = GameInfo.gLocalPlayer.AnimObj;

                    //Vector3 pos;
                    //Quaternion rot;

                    //combatCamera.RetachAndReset(out pos, out rot);

                    //combatCamera.transform.position = shotCamera.transform.position;
                    //combatCamera.transform.rotation = shotCamera.transform.rotation;
                    //combatCamera.CutSceneFlyto(pos, rot, duration, ()=> { combatCamera.targetObject = go; });
                    //RetachCamera();
                    combatCamera.targetObject = go;

                } 

            }
        }
         
        protected void FlyCamera(float transition)
        {
            if(PhotonNetwork.connected)
            {
                shotCamera.transform.rotation = Quaternion.Slerp(shotCamera.transform.rotation, targetRotation, transition);
                shotCamera.transform.position = Vector3.Lerp(shotCamera.transform.position, targetPosition, transition);
                shotCamera.fieldOfView = Mathf.Lerp(shotCamera.fieldOfView, targetFov, transition);
            }

        }

        protected void RetachCamera()
        {
            
        }

        /// <summary>
        /// Firetime is reached when playing in reverse, disable the effect.
        /// </summary>
        public override void ReverseTrigger()
        {
            End();
        }

        protected bool PositionUpdated = false;

        /// <summary>
        /// Update the effect over time, progressing the transition
        /// </summary>
        /// <param name="time">The time this action has been active</param>
        /// <param name="deltaTime">The time since the last update</param>
        public override void UpdateTime(float time, float deltaTime)
        {
            if (!PhotonNetwork.connected)
                return;
            //Debug.Log("-------------------UpdateTime--------------------------");
            if (!PositionUpdated)
            {
                if (time == 0)
                    return;
                targetPosition =  GameInfo.gCombat.PlayerCamera.transform.position;
                targetRotation = GameInfo.gCombat.PlayerCamera.transform.rotation;
                Camera gamecam = GameInfo.gCombat.PlayerCamera.GetComponent<Camera>();
                targetFov = gamecam.fieldOfView;
                PositionUpdated = true;
                return;
            }
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
                    UpdateTime(1f, 1f);
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