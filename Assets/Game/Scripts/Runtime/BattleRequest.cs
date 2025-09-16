// Assets/Game/Scripts/Runtime/BattleRequest.cs (파일이 없다면 새로 생성)

namespace Game.Runtime
{
    /// <summary>
    /// 전투 시작에 필요한 최소한의 데이터.
    /// RunManager가 생성하여 BattleContext를 통해 BattleEntry로 전달됩니다.
    /// </summary>
    public sealed class BattleRequest
    {
        public string EncounterId { get; }
        public int Seed { get; }
        public int Difficulty { get; }

        public BattleRequest(string encounterId, int seed, int difficulty)
        {
            EncounterId = encounterId;
            Seed = seed;
            Difficulty = difficulty;
        }
    }
}