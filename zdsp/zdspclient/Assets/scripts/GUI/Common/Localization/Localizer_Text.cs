using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Localizer_Text : MonoBehaviour {

    // Text component to edit
    private Text text;

    // String to localize
    public string localizeText = "com_defaulttext";

    // Use this for initialization
    void Start () {

        bool resText = CheckForText();
        //bool resTabs = false;

        if(resText == false)
        {
            Debug.Log("No Text found! Please make sure the component you attach to exists!");

            return;
        }

        if(resText)
        {
            // Update text
            text.text = GUILocalizationRepo.GetLocalizedString(localizeText);
        }
	}

    bool CheckForText()
    {
        text = gameObject.GetComponent<Text>();

        if (text == null)
        {
            text = gameObject.GetComponentInChildren<Text>();
        }

        if (text == null)
        {
            // TEXT COMPONENT NOT FOUND!
            Debug.Log("Text component not found!");

            return false;
        }

        return true;
    }

	// Update is called once per frame
	/*void Update () {
	
	}*/
}
