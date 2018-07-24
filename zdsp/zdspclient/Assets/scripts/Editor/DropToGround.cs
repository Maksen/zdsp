using UnityEngine;
using UnityEditor;
using System.Collections;

public class DropToGround : MonoBehaviour {
	[MenuItem ("Tools/Drop to Ground %&t")]
	static void Align() {
		Transform [] transforms = Selection.transforms;
		foreach (Transform myTransform in transforms) {
            RaycastHit[] hits = Physics.RaycastAll(myTransform.position + Vector3.up * 10, -Vector3.up);
            foreach (var hit in hits)
            {
                if (hit.collider.transform == myTransform)
                    continue;
                Vector3 targetPosition = hit.point;
                if (myTransform.gameObject.GetComponent<MeshFilter>() != null)
                {
                    Bounds bounds = myTransform.gameObject.GetComponent<MeshFilter>().sharedMesh.bounds;
                    targetPosition.y += bounds.extents.y;
                }
                myTransform.position = targetPosition;
                break;
            }
		}
	}
}
