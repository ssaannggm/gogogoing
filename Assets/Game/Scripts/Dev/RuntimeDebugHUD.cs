// Assets/Game/Scripts/Dev/RuntimeDebugHUD.cs
using UnityEngine;
using TMPro;
using System.Text;
using Game.Services;
using Game.Runtime;
using Game.Battle;

public sealed class RuntimeDebugHUD : MonoBehaviour
{
    [Header("Optional: 직접 넣을 TMP Text (비우면 자동 생성)")]
    [SerializeField] TMP_Text label;
    [SerializeField] KeyCode toggleKey = KeyCode.F1;
    [SerializeField] KeyCode slowKey = KeyCode.F2;
    [SerializeField] KeyCode titleKey = KeyCode.F5;
    [SerializeField] KeyCode gameKey = KeyCode.F6;

    bool _visible = true;
    float _slow = 0.2f;

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if (!label) label = CreateOverlay();
    }

    TMP_Text CreateOverlay()
    {
        var canvasGO = new GameObject("[DevHUD]");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;
        DontDestroyOnLoad(canvasGO);

        var go = new GameObject("Label");
        go.transform.SetParent(canvasGO.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(12, -12);
        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.fontSize = 18;
        txt.enableWordWrapping = false;
        txt.raycastTarget = false;
        return txt;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _visible = !_visible;
            if (label) label.gameObject.SetActive(_visible);
        }
        if (Input.GetKeyDown(slowKey))
        {
            Time.timeScale = Mathf.Approximately(Time.timeScale, 1f) ? _slow : 1f;
        }
        if (Input.GetKeyDown(titleKey))
        {
            StartCoroutine(SceneFlow.I.LoadTitleScene());
        }
        if (Input.GetKeyDown(gameKey))
        {
            StartCoroutine(SceneFlow.I.LoadGameScene());
        }

        if (!_visible || !label) return;
        label.text = BuildText();
    }

    string BuildText()
    {
        var sb = new StringBuilder();
        var gm = GameManager.I;
        var flow = FindFirstObjectByType<GameFlowController>();
        var bm = FindFirstObjectByType<BattleManager>();
        var fader = FadeCanvas.TryGet();
        var cg = fader ? fader.GetComponent<CanvasGroup>() : null;

        float fps = 1f / Mathf.Max(0.0001f, Time.smoothDeltaTime);

        sb.AppendLine($"State: {(gm ? gm.State.ToString() : "-")} | Phase: {(flow ? flow.Phase.ToString() : "-")}");
        sb.AppendLine($"RunSeed: {(gm?.CurrentRun != null ? gm.CurrentRun.Seed.ToString() : "-")}");
        sb.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine($"Fader: alpha={(cg ? cg.alpha.ToString("F2") : "-")} | blocks={(cg ? cg.blocksRaycasts.ToString() : "-")}");
        sb.AppendLine($"TimeScale: {Time.timeScale:F2} | FPS≈{fps:0}");
        if (bm)
        {
            sb.AppendLine($"BattleState: {bm.State}");
        }

        // BattleManager에서 수집한 최근 전투 메트릭스 표시
        var m = BattleManager.LastMetrics;
        if (m.valid)
        {
            sb.AppendLine($"Battle: {(m.victory ? "Victory" : "Defeat")} ({m.endReason})  t={m.elapsedSeconds:0.000}s");
            sb.AppendLine($"Spawn  A/E: {m.alliesSpawned}/{m.enemiesSpawned}   Dead A/E: {m.alliesDied}/{m.enemiesDied}");
            sb.AppendLine($"Managed Mem Δ: {m.managedMemDelta/1024f:0.0} KB  (before {m.managedMemBefore/1024f:0.0} KB → after {m.managedMemAfter/1024f:0.0} KB)");
        }

        sb.AppendLine("Keys: F1 HUD, F2 Slow, F5 Title, F6 Game, Map(I/E/B) MapMode ");

        return sb.ToString();
    }
}

