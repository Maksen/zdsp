using UnityEngine;

public class Hud_DestinyNews : MonoBehaviour
{
    public void OnClickOk()
    {
        UIManager.OpenWindow(WindowType.Destiny);
    }
}
