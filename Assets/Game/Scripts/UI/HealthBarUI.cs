// Assets/Game/Scripts/UI/HealthBarUI.cs (최종 수정 코드)
using UnityEngine;
using UnityEngine.UI;
using Game.Combat;
using DG.Tweening;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("애니메이션 설정")]
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

        // [수정] OnHealthChanged 이벤트에 직접 UpdateUI 함수를 등록합니다.
        // 이 이벤트는 현재 체력과 최대 체력을 직접 전달해줍니다.
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

    // [수정] 이제 함수가 체력 값을 직접 파라미터로 받습니다.
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