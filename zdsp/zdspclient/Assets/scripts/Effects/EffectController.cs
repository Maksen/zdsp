using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Entities;

public class EffectController : MonoBehaviour
{
    public Animation Anim { get; set; }
    public Animator Animator;
     
    public Dictionary<string, EffectRef> mEfxMap;
    private bool bShowAnim;
    private bool bShowEfx;
     
    void Awake()
    {
        mEfxMap = new Dictionary<string, EffectRef>();
        bShowEfx = true;
        bShowAnim = true;      
    }

    public void ShowAnimStates()
    {
        //if (Anim != null)
        //{
        //    Debug.Log("Animations state ---------");
        //    foreach (AnimationState state in Anim)
        //    {
        //        Debug.Log(state.name);
        //    }
        //}
        //if (Animator != null)
        //{
        //    Debug.Log("Animator state ---------");
        //    foreach (var clip in Animator.runtimeAnimatorController.animationClips)
        //        Debug.Log(clip.name);
        //}
    }

    private bool AddEffect(string efxname, Vector3? dir)
    {
        GameObject efxObjCache = EfxSystem.Instance.GetEffectByName(efxname);
        if (efxObjCache != null)
        {
            GameObject efxObj = GameObject.Instantiate(efxObjCache);
            efxObj.SetActive(true);
            EffectRef efxRef = efxObj.GetComponent<EffectRef>();
            if (!dir.HasValue ) 
            {               
                efxRef.Attach(gameObject);
                efxObj.transform.SetParent(gameObject.transform);//attach to parent
            }
            else
            {
                //att to the world for modify effect orientation 
                efxObj.transform.SetParent(null);
                efxObj.transform.position = gameObject.transform.position;
                efxObj.transform.rotation = gameObject.transform.rotation;
            } 
            efxRef.Deactive();
            mEfxMap.Add( efxname, efxRef);
            return true;
        }
        return false;
    }

    public void ShowAnim(bool showAnim)
    {
        bShowAnim = showAnim;
    }

    public void ShowEfx(bool showEfx)
    {
        bShowEfx = showEfx;
    }

    public void SetAnimSpeed(string animation, float speed)
    {
        if (Animator == null)
            Anim[animation].speed = speed;
        else
        {
            if (animation.Contains("atk"))
                Animator.SetFloat("atkSpeed", speed);
            else if (animation.Contains("move"))
                Animator.SetFloat("moveSpeed", speed);
        }
    }

    public void PlayAnimation(string animation, float fadeLength)
    {
        if (Animator != null)
        {
            if (fadeLength > 0)
                Animator.CrossFade(animation, fadeLength);
            else
                Animator.PlayFromStart(animation);
        }
        else
        {
            if (fadeLength > 0)
                Anim.CrossFade(animation, fadeLength);
            else
                Anim.Play(animation);
        }
    }

    private int lastFrameCount = -1;
    public void PlayEffect(string animname, string efxname, Vector3? dir = null, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null, bool crossfade = true)
    {
        if (bShowAnim && !string.IsNullOrEmpty(animname))
        {
            // trying to crossfade mutiple animation in same frame will result in later animation not playing
            if (crossfade)
                crossfade = Time.frameCount > lastFrameCount;

            if (Animator != null)
            {
                if (crossfade)
                    Animator.CrossFade(animname, 0.1f);
                else
                    Animator.PlayFromStart(animname);
            }
            else if (Anim != null)
            {
                if (crossfade)
                    Anim.CrossFade(animname, 0.1f);
                else
                    Anim.Play(animname);
            }
            lastFrameCount = Time.frameCount;
        }

        if (bShowEfx)
            PlayEffectInMap(efxname, dir, duration, targetPos, targetEntity);
    }

    public void PlaySEEffect(string efxname, Vector3? dir, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null)
    {
        if (bShowEfx)
            PlayEffectInMap(efxname, dir, duration, targetPos, targetEntity);   
    }

    private bool HandlePlayEffect(string efxname, Vector3? dir)
    {
        if (string.IsNullOrEmpty(efxname))
            return false;
        if (bShowEfx)
        {
            if (mEfxMap.ContainsKey(efxname))
                return true;
            else
                return AddEffect(efxname, dir); //try to load it from prefab 
        } 
        return false;  
    }
     
    private void PlayEffectInMap(string efxname, Vector3? dir, float duration, Vector3? targetPos, Entity targetEntity)
    {
        //Debug.Log("effect count: " + mEfxMap.Count);
        if (!HandlePlayEffect(efxname, dir))
            return; 
        if (mEfxMap[efxname] != null)
        {
            EffectRef effectRef = mEfxMap[efxname];
            if (dir.HasValue)
            {
                //overrite the default effect prefab orientation; 
                effectRef.transform.position = transform.position;
                effectRef.transform.forward = dir.Value;
            }
            ParticleHoming homing = effectRef.GetComponent<ParticleHoming>();
            if (homing != null)
                homing.SetTarget(targetPos, targetEntity);
            effectRef.Restart();
            if (duration != -1)
                StartCoroutine(KillEffectObject(efxname, duration));
        } 
    }

    public void StopEffect(string efxname)
    {
        if (mEfxMap.ContainsKey(efxname))
        {
            //mEfxMap[efxname].Deactive();
            string key =  efxname;
            if (mEfxMap.ContainsKey(key))
            {
                EffectRef effectRef = mEfxMap[key];
                if (effectRef != null)
                {
                    effectRef.Detach();
                    GameObject.Destroy(effectRef.gameObject);
                }
                mEfxMap.Remove(key);
            }
        }
    }

    public void StopAllEffects()
    {
        foreach (KeyValuePair<string, EffectRef> kvp in mEfxMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Detach();
                GameObject.Destroy(kvp.Value.gameObject);
            }
        }
        mEfxMap.Clear();
    }
    
    IEnumerator KillEffectObject(string efxname, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (mEfxMap.ContainsKey(efxname))
            StopEffect(efxname);
    }

    void OnDestroy()
    {
        StopAllEffects();
    }
}
