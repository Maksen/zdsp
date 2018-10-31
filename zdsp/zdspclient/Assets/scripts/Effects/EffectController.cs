using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Entities;

public class EffectController : MonoBehaviour
{
    public Animator Animator { get; set; }

    public Dictionary<string, EffectRef> mEfxMap = new Dictionary<string, EffectRef>();  // key: effect path
    private bool bShowAnim = true;
    private bool bShowEfx = true;

    private int lastPlayAnimFrameCount = -1;

    private bool AddEffect(string efxpath, Vector3? dir)
    {
        GameObject efxObjCache = EfxSystem.Instance.GetEffectByPath(efxpath);
        if (efxObjCache != null)
        {
            GameObject efxObj = Instantiate(efxObjCache);
            efxObj.SetActive(true);
            EffectRef efxRef = efxObj.GetComponent<EffectRef>();
            if (efxRef != null)
            {
                if (!dir.HasValue)
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
                mEfxMap.Add(efxpath, efxRef);
                return true;
            }
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
        if (animation.Contains("atk"))
            Animator.SetFloat("atkSpeed", speed);
        else if (animation.Contains("move"))
            Animator.SetFloat("moveSpeed", speed);
    }

    public void PlayAnimation(string animname, float fadeLength)
    {
        if (bShowAnim && !string.IsNullOrEmpty(animname))
        {
            // trying to crossfade mutiple animation in same frame will result in later animation not playing
            bool crossfade = fadeLength > 0;
            if (crossfade)
                crossfade = Time.frameCount > lastPlayAnimFrameCount;

            if (crossfade)
                Animator.CrossFade(animname, fadeLength);
            else
                Animator.PlayFromStart(animname);

            lastPlayAnimFrameCount = Time.frameCount;
        }
    }

    public void PlayEffect(string animname, string efxname, Vector3? dir = null, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null, bool crossfade = true)
    {
        string efxpath = EfxSystem.Instance.GetEffectPathByName(efxname);
        PlayEffectByPath(animname, efxpath, dir, duration, targetPos, targetEntity, crossfade);
    }

    public void PlaySEEffect(string efxname, Vector3? dir, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null)
    {
        if (bShowEfx)
        {
            string efxpath = EfxSystem.Instance.GetEffectPathByName(efxname);
            PlayEffectInMap(efxpath, dir, duration, targetPos, targetEntity);
        }
    }

    private void PlayEffectByPath(string animname, string efxpath, Vector3? dir = null, float duration = -1, Vector3? targetPos = null, Entity targetEntity = null, bool crossfade = true)
    {
        if (bShowAnim && !string.IsNullOrEmpty(animname))
        {
            // trying to crossfade mutiple animation in same frame will result in later animation not playing
            if (crossfade)
                crossfade = Time.frameCount > lastPlayAnimFrameCount;

            if (crossfade)
                Animator.CrossFade(animname, 0.1f);
            else
                Animator.PlayFromStart(animname);

            lastPlayAnimFrameCount = Time.frameCount;
        }

        if (bShowEfx)
            PlayEffectInMap(efxpath, dir, duration, targetPos, targetEntity);
    }

    private bool HandlePlayEffect(string efxpath, Vector3? dir)
    {
        if (string.IsNullOrEmpty(efxpath))
            return false;
        if (bShowEfx)
        {
            if (mEfxMap.ContainsKey(efxpath))
                return true;
            else
                return AddEffect(efxpath, dir);
        }
        return false;
    }

    private void PlayEffectInMap(string efxpath, Vector3? dir, float duration, Vector3? targetPos, Entity targetEntity)
    {
        if (!HandlePlayEffect(efxpath, dir))
            return;

        EffectRef effectRef;
        if (mEfxMap.TryGetValue(efxpath, out effectRef))
        {
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
                StartCoroutine(KillEffectObject(efxpath, duration));
        }
    }

    public void StopEffect(string efxpath)
    {
        EffectRef effectRef;
        if (mEfxMap.TryGetValue(efxpath, out effectRef))
        {
            effectRef.Detach();
            Destroy(effectRef.gameObject);
            mEfxMap.Remove(efxpath);
        }
    }

    public void StopAllEffects()
    {
        foreach (KeyValuePair<string, EffectRef> kvp in mEfxMap)
        {
            kvp.Value.Detach();
            Destroy(kvp.Value.gameObject);
        }
        mEfxMap.Clear();
    }

    private IEnumerator KillEffectObject(string efxpath, float duration)
    {
        yield return new WaitForSeconds(duration);
        StopEffect(efxpath);
    }

    void OnDestroy()
    {
        StopAllEffects();
    }
}