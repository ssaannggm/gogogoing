// Assets/Game/Scripts/Services/AudioService.cs
using UnityEngine;

namespace Game.Services
{
    public class AudioService : MonoBehaviour
    {
        [Range(0, 1)] public float bgmVolume = 0.6f;
        [Range(0, 1)] public float sfxVolume = 0.8f;
        public void PlaySFX(AudioClip clip, Vector3 pos)
        {
            if (!clip) return;
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
        }
    }
}
