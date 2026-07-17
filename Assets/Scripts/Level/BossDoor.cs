using System.Collections;
using UnityEngine;

namespace MB
{
    /// Classic boss shutter: opens with a rattle, auto-walks the player
    /// through, closes behind. The final door also pans the camera into the
    /// boss room and starts the boss intro.
    public class BossDoor : MonoBehaviour
    {
        public bool isFinal;
        public Rect roomBounds;
        public BossController boss;
        public float doorHeight = 4f;

        [HideInInspector] public bool used;

        public void OnPlayerApproach(Collider2D other)
        {
            if (used || other.gameObject.layer != Layers.Player) return;
            var sc = StageController.I;
            if (sc == null || sc.CutsceneActive) return;
            used = true;
            sc.RunBossDoor(this);
        }

        public IEnumerator Open()
        {
            Sfx.Play(SfxId.Door);
            yield return Slide(Vector3.up * (doorHeight - 0.2f));
        }

        public IEnumerator Close()
        {
            Sfx.Play(SfxId.Door);
            yield return Slide(Vector3.down * (doorHeight - 0.2f));
        }

        IEnumerator Slide(Vector3 delta)
        {
            Vector3 from = transform.position;
            Vector3 to = from + delta;
            float t = 0f, dur = 0.8f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, t / dur);
                yield return null;
            }
            transform.position = to;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + new Vector3(0.5f, doorHeight / 2f, 0),
                new Vector3(1, doorHeight, 0));
            if (isFinal)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);
            }
        }
    }
}
