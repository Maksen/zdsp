using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using Zealot.Audio;

public class EffectRef : MonoBehaviour
{
    public bool SpawnAvatar = false;
    public GameObject Refmodel;
    public string Refbone;
    public string RefAnimation;
    public Vector3 Offset = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public bool Attached = false;

    public float WeaponTrailStartDelay = 0.0f;
    public float WeaponTrailLifeTime = 1.0f;
    public AudioClip AudioClip;

    public GameObject[] Emitters = new GameObject[1];

    GameObject avatar = null;
    Animation anim = null;
    GameObject parent_object = null;
    Xft.XWeaponTrail weapontail = null;

    public void Awake()
    {
#if UNITY_EDITOR
        if (SpawnAvatar)
            SpawnRefModel();

        if (Attached)
            Detach_Editor();

        Attach_Editor();
        Restart();
#endif
    }

    public void Restart()
    {
        ParticleHoming homing = GetComponent<ParticleHoming>();
        if (homing != null)
            homing.Restart();
        foreach (GameObject emitter in Emitters)
        {
            if (emitter != null)
                emitter.GetComponent<EmitterRef>().Restart();
        }

        if (AudioClip != null)
        {
            if (SoundFX.Instance == null)
            {
                var soundObj = new GameObject();
                soundObj.name = "SoundFX";
                soundObj.AddComponent<SoundFX>();
            }
            SoundFX.Instance.PlayOneShot(AudioClip);
        }

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(RefAnimation) && string.IsNullOrEmpty(GameInfo.mChar))
        {
            //Debug.LogWarning("Reselect Ref Animation");
            return;
        }

        if (SpawnAvatar && anim != null)
            anim.Play(RefAnimation);
        else if (avatar != null)
        {
            if (GameInfo.gClientState != PiliClientState.Combat) //fix for gameplay in UnityEditor 
            {
                Animation oAnim = avatar.GetComponent<Animation>();
                if (oAnim != null)
                    oAnim.Play(RefAnimation);
                else
                {
                    Animator anim = avatar.GetComponent<Animator>();
                    if (anim != null)
                        anim.PlayFromStart(RefAnimation);
                }
            }
        }
#endif
        if (weapontail != null)
        {
            //weapontail.Restart();
            StartCoroutine(DelayAndActivateWeaponTrail());
        }
    }

    private IEnumerator DelayAndActivateWeaponTrail()
    {
        //Debug.Log(" weapontrail parameters :" + WeaponTrailStartDelay.ToString() + " " + WeaponTrailLifeTime.ToString());
        if (weapontail.gameObject.GetActive())
            weapontail.Deactivate();
        if (WeaponTrailStartDelay > 0)
        {
            yield return new WaitForSeconds(WeaponTrailStartDelay);
            weapontail.Activate();
        }
        else
        {
            weapontail.Activate();
        }
        if (WeaponTrailLifeTime > 0)
        {
            yield return new WaitForSeconds(WeaponTrailLifeTime);
            weapontail.Deactivate();
        }
        yield return null;
    }

    public void AttachEffect(GameObject gameobject = null)
    {
        parent_object = transform.parent == null ? null : transform.parent.gameObject as GameObject;
        GameObject ref_prefab = gameobject == null ? parent_object : gameobject;

#if UNITY_EDITOR
        parent_object = avatar = ref_prefab;
#endif

        if (ref_prefab != null)
        {
            transform.SetParent(ref_prefab.transform);
            transform.localPosition = Offset;
            transform.localRotation = Quaternion.identity;
            transform.Rotate(Rotation);
            transform.localScale = Vector3.Scale(ref_prefab.transform.localScale, Scale);
        }
    }

    public void DetachEffect()
    {
        transform.SetParent(parent_object == null ? null : parent_object.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void AttachEmitters(GameObject gameobject = null)
    {
        GameObject ref_prefab = gameobject == null ? parent_object : gameobject;
        CacheEmitters();
        foreach (GameObject emitter in Emitters)
        {
            emitter.GetComponent<EmitterRef>().AttachTo(ref_prefab);
        }
    }

    public void DetachEmitters()
    {
        foreach (GameObject emitter in Emitters)
        {
            if (emitter == null)
                continue;
            EmitterRef eRef = emitter.GetComponent<EmitterRef>();
            if (eRef != null)
                eRef.Detach(gameObject);
        }
    }

    private void CacheEmitters()
    {
        EmitterRef[] emitters = gameObject.GetComponentsInChildren<EmitterRef>(true);
        Emitters = new GameObject[emitters.Length];

        for (int i = 0; i < emitters.Length; i++)
        {
            Emitters[i] = emitters[i].gameObject;
        }

        weapontail = gameObject.GetComponentInChildren<Xft.XWeaponTrail>();
        if (weapontail != null)
        {
            //Debug.Log("weapontrail is found");
            weapontail.Deactivate();
        }

    }



    #region Runtime
    public void Attach(GameObject parent)
    {
        AttachEffect(parent);
        AttachEmitters(parent);
    }

    public void Detach()
    {
        DetachEffect();
        DetachEmitters();
    }

    public void Deactive()
    {
        foreach (GameObject emitter in Emitters)
        {
            if (emitter == null)
                continue;
            EmitterRef eRef = emitter.GetComponent<EmitterRef>();
            if (eRef)
                eRef.Deactive();
        }
    }
    #endregion

    public Transform[] GetActorChildren()
    {
#if UNITY_EDITOR
        if (Refmodel != null)
            return Refmodel.transform.GetComponentsInChildren<Transform>(true);
#endif
        return null;
    }

#if UNITY_EDITOR
    #region Editor
    private void SpawnRefModel()
    {
        if (Refmodel)
        {
            avatar = Instantiate(Refmodel, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 270, 0))) as GameObject;
            avatar.name = Refmodel.name;

            if (RefAnimation != "")
            {
                anim = avatar.GetComponent<Animation>();
                foreach (AnimationClip clip in AnimationUtility.GetAnimationClips(Refmodel))
                {
                    if (clip != null)
                    {
                        anim.AddClip(clip, clip.name);
                        anim[clip.name].wrapMode = WrapMode.Once;
                    }
                }
                anim.Play(RefAnimation);
            }
        }
        else if (!Refmodel)
        {
            Debug.LogWarning("Refrence Model Missing.");
        }
    }

    public AnimationClip[] GetAnimationClips()
    {
        if (Refmodel != null)
            return AnimationUtility.GetAnimationClips(Refmodel);

        return null;
    }

    public string GetRefanimationName()
    {
        return RefAnimation;
    }

    public void OnCopyTransformValue()
    {
        Offset = transform.localPosition;
        Rotation = transform.localRotation.eulerAngles;
        Scale = transform.localScale;
    }

    public void OnResetTransformValue()
    {
        Offset = Vector3.zero;
        Rotation = Vector3.zero;
        Scale = Vector3.one;
    }

    public void Attach_Editor()
    {
        Attached = true;
        AttachEffect(Refmodel == null ? null : GameObject.Find(Refmodel.name));
        AttachEmitters(Refmodel == null ? null : GameObject.Find(Refmodel.name));
    }

    public void Detach_Editor()
    {
        Attached = false;
        DetachEffect();
        DetachEmitters();
    }
    #endregion
#endif
}

