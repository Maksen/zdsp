using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zealot.Common;

public class ParticleLabelManager : MonoBehaviour
{
    private struct ParticleLabel
    {
        public Text mDmgText;
        public ParticleSystem mPartSys;
        public int mIndex;

        public ParticleLabel(Text dmgTxt, ParticleSystem ps, int index)
        {
            mDmgText = dmgTxt;
            mPartSys = ps;
            mIndex = index;
        }
    }

    public Camera TextCamera;
    public GameObject ParticlePrefab;
    public GameObject ParticleSysPrefab;
    //public RectTransform mRectTrans;
    public Transform particleparent;
    public Transform particleSysParent;
    public List<Font> fontLst = new List<Font>();

    CommandBuffer commandBuffer;
    RenderTexture debugRT;
    Queue<ParticleLabel> offParticleLabelList = new Queue<ParticleLabel>();
    Queue<ParticleLabel> onParticleLabelList = new Queue<ParticleLabel>();
    int totalPoolSize;

    // Use this for initialization
    public void Init(int poolsize)
    {
        ClearParticlePool();
        for (int i = 0; i < poolsize; ++i)
        {
            GameObject obj1 = Instantiate(ParticlePrefab, particleparent);
            Text obj1txt = obj1.GetComponent<Text>();
            GameObject obj2 = Instantiate(ParticleSysPrefab, particleSysParent);
            ParticleSystem obj2ps = obj2.GetComponent<ParticleSystem>();
            ParticleLabel pl = new ParticleLabel(obj1txt, obj2ps, i);
            offParticleLabelList.Enqueue(pl);
        }

        totalPoolSize = poolsize;
    }
    public void ClearParticlePool()
    {
        while (onParticleLabelList.Count > 0)
        {
            ParticleLabel pl = onParticleLabelList.Dequeue();
            Destroy(pl.mDmgText);
            Destroy(pl.mPartSys);
        }

        while (offParticleLabelList.Count > 0)
        {
            ParticleLabel pl = offParticleLabelList.Dequeue();
            Destroy(pl.mDmgText);
            Destroy(pl.mPartSys);
        }
    }
    private void OnDestroy()
    {
        ClearParticlePool();
    }

    public void Emit(AttackResult ar, Vector2 uipos)
    {
        if (offParticleLabelList.Count <= 0)
            return;

        debugRT = new RenderTexture(TextCamera.pixelWidth, TextCamera.pixelHeight, 16);
        debugRT.Create();

        ParticleLabel pl = offParticleLabelList.Dequeue();
        pl.mDmgText.text = ar.RealDamage.ToString();
        var ts = pl.mPartSys.textureSheetAnimation;
        ts.enabled = true;
        //pl.mObj.transform.localPosition = uipos;

        float delta = ((float)pl.mIndex) / totalPoolSize;
        var curve = new ParticleSystem.MinMaxCurve(delta);
        ts.frameOverTime = curve;

        pl.mPartSys.Emit(1);
        onParticleLabelList.Enqueue(pl);
    }

    // Update is called once per frame
    void Update()
    {
        //Move all dead particles to off list
        while (onParticleLabelList.Count > 0)
        {
            if (onParticleLabelList.Peek().mPartSys.IsAlive())
                break;

            offParticleLabelList.Enqueue(onParticleLabelList.Dequeue());
        }
    }

    private void UpdateAchorPos(Vector2 pos)
    {
        //mRectTrans.anchoredPosition = pos;
    }

    public bool HasFreeParticles()
    {
        return offParticleLabelList.Count > 0;
    }
    public int GetNumFreeParticles()
    {
        return offParticleLabelList.Count;
    }
}
