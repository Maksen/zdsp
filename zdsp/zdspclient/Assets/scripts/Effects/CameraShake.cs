using EZCameraShake;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Only shake for local player.")]
    bool local = true;

    [SerializeField]
    [Tooltip("Delay in seconds before starting the shake.")]
    float startDelay = 0f;

    [SerializeField]
    [Tooltip("How much influence this shake has over the local position axes of the camera.")]
    Vector3 positionInfluence = new Vector3(0.15f, 0.15f, 0.15f);

    [SerializeField]
    [Tooltip("How much influence this shake has over the local rotation axes of the camera.")]
    Vector3 rotationInfluence = new Vector3(1f, 1f, 1f);

    [SerializeField]
    [Tooltip("Intensity of the shake.")]
    float magnitude = 1f;

    [SerializeField]
    [Tooltip("Roughness of the shake.")]
    float roughness = 1f;

    [SerializeField]
    [Tooltip("Time in seconds for the shake to reach peak intensity.")]
    float fadeInTime = 0.1f;

    [SerializeField]
    [Tooltip("Time in seconds to take for the shake to fade out after reaching peak intensity.")]
    float fadeOutTime = 1f;

    private GameObject parentObject;

    public void ShakeOnce()
    {
        if (CameraShaker.Instance != null && CanShake())
            CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadeInTime, fadeOutTime, positionInfluence, rotationInfluence, startDelay);
    }

    private bool CanShake()
    {
        if (parentObject == null)
        {
            CharacterController cctrl = GetComponentInParent<CharacterController>();
            if (cctrl != null)
                parentObject = cctrl.gameObject;
        }

        if (parentObject != null)
        {
            if (local)
            {
#if UNITY_EDITOR
                if (GameInfo.gCombat == null)  // for non-ingame in editor
                    return true;
#endif
                return parentObject.CompareTag("LocalPlayer");
            }
            else
                return true;
        }
        return true;
    }
}