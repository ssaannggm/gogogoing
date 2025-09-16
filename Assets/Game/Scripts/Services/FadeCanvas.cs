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

        if (!cg) cg = GetComponent<CanvasGroup>(); // ������ġ
        if (cg)
        {
            cg.alpha = 0f;                // ������ ����
            cg.blocksRaycasts = false;    // �Է� ���
            cg.interactable = false;
        }

        // Ȥ�� �� �ε� �� ���İ� 1�� ä ���� ������ �ڵ� ����
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
            cg.alpha = Mathf.Lerp(0f, 1f, t / dur); // 0 �� 1
            yield return null;
        }
        cg.alpha = 1f; // ���� ����
    }

    public IEnumerator FadeIn()
    {
        if (!cg) yield break;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / dur); // 1 �� 0  ���߿�
            yield return null;
        }
        cg.alpha = 0f;                 // ���� ����
        cg.blocksRaycasts = false;     // �Է� �ٽ� ���
    }

    // �ʿ�� ��� ���� ������
    public void ForceClear()
    {
        if (!cg) return;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }
}
