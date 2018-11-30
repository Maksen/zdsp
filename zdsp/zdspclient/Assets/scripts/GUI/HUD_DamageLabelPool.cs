using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Common.Entities;

public class HUD_DamageLabelPool : MonoBehaviour
{
    public delegate Vector2 WorldToScreenSpaceFunc(Vector3 wpos);
    private struct DamageRecord
    {
        public AttackResult ar;
        public Vector3 worldpos;

        public DamageRecord(AttackResult _ar, Vector3 _worldpos)
        {
            ar = _ar;
            worldpos = _worldpos;
        }
    }

    Queue<GameObject> mPool = null;
    Dictionary<int, Queue<DamageRecord>> mDRQ_Dic = null;
    Vector3 mGlobalOffset_Worldspace = new Vector3();

    Camera cam;
    RectTransform GameCanvas;

    const float mConstLabelJumpCD = 0.1f;
    const int mConstPoolSize = 40;
    const float mConstMinOffset = 0.1f;
    const float mConstMaxOffset = 0.25f;
    const int mDrLimit = 10;                //damage label limit before system jump >1 label per update

    public void Awake()
    {
        GameObject dmglabelParent = UIManager.GetWidget(HUDWidgetType.DamageLabel);

        //For transforming to screen space
        GameCanvas = UIManager.GetHUDGameCanvas();
        cam = Camera.main;
        if (GameCanvas == null)
        {
            Debug.LogError("No canvas found. Check scene settings....");
            return;
        }
        if (cam == null)
        {
            Debug.LogError("No UI camera found. Check scene settings....");
            return;
        }

        //Do nothing if pool is created
        if (mPool != null && mPool.Count > 0)
        {
            Debug.LogWarning("HUD_DamageLabelPool.CreateDmgLabelPool: Create pool is called when pool is already created");
            return;
        }
        //Do nothing if cannot find parent
        if (dmglabelParent == null)
        {
            Debug.LogWarning("HUD_DamageLabelPool.CreateDmgLabelPool: Unable to find damage label parent");
            return;
        }

        mDRQ_Dic = new Dictionary<int, Queue<DamageRecord>>();
        GameObject prefab = UIManager.UIHierarchy.DmgLabelPrefab;
        mPool = ObjMgr.Instance.InitGameObjectPoolQueue(dmglabelParent.transform, prefab, prefab.transform.localPosition, prefab.transform.localScale, mConstPoolSize);
    }

