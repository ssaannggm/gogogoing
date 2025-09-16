using UnityEngine;

namespace Game.Runtime
{
    public interface IGameMode
    {
        // �� ��� ��Ʈ�� ���� ������Ʈ�� GameFlow���� ���Թ���
        void Setup(GameFlowController flow);
        void EnterMode();   // ȭ�� �ѱ�, �Է� ���ε�
        void ExitMode();    // ȭ�� ����, �ڵ鷯 ����
    }
}
