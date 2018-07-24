using UnityEngine;

public class ClearSight : MonoBehaviour
{
    private int layerMask;
    private ZDSPCamera mainCam;

    void Awake()
    {
        int layerIndex = LayerMask.NameToLayer("Hideable");
        layerMask = 1 << layerIndex;

        layerIndex = LayerMask.NameToLayer("Hideable and Nav_Collision");
        layerMask |= 1 << layerIndex;
        //layerMask = ~layerMask;

        mainCam = gameObject.GetComponent<ZDSPCamera>();
    }

    void Update()
    {
        if (mainCam.targetObject == null)
            return;

        Vector3 playerCenter = mainCam.targetObject.transform.position + new Vector3(0, mainCam.Height, 0);
        Vector3 direction = transform.position - playerCenter;

        RaycastHit[] hits;
        Ray ray = new Ray(playerCenter, direction);
        hits = Physics.RaycastAll(ray, mainCam.Distance, layerMask);

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null)
                continue; //no renderer attached

            AutoHide hideComponent = renderer.GetComponent<AutoHide>();
            if (hideComponent == null) // if no script is attached, attach one
            {
                hideComponent = renderer.gameObject.AddComponent<AutoHide>();
            }
            hideComponent.BeTransparent(renderer); // get called every frame to reset the falloff
        }
    }
}