using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleChildHoming : MonoBehaviour
{
    public Transform target = null;
    public float homing_strength = 10.0f;

    public GameObject child_object = null;

    List<GameObject> pool = new List<GameObject>();

    [HideInInspector]
    public Transform poolfilter = null;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() 
	{
        var particlesystem = GetComponent<ParticleSystem>();
        UpdateHoming(particlesystem);

        BindChildParticles();
    }

    private void OnDisable()
    {
        FlushChildren();
    }

    List<GameObject> GetChildren()
    {
        var ret = new List<GameObject>();
        for (int i = 0; i < poolfilter.transform.childCount; ++i)
            ret.Add(poolfilter.transform.GetChild(i).gameObject);

        return ret;
    }

    void FlushChildren()
    {
        if (poolfilter == null)
            return;

        if(Application.isEditor == false)
            Destroy(poolfilter.gameObject); 
        else
            DestroyImmediate(poolfilter.gameObject);

        poolfilter = null;
    }

    void BindChildParticles()
    {
        if (child_object == null)
            return;

        if (poolfilter == null)
        {
            poolfilter = new GameObject("PoolFilter").transform;

            poolfilter.transform.parent = transform;
        }

        pool = GetChildren();

        var parentsystem = GetComponent<ParticleSystem>();

        if (pool.Count < parentsystem.particleCount)
        {
            int difference = parentsystem.particleCount - pool.Count;

            for (int i = 0; i < difference; ++i)
            {
                var newparticle = Instantiate(child_object, poolfilter);
                newparticle.transform.SetParent(poolfilter, true);
                newparticle.SetActive(true);

                pool.Add(newparticle); 
            }
        }

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[parentsystem.particleCount];
        parentsystem.GetParticles(particles);

        int count = 0;
        foreach (var child in pool)
        {
            if (count >= parentsystem.particleCount)
                child.SetActive(false);
            else
            {
                child.SetActive(true);

                if (float.IsNaN(particles[count].position.x) || float.IsNaN(particles[count].position.y) || float.IsNaN(particles[count].position.z))
                    continue;
                else
                {
                    bool snap_detected = (child.transform.position - particles[count].position).sqrMagnitude > particles[count].velocity.sqrMagnitude * 2;
                    bool lifetime_over = particles[count].remainingLifetime < 0.01;

                    //if (lifetime_over || snap_detected)
                    //    child.SetActive(false);

                    child.transform.position = particles[count].position;
                }
            }

            ++count;
        }
    }

    void UpdateHoming(ParticleSystem system)
    {        
        if (system == null)
            return;

        if (target == null)
            return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[system.particleCount];
        var count = system.GetParticles(particles);

        var worldtolocal = Matrix4x4.identity;
        if (system.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            worldtolocal = transform.worldToLocalMatrix;

        var localtarget = (worldtolocal * target.position);
        var LT3 = new Vector3(localtarget.x, localtarget.y, localtarget.z);

        for (int i = 0; i < count; ++i)
        {
            float delta = 1.0f - (particles[i].remainingLifetime / particles[i].startLifetime);

            var dir = (LT3 - particles[i].position).normalized;

            particles[i].velocity += dir * homing_strength;
        }

        system.SetParticles(particles, count);
    }
}
