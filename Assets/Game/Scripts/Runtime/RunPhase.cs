namespace Game.Runtime
{
    public enum RunPhase
    {
        None,
        Recruitment, // <-- [추가] 초기 파티 영입 단계
        MapSelect,
        Event,
        Battle
    }
}
