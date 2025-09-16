// Assets/Game/Scripts/Runtime/BattleRequest.cs (������ ���ٸ� ���� ����)

namespace Game.Runtime
{
    /// <summary>
    /// ���� ���ۿ� �ʿ��� �ּ����� ������.
    /// RunManager�� �����Ͽ� BattleContext�� ���� BattleEntry�� ���޵˴ϴ�.
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