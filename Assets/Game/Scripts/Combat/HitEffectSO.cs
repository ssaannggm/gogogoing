// Assets/Game/Scripts/Combat/HitEffectSO.cs
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Combat
{
    [CreateAssetMenu(menuName = "Game/Combat/Hit Effect", fileName = "HitEffect_")]
    public sealed class HitEffectSO : ScriptableObject
    {
        [Header("VFX")]
        [Tooltip("피격 시 생성할 파티클/스프라이트 이펙트 프리팹")]
        public GameObject vfxPrefab;
        [Tooltip("풀로 반환되기까지 유지 시간(초). 실제 파티클 길이 이상으로 두세요.")]
        public float vfxLifetime = 0.8f;

        [Header("VFX Tuning")]
        [Tooltip("계산된 기준 위치(앵커/히트포인트/바운즈 중심)에서의 오프셋(월드 유닛)")]
        public Vector2 vfxOffset = new Vector2(0f, 0.5f);
        [Min(0.01f), Tooltip("VFX 절대 스케일(프리팹 기본 1 기준)")]
        public float vfxScale = 0.8f;
        [Tooltip("픽셀 퍼펙트 사용 시, 오프셋 적용 후 좌표를 픽셀 그리드에 스냅")]
        public bool snapOffsetToPixel = true;

        [Header("SFX")]
        [Tooltip("피격 사운드 클립(없으면 무음)")]
        public AudioClip sfx;
        [Range(0f, 1f), Tooltip("사운드 볼륨")]
        public float volume = 0.85f;
        [Range(0.1f, 3f), Tooltip("기본 피치")]
        public float pitch = 1.0f;
        [Range(0f, 0.3f), Tooltip("피치 랜덤 변조 폭(±)")]
        public float pitchJitter = 0.05f;
        [Tooltip("출력 오디오 믹서 그룹(SFX 버스 권장)")]
        public AudioMixerGroup mixerGroup;
        [Min(0), Tooltip("동일 클립 동시 재생 제한 개수")]
        public int maxInstances = 4;
        [Min(0f), Tooltip("같은 클립 최소 간격(초)")]
        public float instanceCooldown = 0.03f;

        [Header("Camera / Feel")]
        [Tooltip("카메라 셰이크 진폭(0이면 미사용)")]
        public float shakeAmplitude = 0f;   // 기본 끔
        [Tooltip("카메라 셰이크 지속 시간(초)")]
        public float shakeDuration = 0.00f;
        [Tooltip("히트스톱 시간(초, 0이면 미사용)")]
        public float hitstop = 0f;          // 기본 끔

        [Header("Flash / Damage Number")]
        [Tooltip("플래시 컬러(Flash 컴포넌트가 있을 때만 적용)")]
        public Color flashColor = Color.white;
        [Tooltip("플래시 지속 시간(초, 0이면 미사용)")]
        public float flashDuration = 0f;    // 기본 끔
        [Tooltip("데미지 숫자 프리팹(TMP 3D 권장). 없으면 미사용")]
        public GameObject damageNumberPrefab;
        [Tooltip("데미지 숫자 표시 시간(초)")]
        public float damageNumberLifetime = 0.8f;
    }
}
