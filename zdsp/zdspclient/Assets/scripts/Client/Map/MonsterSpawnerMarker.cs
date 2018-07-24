using UnityEngine;


namespace Zealot.Spawners
{
    [AddComponentMenu("Markers at Client/MonsterSpawnerMarker")]
    public class MonsterSpawnerMarker : MonoBehaviour
    {
        [Tooltip("Archetype Link to gamedb NPC table.")]
        public string archetype;
        [Tooltip("ArchetypeGroup Link to gamedb NPCGroup table.")]
        public string archetypeGroup;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 3.0f);
        }
    }
}
