using UnityEngine;

namespace MB
{
    /// Serialization-safe trigger for boss doors (lives on a child collider).
    public class BossDoorTrigger : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var door = GetComponentInParent<BossDoor>();
            if (door != null) door.OnPlayerApproach(other);
        }
    }
}
