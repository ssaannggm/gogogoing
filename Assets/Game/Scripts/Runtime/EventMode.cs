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
            // 카드 렌더링, 선택지 클릭 핸들러 등록
        }

        public void ExitMode()
        {
            // 핸들러 해제
        }

        // 임시: 아무 키 입력 시 종료
        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Input.anyKeyDown)
            {
                // 선택지 결과를 RunManager에 반영하는 훅은 여기서 연결
                _flow.RequestMap();
            }
        }
    }
}
