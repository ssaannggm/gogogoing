using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Services;

public sealed class TitleUIController : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button endButton;
    [SerializeField] TMP_Text stateText;      // ����
    [SerializeField] int fixedSeed = 0;       // 0�̸� ����

    bool _starting;

    void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnClickStart);
        if (endButton) endButton.onClick.AddListener(OnClickQuit);
    }

    void OnEnable()
    {
        SetInteractable(true);
        if (stateText) stateText.text = "Ready.";
        Time.timeScale = 1f; // Ȥ�� �� Ÿ�ӽ����� ���� ����
    }

    void SetInteractable(bool v)
    {
        if (startButton) startButton.interactable = v;
        if (endButton) endButton.interactable = v;
    }

    void OnClickStart()
    {
        if (_starting || GameManager.I == null) return;
        _starting = true;
        SetInteractable(false);
        StartCoroutine(CoStart());
    }

    IEnumerator CoStart()
    {
        int seed = fixedSeed != 0 ? fixedSeed : Random.Range(1, int.MaxValue);
        GameManager.I.StartNewRun(seed);
        if (stateText) stateText.text = $"Starting... (Seed {seed})";
        yield return SceneFlow.I.LoadGameScene();
    }

    void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
