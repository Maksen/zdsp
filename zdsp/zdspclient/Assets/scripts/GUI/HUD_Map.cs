using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUD_Map : MonoBehaviour
{
    #region +++ Game Objects +++
    [Header("Game Object Shortcut")]
    [SerializeField]
    Text mTxtMapName;
    [SerializeField]
    Text mTxtServerName;
    [SerializeField]
    Button mBtnWorldMap;
    [SerializeField]
    Button mBtnClose;
    #endregion
    #region +++ Parent Game Objects +++
    [Header("Parents")]
    [SerializeField]
    GameObject mExpanderGO;
    [SerializeField]
    GameObject mTeleportGO;
    [SerializeField]
    GameObject mReviveGO;
    [SerializeField]
    GameObject mMonsterGO;
    [SerializeField]
    GameObject mQuestNPCGO;
    [SerializeField]
    GameObject mShopNPCGO;
    [SerializeField]
    GameObject mBossGO;
    [SerializeField]
    GameObject mMiniBossGO;
    [SerializeField]
    GameObject mPartyGO;
    [SerializeField]
    GameObject mPlayerGO;
    #endregion
    #region +++ Prefab +++
    [Header("Prefab")]
    [SerializeField]
    GameObject mExpanderPrefab;
    [SerializeField]
    GameObject mMapIconPrefab;
    #endregion
}
