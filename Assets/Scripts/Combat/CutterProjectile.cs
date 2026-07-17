using UnityEngine;

namespace MB
{
    /// Boomerang blade: flies out, then curves back to its owner.
    public class CutterProjectile : Projectile
    {
        public Transform owner;
        public float outSpeed = 11f;
        public float returnSpeed = 13f;
        public float outTime = 0.32f;

        float t;
        bool returning;

        protected override void Update()
        {
            base.Update();
            if (this == null || deflected) return;

            t += Time.deltaTime;
            transform.Rotate(0, 0, 720f * Time.deltaTime);

            if (!returning && t >= outTime) returning = true;

            if (returning)
            {
                if (owner == null) { Destroy(gameObject); return; }
                Vector2 target = (Vector2)owner.position + Vector2.up * 0.9f;
                Vector2 dir = (target - (Vector2)transform.position);
                if (dir.magnitude < 0.5f) { Destroy(gameObject); return; }
                rb.linearVelocity = dir.normalized * returnSpeed;
            }
        }
    }
}
