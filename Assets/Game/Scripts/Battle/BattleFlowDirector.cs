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
                    // ����: �ٷ� ���� ����(���� ��) ����
                    // e.Run ���ο� floorIndex ���� ���°� �ִٸ� ������Ű��,
                    // �׿� �´� ������ ���� BattleManager�� ����/��ü�� �� RunStarted�� ������ϴ� ������ ������ �� �ֽ��ϴ�.
                    EventBus.Raise(new RunStarted(e.Run));
                    break;

                case BattleResult.Draw:
                    // ��Ģ�� ���� ������ or �й� ó��
                    EventBus.Raise(new RunEnded(false));
                    GameManager.I.EndRun(false);
                    break;

                case BattleResult.Defeat:
                    EventBus.Raise(new RunEnded(false));
                    GameManager.I.EndRun(false);
                    break;

                case BattleResult.Aborted:
                    // Ÿ��Ʋ�� ���� ��
                    break;
            }
        }
    }
}
