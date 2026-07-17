using System.Collections.Generic;
using UnityEngine;

namespace MB
{
    /// Generates all placeholder sprites procedurally (pixel maps + drawing helpers).
    /// Sprites are cached and marked HideAndDontSave so they are never serialized
    /// into scenes -- AutoSprite / visual scripts re-assign them at load time.
    /// To use real art later: give the SpriteRenderer a real sprite and remove AutoSprite.
    public static class SpriteFactory
    {
        const float PPU = 16f;
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string key)
        {
            if (cache.TryGetValue(key, out var s) && s != null) return s;
            s = Build(key);
            cache[key] = s;
            return s;
        }

        // ---------- palette ----------
        static readonly Color32 K = new Color32(16, 16, 24, 255);     // outline
        static readonly Color32 W = new Color32(255, 255, 255, 255);
        static readonly Color32 L = new Color32(200, 200, 210, 255);
        static readonly Color32 D = new Color32(110, 110, 125, 255);
        static readonly Color32 S = new Color32(252, 216, 168, 255);  // skin
        static readonly Color32 R = new Color32(228, 68, 52, 255);    // red accent
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        static Dictionary<char, Color32> Pal() => new Dictionary<char, Color32>
        { { 'K', K }, { 'W', W }, { 'L', L }, { 'D', D }, { 'S', S }, { 'R', R } };

        // ---------- build dispatch ----------
        static Sprite Build(string key)
        {
            switch (key)
            {
                case "white": return Solid(1, 1, W, 0.5f, 0.5f);
                case "block": return BlockTile();
                case "ladder": return LadderTile();
                case "spike": return SpikeTile();
                case "door": return DoorTile();
                case "beam": return Beam();
                case "bar": return BarTexture();
                case "orb": return Circle(8, W, true);
                case "bullet": return Circle(5, W, true);
                case "bullet_mid": return Circle(8, W, true);
                case "bullet_full": return ChargeShot();
                case "eshot": return Circle(6, R, true);
                case "cutter": return Cutter();
                case "bomb": return Bomb();
                case "hp_small": return Pellet(6);
                case "hp_big": return Pellet(10);
                case "en_small": return Pellet(6);
                case "en_big": return Pellet(10);
                case "etank": return Tank('E');
                case "wtank": return Tank('W');
                case "oneup": return FromMap(OneUpMap, 0.5f, 0f);
                case "player_idle": return FromMap(PlayerIdle, 0.5f, 0f);
                case "player_run0": return FromMap(PlayerRun0, 0.5f, 0f);
                case "player_run1": return FromMap(PlayerRun1, 0.5f, 0f);
                case "player_jump": return FromMap(PlayerJump, 0.5f, 0f);
                case "player_slide": return FromMap(PlayerSlide, 0.5f, 0f);
                case "met_closed": return FromMap(MetClosed, 0.5f, 0f);
                case "met_open": return FromMap(MetOpen, 0.5f, 0f);
                case "walker_0": return FromMap(Walker0, 0.5f, 0f);
                case "walker_1": return FromMap(Walker1, 0.5f, 0f);
                case "flyer_0": return FromMap(Flyer0, 0.5f, 0.5f);
                case "flyer_1": return FromMap(Flyer1, 0.5f, 0.5f);
                case "boss_idle": return FromMap(BossIdle, 0.5f, 0f);
                default:
                    Debug.LogWarning($"SpriteFactory: unknown key '{key}'");
                    return Solid(1, 1, new Color32(255, 0, 255, 255), 0.5f, 0.5f);
            }
        }

