// Assets/Game/Scripts/Services/GameManager.cs
using UnityEngine;
using Game.Core;
using Game.Runtime;
using UnityEngine.InputSystem; // 새로운 Input System을 사용하기 위해

namespace Game.Services
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public float TimeScale => _timeScale;
        public Runtime.RunManager CurrentRun { get; private set; }

        float _timeScale = 1f;

        // 서비스 핸들
        public RNGService RNG { get; private set; }
        public PoolService Pool { get; private set; }
        public AudioService Audio { get; private set; }
        public SaveService Save { get; private set; }
        public DataCatalog Data { get; private set; }

        [Header("전역 참조")]
        private GameFlowController _flowController;
        private InventoryPartyMode _inventoryUI;

        public GameFlowController FlowController => _flowController;
        public InventoryPartyMode InventoryUI => _inventoryUI;

        private PlayerControls _playerControls; // Input Actions 클래스

        void Awake()
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
            // Input Actions 초기화
            _playerControls = new PlayerControls();
        }

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
            CurrentRun = new Runtime.RunManager(seed, Data);
            SetState(GameState.Run);
            EventBus.Raise(new RunStarted(CurrentRun));
        }

        public void EndRun(bool victory)
        {
            EventBus.Raise(new RunEnded(victory));
            CurrentRun = null;
            SetState(GameState.Title);
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) Save.TryAutoSave();
        }

        void OnApplicationQuit()
        {
            Save.TryAutoSave();
        }

        void OnEnable()
        {
            // 'ToggleInventory' 액션이 수행되었을 때 OnToggleInventory 함수를 호출하도록 구독
            _playerControls.UI.ToggleInventory.performed += ToggleInventory;
            _playerControls.UI.Enable(); // UI 액션 맵 활성화
        }

        void OnDisable()
        {
            _playerControls.UI.ToggleInventory.performed -= ToggleInventory;
            _playerControls.UI.Disable();
        }

        // 'I' 키를 눌렀을 때 호출될 함수
        private void ToggleInventory(InputAction.CallbackContext context)
        {
            if (State != GameState.Run || _flowController == null || _inventoryUI == null) return;

            // 영입 중에는 인벤토리 비활성화
            if (_flowController.Phase == RunPhase.Recruitment) return;

            // --- ✨ 여기가 핵심 수정 부분 ✨ ---
            if (_inventoryUI.gameObject.activeInHierarchy)
            {
                // 현재 전투 페이즈(준비 중 포함)인지 확인
                if (_flowController.Phase == RunPhase.Battle)
                {
                    // 전투 중일 때는 맵으로 가지 않고, UI만 닫습니다.
                    _inventoryUI.ExitMode();
                }
                else
                {
                    // 맵이나 이벤트 화면일 때는 맵으로 돌아갑니다.
                    _flowController.RequestMap();
                }
            }
            // --- 수정 끝 ---
            else // 인벤토리가 닫혀있을 때
            {
                bool isReadOnly = (_flowController.Phase == RunPhase.Battle);
                _inventoryUI.Open(isReadOnly);
            }
        }
        // --- ✨ 여기가 핵심! 체크인(등록) 함수 추가 ---
        public void RegisterFlowController(GameFlowController fc)
        {
            _flowController = fc;
            Debug.Log("GameFlowController가 GameManager에 등록되었습니다.");
        }

        public void RegisterInventoryUI(InventoryPartyMode ui)
        {
            _inventoryUI = ui;
            Debug.Log("InventoryUI가 GameManager에 등록되었습니다.");
        }
    }
}
