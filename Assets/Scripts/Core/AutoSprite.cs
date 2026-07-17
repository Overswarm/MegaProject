using UnityEngine;

namespace MB
{
    /// Assigns a procedurally generated sprite to this object's SpriteRenderer,
    /// both at runtime and in the editor (so generated scenes are visible while
    /// level editing). Because SpriteFactory sprites are HideAndDontSave, nothing
    /// is serialized into the scene -- this component restores the sprite on load.
    /// Replace with real art by removing this component and assigning a sprite.
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class AutoSprite : MonoBehaviour
    {
        public string key = "white";
        public Color tint = Color.white;
        public int sortingOrder;
        public Vector2 tiledSize = Vector2.zero;   // if non-zero, uses Tiled draw mode

        void OnEnable() { Refresh(); }
#if UNITY_EDITOR
        void Update() { if (!Application.isPlaying) Refresh(); }
#endif

        public void Refresh()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
            if (sr.sprite == null || sr.sprite.name == "") sr.sprite = SpriteFactory.Get(key);
            if (sr.sprite == null) sr.sprite = SpriteFactory.Get(key);
            sr.color = tint;
            sr.sortingOrder = sortingOrder;
            if (tiledSize != Vector2.zero)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = tiledSize;
            }
        }

        public static AutoSprite Attach(GameObject go, string key, Color tint, int order, Vector2? tiled = null)
        {
            if (go.GetComponent<SpriteRenderer>() == null) go.AddComponent<SpriteRenderer>();
            var a = go.GetComponent<AutoSprite>();
            if (a == null) a = go.AddComponent<AutoSprite>();
            a.key = key; a.tint = tint; a.sortingOrder = order;
            a.tiledSize = tiled ?? Vector2.zero;
            a.Refresh();
            return a;
        }
    }
}
