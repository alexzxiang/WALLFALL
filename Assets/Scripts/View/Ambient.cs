using UnityEngine;

namespace Wallfall
{
    /// <summary>Slow horizontal drift for background parallax layers — the world breathes.</summary>
    public class AmbientDrift : MonoBehaviour
    {
        public float Amplitude = 0.4f;
        public float Speed = 0.05f;
        public float Phase;

        Vector3 _base;

        void Start() { _base = transform.position; }

        void Update()
        {
            transform.position = _base + new Vector3(
                Mathf.Sin(Time.time * Speed * Mathf.PI * 2f + Phase) * Amplitude, 0f, 0f);
        }
    }

    /// <summary>Softly rising light specks over a board — ambient life, lane-tinted.</summary>
    public class AmbientSpecks : MonoBehaviour
    {
        public Color Tint = Color.white;
        public Vector3 Center;
        public float Width = 8f, Height = 7f;
        public int Count = 10;

        Transform[] _specks;
        SpriteRenderer[] _srs;
        float[] _speed;
        float[] _sway;

        void Start()
        {
            _specks = new Transform[Count];
            _srs = new SpriteRenderer[Count];
            _speed = new float[Count];
            _sway = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                var go = new GameObject($"speck{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Circle;
                var c = Color.Lerp(Tint, Color.white, 0.4f);
                c.a = Random.Range(0.10f, 0.28f);
                sr.color = c;
                sr.sortingOrder = -9;
                go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.14f);
                go.transform.position = RandomPos();
                _specks[i] = go.transform;
                _srs[i] = sr;
                _speed[i] = Random.Range(0.12f, 0.35f);
                _sway[i] = Random.Range(0.5f, 1.6f);
            }
        }

        Vector3 RandomPos() => Center + new Vector3(
            Random.Range(-Width / 2f, Width / 2f),
            Random.Range(-Height / 2f, Height / 2f), 0f);

        void Update()
        {
            for (int i = 0; i < Count; i++)
            {
                var p = _specks[i].position;
                p.y += _speed[i] * Time.deltaTime;
                p.x += Mathf.Sin(Time.time * _sway[i] + i) * 0.08f * Time.deltaTime;
                if (p.y > Center.y + Height / 2f)
                {
                    p = RandomPos();
                    p.y = Center.y - Height / 2f;
                }
                _specks[i].position = p;
            }
        }
    }

    /// <summary>Loops the ambient soundtrack after a user gesture unlocks browser audio.</summary>
    public static class Music
    {
        static AudioSource _source;

        public static void Play()
        {
            if (_source != null) return;
            var clip = SpriteBank.MusicClip();
            if (clip == null) return;
            var go = new GameObject("Music");
            Object.DontDestroyOnLoad(go);
            _source = go.AddComponent<AudioSource>();
            _source.clip = clip;
            _source.loop = true;
            _source.volume = 0.32f;
            _source.Play();
        }
    }
}
