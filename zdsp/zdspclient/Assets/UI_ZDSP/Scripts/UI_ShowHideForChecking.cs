using UnityEngine;
using UnityEngine.UI;

public class UI_ShowHideForChecking : MonoBehaviour
{
    public string ObjectName;

    public void ShowMe()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
            //print(go.name);
            if (go.name == ObjectName)
				 go.SetActive(!go.activeInHierarchy);
    }
}
