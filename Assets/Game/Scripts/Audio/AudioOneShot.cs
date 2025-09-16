// Assets/Game/Scripts/Audio/AudioOneShot.cs
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public static class AudioOneShot
{
    public sealed class Facade
    {
        public void Configure(int voices3D, int voices2D = 0, AudioMixerGroup mixer = null)
            => AudioOneShot.Configure(voices3D, voices2D, mixer);

        public void Play(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
            => AudioOneShot.Play3D(clip, position, volume, pitch, mixer);

        public void Play3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
            => AudioOneShot.Play3D(clip, position, volume, pitch, mixer);

        public void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
            => AudioOneShot.Play2D(clip, volume, pitch, mixer);

        public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
            => AudioOneShot.PlayAt(clip, position, volume, pitch, 0f, mixer, 0, 0f);

        public void PlayAt(AudioClip clip, Transform t, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
            => AudioOneShot.PlayAt(clip, t ? t.position : Vector3.zero, volume, pitch, 0f, mixer, 0, 0f);

        public void PlayAt(AudioClip clip, Vector3 position, float volume, float pitch, float pitchJitter,
                           AudioMixerGroup mixer, int maxInstances, float cooldownSeconds)
            => AudioOneShot.PlayAt(clip, position, volume, pitch, pitchJitter, mixer, maxInstances, cooldownSeconds);

        public void PlayAt(AudioClip clip, Transform t, float volume, float pitch, float pitchJitter,
                           AudioMixerGroup mixer, int maxInstances, float cooldownSeconds)
            => AudioOneShot.PlayAt(clip, t ? t.position : Vector3.zero, volume, pitch, pitchJitter, mixer, maxInstances, cooldownSeconds);
    }
    public static readonly Facade I = new Facade();

    static readonly List<AudioSource> pool3D = new();
    static readonly List<AudioSource> pool2D = new();
    static readonly Dictionary<AudioClip, float> lastPlayTime = new();
    static Transform root;
    static int maxVoices3D = 16;
    static int maxVoices2D = 8;

    public static void Configure(int voices3D, int voices2D = 0, AudioMixerGroup mixer = null)
    {
        maxVoices3D = Mathf.Max(1, voices3D);
        if (voices2D > 0) maxVoices2D = Mathf.Max(1, voices2D);

        EnsureRoot();
        while (pool3D.Count < maxVoices3D) pool3D.Add(CreateSource(true, mixer));
        while (pool2D.Count < maxVoices2D) pool2D.Add(CreateSource(false, mixer));
    }

    public static void Play3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
    {
        if (!clip) return;
        EnsureRoot();
        var src = FindIdleSource(pool3D, maxVoices3D, true, mixer);
        if (!src) return;
        src.transform.position = position;
        SetupAndPlay(src, clip, volume, pitch);
    }

    public static void Play(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
        => Play3D(clip, position, volume, pitch, mixer);

    public static void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
    {
        if (!clip) return;
        EnsureRoot();
        var src = FindIdleSource(pool2D, maxVoices2D, false, mixer);
        if (!src) return;
        SetupAndPlay(src, clip, volume, pitch);
    }

    public static void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
        => PlayAt(clip, position, volume, pitch, 0f, mixer, 0, 0f);

    public static void PlayAt(AudioClip clip, Transform t, float volume = 1f, float pitch = 1f, AudioMixerGroup mixer = null)
        => PlayAt(clip, t ? t.position : Vector3.zero, volume, pitch, 0f, mixer, 0, 0f);

    public static void PlayAt(AudioClip clip, Vector3 position, float volume, float pitch, float pitchJitter,
                              AudioMixerGroup mixer, int maxInstances, float cooldownSeconds)
    {
        if (!clip) return;

        if (cooldownSeconds > 0f && lastPlayTime.TryGetValue(clip, out float last))
            if (Time.unscaledTime - last < cooldownSeconds) return;

        if (maxInstances > 0 && CountPlaying(pool3D, clip) >= maxInstances) return;

        EnsureRoot();
        var src = FindIdleSource(pool3D, maxVoices3D, true, mixer);
        if (!src) return;

        src.transform.position = position;
        float jitter = Mathf.Clamp(pitchJitter, 0f, 1f);
        float p = Mathf.Clamp(pitch + Random.Range(-jitter, jitter), 0.5f, 2f);

        SetupAndPlay(src, clip, volume, p);
        lastPlayTime[clip] = Time.unscaledTime;
    }

    public static void PlayAt2D(AudioClip clip, float volume, float pitch, float pitchJitter,
                                AudioMixerGroup mixer, int maxInstances, float cooldownSeconds)
    {
        if (!clip) return;

        if (cooldownSeconds > 0f && lastPlayTime.TryGetValue(clip, out float last))
            if (Time.unscaledTime - last < cooldownSeconds) return;

        if (maxInstances > 0 && CountPlaying(pool2D, clip) >= maxInstances) return;

        EnsureRoot();
        var src = FindIdleSource(pool2D, maxVoices2D, false, mixer);
        if (!src) return;

        float jitter = Mathf.Clamp(pitchJitter, 0f, 1f);
        float p = Mathf.Clamp(pitch + Random.Range(-jitter, jitter), 0.5f, 2f);

        SetupAndPlay(src, clip, volume, p);
        lastPlayTime[clip] = Time.unscaledTime;
    }

    static void EnsureRoot()
    {
        if (root) return;
        var go = new GameObject("AudioOneShotRoot");
        Object.DontDestroyOnLoad(go);
        root = go.transform;
    }

    static AudioSource FindIdleSource(List<AudioSource> pool, int cap, bool is3D, AudioMixerGroup mixer)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            var s = pool[i];
            if (!s.isPlaying) { ApplyMixerIfNeeded(s, mixer); return s; }
        }
        if (pool.Count < cap)
        {
            var src = CreateSource(is3D, mixer);
            pool.Add(src);
            return src;
        }
        return null;
    }

    static int CountPlaying(List<AudioSource> pool, AudioClip clip)
    {
        int n = 0;
        for (int i = 0; i < pool.Count; i++)
            if (pool[i].isPlaying && pool[i].clip == clip) n++;
        return n;
    }

    static AudioSource CreateSource(bool is3D, AudioMixerGroup mixer)
    {
        var go = new GameObject(is3D ? "OneShotSrc3D" : "OneShotSrc2D");
        go.transform.SetParent(root, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = is3D ? 1f : 0f;
        src.rolloffMode = AudioRolloffMode.Linear;
        if (mixer) src.outputAudioMixerGroup = mixer;
        return src;
    }

    static void ApplyMixerIfNeeded(AudioSource src, AudioMixerGroup mixer)
    {
        if (mixer && src.outputAudioMixerGroup != mixer)
            src.outputAudioMixerGroup = mixer;
    }

    static void SetupAndPlay(AudioSource src, AudioClip clip, float volume, float pitch)
    {
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        src.clip = clip;
        src.Play();
    }
}
