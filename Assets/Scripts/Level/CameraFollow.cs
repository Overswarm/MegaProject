using System.Collections;
using UnityEngine;

namespace MB
{
    /// Follows the player clamped inside stage bounds; supports the boss-room
    /// pan (bounds transition) used by the final boss door.
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Rect bounds = new Rect(-1000, -1000, 2000, 2000);

        Camera cam;
        bool transitioning;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;   // 15 tiles tall, NES-ish framing
        }

        void LateUpdate()
        {
            if (transitioning || target == null) return;
            Vector3 want = new Vector3(target.position.x, target.position.y + 1.2f, -10f);
            transform.position = Clamp(want, bounds);
        }

        Vector3 Clamp(Vector3 pos, Rect b)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float x = (b.width <= halfW * 2f)
                ? b.center.x
                : Mathf.Clamp(pos.x, b.xMin + halfW, b.xMax - halfW);
            float y = (b.height <= halfH * 2f)
                ? b.center.y
                : Mathf.Clamp(pos.y, b.yMin + halfH, b.yMax - halfH);
            return new Vector3(x, y, -10f);
        }

        public void SnapTo(Rect newBounds)
        {
            bounds = newBounds;
            if (target != null)
                transform.position = Clamp(new Vector3(target.position.x, target.position.y + 1.2f, -10f), bounds);
        }

        public IEnumerator TransitionTo(Rect newBounds, float duration)
        {
            transitioning = true;
            Vector3 from = transform.position;
            Vector3 to = Clamp(target != null
                ? new Vector3(target.position.x, target.position.y + 1.2f, -10f)
                : from, newBounds);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / duration));
                yield return null;
            }
            bounds = newBounds;
            transitioning = false;
        }
    }
}
