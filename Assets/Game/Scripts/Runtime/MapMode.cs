// Assets/Game/Scripts/Runtime/MapMode.cs
using UnityEngine;
using Game.Services;   // GameManager, DataCatalog
using Game.UI;        // MapUIView
using Game.Data;      // MapNodeSO, EncounterSO

namespace Game.Runtime
{
    /// <summary>
    /// ���� ȭ�� ���: ���� ��带 �������ϰ� ���� ��� ���ÿ� ���� ��� ��ȯ�� �����մϴ�.
    /// </summary>
    public sealed class MapMode : MonoBehaviour, IGameMode
    {
        GameFlowController _flow;

        [Header("Wire in Inspector (under MapRoot)")]
        [SerializeField] MapUIView view;       // Title/Desc/Choices�� �׸��� UI ��
        [SerializeField] MapNodeSO startNode;  // ù ���� �� ����� ���� ���

        public void Setup(GameFlowController flow) => _flow = flow;

        void Awake()
        {
            // �Ǽ� ����: �ڽĿ��� �ڵ� Ž��
            if (!view) view = GetComponentInChildren<MapUIView>(includeInactive: true);
        }

        public void EnterMode()
        {
            var run = GameManager.I?.CurrentRun;
            if (run == null)
            {
                Debug.LogWarning("[MapMode] Run�� �����ϴ�. GameManager.StartNewRun ���� �����ؾ� �մϴ�.");
                return;
            }

            // ���� ����� ���� ��尡 ������ ���� ���� �ʱ�ȭ
            var node = run.GetCurrentNode();
            if (node == null)
            {
                if (!startNode)
                {
                    Debug.LogError("[MapMode] startNode�� ��� �ֽ��ϴ�.");
                    return;
                }
                run.SetCurrentNode(startNode);
                node = startNode;
            }

            Render(node);
        }

        public void ExitMode()
        {
            // �ʿ��� ��� UI ��Ȱ�� �� ����
        }

        void Render(MapNodeSO node)
        {
            if (!view)
            {
                Debug.LogError("[MapMode] MapUIView ���۷����� �����ϴ�.");
                return;
            }
            view.Render(node, OnPickNext);
        }

        void OnPickNext(MapNodeSO next)
        {
            if (!next) return;

            var run = GameManager.I?.CurrentRun;
            if (run == null) return;

            // ���� ��� ����(������ nodeId��)
            run.SetCurrentNode(next);

            // �б�: �κ��丮 / �̺�Ʈ / ��Ʋ / �Ϲ� ���
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
                // ���� ��û ������ ���� �� ���ؽ�Ʈ�� �ְ� ��Ʋ ���� ��ȯ
                var req = run.BuildBattleRequest(next.encounter, difficulty: 0);
                BattleContext.Current = req;
                _flow.RequestBattle();
                return;
            }

            // �Ϲ� ���� �ٽ� �� ������
            Render(next);
        }

#if UNITY_EDITOR
        // �����Ϳ� �׽�Ʈ: ���ؽ�Ʈ �޴����� ���� ���� ��� ����
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
