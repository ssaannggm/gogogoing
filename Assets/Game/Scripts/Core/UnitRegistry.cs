// Assets/Game/Scripts/Core/UnitRegistry.cs
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;

public static class UnitRegistry
{
    static readonly HashSet<UnitStats> allies = new();
    static readonly HashSet<UnitStats> enemies = new();

    public static void Register(UnitStats st)
    {
        if (!st) return;
        if (st.team == Team.Ally) allies.Add(st);
        else enemies.Add(st);
    }

    public static void Unregister(UnitStats st)
    {
        if (!st) return;
        if (st.team == Team.Ally) allies.Remove(st);
        else enemies.Remove(st);
    }

    public static UnitStats FirstAlive(Team t)
    {
        var set = (t == Team.Ally) ? allies : enemies;
        foreach (var st in set) { if (st && st.GetComponent<Health>() is { IsDead: false }) return st; }
        return null;
    }
}
