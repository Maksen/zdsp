using UnityEngine;
using UnityEngine.UI;

public class GameIconCmpt_Upgrade : MonoBehaviour
{
    [SerializeField]
    Text txtUpgrade = null;

    public void SetUpgradeCount(int count)
    {
        txtUpgrade.text = (count > 0) ? string.Format("+{0}", 0) : "";
    }
}
