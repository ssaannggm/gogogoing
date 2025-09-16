// Assets/Game/Scripts/Runtime/GameFlowController.cs (수정 완료된 코드)
using System.Collections;
using UnityEngine;
using Game.Services;
using Game.Core;

namespace Game.Runtime
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Mode Roots")]
        [SerializeField] private GameObject recruitmentRoot; // [추가] RecruitmentSystem 오브젝트 연결
        [SerializeField] private GameObject mapRoot;
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private GameObject eventRoot;
        [SerializeField] private GameObject battleRoot;

        private IGameMode _recruit, _map, _inv, _evt, _battle; // [추가] _recruit
        private IGameMode _current;
        public RunPhase Phase { get; private set; }

        void Awake()
        {
            // --- ✨ 여기가 핵심 수정 부분 ✨ ---
            // 1. GameManager에 자기 자신을 먼저 등록합니다.
            GameManager.I?.RegisterFlowController(this);

            // [추가] recruitmentRoot에서 IGameMode 인터페이스를 가져옵니다.
            // 이를 위해 RecruitmentManager.cs가 IGameMode를 구현해야 합니다. (아래 3단계 참고)
            _recruit = recruitmentRoot ? recruitmentRoot.GetComponent<IGameMode>() : null;
            _map = mapRoot ? mapRoot.GetComponent<IGameMode>() : null;
            _inv = inventoryRoot ? inventoryRoot.GetComponent<IGameMode>() : null;
            _evt = eventRoot ? eventRoot.GetComponent<IGameMode>() : null;
            _battle = battleRoot ? battleRoot.GetComponent<IGameMode>() : null;

            // 3. 찾은 자식들을 GameManager에 직접 등록해줍니다.
            if (_inv is InventoryPartyMode inventoryPartyMode)
            {
                GameManager.I?.RegisterInventoryUI(inventoryPartyMode);
            }
            // --- 수정 끝 ---

            // [추가] _recruit.Setup() 호출
            _recruit?.Setup(this);
            _map?.Setup(this);
            _evt?.Setup(this);
            _inv?.Setup(this);
            _battle?.Setup(this);

            // 처음엔 모두 꺼 둠
            SetAllInactive();

            if (GameManager.I != null && GameManager.I.State == GameState.Run)
                StartCoroutine(BeginRunFlow());
            else
                EventBus.Subscribe<RunStarted>(_ => StartCoroutine(BeginRunFlow()));

            
        }

        IEnumerator BeginRunFlow()
        {
            var fader = FadeCanvas.TryGet();
            if (fader != null) yield return fader.FadeIn();

            GameManager.I?.Save?.TryAutoSave();

            // [수정] 첫 진입은 지도 선택이 아닌, 파티 영입으로 변경합니다.
            yield return SwitchPhase(RunPhase.Recruitment);
        }

        public IEnumerator SwitchPhase(RunPhase next)
        {
            _current?.ExitMode();
            SetAllInactive();

            Phase = next;
            GameManager.I?.Save?.TryAutoSave();

            switch (next)
            {
                // [추가] Recruitment 단계에 대한 처리
                case RunPhase.Recruitment:
                    if (recruitmentRoot) recruitmentRoot.SetActive(true);
                    _current = _recruit; break;

                case RunPhase.MapSelect:
                    if (mapRoot) mapRoot.SetActive(true);
                    _current = _map; break;


                case RunPhase.Event:
                    if (eventRoot) eventRoot.SetActive(true);
                    _current = _evt; break;

                case RunPhase.Battle:
                    if (battleRoot) battleRoot.SetActive(true);
                    _current = _battle; break;
            }

            _current?.EnterMode();
            yield break;
        }

        void SetAllInactive()
        {
            // [추가] recruitmentRoot도 비활성화 목록에 포함
            if (recruitmentRoot) recruitmentRoot.SetActive(false);
            if (mapRoot) mapRoot.SetActive(false);
            if (inventoryRoot) inventoryRoot.SetActive(false);
            if (eventRoot) eventRoot.SetActive(false);
            if (battleRoot) battleRoot.SetActive(false);
        }

        // --- 외부에서 특정 단계를 요청하는 함수들 ---
        public void RequestRecruitment() => StartCoroutine(SwitchPhase(RunPhase.Recruitment));
        public void RequestMap() => StartCoroutine(SwitchPhase(RunPhase.MapSelect));
        public void RequestEvent() => StartCoroutine(SwitchPhase(RunPhase.Event));
        public void RequestBattle() => StartCoroutine(SwitchPhase(RunPhase.Battle));
    }
}