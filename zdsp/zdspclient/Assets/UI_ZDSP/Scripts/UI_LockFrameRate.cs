using UnityEngine;
using UnityEngine.UI;

public class UI_LockFrameRate : MonoBehaviour
{
    bool isLocked;

    public void Locked()
    {
        isLocked = !isLocked;
        Application.targetFrameRate = isLocked ? Application.targetFrameRate = 30 : Application.targetFrameRate = 60;
        
    }
}
