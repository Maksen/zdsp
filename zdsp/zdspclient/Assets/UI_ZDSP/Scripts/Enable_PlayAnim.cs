using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enable_PlayAnim : MonoBehaviour {
    public Animator AnimatorName;
    public string StateName = null;

    void OnEnable () {
            AnimatorName.Play(StateName);
    }
}
