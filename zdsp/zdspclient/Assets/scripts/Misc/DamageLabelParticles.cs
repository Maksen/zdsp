using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageLabelParticles : MonoBehaviour {

    public ParticleLabel NormalDamage_E;
    public ParticleLabel NormalDamage_F;
    public ParticleLabel DOT;
    public ParticleLabel Heal;
    public ParticleLabel Critical;
    public ParticleLabel Total;
    public ParticleLabel Miss;
    public ParticleLabel Dodge;

    public static DamageLabelParticles instance = null;

    // Use this for initialization
    void Start () {
        transform.parent = null;
        instance = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
