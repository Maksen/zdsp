using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TransformCopy
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformCopy(Transform t)
    {
        position = new Vector3(t.position.x, t.position.y, t.position.z);
        rotation = new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
        scale = new Vector3(t.localScale.x, t.localScale.y, t.localScale.z);
    }

    public void Set(Transform t)
    {
        t.position = position;
        t.rotation = rotation;
        t.localScale = scale;
    }
}

public class ParticleLabel : MonoBehaviour {

    public Camera TextCamera;
    public ParticleSystem LabelParticle;
    public Text RenderText;
    public Transform TextParent;
    List<Text> RenderTexts;

    CommandBuffer commandBuffer;
    RenderTexture debugRT;

    public bool ToEmit = false;
    public string EmittedText = "";
    public bool RandomNumber = true;
    public Transform particleparent;

    List<ParticleSystem> particlesystems;

    int size = 16;
    int index = 0;
    // Use this for initialization
    void Start()
    {
    }

    bool inited = false;
    void init()
    {        
        Reset();
        gameObject.SetActive(true);

        particlesystems = new List<ParticleSystem>();
        var tsa = LabelParticle.textureSheetAnimation;
        tsa.numTilesY = size;
        for (int i = 0; i < size; ++i)
        {
            var system = Instantiate(LabelParticle, particleparent);
            system.gameObject.SetActive(true);            
            particlesystems.Add(system);
        }
        LabelParticle.gameObject.SetActive(false);

        RenderTexts = new List<Text>();
        for (int i = 0; i < size; ++i)
        {
            var text = Instantiate(RenderText, TextParent);
            text.gameObject.SetActive(true);
            text.transform.localPosition = RenderText.transform.localPosition + new Vector3(0.0f, -RenderText.rectTransform.rect.height * i, 0.0f);
            RenderTexts.Add(text);
        }
        RenderText.gameObject.SetActive(false);

        inited = true;
    }

    void SetLabelPositions()
    {
        int i = 0;
        foreach(var text in RenderTexts)
        {
            text.gameObject.SetActive(true);
            text.transform.localPosition = RenderText.transform.localPosition + new Vector3(0.0f, -RenderText.rectTransform.rect.height * i, 0.0f);
            ++i;
        }
    }

    private void Reset()
    {
        if(particlesystems != null)
        foreach (var system in particlesystems)
        {
            Destroy(system);
        }
        particlesystems = null;

        if(RenderTexts != null)
        foreach (var text in RenderTexts)
        {
            Destroy(text);
        }
        RenderTexts = null;

        inited = false;
    }

    class EmitCommand
    {
        public string text = "";
        public TransformCopy transform;  
    }
    List<EmitCommand> command_buffer = new List<EmitCommand>();
    public void Emit(string text)
    {
        command_buffer.Add(new EmitCommand { text = text, transform = new TransformCopy(gameObject.transform) });

        if (inited == false)
            init();
    }

    public float RandomDirectionMagnitude = 15.0f;
    void Emit(EmitCommand cmd)
    {
        var text = cmd.text;
        cmd.transform.Set(gameObject.transform);

        SetLabelPositions();
        RenderTexts[index].text = text;

        //debugRT = new RenderTexture(TextCamera.pixelWidth, TextCamera.pixelHeight, 16);
        //debugRT.Create();
        
        var currentsystem = particlesystems[index];
        currentsystem.gameObject.transform.rotation = Quaternion.identity * Quaternion.Euler(Random.Range(-RandomDirectionMagnitude, RandomDirectionMagnitude), Random.Range(-RandomDirectionMagnitude, RandomDirectionMagnitude), Random.Range(-RandomDirectionMagnitude, RandomDirectionMagnitude));

        var ts = currentsystem.textureSheetAnimation;
        ts.enabled = true;

        var delta = ((float)index) / ((float)size);
        var curve = new ParticleSystem.MinMaxCurve(delta);
        ts.frameOverTime = curve;

        currentsystem.randomSeed = (uint)(Time.realtimeSinceStartup * 1000);
        currentsystem.useAutoRandomSeed = false;
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

        if (command_buffer.Count > 0)
        {
            var emitted = command_buffer[0];
            Emit(emitted);
            command_buffer.Remove(emitted);
        }
	}
}
