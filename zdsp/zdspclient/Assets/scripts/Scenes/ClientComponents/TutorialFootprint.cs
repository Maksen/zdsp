using UnityEngine;
using System.Collections;

public class TutorialFootprint : MonoBehaviour {

    public int footprintIndex;
    // Use this for initialization
    void Awake()
    {
        TrainingRealmContoller.Instance.RegisterFootprint(this);
        //gameObject.SetActive(false);//turn off on start
    }
}
