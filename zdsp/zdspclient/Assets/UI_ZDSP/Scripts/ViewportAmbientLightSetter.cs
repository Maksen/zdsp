using UnityEngine;

class ViewportAmbientLightSetter : MonoBehaviour
{
    void OnEnable()
    {
        if(PhotonNetwork.connected && GameInfo.gCombat)
        {
            GameInfo.gCombat.SetUIAmbientLight(true);
        }
    }

    void OnDisable()
    {
        if (PhotonNetwork.connected && GameInfo.gCombat)
        {
            GameInfo.gCombat.SetUIAmbientLight(false);
        }
    }
}