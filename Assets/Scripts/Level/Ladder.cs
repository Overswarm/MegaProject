using UnityEngine;

namespace MB
{
    /// Climbable ladder zone. A one-way platform collider sits at the top
    /// (classic ladder-top behavior); PlayerController ignores it while climbing.
    public class Ladder : MonoBehaviour
    {
        public float height = 4f;
        public Collider2D TopPlatform;

        public float CenterX => transform.position.x;
        public float TopY => transform.position.y + height;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * height / 2f,
                new Vector3(1, height, 0));
        }
    }
}
