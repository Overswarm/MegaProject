using UnityEngine;

namespace MB
{
    /// Respawn point. Index 0 is the stage start; higher indices overwrite
    /// lower ones as the player passes them. Respawning reloads the scene and
    /// spawns at the highest checkpoint reached.
    public class Checkpoint : MonoBehaviour
    {
        public int index;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != Layers.Player) return;
            if (index > GameManager.I.CheckpointIndex)
                GameManager.I.CheckpointIndex = index;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.5f, 2f, 0));
        }
    }
}
