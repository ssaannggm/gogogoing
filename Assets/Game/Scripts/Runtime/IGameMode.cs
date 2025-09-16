using UnityEngine;

namespace Game.Runtime
{
    public interface IGameMode
    {
        // 각 모드 루트에 붙은 컴포넌트가 GameFlow에서 주입받음
        void Setup(GameFlowController flow);
        void EnterMode();   // 화면 켜기, 입력 바인딩
        void ExitMode();    // 화면 끄기, 핸들러 해제
    }
}
