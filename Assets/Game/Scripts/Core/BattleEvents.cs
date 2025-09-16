// Assets/Game/Scripts/Core/BattleEvents.cs
using Game.Runtime;

namespace Game.Core
{
    public enum BattleResult { Victory, Defeat, Draw, Aborted }

    // ���� ����/���� �̺�Ʈ (EventBus.Raise(...)�� ����)
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
