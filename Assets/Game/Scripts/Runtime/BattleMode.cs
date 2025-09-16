// Assets/Game/Scripts/Runtime/BattleMode.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core; // EventBus

namespace Game.Runtime
{
    public sealed class BattleMode : MonoBehaviour, IGameMode
    {
        GameFlowController _flow;
        bool _ended;
        System.Action<BattleEnded> _onEnd;

        public void Setup(GameFlowController flow) => _flow = flow;

        public void EnterMode()
        {
            _ended = false;
            _onEnd = _ => _ended = true;
            EventBus.Subscribe(_onEnd);
            StartCoroutine(CoBattleFlow());
        }

        public void ExitMode()
        {
            EventBus.Unsubscribe(_onEnd);
            _onEnd = null;
        }

        IEnumerator CoBattleFlow()
        {
            var fader = FadeCanvas.TryGet();
            if (fader != null) yield return fader.FadeOut();

            // 1) 배틀 씬 Additive 로드
            var load = SceneManager.LoadSceneAsync(Scenes.Battle, LoadSceneMode.Additive);
            while (!load.isDone) yield return null;

            // 2) 필요 시 ActiveScene을 전환(카메라/라이트가 배틀 씬에 있을 경우만)
            // SceneManager.SetActiveScene(SceneManager.GetSceneByName(Scenes.Battle));

            if (fader != null) yield return fader.FadeIn();

            // 3) 전투 종료 이벤트 대기
            while (!_ended) yield return null;

            // 4) 정리: 페이드 → 언로드 → 페이드인 → 컨텍스트 클리어 → 지도 복귀
            if (fader != null) yield return fader.FadeOut();

            var battleScene = SceneManager.GetSceneByName(Scenes.Battle);
            if (battleScene.IsValid())
                yield return SceneManager.UnloadSceneAsync(battleScene);

            BattleContext.Clear();

            if (fader != null) yield return fader.FadeIn();

            _flow.RequestMap();
        }
    }

    // 배틀 종료 신호용 이벤트 페이로드(단순화)
    //public readonly struct BattleEnded
    //{
    //    public readonly bool Victory;
    //    public BattleEnded(bool v) { Victory = v; }
    //}
}