    void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        if (mPool != null)
            ObjMgr.Instance.DestroyContainerObject(mPool);
        if (mDRQ_Dic != null)
            mDRQ_Dic.Clear();
    }

    public void Setup(AttackResult ar, Vector3 vEntityWorldSpacePos)
    {
        DamageRecord dr = new DamageRecord(ar, vEntityWorldSpacePos);
        int pid = ar.TargetPID;
        Queue<DamageRecord> _queue;
        if (!mDRQ_Dic.TryGetValue(pid, out _queue))
        {
            _queue = new Queue<DamageRecord>();
            mDRQ_Dic.Add(pid, _queue);
        }
        _queue.Enqueue(dr);
    }

    public bool particlebased = true;
    float playedtime = 0;
    private void Update()
    {
        //Play one every 100ms, provided there is one damage to play
        float now = Time.time;
        if (now - playedtime < mConstLabelJumpCD || mDRQ_Dic.Count <= 0 || mPool.Count <= 0)
            return;

        int _pool_Count = mPool.Count;
        List<int> removeLst = new List<int>();
        foreach (KeyValuePair<int, Queue<DamageRecord>> eRecord in mDRQ_Dic)
        {
            Entity e = GameInfo.gCombat.mEntitySystem.GetEntityByPID(eRecord.Key);

            if (particlebased && eRecord.Value.Count > 0)
            {
                EmitDamageParticle(e, eRecord.Value.Dequeue());
            }
            else
            {
                //Check if actorghost is omae wa mou shindeiru
                //Record those who shindeiru
                if (e == null)
                {
                    removeLst.Add(eRecord.Key);
                    continue;
                }

                //Take 1 damage record and play
                Play(eRecord.Value);
            }
        }

        //omae wa mou shindeiru
        for (int i = 0; i < removeLst.Count; ++i)
        {
            mDRQ_Dic.Remove(removeLst[i]);
        }

        if (mPool.Count < _pool_Count) //indicate some lable played
            playedtime = now;
    }

    void EmitDamageParticle(Entity target, DamageRecord damage)
    {
        if (DamageLabelParticles.instance != null)
        {
            var value = damage.ar.RealDamage.ToString();

            ParticleLabel emitter = null;
            if (damage.ar.IsCritical)
            {
                emitter = DamageLabelParticles.instance.Critical;
            }
            else if (damage.ar.IsEvasion)
            {
                if (target.IsMonster())
                {
                    emitter = DamageLabelParticles.instance.Dodge;
                    value = "b";
                }
                else
                {
                    emitter = DamageLabelParticles.instance.Miss;
                    value = "a";
                }
            }
            else if (damage.ar.IsHeal)
            {
                emitter = DamageLabelParticles.instance.Heal;
            }
            else if (damage.ar.IsDot)
            {
                emitter = DamageLabelParticles.instance.DOT;
            }
            else 
            {
                if (target.EntityType == EntityType.Player || target.EntityType == EntityType.PlayerGhost || target.EntityType == EntityType.AIPlayer)
                {
                    emitter = DamageLabelParticles.instance.NormalDamage_E;
                }
                else
                    emitter = DamageLabelParticles.instance.NormalDamage_F;
            }

            if (emitter != null)
            {
                emitter.gameObject.SetActive(true);
                emitter.transform.position = target.Position + new Vector3(0.0f, target.Radius*2, 0.0f);                

                emitter.Emit(value);
            }
        }
    }

    private void Play(Queue<DamageRecord> qDR)
    {
        //Do nothing if no dmgRecord or no dmglabel instances
        int _qDR_Count = qDR.Count;
        int _pool_Count = mPool.Count;
        if (_qDR_Count == 0 || _pool_Count == 0)
            return;

        //Retrieve more than 1 if dmg record queue is long
        int labelsPerInterval = _qDR_Count / mDrLimit + 1;
        if (labelsPerInterval > _pool_Count)
            labelsPerInterval = _pool_Count;

        for (int i = 0; i < labelsPerInterval; ++i)
        {
            DamageRecord dr = qDR.Dequeue();
            GameObject ol = mPool.Dequeue();
            HUD_DamageLabel l = ol.GetComponent<HUD_DamageLabel>();
            Vector3 tVec = mGlobalOffset_Worldspace + dr.worldpos;

            //Offset to prevent stacking for rendering >1 label per update
            int randSign = GameUtils.RandomInt(0, 1);
            randSign = (randSign == 0) ? -1 : 1;
            int randSign2 = GameUtils.RandomInt(0, 1);
            randSign2 = (randSign2 == 0) ? -1 : 1;
            float randValX = (float)GameUtils.Random(mConstMinOffset, mConstMaxOffset) * randSign;
            float randValY = (float)GameUtils.Random(mConstMinOffset + i * mConstMinOffset, mConstMaxOffset + i * mConstMinOffset) * randSign2;
            tVec.x += randValX;
            tVec.y += randValY;

            //Setup and play the animation
            l.Setup(dr.ar, getCanvasPosition(tVec), OnEndPlay);
        }

        //DamageRecord dr = qDR.Dequeue();
        //GameObject ol = mPool.Dequeue();
        //HUD_DamageLabel l = ol.GetComponent<HUD_DamageLabel>();
        //Vector3 tVec = mGlobalOffset_Worldspace + dr.worldpos;

        //int randSign = GameUtils.RandomInt(0, 1);
        //randSign = (randSign == 0) ? -1 : 1;
        //float randVal = (float)GameUtils.Random(0.1, 0.25) * randSign;

        //Vector3 off = new Vector3(randVal, 0f, 0f);
        //tVec += off;

        ////Setup and play the animation
        //l.Setup(dr.ar, getCanvasPosition(tVec), OnEndPlay);
    }

    private void OnEndPlay(GameObject damageLabel)
    {
        mPool.Enqueue(damageLabel);//requeue it.
    }

    public void Show(bool flag)
    {
        GameObject dmglabelParent = UIManager.GetWidget(HUDWidgetType.DamageLabel);
        dmglabelParent.SetActive(flag);
    }

    public void SetGlobalOffset_WorldSpace(Vector3 offset)
    {
        mGlobalOffset_Worldspace = offset;
    }

    private Vector2 getCanvasPosition(Vector2 UIOffset)
    {
        Vector2 ViewportPosition = cam.WorldToViewportPoint(this.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * GameCanvas.sizeDelta.x) - (GameCanvas.sizeDelta.x * 0.5f)) + UIOffset.x,
            ((ViewportPosition.y * GameCanvas.sizeDelta.y) - (GameCanvas.sizeDelta.y * 0.5f)) + UIOffset.y);
        return WorldObject_ScreenPosition;
    }

    private Vector2 getCanvasPosition(Vector3 WorldSpacePos)
    {
        //float height = GameInfo.gLocalPlayer.CharController.height;

        Vector2 ViewportPosition = cam.WorldToViewportPoint(WorldSpacePos);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * GameCanvas.sizeDelta.x) - (GameCanvas.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * GameCanvas.sizeDelta.y) - (GameCanvas.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    /// <summary>
    /// Meant for debug purposes
    /// </summary>
    public void TestLoopPlay()
    {
        AttackResult res = new AttackResult(GameInfo.gLocalPlayer.GetPersistentID(), 100);
        int count = 100;
        while (count-- > 0)
        {
            Setup(res, GameInfo.gLocalPlayer.Position);
        }
    }
}