using UnityEngine;

namespace MB
{
    /// Place these in levels instead of raw enemies. Spawns its enemy on load
    /// and respawns it NES-style: once the dead enemy's spawn point has gone
    /// off screen and comes back on, the enemy is back.
    public class EnemySpawner : MonoBehaviour
    {
        public EnemyKind kind = EnemyKind.Met;
        public int facing = -1;

        GameObject current;
        bool dead;
        bool armed;

        void Start()
        {
            // hide the editor preview sprite while playing
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            Spawn();
        }

        void Spawn()
        {
            current = EntityFactory.CreateEnemy(kind, transform.position, facing);
            current.transform.SetParent(transform.parent, true);  // keep hierarchy tidy
            var e = current.GetComponent<EnemyBase>();
            if (e != null) e.onDeath += () => dead = true;
            dead = false;
            armed = false;
        }

        void Update()
        {
            if (!dead) return;

            var cam = Camera.main;
            if (cam == null) return;
            var v = cam.WorldToViewportPoint(transform.position);
            bool onscreen = v.x > -0.1f && v.x < 1.1f && v.y > -0.1f && v.y < 1.1f;

            if (!onscreen) armed = true;
            else if (armed)
            {
                var sc = StageController.I;
                if (sc != null && sc.Player != null &&
                    Vector2.Distance(sc.Player.transform.position, transform.position) < 3f)
                    return; // don't respawn on the player's face
                Spawn();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(1, 1, 0));
        }
    }
}
