// Assets/Game/Scripts/Runtime/BattleContext.cs
namespace Game.Runtime
{
    public static class BattleContext
    {
        public static BattleRequest Current;
        public static void Clear() => Current = null;
    }
}
