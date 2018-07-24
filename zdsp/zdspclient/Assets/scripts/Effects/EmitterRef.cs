using UnityEngine;
using System.Collections;
//using Xft;

public class EmitterRef : MonoBehaviour
{
    public string Refbone;
    public Vector3 Offset = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public bool WorldSpace = false;

    public void Awake()
    {
        //XffectComponent xffect = GetComponent<XffectComponent> ();
        //if (xffect)
        //xffect.Initialize ();
    }

    public string GetRefboneName()
    {
        return Refbone;
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

    public Transform[] GetActorChildren()
    {
        if (transform.parent != null)
        {
            GameObject parent = transform.parent.gameObject as GameObject;
            if (parent)
            {
                EffectRef effect = parent.GetComponent<EffectRef>();
                if (effect)
                    return effect.GetActorChildren();
            }
        }

        return null;
    }

    public void AttachTo(GameObject parentobj)
    {
        if (parentobj != null)
        {
            Transform[] children = parentobj.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.name == Refbone)
                {
                    if (WorldSpace)
                    {
                        transform.SetParent(null);
                        transform.position = child.transform.position + Offset;
                        transform.rotation = child.transform.rotation;
                    }
                    else
                    {
                        transform.SetParent(child.transform);
                        transform.localPosition = Offset;
                        transform.localRotation = Quaternion.identity;
                    }
                    transform.Rotate(Rotation);
                    transform.localScale = Vector3.Scale(parentobj.transform.localScale, Scale);
                    return;
                }
            }
        }
    }

    public void Detach(GameObject parentobj)
    {
        if (parentobj != null)
        {
            gameObject.transform.SetParent(parentobj.transform);
            gameObject.transform.localPosition = Offset;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            transform.Rotate(Rotation);
            transform.localScale = Vector3.Scale(parentobj.transform.localScale, Scale);
        }
    }

    public void Restart()
    {
        //XffectComponent xffect = GetComponent<XffectComponent>();
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();

        Animator[] animators = GetComponentsInChildren<Animator>();
        if (animators != null && animators.Length > 0)
        {
            for (int j = 0; j < animators.Length; j++)
            {
                Animator anim = animators[j];
                RuntimeAnimatorController ac = anim.runtimeAnimatorController;
                anim.Play(ac.animationClips[0].name, 0, 0f);
                //anim.Play(ac.layers[0].stateMachine.defaultState.name, 0, 0f);                
            }
        }

//        if (xffect)
//        {
//#if UNITY_EDITOR
//            //if (GameInfo.gLocalPlayer == null)
//            {
//                xffect.EditView = true;
//                xffect.EnableEditView();
//                xffect.ResetEditScene();
//            }
//#endif

//            xffect.Active();//need for particle to play. 
//        }


        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            particle.Clear();
            particle.Play();
        }

        foreach (EffectObjectController obj in gameObject.GetComponentsInChildren<EffectObjectController>())
            obj.Replay();
    }

    public void Deactive()
    {
        //XffectComponent xffect = GetComponent<XffectComponent>();
        //if (xffect)
        //    xffect.DeActive();

        ParticleSystem[] ps = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < ps.Length; i++)
        {
            var main = ps[i].main;
            main.playOnAwake = false;
        }
    }
}
