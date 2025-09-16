using UnityEngine;
using TMPro;

namespace Game.Runtime
{
    public sealed class EventMode : MonoBehaviour, IGameMode
    {
        GameFlowController _flow;
        [SerializeField] TMP_Text debugLabel;

        public void Setup(GameFlowController flow) => _flow = flow;

        public void EnterMode()
        {
            if (debugLabel) debugLabel.text = "EVENT";
            // ī�� ������, ������ Ŭ�� �ڵ鷯 ���
        }

        public void ExitMode()
        {
            // �ڵ鷯 ����
        }

        // �ӽ�: �ƹ� Ű �Է� �� ����
        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Input.anyKeyDown)
            {
                // ������ ����� RunManager�� �ݿ��ϴ� ���� ���⼭ ����
                _flow.RequestMap();
            }
        }
    }
}
