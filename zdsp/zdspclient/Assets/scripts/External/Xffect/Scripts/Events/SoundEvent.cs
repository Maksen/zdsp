using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Zealot.Audio;

namespace Xft
{
	public class SoundEvent : XftEvent 
	{
		static AudioListener m_Listener;
		public SoundEvent(XftEventComponent owner)
			: base(XEventType.Sound, owner)
		{
			
		}
		
		protected AudioSource PlaySound(AudioClip clip, float volume, float pitch)
		{
            
            //apply global sound config
            
            volume *= Xft.GlobalConfig.SoundVolume;
            
			if (clip != null)
        	{
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    if (m_Listener == null)
                    {
                        Camera mainCamera = Camera.main;
                        if (mainCamera != null)
                            m_Listener = mainCamera.gameObject.GetComponent<AudioListener>();
                        else
                            m_Listener = GameObject.FindObjectOfType(typeof(AudioListener)) as AudioListener;

                        if (m_Listener == null)
                        {
                            if (mainCamera == null) mainCamera = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
                            if (mainCamera != null) m_Listener = mainCamera.gameObject.AddComponent<AudioListener>();
                        }
                    }

                    if (m_Listener != null)
                    {
                        AudioSource source = m_Listener.GetComponent<AudioSource>();
                        if (source == null) source = m_Listener.gameObject.AddComponent<AudioSource>();
                        source.pitch = pitch;

                        source.loop = m_owner.IsSoundLoop;

                        if (!m_owner.IsSoundLoop)
                        {
                            source.PlayOneShot(clip, volume);
                        }
                        else
                        {
                            source.clip = clip;
                            source.volume = volume;
                            source.Play();
                        }

                        return source;
                    }
                }

                Camera cam = Camera.main;
                if (cam != null)
                    m_Listener = cam.gameObject.GetComponent<AudioListener>();
                if (m_Listener != null)
                    Object.Destroy(m_Listener);
#endif

                if (SoundFX.Instance == null)
                {
                    var soundObj = new GameObject();
                    soundObj.name = "SoundFX";
                    soundObj.AddComponent<SoundFX>();
                }

                SoundFX.Instance.SetLoop(m_owner.IsSoundLoop);

                if (!m_owner.IsSoundLoop)
                {
                    SoundFX.Instance.PlayOneShot(clip, volume);
                }
                else
                { 
                    SoundFX.Instance.Play(clip, volume);
                }

            }
        	return null;
		}
  
        
        public override void Reset ()
        {
            base.Reset ();
            
            if (m_Listener != null && m_Listener.GetComponent<AudioSource>() != null && m_owner.IsSoundLoop)
                m_Listener.GetComponent<AudioSource>().Stop();
        }
		
		public override void OnBegin ()
		{
			base.OnBegin ();
			PlaySound(m_owner.Clip,m_owner.Volume,m_owner.Pitch);
		}
	}
}

