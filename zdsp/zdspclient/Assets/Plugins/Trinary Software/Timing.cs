using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// /////////////////////////////////////////////////////////////////////////////////////////
//                                        TIMING
// 
// This is an imporved implementation of coroutines that boasts zero per-frame memory allocations.
//  It serves as the "Time" portion of the Movement / Time plugin, which can be found here:
//  https://www.assetstore.unity3d.com/en/#!/content/50796
// 
// For manual, support, or upgrade guide visit http://trinary.tech/
//
// Created by Teal Rogers
// Trinary Software
// All rights preserved.
// /////////////////////////////////////////////////////////////////////////////////////////

namespace MEC
{
    public class Timing : MonoBehaviour
    {
        public long NumberOfUpdateCoroutines;
        public long NumberOfFixedUpdateCoroutines;
        public long NumberOfLateUpdateCoroutines;

        private static Timing _instance;

        private const int CoroutineArrayChunkSize = 128;

        private IEnumerator<float>[] UpdateCoroutines = new IEnumerator<float>[CoroutineArrayChunkSize * 4];
        private IEnumerator<float>[] LateUpdateCoroutines = new IEnumerator<float>[CoroutineArrayChunkSize];
        private IEnumerator<float>[] FixedUpdateCoroutines = new IEnumerator<float>[CoroutineArrayChunkSize];

        private long NextUpdateCoroutineSlot = 0L;
        private long NextLateUpdateCoroutineSlot = 0L;
        private long NextFixedUpdateCoroutineSlot = 0L;

        private ushort _framesSinceUpdate;
        private const ushort FramesBeforeMaintenance = 64;

