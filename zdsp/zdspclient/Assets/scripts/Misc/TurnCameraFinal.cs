using UnityEngine;

public class TurnCameraFinal : MonoBehaviour
{
    public float bullettime = 1.0f;
    public float speed = 5.0f;
    public Animation p1;
    public Animation p2;
    public Animation p3;
    public Animation p4;

    public AnimationClip p1Anim;
    public AnimationClip p2Anim;
    public AnimationClip p3Anim;
    public AnimationClip p4Anim;

    private Quaternion qTo = Quaternion.identity;

    private float originalTS;

    void Awake()
    {
        originalTS = Time.timeScale;
    }

    public void Start()
    {
        SelectP4();
    }

    void OnEnable()
    {
        Time.timeScale = bullettime;
    }

    void OnDisable()
    {
        ResetTimeScale();
    }

    void OnDestroy()
    {
        ResetTimeScale();
    }

    void ResetTimeScale()
    {
        Time.timeScale = originalTS;
    }

    void Update()
    {
        this.transform.rotation = Quaternion.Slerp(transform.rotation, qTo, speed * Time.deltaTime);
    }

    public void SelectP1()
    {
        qTo = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        if (p1 != null)
        {
            p1.Stop(p1Anim.name);
            p1.Play(p1Anim.name);
        }
    }

    public void SelectP2()
    {
        qTo = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        if (p2 != null)
        {
            p2.Stop(p2Anim.name);
            p2.Play(p2Anim.name);
        }
    }

    public void SelectP3()
    {
        qTo = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        if (p3 != null)
        {
            p3.Stop(p3Anim.name);
            p3.Play(p3Anim.name);
        }
    }

    public void SelectP4()
    {
        qTo = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        if (p4 != null)
        {
            p4.Stop(p4Anim.name);
            p4.Play(p4Anim.name);
        }
    }
}

