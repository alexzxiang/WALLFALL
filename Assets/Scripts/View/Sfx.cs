using UnityEngine;

namespace Wallfall
{
    /// <summary>UI sound playback (JDSherbert pack via SpriteBank). Safe no-op when clips are missing.</summary>
    public static class Sfx
    {
        static AudioSource _source;

        public static void Play(string name, float volume = 0.7f, float pitch = 1f)
        {
            var clip = SpriteBank.Sfx(name);
            if (clip == null) return;
            if (_source == null)
            {
                var go = new GameObject("Sfx");
                Object.DontDestroyOnLoad(go);
                _source = go.AddComponent<AudioSource>();
                _source.playOnAwake = false;
            }
            _source.pitch = pitch;
            _source.PlayOneShot(clip, volume);
        }
    }
}
