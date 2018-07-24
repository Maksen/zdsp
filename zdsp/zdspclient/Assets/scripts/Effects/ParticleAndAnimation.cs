using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class ParticleAndAnimation : MonoBehaviour
{
#if UNITY_EDITOR
    [Serializable]
    public struct AniamtorObject
    {
        public GameObject obj;
        public string state;
    }
    public AniamtorObject[] AnimatorObjects;
    public float Length = 0.0f;

    private float Timer = 0.0f;
    private bool Loop = false;

    void Start () 
	{
        PlayOnce();
    }

    public void GetAnimatorObject()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        AnimatorObjects = new AniamtorObject[animators.Length];
        for(int i=0;i<animators.Length;i++)
        {
            AnimatorObjects[i].obj = animators[i].gameObject;
            AnimatorObjects[i].state = "";
        }
    }

    public string GetAnimationState(int index)
    {
        if (AnimatorObjects[index].state != "")
            return AnimatorObjects[index].state;
        else
        {
            Animator anim = AnimatorObjects[index].obj.GetComponent<Animator>();
            AnimatorController ac = anim.runtimeAnimatorController as AnimatorController;
            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorObjects[index].state = sm.defaultState.name;
            return sm.defaultState.name;
        }
    }

    public void SetAnimationState(int index, string name)
    {
        AnimatorObjects[index].state = name;
    }

    void Update()
    {
        Timer += Time.deltaTime;
        
        if (Timer > Length && Loop)
        {
            StopPlay();
            PlayLoop();
        }
    }

    void StopPlay()
    {
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in pss)
        {
            ps.Clear();
            ps.Stop();
        }
        Animation[] anis = GetComponentsInChildren<Animation>(true);
        foreach (Animation an in anis)
        {
            an.Stop();
        }
        foreach (AniamtorObject entry in AnimatorObjects)
        {
            entry.obj.GetComponent<Animator>().StopPlayback();
        }
    }

    void Play()
    {
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in pss)
        {
            ps.loop = false;
            ps.Play();
        }
        Animation[] anis = GetComponentsInChildren<Animation>(true);
        foreach (Animation an in anis)
        {
            an.wrapMode = WrapMode.Once;
            an.Play();
        }
        foreach (AniamtorObject entry in AnimatorObjects)
        {
            entry.obj.GetComponent<Animator>().Play(entry.state, -1, 0f);
        }
    }
	
	[ContextMenu("Play Loop")]
	public void PlayLoop()
	{
        Timer = 0.0f;
        Loop = true;
        Play();
    }
	
	[ContextMenu("Play Once")]
	public void PlayOnce () 
	{
        StopPlay();
        Loop = false;
        Play();
    }
#endif
}
