using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainingSkillButtonStepMB : MonoBehaviour {

    // Use this for initialization
    [SerializeField]
    public GameObject step4;
    [SerializeField]
    public GameObject step5;
    [SerializeField]
    public GameObject step6;
    [SerializeField]
    public List<GameObject> step7;
    [SerializeField]
    public GameObject root;
    [SerializeField]
    public List<GameObject> notHides;

    public void OnStep(int step)
    {
        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject thechild = root.transform.GetChild(i).gameObject;
            if (notHides!=null && notHides.Contains(thechild))
            {
                continue;
            }
            thechild.SetActive(false); 
        }
        if (step > 3)
        {
            step4.SetActive(true);
        }
        if (step >4)
        {
            step5.SetActive(true);
        }
        if (step > 5)
        {
            step6.SetActive(true);
        }
        if (step > 6)
        {
            if (step7 != null)
            {
                foreach(GameObject go in step7)
                {
                    go.SetActive(true);
                }
            }
            
        }
    }
}
