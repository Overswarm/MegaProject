using System.Collections.Generic;
using UnityEngine;

namespace MB
{
    public enum SfxId
    {
        Shoot, ChargeFull, Hit, Deflect, ExplodeSmall, ExplodeBig,
        Pickup, Energy, Hurt, Door, Ready, Cursor, Select, Buzz,
        ETank, OneUp, BossHit, Land, Teleport
    }

    /// Tiny NES-style square/noise wave synthesizer. All clips generated at runtime.
    public static class Sfx
    {
        static AudioSource src;
        static readonly Dictionary<SfxId, AudioClip> clips = new Dictionary<SfxId, AudioClip>();
        const int SR = 22050;

        static void Ensure()
        {
            if (src != null) return;
            var go = new GameObject("SfxPlayer");
            Object.DontDestroyOnLoad(go);
            src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
        }

        public static void Play(SfxId id, float vol = 0.5f)
        {
            if (!Application.isPlaying) return;
            Ensure();
            if (!clips.TryGetValue(id, out var clip) || clip == null)
            {
                clip = Generate(id);
                clips[id] = clip;
            }
            if (clip != null) src.PlayOneShot(clip, vol);
        }

        static AudioClip Generate(SfxId id)
        {
            switch (id)
            {
                case SfxId.Shoot: return Square(0.08f, t => 880 - t * 3000, 0.35f);
                case SfxId.ChargeFull: return Square(0.15f, t => 440 + t * 2200, 0.3f);
                case SfxId.Hit: return Square(0.07f, t => 220 - t * 800, 0.4f);
                case SfxId.Deflect: return Square(0.09f, t => 1400 + Mathf.Sin(t * 90) * 300, 0.3f);
                case SfxId.ExplodeSmall: return Noise(0.22f, 0.4f);
                case SfxId.ExplodeBig: return Noise(0.6f, 0.5f);
                case SfxId.Pickup: return Square(0.09f, t => t < 0.045f ? 660 : 990, 0.35f);
                case SfxId.Energy: return Square(0.05f, t => 1046, 0.3f);
                case SfxId.Hurt: return Square(0.25f, t => 300 - t * 500, 0.4f);
                case SfxId.Door: return Square(0.5f, t => 160 + Mathf.PingPong(t * 500, 60), 0.35f);
                case SfxId.Ready: return Square(0.1f, t => 784, 0.3f);
                case SfxId.Cursor: return Square(0.04f, t => 700, 0.25f);
                case SfxId.Select: return Square(0.18f, t => t < 0.06f ? 660 : t < 0.12f ? 880 : 1100, 0.35f);
                case SfxId.Buzz: return Square(0.18f, t => 110, 0.35f);
                case SfxId.ETank: return Square(0.4f, t => 523 + Mathf.Floor(t * 12) * 60, 0.3f);
                case SfxId.OneUp: return Square(0.5f, t => 523 + Mathf.Floor(t * 8) * 110, 0.3f);
                case SfxId.BossHit: return Square(0.1f, t => 180 - t * 300, 0.45f);
                case SfxId.Land: return Square(0.04f, t => 200, 0.2f);
                case SfxId.Teleport: return Square(0.3f, t => 1800 - t * 4500, 0.35f);
                default: return null;
            }
        }

        static AudioClip Square(float dur, System.Func<float, float> freqAt, float amp)
        {
            int n = Mathf.CeilToInt(dur * SR);
            var data = new float[n];
            float phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float f = Mathf.Max(30, freqAt(t));
                phase += f / SR;
                float env = 1f - (t / dur);
                data[i] = (Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f) * amp * env;
            }
            var clip = AudioClip.Create("sfx", n, 1, SR, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Noise(float dur, float amp)
        {
            int n = Mathf.CeilToInt(dur * SR);
            var data = new float[n];
            var rng = new System.Random(1234);
            float v = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                if (i % 3 == 0) v = (float)(rng.NextDouble() * 2 - 1);
                float env = 1f - (t / dur);
                data[i] = v * amp * env * env;
            }
            var clip = AudioClip.Create("sfxn", n, 1, SR, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
