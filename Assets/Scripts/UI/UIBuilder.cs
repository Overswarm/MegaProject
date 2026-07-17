using UnityEngine;
using UnityEngine.UI;

namespace MB
{
    /// Code-built uGUI helpers (no prefabs, no TextMeshPro dependency).
    /// Reference resolution is 426x240 -- a 16:9 take on the NES's 240 lines.
    public static class UIBuilder
    {
        public static Font PixelFont =>
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Canvas NewCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortOrder;
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(426, 240);
            s.matchWidthOrHeight = 1f;
            return c;
        }

        static RectTransform Rect(GameObject go, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Text AddText(Transform parent, string str, int size, Vector2 anchor,
            Vector2 pos, Vector2 rectSize, TextAnchor align, Color color)
        {
            var go = new GameObject("Text");
            Rect(go, parent, anchor, pos, rectSize);
            var t = go.AddComponent<Text>();
            t.font = PixelFont;
            t.fontSize = size;
            t.text = str;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Image AddImage(Transform parent, string spriteKey, Vector2 anchor,
            Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject("Image");
            Rect(go, parent, anchor, pos, size);
            var img = go.AddComponent<Image>();
            if (!string.IsNullOrEmpty(spriteKey)) img.sprite = SpriteFactory.Get(spriteKey);
            img.color = color;
            return img;
        }

        /// Segmented 28-tick bar (vertical by default; rotated = horizontal,
        /// filling left to right).
        public static Image AddBar(Transform parent, Vector2 anchor, Vector2 pos,
            Color color, bool rotated = false)
        {
            var bg = AddImage(parent, null, anchor, pos, new Vector2(10, 58), Color.black);
            var go = new GameObject("Fill");
            var rt = Rect(go, bg.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8, 56));
            var img = go.AddComponent<Image>();
            img.sprite = SpriteFactory.Get("bar");
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Vertical;
            img.fillOrigin = (int)Image.OriginVertical.Bottom;
            if (rotated)
                bg.rectTransform.localEulerAngles = new Vector3(0, 0, -90);
            return img;
        }
    }
}
