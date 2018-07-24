using UnityEngine;
using UnityEngine.UI;

public class QA_Query : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Button queryStatsBtn = transform.GetComponent<Button>();
        queryStatsBtn.onClick.AddListener(this.OnClick);
    }
	
    void OnClick()
    {
        GameObject queryStatsInput = GameObject.Find("Player Name Input");

        if (queryStatsInput != null)
        {
            Debug.Log(queryStatsInput + " found!");
            InputField queryStatsText = queryStatsInput.GetComponentInChildren<InputField>();
            QueryStats(queryStatsText.text);
        }
    }

    private void QueryStats(string playerName)
    {
        //int itmID = int.Parse(playerName);
        Debug.Log("Querying Player Stats! Player Name: " + playerName);

        // TODO: Find player by name
        // Display data
    }
}
