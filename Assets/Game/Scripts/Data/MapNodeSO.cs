// Assets/Game/Scripts/Data/MapNodeSO.cs
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Map Node", fileName = "MapNode_")]
    public sealed class MapNodeSO : ScriptableObject
    {
        public string nodeId;                // 고유 ID(직접 부여)
        public string title;
        [TextArea] public string desc;

        // 이 노드에서 트리거할 전투(없으면 비전투 노드)
        public EncounterSO encounter;

        // 다음 이동 후보(간단히 문자열 ID로)
        public MapNodeSO[] nextNodes;
        public bool opensInventory;          // 진입 시 인벤토리 열기 여부
        public bool opensEvent;              // 이벤트 카드 표시 여부
    }
}
