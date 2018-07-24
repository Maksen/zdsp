using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Common.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum DamageType
{
    Normal,
    Critical,
    DOT,    //Damage over time
    Heal,
    Miss,
    Dodge,
    Block,
};

/// <summary>
/// Ready to use single damage label
/// </summary>
public class HUD_DamageLabel : MonoBehaviour
{
    public delegate void DamageLabelCallback(GameObject dl);

    [SerializeField]
    Animator mAnimator;
    [SerializeField]
    Text mDmgInteger;

    [SerializeField]
    String mDodgeIdentifier = "b";
    [SerializeField]
    String mMissIdentifier = "a";
    String mStateName = "";

    [SerializeField]
    RectTransform mRectTrans;
    [HideInInspector]
    public Vector3 mOffset_WorldSpace;

    [Header("Animator State Names")]
    #region Animator State Names
    [SerializeField]
    String mNormalDmgSM;
    [SerializeField]
    String mNormalDmgPlayerSM;
    [SerializeField]
    String mDebuffBleedSM;
    //[SerializeField]
    //String mDebuffBleedPlayerSM;
    [SerializeField]
    String mHealSM;
    [SerializeField]
    String mCriticalSM;
    [SerializeField]
    String mTotalSM;
    [SerializeField]
    String mMissSM;
    [SerializeField]
    String mDodgeSM;
    //[SerializeField]
    //String mCriticalPlayerSM;
    #endregion

    ZDSPCamera mainCam;
    DamageLabelCallback dlCallBack;
    private int localplayerpid = 0;
    void Awake()
    {
        mainCam = GameInfo.gCombat.PlayerCamera.GetComponent<ZDSPCamera>();
        localplayerpid = GameInfo.gLocalPlayer.GetPersistentID();
    }

    private void UpdateAchorPos(Vector2 pos)
    {
        mRectTrans.anchoredPosition = pos;
    }

    public void Setup(AttackResult ar, Vector2 uipos, DamageLabelCallback dlcb)
    {
        UpdateAchorPos(uipos);
        bool isEnemy = true;
        dlCallBack = dlcb;

        if (ar.IsHeal)
        {
            mDmgInteger.text = ar.RealDamage.ToString();
            mStateName = mHealSM;
        }
        else if (ar.IsDot)
        {
            if (isEnemy)
            {
                mStateName = mDebuffBleedSM;
            }
            //else
            //{
            //    mStateName = mDebuffBleedPlayerSM;
            //}
            mDmgInteger.text = ar.RealDamage.ToString();
        }
        else if (ar.IsCritical)
        {
            //if (isEnemy)
            //{
                mStateName = mCriticalSM;
            //}
            //else
            //{
            //    mStateName = mCriticalPlayerSM;
            //}
            mDmgInteger.text = ar.RealDamage.ToString();
        }
        else if (ar.IsEvasion)
        {
            if (GameInfo.gLocalPlayer.GetPersistentID() == ar.TargetPID)
            {
                mStateName = mDodgeSM; //"Damage_Dodge";
                mDmgInteger.text = mDodgeIdentifier;
            }
            else
            {
                mStateName = mMissSM; //"Damage_Miss";
                mDmgInteger.text = mMissIdentifier;
            }
        }
        else if (ar.RealDamage > 0) //Normal dmg 
        {
            if (isEnemy)
            {
                mStateName = mNormalDmgSM;
            }
            if (localplayerpid == ar.TargetPID || ar.TargetPID == 0)
            {
                mStateName = mNormalDmgPlayerSM;
            }
            mDmgInteger.text = ar.RealDamage.ToString();
        }
        Play();
    }

    private void Play(float delay = 0f)
    {
        gameObject.SetActive(true);
        mAnimator.Play(mStateName);
    }

    public void EndPlay()
    {
        gameObject.SetActive(false);
        dlCallBack(this.gameObject);
    }
}

#if UNITY_EDITOR
//[CustomEditor(typeof(HUD_DamageLabel))]
//public class HUD_DamageLabelEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        HUD_DamageLabel dl = (HUD_DamageLabel)target;
//        if (GUILayout.Button("Normal Dmg"))
//        {
//            dl.Setup(DamageType.Normal, 9999);
//            dl.Play();
//        }
//        if (GUILayout.Button("Critical Dmg"))
//        {
//            dl.Setup(DamageType.Critical, 9999);
//            dl.Play();
//        }
//        if (GUILayout.Button("DOT Dmg"))
//        {
//            dl.Setup(DamageType.DOT, 9999);
//            dl.Play();
//        }
//        if (GUILayout.Button("Heal Dmg"))
//        {
//            dl.Setup(DamageType.Heal, 9999);
//            dl.Play();
//        }
//        if (GUILayout.Button("Miss Dmg"))
//        {
//            dl.Setup(DamageType.Miss, 9999);
//            dl.Play();
//        }
//        if (GUILayout.Button("Dodge Dmg"))
//        {
//            dl.Setup(DamageType.Dodge, 9999);
//            dl.Play();
//        }
//        DrawDefaultInspector();
//        base.OnInspectorGUI();
//    }
//}
#endif
