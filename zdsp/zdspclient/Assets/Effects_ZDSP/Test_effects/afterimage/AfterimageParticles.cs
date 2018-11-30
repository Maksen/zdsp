using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PooledEffects : MonoBehaviour
{
    public abstract void Restart();
}

public class AfterimageParticles : PooledEffects
{
    public SkinnedMeshRenderer target;

    public float frequency = 2.0f;
    public float duration = 0.0f;
    public float delay = 0.0f;

    ParticleSystem[] particlesystems;
    ParticleSystemRenderer[] particlerenderers;

    // Use this for initialization
    void Start()
    {
        init();
    }

    private void OnEnable()
    {
        init();
    }

    public override void Restart()
    {
        init();
    }

    void init()
    {
        durationclock = 0.0f;
        particlerenderers = GetComponentsInChildren<ParticleSystemRenderer>();
        particlesystems = GetComponentsInChildren<ParticleSystem>();
    }

    float clock = 0.0f;
    int index = 0;
    bool running = true;
    float durationclock = 0.0f;
    // Update is called once per frame
    void Update()
    {
        UpdateClocks();

        if (running)
        {
            float period = 1.0f / frequency;
            clock += Time.deltaTime;

            if (clock >= period)
            {
                Mesh mesh = new Mesh();
                target.BakeMesh(mesh);

                particlerenderers[index].mesh = mesh;
                particlesystems[index].Emit(1);

                clock = 0.0f;
                index = (index + 1) % particlesystems.Length;
            }            
        }
    }

    void UpdateClocks()
    {
        if (durationclock < delay)
        {
            running = false;

            durationclock += Time.deltaTime;            
        }
        else
        {
            if (duration == 0.0f)
            {
                running = true;
                return;
            }

            running = durationclock < (delay + duration);

            durationclock += Time.deltaTime;
        }
    }
}
