// Assets/Game/Scripts/Runtime/InventoryMode.cs
using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.Runtime
{
    public sealed class InventoryMode : MonoBehaviour, IGameMode
    {
        GameFlowController _flow;

        [SerializeField] TMP_Text debugLabel;
        [SerializeField] GameObject inventoryRoot;   // �κ��丮 UI ��Ʈ(����)
        [SerializeField] bool armInputNextFrame = true; // �Է� ���� ���� ����

        bool _inputArmed;

        public void Setup(GameFlowController flow) => _flow = flow;

        public void EnterMode()
        {
            if (debugLabel) debugLabel.text = "INVENTORY";
            if (inventoryRoot) inventoryRoot.SetActive(true);

            _inputArmed = !armInputNextFrame;
            if (armInputNextFrame) StartCoroutine(ArmInputNextFrame());
        }

        public void ExitMode()
        {
            if (inventoryRoot) inventoryRoot.SetActive(false);
            _inputArmed = false;
        }

        IEnumerator ArmInputNextFrame()
        {
            yield return null; // �� ������ ���: UI Ȱ��/���̾ƿ� �Ϸ� ���
            _inputArmed = true;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_inputArmed && Input.GetKeyDown(KeyCode.Return))
                _flow.RequestMap(); // �⺻ �帧: �κ��丮 ���� �� �ٽ� ����
        }
    }
}
