// Assets/Game/Scripts/Data/MapNodeSO.cs
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Map Node", fileName = "MapNode_")]
    public sealed class MapNodeSO : ScriptableObject
    {
        public string nodeId;                // ���� ID(���� �ο�)
        public string title;
        [TextArea] public string desc;

        // �� ��忡�� Ʈ������ ����(������ ������ ���)
        public EncounterSO encounter;

        // ���� �̵� �ĺ�(������ ���ڿ� ID��)
        public MapNodeSO[] nextNodes;
        public bool opensInventory;          // ���� �� �κ��丮 ���� ����
        public bool opensEvent;              // �̺�Ʈ ī�� ǥ�� ����
    }
}
