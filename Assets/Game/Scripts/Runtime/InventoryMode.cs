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
        [SerializeField] GameObject inventoryRoot;   // 인벤토리 UI 루트(선택)
        [SerializeField] bool armInputNextFrame = true; // 입력 무장 지연 여부

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
            yield return null; // 한 프레임 대기: UI 활성/레이아웃 완료 대기
            _inputArmed = true;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_inputArmed && Input.GetKeyDown(KeyCode.Return))
                _flow.RequestMap(); // 기본 흐름: 인벤토리 종료 후 다시 지도
        }
    }
}
