using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoCharacterCreate : MonoBehaviour {

    public void DoIt()
    {
        SceneLoader.Instance.LoadLevel("UI_CreateChar");
    }
}
