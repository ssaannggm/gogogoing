// FadeCanvas.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FadeCanvas : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] float dur = 0.25f;
    [SerializeField] bool blockRaycast = true;
    [SerializeField] bool dontDestroyOnLoad = true;

    static FadeCanvas _i;
    public static FadeCanvas TryGet() => _i;

    void Awake()
    {
        if (_i != null && _i != this) { Destroy(gameObject); return; }
        _i = this;
        //if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        if (!cg) cg = GetComponent<CanvasGroup>(); // 안전장치
        if (cg)
        {
            cg.alpha = 0f;                // 시작은 투명
            cg.blocksRaycasts = false;    // 입력 통과
            cg.interactable = false;
        }

        // 혹시 씬 로드 후 알파가 1인 채 남아 있으면 자동 복구
        SceneManager.sceneLoaded += (_, __) => {
            if (cg && cg.alpha >= 0.99f) StartCoroutine(FadeIn());
        };
    }

    public IEnumerator FadeOut()
    {
        if (!cg) yield break;
        cg.blocksRaycasts = blockRaycast;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / dur); // 0 → 1
            yield return null;
        }
        cg.alpha = 1f; // 완전 검정
    }

    public IEnumerator FadeIn()
    {
        if (!cg) yield break;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / dur); // 1 → 0  ★중요
            yield return null;
        }
        cg.alpha = 0f;                 // 완전 투명
        cg.blocksRaycasts = false;     // 입력 다시 허용
    }

    // 필요시 즉시 상태 복구용
    public void ForceClear()
    {
        if (!cg) return;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }
}
