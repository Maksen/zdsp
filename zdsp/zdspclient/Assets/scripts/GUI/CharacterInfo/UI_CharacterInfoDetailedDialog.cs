using UnityEngine;
using UnityEngine.UI;

using System.Collections;

/// <summary>
/// Used for UI_CharacterStats dialog when holding down a particular stats
/// Calculate the total stats value, and the percentage effect produced
/// Also, display all the sources that contribute to the total value
/// </summary>
public class UI_CharacterInfoDetailedDialog : MonoBehaviour
{
    #region Inspector linked var
    [SerializeField]
    Text _mStatName;
    [SerializeField]
    Text _mStatValue;
    [SerializeField]
    Text _mStatPercentage;

    //Need to accomdate all sources
    #endregion

    #region Property
    public string StatName
    {
        get { return _mStatName.text; }
        set { _mStatName.text = value; }
    }
    public int StatValue
    {
#if _ENABLE_GET_PROPERTY_
        get
        {
            int res;
            if (int.TryParse(_mStatValue.text, out res))
            {
                Debug.Log("UI_CharacterStatsDetailedDialog stat value get property: parse failed.");
                return -1;
            }
            return res;
        }
#endif
        set
        {
            value = Mathf.Max(0, value);
            _mStatValue.text = value.ToString();
        }
    }
    public int StatPercentage
    {
#if _ENABLE_GET_PROPERTY_
        get
        {
            int res;
            if (int.TryParse(_mStatPercentage.text, out res))
            {
                Debug.Log("UI_CharacterStatsDetailedDialog stat percentage get property: parse failed.");
                return -1;
            }
            return res;
        }
#endif
        set
        {
            value = Mathf.Max(0, value);
            _mStatPercentage.text = value.ToString();
        }
    }
    #endregion

    // Use this for initialization
    void Awake()
    {

    }
}
