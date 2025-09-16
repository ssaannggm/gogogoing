// Assets/Game/Scripts/UI/RunPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;                 // ★ TMP 사용
using Game.Services;        // GameManager
using Game.Core;            // GameState, EventBus

namespace Game.UI
{
    public sealed class RunPanel : MonoBehaviour
    {
        [Header("Wire in Inspector (under Canvas)")]
        [SerializeField] Button startButton;
        [SerializeField] Button endButton;

        // ★ TMP_Text 사용 (TextMeshPro UGUI)
        [SerializeField] TMP_Text stateText;

        [Header("Start Options")]
        [SerializeField] int fixedSeed = 0;  // 0이면 랜덤 시드

        void Awake()
        {
            // 자동 탐색(이름 맞추면 편의), 수동 연결도 가능
            if (!startButton) startButton = transform.Find("StartButton")?.GetComponent<Button>();
            if (!endButton) endButton = transform.Find("EndButton")?.GetComponent<Button>();
            if (!stateText) stateText = transform.Find("StateText")?.GetComponent<TMP_Text>();

            if (startButton) startButton.onClick.AddListener(OnClickStart);
            if (endButton) endButton.onClick.AddListener(OnClickEnd);

            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Subscribe<RunStarted>(_ => RefreshUI());
            EventBus.Subscribe<RunEnded>(_ => RefreshUI());
        }

        void OnDestroy()
        {
            if (startButton) startButton.onClick.RemoveListener(OnClickStart);
            if (endButton) endButton.onClick.RemoveListener(OnClickEnd);

            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
            // 람다 구독 해제는 생략 가능(경량)
        }

        void Start()
        {
            if (GameManager.I == null)
            {
                SetStateLabel("No GameManager in scene.\nPut AppBootstrap prefab in scene.");
                SetInteractable(false, false);
                return;
            }
            RefreshUI();
        }

        void OnClickStart()
        {
            if (GameManager.I == null) return;
            int seed = fixedSeed != 0 ? fixedSeed : Random.Range(1, 1_000_000);
            GameManager.I.StartNewRun(seed);
            RefreshUI();
        }

        void OnClickEnd()
        {
            if (GameManager.I == null) return;
            GameManager.I.EndRun(victory: true);
            RefreshUI();
        }

        void OnGameStateChanged(GameStateChanged e) => RefreshUI();

        void RefreshUI()
        {
            if (GameManager.I == null) return;
            bool isRunning = GameManager.I.State == GameState.Run;
            SetInteractable(start: !isRunning, end: isRunning);

            string label = isRunning
                ? $"State: RUN (Seed: {GameManager.I.CurrentRun?.Seed.ToString() ?? "-"})"
                : $"State: {GameManager.I.State}";
            SetStateLabel(label);
        }

        void SetInteractable(bool start, bool end)
        {
            if (startButton) startButton.interactable = start;
            if (endButton) endButton.interactable = end;
        }

        void SetStateLabel(string msg)
        {
            if (stateText) stateText.text = msg;
        }
    }
}
