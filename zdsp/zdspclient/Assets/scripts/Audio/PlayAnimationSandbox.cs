using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Zealot.Audio;

#if UNITY_EDITOR
[Serializable]
public class AnimEffectSoundPair
{
    public string animationName;
    public AudioClip soundClip;
    public GameObject skillEffect;
    public GameObject hitEffect;
}
#endif

[RequireComponent(typeof(Animation))]
public class PlayAnimationSandbox : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    public List<AnimEffectSoundPair> animSoundList; 

    //[HideInInspector]
    public GameObject target = null;
    [HideInInspector]
    public ZDSPCamera cam = null;
    
    void Awake()
    {
        //Animation animationObj = target.GetComponent<Animation>();

        //foreach (var animSound in animSoundList)
        //{
        //    AnimationClip clip = animationObj.GetClip(animSound.animationName);
        //    if (clip != null)
        //    {
        //        AnimationEvent evt = new AnimationEvent();
        //        evt.stringParameter = animSound.animationName;
        //        evt.time = 0;
        //        evt.functionName = "PlayCameEffect";
        //        clip.AddEvent(evt);
        //    }
        //} 
        StartCoroutine(EnableCamera());

    }
     
    public IEnumerator EnableCamera()
    {
        yield return new WaitForSeconds(1.0f);
        if (cam == null)
        {
            cam = target.GetComponent<ZDSPCamera>();
        }
        cam.SetCameraActive(true);
        yield return null;
    }
    public void Init()
    {
        animSoundList = new List<AnimEffectSoundPair>();
        Animation animation = target.GetComponent<Animation>();
        foreach (AnimationState astate in animation)
        {
            AnimEffectSoundPair t = new AnimEffectSoundPair();
            t.animationName = astate.name;
            animSoundList.Add(t);
        }
    }
  
    public void PlayCameEffect(string animName)
    {
        //FollowCamera followcam = 
        Debug.Log(animName + " start playing.");
    }

    private void PlayClip(AudioClip audioClip)
    {
        //playonce;
        SoundFX.Instance.PlayOneShot(audioClip);
    }

    /// <summary>
    /// the playAnim and playEffect is true when in test scene.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="playAnim"></param>
    /// <param name="playEffect"></param>
    public void PlayAnimation(string clip, bool playAnim , bool playEffect , bool playHitEffect )
    {
        
        AnimEffectSoundPair currEntry = null;
        if (playAnim)
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator.HasState(clip))
            {
                Debug.Log("playing " + clip);
                animator.CrossFade(clip, 0.2f);
            }

            if (playEffect)
            {
                foreach (AnimEffectSoundPair entry in animSoundList)
                {
                    if (entry.animationName == clip)
                    {
                        currEntry = entry;
                        if (entry.skillEffect == null)
                            break;
                        GameObject efxObj = GameObject.Instantiate<GameObject>(entry.skillEffect);
                        if (efxObj == null)
                            break;
                        efxObj.SetActive(true);
                        EffectRef efxRef = efxObj.GetComponent<EffectRef>();
                        efxRef.Attach(target);
                        efxRef.Restart();
                        break;
                    }
                }
            }
        }
       
        foreach (AnimEffectSoundPair entry in animSoundList)
        {
            if (entry.animationName == clip)
            {
                PlayClip(entry.soundClip);
                break;
            }
        }

        //play gethit effect 
        if (playHitEffect && currEntry!=null)
        {
            if (currEntry.hitEffect != null)
            {
                if (GetMonster())
                {
                    GameObject efxObj = GameObject.Instantiate<GameObject>(currEntry.hitEffect);
                    efxObj.SetActive(true);
                    EffectRef efxRef = efxObj.GetComponent<EffectRef>();
                    efxRef.Attach(GetMonster());
                    efxRef.Restart();
                }
            }
        } 
    }

    private GameObject _Monster;
    private GameObject GetMonster()
    {
        if (_Monster != null)
            return _Monster;
        _Monster = GameObject.FindGameObjectWithTag("Monster");
        if (_Monster == null)
            Debug.LogErrorFormat("Monster not set in the test scene");

        return _Monster;
    }

#endif
}
