using UnityEngine;

namespace Zealot.Spawners
{
    public class PositionHelper : MonoBehaviour {
        [Tooltip("Radius measured from the monster within which it will be able detect any threats")]
        public float aggroRadius;
        [Tooltip("Radius measured from the spawner within which it is free to engage in combat")]
        public float combatRadius;

        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        void OnDrawGizmosSelected()
        {
            Color color = Gizmos.color;
            if (aggroRadius > 0)
                DrawCircle(transform.position, aggroRadius, Color.green);
            if (combatRadius > 0)
                DrawCircle(transform.position, combatRadius, Color.red);
            Gizmos.color = color;
        }

        void DrawCircle(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            float theta = 0f;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = center + new Vector3(x, 2, y);
            Vector3 newPos = pos;
            Vector3 lastPos = pos;
            for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = center + new Vector3(x, 2, y);
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }
    }
}