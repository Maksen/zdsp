using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using Xft;
using Zealot.Audio;

public class EffectRef : MonoBehaviour
{
    public bool UseRefCamera = false;
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

    private EmitterRef[] Emitters;
    private XWeaponTrail weapontail;
    private ParticleHoming particleHoming;
    private CameraShake[] cameraShakers;
    private CameraZoom[] cameraZoomers;

    private GameObject avatar;
    private Animation anim;
    private GameObject parent_object;

    private void Awake()
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
#if UNITY_EDITOR
        if (UseRefCamera)
            CreateRefCameraIfAbsent();

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

        if (particleHoming != null)
            particleHoming.Restart();

        for (int i = 0; i < Emitters.Length; i++)
            Emitters[i].Restart();

        if (weapontail != null)
            StartCoroutine(DelayAndActivateWeaponTrail());

        if (AudioClip != null)
        {
            if (SoundFX.Instance == null)
            {
                var soundObj = new GameObject("SoundFX");
                soundObj.AddComponent<SoundFX>();
            }
            SoundFX.Instance.PlayOneShot(AudioClip);
        }

        for (int i = 0; i < cameraShakers.Length; i++)
            cameraShakers[i].ShakeOnce();

        for (int i = 0; i < cameraZoomers.Length; i++)
            cameraZoomers[i].StartZoom();
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

        for (int i = 0; i < Emitters.Length; i++)
            Emitters[i].AttachTo(ref_prefab);
    }

    public void DetachEmitters()
    {
        for (int i = 0; i < Emitters.Length; i++)
            Emitters[i].Detach(gameObject);
    }

    private void CacheEmitters()
    {
        Emitters = GetComponentsInChildren<EmitterRef>(true);

        weapontail = GetComponentInChildren<XWeaponTrail>();
        if (weapontail != null)
            weapontail.Deactivate();

        particleHoming = GetComponent<ParticleHoming>();

        cameraShakers = GetComponentsInChildren<CameraShake>();
        cameraZoomers = GetComponentsInChildren<CameraZoom>();
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
        for (int i = 0; i < Emitters.Length; i++)
            Emitters[i].Deactive();
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

    private void CreateRefCameraIfAbsent()
    {
        if (GameInfo.gCombat == null) // for editor only non in-game
        {
            ZDSPStaticCamera refCam = FindObjectOfType(typeof(ZDSPStaticCamera)) as ZDSPStaticCamera;
            if (refCam == null)
            {
                // disable all current cameras
                Camera[] cams = FindObjectsOfType(typeof(Camera)) as Camera[];
                if (cams != null)
                {
                    for (int i = 0; i < cams.Length; i++)
                        cams[i].gameObject.SetActive(false);
                }

                Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Tools/Static_camera.prefab", typeof(GameObject));
                GameObject camera = Instantiate(prefab) as GameObject;
                camera.GetComponent<ZDSPStaticCamera>().InitTarget(Refmodel);
            }
        }
    }
    #endregion
#endif
}
