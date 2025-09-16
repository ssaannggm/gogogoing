// Assets/Game/Scripts/Battle/BattleEntry.cs (수정 완료된 코드)
using UnityEngine;
using Game.Runtime;
using Game.Services;
using Game.Data;

namespace Game.Battle
{
    public class BattleEntry : MonoBehaviour
    {
        [SerializeField] private BattleManager _battleManager;

        void Awake()
        {
            if (!_battleManager) _battleManager = FindObjectOfType<BattleManager>();
        }

        void Start()
        {
            var data = GameManager.I?.Data;
            var request = BattleContext.Current;

            if (_battleManager == null || data == null || request == null)
            {
                Debug.LogError("[BattleEntry] 전투 시작에 필요한 정보가 부족합니다.");
                return;
            }

            EncounterSO encounter = data.GetEncounterById(request.EncounterId);
            if (encounter == null) return;

            // --- ✨ 여기가 핵심 수정 부분 ✨ ---
            // 1. BattleManager에게 전투 '준비'를 지시합니다 (적군만 스폰).
            _battleManager.PrepareBattle(encounter);

            // 2. GameManager를 통해 인벤토리 UI를 '수정 가능' 모드로 엽니다.
            GameManager.I?.InventoryUI?.Open(isReadOnly: false);
            // --- 수정 끝 ---

            BattleContext.Clear();
        }
    }
}