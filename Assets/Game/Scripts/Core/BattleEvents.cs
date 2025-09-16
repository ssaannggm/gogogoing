// Assets/Game/Scripts/Core/BattleEvents.cs
using Game.Runtime;

namespace Game.Core
{
    public enum BattleResult { Victory, Defeat, Draw, Aborted }

    // 전투 시작/종료 이벤트 (EventBus.Raise(...)로 송출)
    public readonly struct BattleStarted
    {
        public readonly RunManager Run;
        public BattleStarted(RunManager run) { Run = run; }
    }

    public readonly struct BattleEnded
    {
        public readonly RunManager Run;
        public readonly BattleResult Result;
        public BattleEnded(RunManager run, BattleResult result)
        {
            Run = run; Result = result;
        }
    }
}