        // ---------- low level helpers ----------
        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = Clear;
            t.SetPixels32(px);
            return t;
        }

        static Sprite ToSprite(Texture2D t, float px, float py)
        {
            t.Apply();
            var sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height),
                new Vector2(px, py), PPU, 0, SpriteMeshType.FullRect);
            sp.hideFlags = HideFlags.HideAndDontSave;
            return sp;
        }

        static Sprite Solid(int w, int h, Color32 c, float px, float py)
        {
            var t = NewTex(w, h);
            var arr = new Color32[w * h];
            for (int i = 0; i < arr.Length; i++) arr[i] = c;
            t.SetPixels32(arr);
            return ToSprite(t, px, py);
        }

        static Sprite FromMap(string[] rows, float px, float py)
        {
            var pal = Pal();
            int h = rows.Length, w = 0;
            foreach (var r in rows) w = Mathf.Max(w, r.Length);
            var t = NewTex(w, h);
            for (int y = 0; y < h; y++)
            {
                var row = rows[h - 1 - y];
                for (int x = 0; x < row.Length; x++)
                {
                    if (pal.TryGetValue(row[x], out var c)) t.SetPixel(x, y, c);
                }
            }
            return ToSprite(t, px, py);
        }

        static Sprite Circle(int d, Color32 c, bool outline)
        {
            var t = NewTex(d, d);
            float r = d / 2f - 0.2f, cx = d / 2f - 0.5f, cy = d / 2f - 0.5f;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= r - 1f) t.SetPixel(x, y, c);
                    else if (dist <= r && outline) t.SetPixel(x, y, K);
                }
            return ToSprite(t, 0.5f, 0.5f);
        }

        // ---------- tiles ----------
        static Sprite BlockTile()
        {
            // brick-ish tile drawn in grayscale; theme color applied via SpriteRenderer tint
            var t = NewTex(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color32 c = L;
                    bool mortarH = (y % 8) == 7;
                    int shift = ((y / 8) % 2) * 8;
                    bool mortarV = ((x + shift) % 16) == 15;
                    if (mortarH || mortarV) c = K;
                    else if ((y % 8) == 6 || ((x + shift) % 16) == 0) c = D;
                    else if ((y % 8) == 0) c = W;
                    t.SetPixel(x, y, c);
                }
            return ToSprite(t, 0f, 0f); // pivot bottom-left so blocks position by corner
        }

        static Sprite LadderTile()
        {
            var t = NewTex(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool rail = (x >= 1 && x <= 2) || (x >= 13 && x <= 14);
                    bool rung = (y >= 3 && y <= 4) || (y >= 11 && y <= 12);
                    if (rail) t.SetPixel(x, y, y % 4 == 0 ? D : L);
                    else if (rung && x >= 3 && x <= 12) t.SetPixel(x, y, D);
                }
            return ToSprite(t, 0.5f, 0f);
        }

        static Sprite SpikeTile()
        {
            var t = NewTex(16, 8);
            for (int s = 0; s < 2; s++)
            {
                int baseX = s * 8;
                for (int y = 0; y < 8; y++)
                {
                    int half = (7 - y) / 2 + 1;
                    for (int x = 4 - half; x < 4 + half; x++)
                    {
                        int gx = baseX + x;
                        if (gx < 0 || gx > 15) continue;
                        t.SetPixel(gx, y, y >= 6 ? W : (x < 4 ? L : D));
                    }
                }
            }
            return ToSprite(t, 0.5f, 0f);
        }

        static Sprite DoorTile()
        {
            var t = NewTex(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color32 c = D;
                    if (x == 0 || x == 15 || y == 0 || y == 15) c = K;
                    else if (x >= 5 && x <= 10 && y >= 5 && y <= 10) c = R;
                    else if (x == 1 || y == 1) c = L;
                    t.SetPixel(x, y, c);
                }
            return ToSprite(t, 0f, 0f);
        }

        static Sprite Beam()
        {
            var t = NewTex(6, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 6; x++)
                    t.SetPixel(x, y, (x >= 2 && x <= 3) ? W : L);
            return ToSprite(t, 0.5f, 0f);
        }

        static Sprite BarTexture()
        {
            // 8 x 56: 28 ticks of 2px (1 lit, 1 gap) for HP/energy bars
            var t = NewTex(8, 56);
            for (int y = 0; y < 56; y++)
                for (int x = 0; x < 8; x++)
                    t.SetPixel(x, y, (y % 2 == 0) ? W : Clear);
            return ToSprite(t, 0.5f, 0f);
        }

        static Sprite ChargeShot()
        {
            var t = NewTex(12, 12);
            float cx = 5.5f, cy = 5.5f;
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= 3f) t.SetPixel(x, y, W);
                    else if (d <= 4.4f) t.SetPixel(x, y, L);
                    else if (d <= 5.6f) t.SetPixel(x, y, ((x + y) % 2 == 0) ? W : Clear);
                }
            return ToSprite(t, 0.5f, 0.5f);
        }

        static Sprite Cutter()
        {
            // crescent: big circle minus offset circle
            var t = NewTex(12, 12);
            float cx = 5.5f, cy = 5.5f;
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    float d1 = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float d2 = Mathf.Sqrt((x - cx - 3) * (x - cx - 3) + (y - cy) * (y - cy));
                    if (d1 <= 5.4f && d2 > 4.2f)
                        t.SetPixel(x, y, d1 > 4.4f ? L : W);
                }
            return ToSprite(t, 0.5f, 0.5f);
        }

        static Sprite Bomb()
        {
            var t = NewTex(8, 11);
            float cx = 3.5f, cy = 3.5f;
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= 3f) t.SetPixel(x, y, x + y < 7 ? L : D);
                    else if (d <= 3.8f) t.SetPixel(x, y, K);
                }
            t.SetPixel(3, 8, K); t.SetPixel(4, 8, K);
            t.SetPixel(4, 9, D); t.SetPixel(5, 10, W); // fuse spark
            return ToSprite(t, 0.5f, 0.5f);
        }

        static Sprite Pellet(int d)
        {
            var t = NewTex(d, d);
            float r = d / 2f - 0.3f, cx = d / 2f - 0.5f, cy = d / 2f - 0.5f;
            for (int y = 0; y < d; y++)
                for (int x = 0; x < d; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= r - 1.2f) t.SetPixel(x, y, ((x + y) % 2 == 0) ? W : L);
                    else if (dist <= r) t.SetPixel(x, y, K);
                }
            return ToSprite(t, 0.5f, 0f);
        }

        static Sprite Tank(char letter)
        {
            var t = NewTex(10, 12);
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 10; x++)
                {
                    if (x == 0 || x == 9 || y == 0 || y == 11) t.SetPixel(x, y, K);
                    else t.SetPixel(x, y, D);
                }
            // 5x7 letter block starting at (2,2), drawn in white
            bool[,] glyph = LetterGlyph(letter);
            for (int gy = 0; gy < 7; gy++)
                for (int gx = 0; gx < 5; gx++)
                    if (glyph[gy, gx]) t.SetPixel(2 + gx, 9 - gy, W);
            return ToSprite(t, 0.5f, 0f);
        }

        static bool[,] LetterGlyph(char c)
        {
            string[] rows;
            if (c == 'E')
                rows = new[] { "XXXXX", "X....", "X....", "XXXX.", "X....", "X....", "XXXXX" };
            else
                rows = new[] { "X...X", "X...X", "X...X", "X.X.X", "X.X.X", "XX.XX", "X...X" };
            var g = new bool[7, 5];
            for (int y = 0; y < 7; y++)
                for (int x = 0; x < 5; x++)
                    g[y, x] = rows[y][x] == 'X';
            return g;
        }

        // ---------- pixel maps (top row first) ----------
        static readonly string[] PlayerIdle =
        {
            "....KKKK....",
            "..KKWWWWKK..",
            ".KWWWWWWWWK.",
            ".KWWKKKKWWK.",
            ".KWKSSSSKWK.",
            ".KWKSKKSKWK.",
            ".KWKSSSSKWK.",
            "..KKSSSSKK..",
            "..KDWWWWDK..",
            ".KDWWWWWWDK.",
            ".KWWDWWDWWK.",
            ".KWWDWWDWWK.",
            "..KWWWWWWK..",
            "..KDWWWWDK..",
            "..KDKKKKDK..",
            "..KWWKKWWK..",
            ".KWWK..KWWK.",
            ".KWWK..KWWK.",
            ".KDDK..KDDK.",
            ".KKKK..KKKK.",
        };

        static readonly string[] PlayerRun0 =
        {
            "....KKKK....",
            "..KKWWWWKK..",
            ".KWWWWWWWWK.",
            ".KWWKKKKWWK.",
            ".KWKSSSSKWK.",
            ".KWKSKKSKWK.",
            ".KWKSSSSKWK.",
            "..KKSSSSKK..",
            "..KDWWWWDK..",
            ".KDWWWWWWDK.",
            ".KWWDWWDWWK.",
            ".KWWDWWDWWK.",
            "..KWWWWWWK..",
            "..KDWWWWDK..",
            "..KDKKKKDK..",
            "..KWWKWWK...",
            ".KWWK.KWWK..",
            "KWWK...KWWK.",
            "KDDK...KDDK.",
            "KKKK...KKKK.",
        };

        static readonly string[] PlayerRun1 =
        {
            "....KKKK....",
            "..KKWWWWKK..",
            ".KWWWWWWWWK.",
            ".KWWKKKKWWK.",
            ".KWKSSSSKWK.",
            ".KWKSKKSKWK.",
            ".KWKSSSSKWK.",
            "..KKSSSSKK..",
            "..KDWWWWDK..",
            ".KDWWWWWWDK.",
            ".KWWDWWDWWK.",
            ".KWWDWWDWWK.",
            "..KWWWWWWK..",
            "..KDWWWWDK..",
            "..KDKKKKDK..",
            "...KWWWWK...",
            "...KWWWWK...",
            "...KWKKWK...",
            "...KDKKDK...",
            "...KKKKKK...",
        };

        static readonly string[] PlayerJump =
        {
            "....KKKK....",
            "..KKWWWWKK..",
            ".KWWWWWWWWK.",
            ".KWWKKKKWWK.",
            ".KWKSSSSKWK.",
            ".KWKSKKSKWK.",
            ".KWKSSSSKWK.",
            "..KKSSSSKK..",
            ".KKDWWWWDKK.",
            "KWDWWWWWWDWK",
            "KWWDWWDWWWWK",
            ".KWDWWDWWKK.",
            "..KWWWWWWK..",
            "..KDWWWWDK..",
            "..KDKKKKDK..",
            ".KWWK..KWWK.",
            ".KWWK...KWWK",
            "KWWK.....KDK",
            "KDDK......KK",
            "KKKK........",
        };

        static readonly string[] PlayerSlide =
        {
            "..........KKKK......",
            "........KKWWWWKK....",
            ".......KWWWWWWWWK...",
            ".......KWWKKKKWWK...",
            ".......KWKSSSSKWK...",
            ".......KWKSKKSKWK...",
            "..KKKKKKWKSSSSKWK...",
            ".KWWWWWWKKSSSSKK....",
            "KWWDWWDWWWWWWDKK....",
            "KWWDWWDWWWWWWWWDKK..",
            ".KDDWWWWDDWWWWDDWWK.",
            ".KKKKKKKKKKKKKKKKKK.",
        };

        static readonly string[] MetClosed =
        {
            "...KKKKKK...",
            "..KWWWWWWK..",
            ".KWWWWWWWWK.",
            "KWWLWWWWLWWK",
            "KWWWWWWWWWWK",
            "KLLLLLLLLLLK",
            ".KKKKKKKKKK.",
            "..KKKKKKKK..",
            ".KK__KK__KK.".Replace('_', 'K'),
            "............",
        };

        static readonly string[] MetOpen =
        {
            "...KKKKKK...",
            "..KWWWWWWK..",
            ".KWWWWWWWWK.",
            "KWWLWWWWLWWK",
            "KLLLLLLLLLLK",
            ".KKKKKKKKKK.",
            ".KSKKSSKKSK.",
            ".KSSSSSSSSK.",
            "..KKKKKKKK..",
            ".KK.KK.KK...",
        };

        static readonly string[] Walker0 =
        {
            "..KKKKKKKK..",
            ".KWWWWWWWWK.",
            ".KWKKWWWWWK.",
            ".KWKKWWLLWK.",
            ".KWWWWWLLWK.",
            "..KKKKKKKK..",
            "...KK..KK...",
            "..KK....KK..",
            "..KK....KK..",
            ".KKK....KKK.",
        };

        static readonly string[] Walker1 =
        {
            "..KKKKKKKK..",
            ".KWWWWWWWWK.",
            ".KWKKWWWWWK.",
            ".KWKKWWLLWK.",
            ".KWWWWWLLWK.",
            "..KKKKKKKK..",
            "....KK.KK...",
            "....KK.KK...",
            "...KK...KK..",
            "..KKK...KKK.",
        };

        static readonly string[] Flyer0 =
        {
            "KK........KK",
            "KWK......KWK",
            "KWWK.KK.KWWK",
            ".KWWKWWKWWK.",
            "..KWWWWWWK..",
            "...KWKKWK...",
            "...KKKKKK...",
            "............",
        };

        static readonly string[] Flyer1 =
        {
            "............",
            "....KKKK....",
            "..KKWWWWKK..",
            ".KWWWWWWWWK.",
            "KWWK.KK.KWWK",
            "KWK..KK..KWK",
            "KK..KWWK..KK",
            "....KKKK....",
        };

        static readonly string[] BossIdle =
        {
            "....KKKKKKKK....",
            "...KWWWWWWWWK...",
            "..KWWKKKKKKWWK..",
            "..KWKWWWWWWKWK..",
            "..KWKWKKKKWKWK..",
            "..KWKWWWWWWKWK..",
            "..KWWKKKKKKWWK..",
            "...KWWWWWWWWK...",
            ".KKKWWWWWWWWKKK.",
            "KWWKWDDDDDDWKWWK",
            "KWWKWDWWWWDWKWWK",
            "KWWKWDWWWWDWKWWK",
            ".KKKWDDDDDDWKKK.",
            "...KWWWWWWWWK...",
            "...KWDKKKKDWK...",
            "...KWDWWWWDWK...",
            "..KKWWKWWKWWKK..",
            "..KWWWK..KWWWK..",
            "..KWWK....KWWK..",
            "..KDDK....KDDK..",
            ".KKKKK....KKKKK.",
        };

        static readonly string[] OneUpMap =
        {
            "..KKKK..",
            ".KWWWWK.",
            "KWWWWWWK",
            "KWKWWKWK",
            "KWWWWWWK",
            ".KSSSSK.",
            "..KKKK..",
        };
    }
}
