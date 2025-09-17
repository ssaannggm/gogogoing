// Assets/Game/Scripts/Services/GameManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Runtime;
using Game.Battle;

namespace Game.Services
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public float TimeScale => _timeScale;
        public RunManager CurrentRun { get; private set; }

        private float _timeScale = 1f;

        // Services
        public RNGService RNG { get; private set; }
        public PoolService Pool { get; private set; }
        public AudioService Audio { get; private set; }
        public SaveService Save { get; private set; }
        public DataCatalog Data { get; private set; }

        [Header("전역 참조(자동 복구됨)")]
        [SerializeField] private GameFlowController _flowController;
        [SerializeField] private InventoryPartyMode _inventoryUI;

        public GameFlowController FlowController => _flowController;
        public InventoryPartyMode InventoryUI => _inventoryUI;

        // New Input System
        private PlayerControls _playerControls;

        // ---------------- Lifecycle ----------------
        private void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            RNG = GetComponent<RNGService>();
            Pool = GetComponent<PoolService>();
            Audio = GetComponent<AudioService>();
            Save = GetComponent<SaveService>();
            Data = GetComponent<DataCatalog>();

            SetState(GameState.Title);

            // 씬 로드 시점마다 참조 자동 복구
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnEnable()
        {
            // 입력 구성
            if (_playerControls == null)
                _playerControls = new PlayerControls();

            // 중복 방지 뒤 등록
            _playerControls.UI.Enable();
            _playerControls.UI.ToggleInventory.performed -= ToggleInventory;
            _playerControls.UI.ToggleInventory.performed += ToggleInventory;

            // 씬 내 전역 참조 자동 탐색(누락 시)
            EnsureSceneRefs();
        }

        private void OnDisable()
        {
            // 입력 해제(안전)
            if (_playerControls != null)
            {
                var action = _playerControls.UI.ToggleInventory;
                if (action != null) action.performed -= ToggleInventory;
                _playerControls.UI.Disable();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // 입력 정리(안전)
            if (_playerControls != null)
            {
                var action = _playerControls.UI.ToggleInventory;
                if (action != null) action.performed -= ToggleInventory;
                _playerControls.Disable();
                _playerControls.Dispose();
                _playerControls = null;
            }
        }

        private void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            // 씬이 바뀌면 자동으로 전역 참조 다시 잡음
            EnsureSceneRefs();
        }

        // ---------------- Public API ----------------
        public void SetState(GameState s)
        {
            State = s;
            EventBus.Raise(new GameStateChanged(s));
        }

        public void SetTimeScale(float x)
        {
            _timeScale = Mathf.Max(0f, x);
            Time.timeScale = _timeScale;
        }

        public void StartNewRun(int seed)
        {
            RNG.Reseed(seed);
            CurrentRun = new RunManager(seed, Data);
            SetState(GameState.Run);
            EventBus.Raise(new RunStarted(CurrentRun));
        }

        public void EndRun(bool victory)
        {
            EventBus.Raise(new RunEnded(victory));
            CurrentRun = null;
            SetState(GameState.Title);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) Save.TryAutoSave();
        }

        private void OnApplicationQuit()
        {
            Save.TryAutoSave();
        }

        // 수동 등록도 가능하게 유지
        public void RegisterFlowController(GameFlowController fc)
        {
            _flowController = fc;
            Debug.Log("[GameManager] GameFlowController registered.");
        }

        public void RegisterInventoryUI(InventoryPartyMode ui)
        {
            _inventoryUI = ui;
            Debug.Log("[GameManager] InventoryUI registered.");
        }

        // ---------------- Input: Toggle Inventory ----------------
        private void ToggleInventory(InputAction.CallbackContext context)
        {
            if (State != GameState.Run) return;

            // 전역 참조 누락 시 자동 보강
            if (_flowController == null || _inventoryUI == null)
                EnsureSceneRefs();

            if (_flowController == null) return; // 여전히 없으면 중단

            // 영입 페이즈 중에는 인벤토리 금지
            if (_flowController.Phase == RunPhase.Recruitment) return;

            // 배틀 페이즈: 단일 권한 위임
            if (_flowController.Phase == RunPhase.Battle)
            {
                var gate = FindObjectOfType<BattleInventoryToggle>(true);
                if (gate != null)
                {
                    gate.Toggle();
                }
                else
                {
                    // 폴백: 게이트가 없을 때만 최소 동작
                    if (_inventoryUI == null) return;
                    bool open = _inventoryUI.gameObject.activeInHierarchy;
                    if (open) _inventoryUI.ExitMode();
                    else _inventoryUI.Open(isReadOnly: true);
                }
                return; // 배틀에선 여기서 종료(중복 토글 방지)
            }

            // 비-배틀(맵/이벤트 등): 기존 동작
            if (_inventoryUI == null) return;

            if (_inventoryUI.gameObject.activeInHierarchy)
            {
                _inventoryUI.ExitMode();
                _flowController.RequestMap();
            }
            else
            {
                _inventoryUI.Open(isReadOnly: false);
            }
        }

        // ---------------- Helpers ----------------
        private void EnsureSceneRefs()
        {
            // 누락된 경우에만 찾기(불필요한 Find 호출 방지)
            if (_flowController == null)
                _flowController = FindObjectOfType<GameFlowController>(true);

            if (_inventoryUI == null)
                _inventoryUI = FindObjectOfType<InventoryPartyMode>(true);
        }
    }
}
