using CinemaDirector.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CinemaDirector
{
    /// <summary>
    /// Enable the Actor related to this event.
    /// </summary>
    [CutsceneItemAttribute("Game Object", "ShowSpeakWords", CutsceneItemGenre.ActorItem)]
    public class ShowSpeakWords : CinemaActorAction, IRevertable
    {

        [SerializeField]
        [Tooltip("the words to display in the lifespan of this action..")]
        private string words = "";
       

        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;


        [Tooltip("The active camera when this action is active..")]
        public Camera shotCamera;
        [Tooltip("The text to show the speaking words..")]
        public Text wordsText;
        [Tooltip("the HUD canvas holding the showing text.")]
        public RectTransform HUDCanvas;
        [Tooltip("the Text canvas under the HUD canvas.")]
        public RectTransform TextCanvas;

        public override void Initialize()
        {
            base.Initialize();
            wordsText.gameObject.SetActive(false);

        }
        /// <summary>
        /// Cache the state of all actors related to this event.
        /// </summary>
        /// <returns>All the revert info related to this event.</returns>
        public RevertInfo[] CacheState()
        {
            List<Transform> actors = new List<Transform>(GetActors());
            List<RevertInfo> reverts = new List<RevertInfo>();
            foreach (Transform go in actors)
            {
                if (go != null)
                {
                    reverts.Add(new RevertInfo(this, go.gameObject, "SetActive", go.gameObject.activeSelf));
                }
            }

            return reverts.ToArray();
        }

        /// <summary>
        /// Enable the given actor.
        /// </summary>
        /// <param name="actor">The actor to be enabled.</param>
        public override void Trigger(GameObject actor)
        {
            if (actor != null)
            {
                actor.SetActive(true);
            }
        }

        /// <summary>
        /// Reverse this triggered event and place the actor back in it's previous state.
        /// </summary>
        /// <param name="actor">The actor.</param>
        public override void End(GameObject actor)
        {
            if (actor != null)
            {
                SetTime(actor, duration, 0.01f);
            }
        }

        public override void ReverseTrigger(GameObject Actor)
        {
            //todo: 
            SetTime(Actor, 0, 0.01f);//just for hiding
        }

        public override void ReverseEnd(GameObject Actor)
        {
            //todo: 
            SetTime(Actor, duration -0.01f, 0.01f);//just for showing
        }

        /// <summary>
        /// Set the action to an arbitrary time.
        /// </summary>
        /// <param name="Actor">The current actor.</param>
        /// <param name="time">the running time of the action</param>
        /// <param name="deltaTime">The deltaTime since the last update call.</param>
        public override void SetTime(GameObject actor, float time, float deltaTime)
        {
            if (actor != null)
            {                
                bool active = time >= 0 && time < Duration;
                wordsText.gameObject.SetActive(active);
                if (active)
                {
                    wordsText.text = words;
                    Vector3 offset = Vector3.up * 3.0f;
                    Vector2 viewportpos =  shotCamera.WorldToViewportPoint(actor.transform.position + offset);
                    Vector2 screenpos = new Vector2();
                    screenpos.x = viewportpos.x * HUDCanvas.sizeDelta.x - HUDCanvas.sizeDelta.x * 0.5f;
                    screenpos.y = viewportpos.y * HUDCanvas.sizeDelta.y - HUDCanvas.sizeDelta.y * 0.5f;
                    TextCanvas.anchoredPosition = screenpos;
                }
            }
        }

        public override void UpdateTime(GameObject Actor, float time, float deltaTime)
        {
            
            SetTime(Actor, time, deltaTime);
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Editor.
        /// </summary>
        public RevertMode EditorRevertMode
        {
            get { return editorRevertMode; }
            set { editorRevertMode = value; }
        }

        /// <summary>
        /// Option for choosing when this Event will Revert to initial state in Runtime.
        /// </summary>
        public RevertMode RuntimeRevertMode
        {
            get { return runtimeRevertMode; }
            set { runtimeRevertMode = value; }
        }

         
    }
}