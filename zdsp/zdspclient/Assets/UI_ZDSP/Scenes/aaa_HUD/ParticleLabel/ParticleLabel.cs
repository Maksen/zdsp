using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ParticleLabel : MonoBehaviour {

    public Camera TextCamera;
    public ParticleSystem LabelParticle;
    public List<Text> RenderTexts;

    CommandBuffer commandBuffer;
    RenderTexture debugRT;

    public bool ToEmit = false;
    public string EmittedText = "";
    public bool RandomNumber = true;
    public Transform particleparent;

    List<ParticleSystem> particlesystems;

    int size = 8;
    int index = 0;
    // Use this for initialization
    void Start ()
    {
        particlesystems = new List<ParticleSystem>();
        for (int i = 0; i < size; ++i)
        {
            var system = Instantiate(LabelParticle, particleparent);
            particlesystems.Add(system);
        }
    }
    
    void Emit(string text)
    {
        RenderTexts[index].text = text;

        debugRT = new RenderTexture(TextCamera.pixelWidth, TextCamera.pixelHeight, 16);
        debugRT.Create();

        var currentsystem = particlesystems[index];

        var ts = currentsystem.textureSheetAnimation;
        ts.enabled = true;

        var delta = ((float)index) / ((float)size);
        var curve = new ParticleSystem.MinMaxCurve(delta);
        ts.frameOverTime = curve;

        currentsystem.Emit(1);
        
        index = (index + 1) % size;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (ToEmit)
        {
            ToEmit = false;

            if (RandomNumber)
                Emit(Random.Range(0, 1000).ToString());
            else
                Emit(EmittedText);
        }		
	}
}
