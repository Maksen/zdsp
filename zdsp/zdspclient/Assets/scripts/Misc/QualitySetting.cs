using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class QualitySetting : MonoBehaviour {
	void Awake()
	{
		GetComponent<Text> ().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
	}

	public void Upgrade()
	{
		QualitySettings.IncreaseLevel ();
		GetComponent<Text> ().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
	}

	public void DownGrade()
	{
		QualitySettings.DecreaseLevel ();
		GetComponent<Text> ().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
	}
}
