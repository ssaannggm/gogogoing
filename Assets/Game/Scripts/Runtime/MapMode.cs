// Assets/Game/Scripts/Runtime/MapMode.cs
using UnityEngine;
using Game.Services;   // GameManager, DataCatalog
using Game.UI;        // MapUIView
using Game.Data;      // MapNodeSO, EncounterSO

namespace Game.Runtime
{
    /// <summary>
    /// 지도 화면 모드: 현재 노드를 렌더링하고 다음 노드 선택에 따라 모드 전환을 수행합니다.
    /// </summary>
    public sealed class MapMode : MonoBehaviour, IGameMode
    {
        GameFlowController _flow;

        [Header("Wire in Inspector (under MapRoot)")]
        [SerializeField] MapUIView view;       // Title/Desc/Choices를 그리는 UI 뷰
        [SerializeField] MapNodeSO startNode;  // 첫 진입 시 사용할 시작 노드

        public void Setup(GameFlowController flow) => _flow = flow;

        void Awake()
        {
            // 실수 방지: 자식에서 자동 탐색
            if (!view) view = GetComponentInChildren<MapUIView>(includeInactive: true);
        }

        public void EnterMode()
        {
            var run = GameManager.I?.CurrentRun;
            if (run == null)
            {
                Debug.LogWarning("[MapMode] Run이 없습니다. GameManager.StartNewRun 이후 진입해야 합니다.");
                return;
            }

            // 런에 저장된 현재 노드가 없으면 시작 노드로 초기화
            var node = run.GetCurrentNode();
            if (node == null)
            {
                if (!startNode)
                {
                    Debug.LogError("[MapMode] startNode가 비어 있습니다.");
                    return;
                }
                run.SetCurrentNode(startNode);
                node = startNode;
            }

            Render(node);
        }

        public void ExitMode()
        {
            // 필요한 경우 UI 비활성 등 정리
        }

        void Render(MapNodeSO node)
        {
            if (!view)
            {
                Debug.LogError("[MapMode] MapUIView 레퍼런스가 없습니다.");
                return;
            }
            view.Render(node, OnPickNext);
        }

        void OnPickNext(MapNodeSO next)
        {
            if (!next) return;

            var run = GameManager.I?.CurrentRun;
            if (run == null) return;

            // 현재 노드 갱신(저장은 nodeId로)
            run.SetCurrentNode(next);

            // 분기: 인벤토리 / 이벤트 / 배틀 / 일반 노드
            if (next.opensInventory)
            {
            //    _flow.RequestInventory();
                return;
            }

            if (next.opensEvent)
            {
                _flow.RequestEvent();
                return;
            }

            if (next.encounter != null)
            {
                // 전투 요청 스냅샷 생성 → 컨텍스트에 넣고 배틀 모드로 전환
                var req = run.BuildBattleRequest(next.encounter, difficulty: 0);
                BattleContext.Current = req;
                _flow.RequestBattle();
                return;
            }

            // 일반 노드면 다시 맵 렌더링
            Render(next);
        }

#if UNITY_EDITOR
        // 에디터용 테스트: 컨텍스트 메뉴에서 시작 노드로 즉시 렌더
        [ContextMenu("DEV: Render StartNode")]
        void DevRenderStart()
        {
            var run = GameManager.I?.CurrentRun;
            if (run == null || !startNode) return;
            run.SetCurrentNode(startNode);
            Render(startNode);
        }
#endif
    }
}
