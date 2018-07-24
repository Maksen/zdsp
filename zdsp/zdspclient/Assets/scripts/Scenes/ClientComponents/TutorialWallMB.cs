using UnityEngine;
using System.Collections;

public class TutorialWallMB : MonoBehaviour {
    public int wallIndex;
	// Use this for initialization
	void Start () {
        TrainingRealmContoller.Instance.RegisterTutorialWalls(this);
	}
	 
}
