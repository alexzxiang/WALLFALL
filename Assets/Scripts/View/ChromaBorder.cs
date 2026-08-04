using UnityEngine;
using UnityEngine.UI;

namespace Wallfall
{
    /// <summary>
    /// A sleek animated gradient border: the outline is a mesh ring whose vertex colors
    /// are a hue/brightness gradient that continuously FLOWS around the perimeter —
    /// a conic-gradient border, not dots. Accent mode drifts around the accent color;
    /// Rainbow mode cycles the full hue wheel.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class ChromaBorder : MaskableGraphic
    {
        public float BorderWidth = 2.5f;
        public Color Accent = Color.white;
        public bool Rainbow;
        public float Speed = 0.3f;   // gradient revolutions per second
        public bool Animate = true;

        const int SegmentsPerEdge = 10;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = GetPixelAdjustedRect();
            float bw = Mathf.Min(BorderWidth, Mathf.Min(r.width, r.height) * 0.5f);

            Vector2[] outer =
            {
                new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
                new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)
            };
            Vector2[] inner =
            {
                new Vector2(r.xMin + bw, r.yMin + bw), new Vector2(r.xMax - bw, r.yMin + bw),
                new Vector2(r.xMax - bw, r.yMax - bw), new Vector2(r.xMin + bw, r.yMax - bw)
            };

            float time = Animate ? Time.unscaledTime * Speed : 0f;
            int baseVert = 0;

            for (int e = 0; e < 4; e++)
            {
                Vector2 a = outer[e], b = outer[(e + 1) % 4];
                Vector2 ai = inner[e], bi = inner[(e + 1) % 4];

                for (int s = 0; s <= SegmentsPerEdge; s++)
                {
                    float k = (float)s / SegmentsPerEdge;
                    float param = (e + k) / 4f; // 0..1 around the perimeter
                    Color32 col = ColorAt(param, time);

                    var vo = UIVertex.simpleVert;
                    vo.position = Vector2.Lerp(a, b, k);
                    vo.color = col;
                    vh.AddVert(vo);

                    var vi = UIVertex.simpleVert;
                    vi.position = Vector2.Lerp(ai, bi, k);
                    vi.color = col;
                    vh.AddVert(vi);

                    if (s > 0)
                    {
                        int i = baseVert + s * 2;
                        vh.AddTriangle(i - 2, i - 1, i);
                        vh.AddTriangle(i, i - 1, i + 1);
                    }
                }
                baseVert += (SegmentsPerEdge + 1) * 2;
            }
        }

        Color32 ColorAt(float param, float time)
        {
            if (Rainbow)
            {
                var rc = Color.HSVToRGB(Mathf.Repeat(param - time, 1f), 0.55f, 1f);
                rc.a = Animate ? 0.95f : 0.5f;
                return rc;
            }

            Color.RGBToHSV(Accent, out float h, out float s, out float v);
            // brightness band + gentle hue drift, both flowing around the perimeter
            float band = 0.5f + 0.5f * Mathf.Sin((param - time) * Mathf.PI * 4f);
            float hue = Mathf.Repeat(h + 0.045f * Mathf.Sin((param - time * 1.35f) * Mathf.PI * 2f), 1f);
            var c = Color.HSVToRGB(hue, s * 0.9f, Mathf.Lerp(0.72f, 1f, band));
            c = Color.Lerp(c, Color.white, band * 0.3f);
            c.a = Animate ? 0.95f : 0.5f;
            return c;
        }

        void Update()
        {
            if (Animate) SetVerticesDirty();
        }
    }
}
