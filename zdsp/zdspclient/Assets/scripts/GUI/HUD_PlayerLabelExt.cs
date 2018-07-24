using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using Zealot.Repository;
using Zealot.Client.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HUD_PlayerLabelExt : MonoBehaviour
{
    public delegate Vector2 GetCanvasPosDelegate(Vector3 worldOffset);

    [SerializeField]
    Image mCutsceneIcon;
    [SerializeField]
    UI_ProgressBarC mCastBar;

    float mCastTimeProgress = 0f;
    float mSkillCastTime = 1f;
    float mSkillCastTimeInv = 1f;

    RectTransform mRectTrans;
    Vector3 mOffset_WorldSpace = Vector3.zero;

    GetCanvasPosDelegate mCanvasPosFunc = null;

    #region Property
    public bool CutsceneTurnOn
    {
        get { return mCutsceneIcon.IsActive(); }
        set { mCutsceneIcon.gameObject.SetActive(value); }
    }
    public float CurrentCastTime
    {
        get { return mCastTimeProgress * mSkillCastTimeInv; }
    }
    public bool IsCasting
    {
        get { return mCastBar.gameObject.GetActive(); }
    }
    public Vector3 WorldSpaceOffset
    {
        set { mOffset_WorldSpace = value; }
    }
    public GetCanvasPosDelegate CanvasPosFunc
    {
        set { mCanvasPosFunc = value; }
    }
    #endregion

    public void Awake()
    {
        mRectTrans = gameObject.GetComponent<RectTransform>();
    }

    public void CastSkill(float skillCastTime)
    {
        //Do nothing if a skill is already casting
        if (mCastBar.gameObject.GetActive())
            return;

        //Record skill cast time
        mCastTimeProgress = 0f;
        mSkillCastTime = skillCastTime;
        mSkillCastTimeInv = 1f / skillCastTime;

        //reset cast bar and start casting
        mCastBar.Value = 0;
        mCastBar.gameObject.SetActive(true);
    }

    public void LateUpdate()
    {
        //Do nothing if not casting
        if (!mCastBar.gameObject.GetActive())
            return;

        //Add dt per frame
        mCastTimeProgress += Time.deltaTime;
        int val = (int)(mCastTimeProgress * mSkillCastTimeInv * mCastBar.Max);
        if (val == mCastBar.Value)
            return;

        //Set current cast time and check if casting is done
        mCastBar.Value = val;
        mCastBar.gameObject.SetActive(mCastBar.Value < mCastBar.Max);
    }

    public void UpdateAchorPos()
    {
        if (mCanvasPosFunc == null)
            return;

        mRectTrans.anchoredPosition = mCanvasPosFunc(mOffset_WorldSpace);
    }

    public void ScaleLabel(Vector3 scale)
    {
        Transform parent = gameObject.transform.parent;
        transform.SetParent(null, false);
        transform.localScale = scale;
        transform.SetParent(parent, false);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HUD_PlayerLabelExt))]
public class HUD_PlayerLabelExtEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HUD_PlayerLabelExt ple = (HUD_PlayerLabelExt)target;

        if (GUILayout.Button("Cast Skill [0.1 sec]"))
        {
            ple.CastSkill(0.1f);
        }
        if (GUILayout.Button("Cast Skill [0.25 sec]"))
        {
            ple.CastSkill(0.25f);
        }
        if (GUILayout.Button("Cast Skill [0.5 sec]"))
        {
            ple.CastSkill(0.5f);
        }
        if (GUILayout.Button("Cast Skill [1 sec]"))
        {
            ple.CastSkill(1f);
        }
        if (GUILayout.Button("Cast Skill [2 sec]"))
        {
            ple.CastSkill(2f);
        }
        if (GUILayout.Button("Cast Skill [3 sec]"))
        {
            ple.CastSkill(3f);
        }
        if (GUILayout.Button("Cast Skill [5 sec]"))
        {
            ple.CastSkill(5f);
        }
        if (GUILayout.Button("Cast Skill [10 sec]"))
        {
            ple.CastSkill(10f);
        }
        if (GUILayout.Button("Cinematic"))
        {
            ple.CutsceneTurnOn = !ple.CutsceneTurnOn;
        }

        base.OnInspectorGUI();
    }
}
#endif