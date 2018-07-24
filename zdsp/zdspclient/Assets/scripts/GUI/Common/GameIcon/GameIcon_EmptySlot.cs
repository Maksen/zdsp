using UnityEngine;

public class GameIcon_EmptySlot : MonoBehaviour
{
    [SerializeField]
    GameObject imgLocked = null;

    bool isLocked = true;

    void Awake()
    {
        imgLocked.SetActive(isLocked);
    }

    public void SetLock(bool isLocked)
    {
        this.isLocked = isLocked;
        imgLocked.SetActive(isLocked);
    }
}
