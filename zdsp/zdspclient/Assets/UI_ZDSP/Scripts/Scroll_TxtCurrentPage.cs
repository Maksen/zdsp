using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(Text))]
public class Scroll_TxtCurrentPage : MonoBehaviour {

    [SerializeField]
    string localString;

    [SerializeField]
    ScrollSnapBase scrollSnap;

    Text txtComp;
    int totalPage = 0;

    private void Awake()
    {
        txtComp = GetComponent<Text>();
    }
    // Use this for initialization
    void Start ()
    {
        scrollSnap.OnSelectionPageChangedEvent.AddListener(SetToggleGraphics);
        totalPage = scrollSnap.ChildObjects.Length;
    }
	
	// Update is called once per frame
	void SetToggleGraphics(int pageNum) {

        txtComp.text = string.Format(localString, pageNum+1 , totalPage);

    }
}
