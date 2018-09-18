using UnityEngine;

public class ZDSPStaticCamera : ZDSPCamera
{
    public void InitTarget(GameObject target)
    {
        targetObject = target;
        cameraTarget = targetObject.transform.position;
    }

    private void LateUpdate()
    {
        UpdateZoom();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        cameraTarget = targetObject.transform.position;
        transform.position = cameraTarget + Vector3.up * Height;
        transform.Translate(Vector3.back * Distance);
    }

    private void UpdateZoom()
    {
        float deltaZoom = 0f;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        deltaZoom -= scroll * 300f;
        
        if (deltaZoom != 0)
        {
            Distance += deltaZoom * Time.unscaledDeltaTime * zoomSpeed;
            Distance = Mathf.Clamp(Distance, minDistance, maxDistance);

            // lower height when distance is closer, higher when further
            float t = Mathf.InverseLerp(minDistance, maxDistance, Distance);
            Height = Mathf.Lerp(minHeight, maxHeight, t);
        }
    }
}