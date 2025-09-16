// Assets/Game/Scripts/Battle/BattleFlowDirector.cs
using UnityEngine;
using Game.Core;
using Game.Services;

namespace Game.Battle
{
    public sealed class BattleFlowDirector : MonoBehaviour
    {
        void OnEnable()
        {
            EventBus.Subscribe<BattleEnded>(OnBattleEnded);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<BattleEnded>(OnBattleEnded);
        }

        void OnBattleEnded(BattleEnded e)
        {
            switch (e.Result)
            {
                case BattleResult.Victory:
                    // 예시: 바로 다음 전투(다음 층) 시작
                    // e.Run 내부에 floorIndex 같은 상태가 있다면 증가시키고,
                    // 그에 맞는 프리팹 셋을 BattleManager에 주입/교체한 뒤 RunStarted를 재발행하는 식으로 설계할 수 있습니다.
                    EventBus.Raise(new RunStarted(e.Run));
                    break;

                case BattleResult.Draw:
                    // 규칙에 따라 재전투 or 패배 처리
                    EventBus.Raise(new RunEnded(false));
                    GameManager.I.EndRun(false);
                    break;

                case BattleResult.Defeat:
                    EventBus.Raise(new RunEnded(false));
                    GameManager.I.EndRun(false);
                    break;

                case BattleResult.Aborted:
                    // 타이틀로 복귀 등
                    break;
            }
        }
    }
}
