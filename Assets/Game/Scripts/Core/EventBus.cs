// Assets/Game/Scripts/Core/EventBus.cs
using System;
using System.Collections.Generic;

namespace Game.Core
{
    public static class EventBus
    {
        static readonly Dictionary<Type, Action<object>> map = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (!map.TryGetValue(typeof(T), out var act)) map[typeof(T)] = _ => { };
            map[typeof(T)] += o => handler((T)o);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (map.TryGetValue(typeof(T), out var act)) map[typeof(T)] -= o => handler((T)o);
        }

        public static void Raise<T>(T evt)
        {
            if (map.TryGetValue(typeof(T), out var act)) act?.Invoke(evt);
        }
    }

    public enum GameState { Boot, Title, Run, Pause }

    public readonly struct GameStateChanged { public readonly GameState State; public GameStateChanged(GameState s) { State = s; } }
    public readonly struct RunStarted { public readonly Game.Runtime.RunManager Run; public RunStarted(Game.Runtime.RunManager r) { Run = r; } }
    public readonly struct RunEnded { public readonly bool Victory; public RunEnded(bool v) { Victory = v; } }
}
