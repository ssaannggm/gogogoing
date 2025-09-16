// Assets/Game/Scripts/Combat/HitEffectSO.cs
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Combat
{
    [CreateAssetMenu(menuName = "Game/Combat/Hit Effect", fileName = "HitEffect_")]
    public sealed class HitEffectSO : ScriptableObject
    {
        [Header("VFX")]
        [Tooltip("�ǰ� �� ������ ��ƼŬ/��������Ʈ ����Ʈ ������")]
        public GameObject vfxPrefab;
        [Tooltip("Ǯ�� ��ȯ�Ǳ���� ���� �ð�(��). ���� ��ƼŬ ���� �̻����� �μ���.")]
        public float vfxLifetime = 0.8f;

        [Header("VFX Tuning")]
        [Tooltip("���� ���� ��ġ(��Ŀ/��Ʈ����Ʈ/�ٿ��� �߽�)������ ������(���� ����)")]
        public Vector2 vfxOffset = new Vector2(0f, 0.5f);
        [Min(0.01f), Tooltip("VFX ���� ������(������ �⺻ 1 ����)")]
        public float vfxScale = 0.8f;
        [Tooltip("�ȼ� ����Ʈ ��� ��, ������ ���� �� ��ǥ�� �ȼ� �׸��忡 ����")]
        public bool snapOffsetToPixel = true;

        [Header("SFX")]
        [Tooltip("�ǰ� ���� Ŭ��(������ ����)")]
        public AudioClip sfx;
        [Range(0f, 1f), Tooltip("���� ����")]
        public float volume = 0.85f;
        [Range(0.1f, 3f), Tooltip("�⺻ ��ġ")]
        public float pitch = 1.0f;
        [Range(0f, 0.3f), Tooltip("��ġ ���� ���� ��(��)")]
        public float pitchJitter = 0.05f;
        [Tooltip("��� ����� �ͼ� �׷�(SFX ���� ����)")]
        public AudioMixerGroup mixerGroup;
        [Min(0), Tooltip("���� Ŭ�� ���� ��� ���� ����")]
        public int maxInstances = 4;
        [Min(0f), Tooltip("���� Ŭ�� �ּ� ����(��)")]
        public float instanceCooldown = 0.03f;

        [Header("Camera / Feel")]
        [Tooltip("ī�޶� ����ũ ����(0�̸� �̻��)")]
        public float shakeAmplitude = 0f;   // �⺻ ��
        [Tooltip("ī�޶� ����ũ ���� �ð�(��)")]
        public float shakeDuration = 0.00f;
        [Tooltip("��Ʈ���� �ð�(��, 0�̸� �̻��)")]
        public float hitstop = 0f;          // �⺻ ��

        [Header("Flash / Damage Number")]
        [Tooltip("�÷��� �÷�(Flash ������Ʈ�� ���� ���� ����)")]
        public Color flashColor = Color.white;
        [Tooltip("�÷��� ���� �ð�(��, 0�̸� �̻��)")]
        public float flashDuration = 0f;    // �⺻ ��
        [Tooltip("������ ���� ������(TMP 3D ����). ������ �̻��")]
        public GameObject damageNumberPrefab;
        [Tooltip("������ ���� ǥ�� �ð�(��)")]
        public float damageNumberLifetime = 0.8f;
    }
}
