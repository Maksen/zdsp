using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class PlayClipAtPoint : MonoBehaviour {
    
    private AudioSource mAudioSource;

    void Start()
    {
        mAudioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(AudioClip mAudioSource)
    {       
            AudioSource.PlayClipAtPoint(mAudioSource, new Vector3(0, 0, 0));  
    }
}