        public static Timing Instance
        {
            get
            {
                if (null == _instance || !_instance.gameObject)
                {
                    GameObject instanceHome;
                    if(EventSystem.current) 
                        instanceHome = EventSystem.current.gameObject;
                    else
                    {
                        instanceHome = GameObject.Find("Movement Effects");
                        if(instanceHome == null)
                        {
                            instanceHome = new GameObject();
                            instanceHome.name = "Movement Effects";
                        }
                    }

                    _instance = instanceHome.GetComponent<Timing>() ?? instanceHome.AddComponent<Timing>();
                }

                return _instance;
            }
            set { _instance = value; }
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// This will kill all coroutines running on the current MEC instance.
        /// </summary>
        public static void KillAllCoroutines()
        {
            if (_instance != null)
                Destroy(_instance);
        }

        /// <summary>
        /// This will pause all coroutines running on the current MEC instance until ResumeAllCoroutines is called.
        /// </summary>
        public static void PauseAllCoroutines()
        {
            if(_instance != null)
                _instance.enabled = false;
        }

        /// <summary>
        /// This resumes all coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        public static void ResumeAllCoroutines()
        {
            if (_instance != null)
                _instance.enabled = true;
        }

        void Update()
        {
            for (long i = 0; i < NextUpdateCoroutineSlot; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Processing Coroutine");

                if (UpdateCoroutines[i] != null && Time.time >= UpdateCoroutines[i].Current)
                {
                    try 
                    {
                        if (!UpdateCoroutines[i].MoveNext() || float.IsNaN(UpdateCoroutines[i].Current))
                            UpdateCoroutines[i] = null;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception while running coroutine. (aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        UpdateCoroutines[i] = null;
                    }
                }

                UnityEngine.Profiling.Profiler.EndSample();
            }

            if (++_framesSinceUpdate > FramesBeforeMaintenance)
            {
                _framesSinceUpdate = 0;

                UnityEngine.Profiling.Profiler.BeginSample("Maintenance Task");

                RemoveUnused();

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        void FixedUpdate()
        {
            for (long i = 0; i < NextFixedUpdateCoroutineSlot; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Processing Coroutine");

                if (FixedUpdateCoroutines[i] != null && Time.time >= FixedUpdateCoroutines[i].Current)
                {
                    try 
                    {
                        if (!FixedUpdateCoroutines[i].MoveNext() || float.IsNaN(FixedUpdateCoroutines[i].Current))
                            FixedUpdateCoroutines[i] = null;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception while running coroutine. (aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        UpdateCoroutines[i] = null;
                    }
                }

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        void LateUpdate()
        {
            for (long i = 0; i < NextLateUpdateCoroutineSlot; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Processing Coroutine");

                if (LateUpdateCoroutines[i] != null && Time.time >= LateUpdateCoroutines[i].Current)
                {
                    try 
                    {
                        if (!LateUpdateCoroutines[i].MoveNext() || float.IsNaN(LateUpdateCoroutines[i].Current))
                            LateUpdateCoroutines[i] = null;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception while running coroutine. (aborting coroutine)\n" + ex.Message + "\n" + ex.StackTrace);
                        UpdateCoroutines[i] = null;
                    }
                }

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private void RemoveUnused()
        {
            long i, j;
            for(i = j = 0L;i < NextUpdateCoroutineSlot;i++)
            {
                if(UpdateCoroutines[i] != null)
                {
                    if(i != j)
                        UpdateCoroutines[j] = UpdateCoroutines[i];
                    j++;
                }
            }
            for(i = j;i < NextUpdateCoroutineSlot;i++)
                UpdateCoroutines[i] = null;

            NumberOfUpdateCoroutines = NextUpdateCoroutineSlot = j;

            for(i = j = 0L;i < NextFixedUpdateCoroutineSlot;i++)
            {
                if(FixedUpdateCoroutines[i] != null)
                {
                    if(i != j)
                        FixedUpdateCoroutines[j] = FixedUpdateCoroutines[i];
                    j++;
                }
            }
            for(i = j;i < NextFixedUpdateCoroutineSlot;i++)
                FixedUpdateCoroutines[i] = null;

            NumberOfFixedUpdateCoroutines = NextFixedUpdateCoroutineSlot = j;

            for(i = j = 0L;i < NextLateUpdateCoroutineSlot;i++)
            {
                if(LateUpdateCoroutines[i] != null)
                {
                    if(i != j)
                        LateUpdateCoroutines[j] = LateUpdateCoroutines[i];
                    j++;
                }
            }
            for(i = j;i < NextLateUpdateCoroutineSlot;i++)
                LateUpdateCoroutines[i] = null;

            NumberOfLateUpdateCoroutines = NextLateUpdateCoroutineSlot = j;
        }

        /// <summary>
        /// Start a coroutine that will run in the normal update loop.
        /// </summary>
        public static void StartUpdateCoroutine(IEnumerator<float> coroutine)
        {
            Instance.RunUpdateCoroutine(coroutine);
        }

        /// <summary>
        /// Start a coroutine that will run on the current Timing instance in the normal update loop.
        /// </summary>
        public void RunUpdateCoroutine(IEnumerator<float> coroutine) 
        {
            if (NextUpdateCoroutineSlot >= UpdateCoroutines.LongLength)
            {
                var oldArray = UpdateCoroutines;
                UpdateCoroutines = new IEnumerator<float>[UpdateCoroutines.LongLength + CoroutineArrayChunkSize];
                for (long i = 0L; i < oldArray.LongLength; i++)
                    UpdateCoroutines[i] = oldArray[i];
            }

            UpdateCoroutines[NextUpdateCoroutineSlot++] = coroutine;
        }

        /// <summary>
        /// Start a coroutine that will run in the fixed update loop.
        /// </summary>
        public static void StartFixedUpdateCoroutine(IEnumerator<float> coroutine)
        {
            Instance.RunFixedUpdateCoroutine(coroutine);
        }

        /// <summary>
        /// Start a coroutine that will run on the current Timing instance in the fixed update loop.
        /// </summary>
        public void RunFixedUpdateCoroutine(IEnumerator<float> coroutine)
        {
            if (NextFixedUpdateCoroutineSlot >= FixedUpdateCoroutines.LongLength)
            {
                var oldArray = FixedUpdateCoroutines;
                FixedUpdateCoroutines = new IEnumerator<float>[FixedUpdateCoroutines.LongLength + CoroutineArrayChunkSize];
                for (long i = 0L; i < oldArray.LongLength; i++)
                    FixedUpdateCoroutines[i] = oldArray[i];
            }

            FixedUpdateCoroutines[NextFixedUpdateCoroutineSlot++] = coroutine;
        }

        /// <summary>
        /// Start a coroutine that will run in the late update loop.
        /// </summary>
        public static void StartLateUpdateCoroutine(IEnumerator<float> coroutine)
        {
            Instance.RunLateUpdateCoroutine(coroutine);
        }

        /// <summary>
        /// Start a coroutine that will run on the current Timing instance in the late update loop.
        /// </summary>
        public void RunLateUpdateCoroutine(IEnumerator<float> coroutine)
        {
            if (NextLateUpdateCoroutineSlot >= LateUpdateCoroutines.LongLength)
            {
                var oldArray = LateUpdateCoroutines;
                LateUpdateCoroutines = new IEnumerator<float>[LateUpdateCoroutines.LongLength + CoroutineArrayChunkSize];
                for (long i = 0L; i < oldArray.LongLength; i++)
                    LateUpdateCoroutines[i] = oldArray[i];
            }

            LateUpdateCoroutines[NextLateUpdateCoroutineSlot++] = coroutine;
        }

        /// <summary>
        /// Stop the given coroutine if it exists.
        /// </summary>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public static bool StopMECCoroutine(IEnumerator<float> coroutine)
        {
            return Instance.StopMECCoroutineOnInstance(coroutine);
        }

        /// <summary>
        /// Stop the given coroutine if it exists in the current Timing instance.
        /// </summary>
        /// <returns>Whether the coroutine was found and stopped.</returns>
        public bool StopMECCoroutineOnInstance(IEnumerator<float> coroutine)
        {
            var type = coroutine.GetType().ToString();

            for (long i = 0; i < NextUpdateCoroutineSlot; i++)
            {
                if (UpdateCoroutines[i] != null && UpdateCoroutines[i].GetType().ToString() == type)
                {
                    UpdateCoroutines[i] = null;
                    return true;
                }
            }

            for (long i = 0; i < NextFixedUpdateCoroutineSlot; i++)
            {
                if (FixedUpdateCoroutines[i] != null && FixedUpdateCoroutines[i].GetType().ToString() == type)
                {
                    FixedUpdateCoroutines[i] = null;
                    return true;
                }
            }

            for (long i = 0; i < NextLateUpdateCoroutineSlot; i++)
            {
                if (LateUpdateCoroutines[i] != null && LateUpdateCoroutines[i].GetType().ToString() == type)
                {
                    LateUpdateCoroutines[i] = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Use in a yield return statement to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public static float WaitForSeconds(float waitTime)
        {
            return Time.time + waitTime;
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action.</param>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        public static void CallDelayed<TRef>(TRef reference, float delay, System.Action<TRef> action)
        {
            if (delay > 0f)
                StartUpdateCoroutine(_CallDelayBack(reference, delay, action));
            else
                action(reference);
        }

        private static IEnumerator<float> _CallDelayBack<TRef>(TRef reference, float delay, System.Action<TRef> action)
        {
            yield return Time.time + delay;

            CallDelayed(reference, 0f, action);
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        public static void CallDelayed(float delay, System.Action action)
        {
            if (delay > 0f)
                StartUpdateCoroutine(_CallDelayBack(delay, action));
            else
                action();
        }

        private static IEnumerator<float> _CallDelayBack(float delay, System.Action action)
        {
            yield return Time.time + delay;

            CallDelayed(0f, action);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously(float timeframe, System.Action action, System.Action onDone = null)
        {
            StartUpdateCoroutine(_CallContinuously(timeframe, action, onDone));
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuouslyLateUpdate(float timeframe, System.Action action, System.Action onDone = null)
        {
            StartLateUpdateCoroutine(_CallContinuously(timeframe, action, onDone));
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuouslyFixedUpdate(float timeframe, System.Action action, System.Action onDone = null)
        {
            StartFixedUpdateCoroutine(_CallContinuously(timeframe, action, onDone));
        }

        private static IEnumerator<float> _CallContinuously(float timeframe, System.Action action, System.Action onDone = null)
        {
            float startTime = Time.time;
            while (Time.time <= startTime + timeframe)
            {
                yield return 0f;

                action();
            }

            if (onDone != null)
                onDone();
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuously<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            StartUpdateCoroutine(_CallContinuously(reference, timeframe, action, onDone));
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuouslyLateUpdate<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            StartLateUpdateCoroutine(_CallContinuously(reference, timeframe, action, onDone));
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        public static void CallContinuouslyFixedUpdate<T>(T reference, float timeframe, System.Action<T> action, System.Action<T> onDone = null)
        {
            StartFixedUpdateCoroutine(_CallContinuously(reference, timeframe, action, onDone));
        }

        private static IEnumerator<float> _CallContinuously<T>(T reference, float timeframe,
            System.Action<T> action, System.Action<T> onDone = null)
        {
            float startTime = Time.time;
            while (Time.time <= startTime + timeframe)
            {
                yield return 0f;

                action(reference);
            }

            if (onDone != null)
                onDone(reference);
        }

    }
}
