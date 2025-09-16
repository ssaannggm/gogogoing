// Assets/Game/Scripts/UI/HealthBarUI.cs (���� ���� �ڵ�)
using UnityEngine;
using UnityEngine.UI;
using Game.Combat;
using DG.Tweening;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    private Health _health;
    private Tween _healthTween;

    void Awake()
    {
        _health = GetComponentInParent<Health>();
        if (_health == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // [����] OnHealthChanged �̺�Ʈ�� ���� UpdateUI �Լ��� ����մϴ�.
        // �� �̺�Ʈ�� ���� ü�°� �ִ� ü���� ���� �������ݴϴ�.
        _health.OnHealthChanged += UpdateUI;
        _health.OnDeath += OnDeath;
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= UpdateUI;
            _health.OnDeath -= OnDeath;
        }
        _healthTween?.Kill();
    }

    // [����] ���� �Լ��� ü�� ���� ���� �Ķ���ͷ� �޽��ϴ�.
    private void UpdateUI(float currentHP, float maxHP)
    {
        if (healthSlider == null) return;

        float fillAmount = maxHP > 0 ? currentHP / maxHP : 0;

        _healthTween?.Kill();
        _healthTween = healthSlider.DOValue(fillAmount, animationDuration)
                                 .SetEase(easeType);

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
        }
    }

    private void OnDeath()
    {
        gameObject.SetActive(false);
    }
}