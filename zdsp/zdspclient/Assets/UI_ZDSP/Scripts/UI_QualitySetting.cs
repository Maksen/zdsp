using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_QualitySetting : MonoBehaviour
{

    private string[] qnames;
    private int i;
    private int qualityLevel;

    void Start()
    {
        qnames = QualitySettings.names;
        qualityLevel = QualitySettings.GetQualityLevel();
        i = qualityLevel;
        GetComponent<Text>().text = qnames[qualityLevel].ToString();
    }

    public void DecreaseQuality()
    {
        if (i > 0)
        {
            QualitySettings.DecreaseLevel(false);
            GetComponent<Text>().text = qnames[QualitySettings.GetQualityLevel()].ToString();
            i--;
        }

    }

    public void IncreaseQuality()
    {
        if (i < qnames.Length)
        {
            QualitySettings.IncreaseLevel(false);
            GetComponent<Text>().text = qnames[QualitySettings.GetQualityLevel()].ToString();
            i++;
        }
    }


}
