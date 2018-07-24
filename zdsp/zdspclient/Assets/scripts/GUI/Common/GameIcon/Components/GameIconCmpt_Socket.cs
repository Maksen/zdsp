using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class GameIconCmpt_Socket : MonoBehaviour
{
    [SerializeField]
    Image[] imgSockets = null;

    [SerializeField]
    Sprite[] spriteSocketStatus = null; // 0 = open, 1 = socketed

    public void SetSocketCount(byte count)
    {
        imgSockets[0].gameObject.SetActive(count > 0);
        imgSockets[1].gameObject.SetActive(count > 1);
    }

    public void SetSocketSlots(byte socketSlot)
    {
        imgSockets[0].sprite = GameUtils.IsBitSet(socketSlot, 0) ? spriteSocketStatus[1] : spriteSocketStatus[0];
        imgSockets[1].sprite = GameUtils.IsBitSet(socketSlot, 1) ? spriteSocketStatus[1] : spriteSocketStatus[0];
    }
}
