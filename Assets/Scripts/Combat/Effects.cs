using UnityEngine;

namespace MB
{
    /// Expanding, fading explosion puff.
    public class ExplosionFx : MonoBehaviour
    {
        public float duration = 0.35f;
        public float startScale = 0.6f;
        public float endScale = 2.2f;
        float t;
        SpriteRenderer sr;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            t += Time.deltaTime;
            float k = t / duration;
            if (k >= 1f) { Destroy(gameObject); return; }
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
            if (sr != null)
            {
                var c = sr.color;
                c.a = 1f - k;
                sr.color = c;
            }
        }
    }

    /// One orb of the classic death explosion ring.
    public class DeathOrb : MonoBehaviour
    {
        public Vector2 velocity;
        float life = 2.2f;
        SpriteRenderer sr;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            transform.position += (Vector3)(velocity * Time.deltaTime);
            if (sr != null) sr.enabled = Mathf.Repeat(Time.time, 0.1f) < 0.07f;
        }
    }
}
