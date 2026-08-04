using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Procedural placeholder sprites (point-filtered, pixel look). Every sprite here is a
    /// stand-in for an itchio asset — swap points are BoardView/UnitView, not game logic.
    /// </summary>
    public static class SpriteFactory
    {
        public static readonly Color Line = Hex("#0E0C16");
        public static readonly Color Cream = Hex("#F4E9D0");
        public static readonly Color Plum = Hex("#2A2438");
        public static readonly Color PlumDeep = Hex("#1A1826");
        public static readonly Color Gold = Hex("#FFD447");
        public static readonly Color HpRed = Hex("#E85D5D");
        public static readonly Color XpBlue = Hex("#5D9EE8");
        public static readonly Color Frozen = Hex("#7A8BA8");

        public static readonly Color[] LaneAccent =
        {
            Hex("#B8C0CC"), Hex("#6EE7F0"), Hex("#6EDB78"), Hex("#FFD447")
        };
        public static readonly Color[] LaneBiome =
        {
            Hex("#333844"), Hex("#274356"), Hex("#27412C"), Hex("#4F4027")
        };
        public static readonly string[] LaneNames = { "IRON", "DIAMOND", "EMERALD", "GOLD" };

        public static Color Hex(string s)
        {
            ColorUtility.TryParseHtmlString(s, out var c);
            return c;
        }

        static Sprite _hex, _hexOutline, _square, _circle, _blobBody, _blobFace;

        public static Sprite HexFill => _hex ?? (_hex = MakeHex(false));
        public static Sprite HexOutline => _hexOutline ?? (_hexOutline = MakeHex(true));
        public static Sprite Square => _square ?? (_square = MakeSquare());
        public static Sprite Circle => _circle ?? (_circle = MakeCircle());
        public static Sprite BlobBody => _blobBody ?? (_blobBody = MakeBlob(false));
        public static Sprite BlobFace => _blobFace ?? (_blobFace = MakeBlob(true));

        static Sprite FromTex(Texture2D tex, float ppu)
        {
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), ppu);
        }

        // Pointy-top hex, TFT-style: crisp opaque outline, translucent fill (both white = tintable).
        // Corners at 30°+60°k so the top/bottom are POINTS: width = sqrt(3)/2 * height. This is
        // the exact tessellation geometry — rows offset by half a hex interlock flush.
        static Sprite MakeHex(bool outlineOnly)
        {
            int h = 96;
            int w = Mathf.RoundToInt(h * Mathf.Sqrt(3f) / 2f); // 83
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Vector2 c = new Vector2(w / 2f, h / 2f);
            float size = h / 2f - 0.5f;

            Vector2[] corners = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float ang = Mathf.Deg2Rad * (60f * i + 30f); // pointy-top: points at 90° and 270°
                corners[i] = c + new Vector2(Mathf.Cos(ang) * size, Mathf.Sin(ang) * size);
            }

            var fill = new Color(1f, 1f, 1f, 0.16f); // transparent interior, outline carries the look
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    bool inside = PointInPoly(p, corners);
                    bool insideInner = PointInPoly(p, Shrink(corners, c, 0.915f));
                    Color col;
                    if (!inside) col = Color.clear;
                    else if (!insideInner) col = Color.white; // clean tintable outline
                    else col = outlineOnly ? Color.clear : fill;
                    tex.SetPixel(x, y, col);
                }
            // world height of a hex = 2 * HexUtil.Size
            return FromTex(tex, h / (2f * HexUtil.Size));
        }

        static Vector2[] Shrink(Vector2[] poly, Vector2 center, float f)
        {
            var r = new Vector2[poly.Length];
            for (int i = 0; i < poly.Length; i++) r[i] = center + (poly[i] - center) * f;
            return r;
        }

        static bool PointInPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }
            return inside;
        }

        static Sprite MakeSquare()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) tex.SetPixel(x, y, Color.white);
            return FromTex(tex, 8f);
        }

        static Sprite MakeCircle()
        {
            int s = 24;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(s / 2f, s / 2f));
                    tex.SetPixel(x, y, d <= s / 2f - 0.5f ? Color.white : Color.clear);
                }
            return FromTex(tex, s);
        }

        // Cute blob: rounded body (white, tintable). Face pass draws only the eyes.
        static Sprite MakeBlob(bool facePass)
        {
            int w = 26, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool body = InRoundedRect(x, y, w, h, 6f);
                    bool border = body && !InRoundedRect(x, y, w, h, 6f, inset: 2);
                    Color col = Color.clear;
                    if (facePass)
                    {
                        bool eye = (y >= 10 && y <= 14) && ((x >= 7 && x <= 9) || (x >= 16 && x <= 18));
                        if (eye) col = Color.white;
                    }
                    else if (body) col = border ? Line : Color.white;
                    tex.SetPixel(x, y, col);
                }
            return FromTex(tex, 34f);
        }

        static bool InRoundedRect(int x, int y, int w, int h, float r, int inset = 0)
        {
            float x0 = inset, y0 = inset, x1 = w - 1 - inset, y1 = h - 1 - inset;
            if (x < x0 || x > x1 || y < y0 || y > y1) return false;
            float cx = Mathf.Clamp(x, x0 + r, x1 - r);
            float cy = Mathf.Clamp(y, y0 + r, y1 - r);
            return (new Vector2(x - cx, y - cy)).sqrMagnitude <= r * r;
        }

        // ---------- thin chroma outline (glass UI style) ----------

        static Sprite _thinFrame;
        /// <summary>Crisp 2px white outline, transparent interior — THE outline for glass panels/buttons.</summary>
        public static Sprite ThinFrame
        {
            get
            {
                if (_thinFrame != null) return _thinFrame;
                int s = 24;
                var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                for (int y = 0; y < s; y++)
                    for (int x = 0; x < s; x++)
                    {
                        int edge = Mathf.Min(Mathf.Min(x, s - 1 - x), Mathf.Min(y, s - 1 - y));
                        int ex = Mathf.Min(x, s - 1 - x);
                        int ey = Mathf.Min(y, s - 1 - y);
                        Color c;
                        if (ex + ey < 2) c = Color.clear;                      // pixel-notched corners
                        else if (ex + ey < 4) c = Color.white;
                        else if (edge < 2) c = Color.white;
                        else c = Color.clear;
                        tex.SetPixel(x, y, c);
                    }
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                _thinFrame = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 24f, 0,
                    SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
                return _thinFrame;
            }
        }

        // ---------- textured 9-slice UI panels (pixel style, not flat color) ----------

        static Sprite _panelDark, _panelCream, _panelGoldBtn;
        public static Sprite PanelDark => _panelDark ?? (_panelDark = MakePanel(
            Hex("#2A2438"), Hex("#3A3150"), Hex("#0E0C16"), Hex("#4A3F66")));
        public static Sprite PanelCream => _panelCream ?? (_panelCream = MakePanel(
            Hex("#F4E9D0"), Hex("#E6D8B8"), Hex("#8B5E3C"), Hex("#FFF6E0")));
        public static Sprite PanelButton => _panelGoldBtn ?? (_panelGoldBtn = MakePanel(
            Hex("#FFD447"), Hex("#EEBE2E"), Hex("#8B5E3C"), Hex("#FFE989")));

        /// <summary>
        /// 48x48 pixel panel: solid fill, crisp dark outline, 2px top bevel highlight and
        /// 2px bottom shade, stepped pixel corners. No dithering — clean, cute, readable.
        /// </summary>
        static Sprite MakePanel(Color fill, Color shade, Color line, Color bevel)
        {
            int s = 48;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    int ex = Mathf.Min(x, s - 1 - x);
                    int ey = Mathf.Min(y, s - 1 - y);
                    int edge = Mathf.Min(ex, ey);

                    // stepped corner cut (2px notch)
                    if (ex + ey < 3) { tex.SetPixel(x, y, Color.clear); continue; }
                    if (ex + ey < 5) { tex.SetPixel(x, y, line); continue; }

                    if (edge < 2) { tex.SetPixel(x, y, line); continue; }
                    if (y >= s - 4 && edge < 4) { tex.SetPixel(x, y, bevel); continue; }   // top highlight
                    if (y < 4 && edge < 4) { tex.SetPixel(x, y, shade); continue; }        // bottom shade
                    tex.SetPixel(x, y, fill);
                }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 48f, 0,
                SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
        }
    }
}
