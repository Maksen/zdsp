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

    //if true user will need to call Init() method manually (in case the content of the scrollview is generated from code or requires special initialization)
    [SerializeField]
    bool initByUser = false;

    Text txtComp;
    int totalPage = 0;

    private void Awake()
    {
        txtComp = GetComponent<Text>();
    }
    void Start ()
    {
        if (initByUser)
            return;

        scrollSnap.OnSelectionPageChangedEvent.AddListener(SetToggleGraphics);
        totalPage = scrollSnap.ChildObjects.Length;
    }

    public void Init()
    {
        scrollSnap.OnSelectionPageChangedEvent.RemoveListener(SetToggleGraphics);
        scrollSnap.OnSelectionPageChangedEvent.AddListener(SetToggleGraphics);
        totalPage = scrollSnap.ChildObjects.Length;
    }

	void SetToggleGraphics(int pageNum)
    {
        txtComp.text = string.Format(localString, pageNum+1 , totalPage);
    }


}
