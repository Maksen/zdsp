using UnityEngine;
using UnityEngine.UI;

public class QA_AddItem : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Button addItemBtn = transform.GetComponent<Button>();
        addItemBtn.onClick.AddListener(this.OnClick);
    }

	void OnClick()
    {
        GameObject itemIDInput = GameObject.Find("Item ID Input");

        if (itemIDInput != null)
        {
            Debug.Log(itemIDInput + " found!");
            InputField itemIDText = itemIDInput.GetComponentInChildren<InputField>();
            AddItem(itemIDText.text);
        }
    }

	private void AddItem(string itemID)
	{
        int itmID = int.Parse(itemID);
        Debug.Log("Adding Item! ID: " + itmID);

        // TODO: Retrieve data from DB
        // Add item to player inventory
    }
}
