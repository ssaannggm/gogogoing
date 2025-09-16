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

            // 1) ��Ʋ �� Additive �ε�
            var load = SceneManager.LoadSceneAsync(Scenes.Battle, LoadSceneMode.Additive);
            while (!load.isDone) yield return null;

            // 2) �ʿ� �� ActiveScene�� ��ȯ(ī�޶�/����Ʈ�� ��Ʋ ���� ���� ��츸)
            // SceneManager.SetActiveScene(SceneManager.GetSceneByName(Scenes.Battle));

            if (fader != null) yield return fader.FadeIn();

            // 3) ���� ���� �̺�Ʈ ���
            while (!_ended) yield return null;

            // 4) ����: ���̵� �� ��ε� �� ���̵��� �� ���ؽ�Ʈ Ŭ���� �� ���� ����
            if (fader != null) yield return fader.FadeOut();

            var battleScene = SceneManager.GetSceneByName(Scenes.Battle);
            if (battleScene.IsValid())
                yield return SceneManager.UnloadSceneAsync(battleScene);

            BattleContext.Clear();

            if (fader != null) yield return fader.FadeIn();

            _flow.RequestMap();
        }
    }

    // ��Ʋ ���� ��ȣ�� �̺�Ʈ ���̷ε�(�ܼ�ȭ)
    //public readonly struct BattleEnded
    //{
    //    public readonly bool Victory;
    //    public BattleEnded(bool v) { Victory = v; }
    //}
}
