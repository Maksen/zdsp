using System.Collections;
using UnityEngine;


public class UI_MoveToObj : MonoBehaviour {

    public Transform ObjectToMove;
    public float speed = 2.0f;
    public bool DoRotation = false;

    public void MoveTo(Transform TargetPos)
    {
        StopAllCoroutines();
        StartCoroutine(LerpFromTo(ObjectToMove.position, TargetPos.position, ObjectToMove.rotation, TargetPos.rotation, speed));
    }

    IEnumerator LerpFromTo(Vector3 startpos, Vector3 endpos, Quaternion startrot, Quaternion endrot, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            //transform.position = Vector3.Lerp(startpos, endpos, t / duration);
            ObjectToMove.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t / duration)));

            if (DoRotation)
            {
                //transform.rotation = Quaternion.Slerp(startrot, endrot, t / duration);
                ObjectToMove.rotation = Quaternion.Slerp(startrot, endrot, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t / duration)));
            }
            
            yield return 0;
        }
        ObjectToMove.position = endpos;
        ObjectToMove.rotation = endrot;
    }
}
