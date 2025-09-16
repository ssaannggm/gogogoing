// Assets/Game/Scripts/Services/AudioOneShot.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Services
{
    public sealed class AudioOneShot : MonoBehaviour
    {
        static AudioOneShot _i; public static AudioOneShot I => _i;

        [SerializeField] int prewarmSources = 16;
        readonly Queue<AudioSource> _idle = new();
        readonly HashSet<AudioSource> _busy = new();
        readonly Dictionary<AudioClip, int> _playing = new();
        readonly Dictionary<AudioClip, float> _last = new();

        void Awake()
        {
            if (_i && _i != this) { Destroy(gameObject); return; }
            _i = this;
            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < prewarmSources; i++) _idle.Enqueue(CreateSource());
        }

        AudioSource CreateSource()
        {
            var go = new GameObject("OneShot");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = 2f; src.maxDistance = 25f;
            src.dopplerLevel = 0f;
            go.SetActive(false);
            return src;
        }

        public void PlayAt(AudioClip clip, Vector3 pos, float vol, float pitch, float jitter, AudioMixerGroup group, int maxInstances, float cooldown)
        {
            if (!clip) return;
            float now = Time.unscaledTime;
            if (_last.TryGetValue(clip, out var t) && (now - t) < cooldown) return;
            if (_playing.TryGetValue(clip, out var n) && n >= maxInstances) return;

            var src = _idle.Count > 0 ? _idle.Dequeue() : CreateSource();
            _busy.Add(src);

            var p = Mathf.Clamp(pitch + Random.Range(-jitter, jitter), 0.1f, 3f);
            src.transform.position = pos;
            src.clip = clip;
            src.outputAudioMixerGroup = group;
            src.volume = vol;
            src.pitch = p;
            src.gameObject.SetActive(true);
            src.Play();

            _last[clip] = now;
            _playing[clip] = (_playing.TryGetValue(clip, out n) ? n : 0) + 1;
            StartCoroutine(ReturnLater(src, clip.length / Mathf.Max(0.01f, p), clip));
        }

        System.Collections.IEnumerator ReturnLater(AudioSource src, float sec, AudioClip key)
        {
            yield return new WaitForSeconds(sec);
            src.Stop();
            src.gameObject.SetActive(false);
            _busy.Remove(src);
            _idle.Enqueue(src);
            _playing[key] = Mathf.Max(0, _playing[key] - 1);
        }
    }
}
