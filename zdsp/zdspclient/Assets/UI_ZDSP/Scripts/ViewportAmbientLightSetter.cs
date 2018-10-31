using UnityEngine;

class ViewportAmbientLightSetter : MonoBehaviour
{
    [SerializeField] Color ambientLightColor = new Color(0.68f, 0.6905f, 0.8f);
    [SerializeField] float ambientIntensity = 1f;

    void OnEnable()
    {
        if (PhotonNetwork.connected && GameInfo.gCombat)
        {
            GameInfo.gCombat.SetUIAmbientLight(true, ambientLightColor, ambientIntensity);
        }
    }

    void OnDisable()
    {
        if (PhotonNetwork.connected && GameInfo.gCombat)
        {
            GameInfo.gCombat.SetUIAmbientLight(false, Color.white, 1);
        }
    }
}