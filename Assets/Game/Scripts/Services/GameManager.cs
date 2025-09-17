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
        // GameManager.cs - ToggleInventory (교체: 핵심 2줄)
        private void ToggleInventory(InputAction.CallbackContext context)
        {
            if (State != GameState.Run || _flowController == null || _inventoryUI == null) return;
            if (_flowController.Phase == RunPhase.Recruitment) return;

            if (_inventoryUI.gameObject.activeInHierarchy)
            {
                _inventoryUI.ExitMode();
            }
            else
            {
                // 맵인 경우엔 무조건 편집 가능, 배틀 중에만 읽기전용
                bool isOnMap = (_flowController.Phase == RunPhase.MapSelect || _flowController.Phase == RunPhase.Event);
                bool isReadOnly = !isOnMap; // 맵/이벤트=false, 그 외(배틀)=true
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
