using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneFlow : MonoBehaviour
{
    static SceneFlow _i;
    public static SceneFlow I
    {
        get
        {
            if (_i != null) return _i;
            var go = new GameObject("[SceneFlow]");
            _i = go.AddComponent<SceneFlow>();
            DontDestroyOnLoad(go);
            return _i;
        }
    }

    bool _busy;

    public IEnumerator LoadGameScene()
    {
        if (_busy) yield break;
        _busy = true;

        var fader = FadeCanvas.TryGet();
        if (fader) yield return fader.FadeOut();

        var op = SceneManager.LoadSceneAsync(Scenes.Game, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        if (fader) yield return fader.FadeIn();
        _busy = false;
    }

    public IEnumerator LoadTitleScene()
    {
        if (_busy) yield break;
        _busy = true;

        var fader = FadeCanvas.TryGet();
        if (fader) yield return fader.FadeOut();

        var op = SceneManager.LoadSceneAsync(Scenes.Title, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        if (fader) yield return fader.FadeIn();
        _busy = false;
    }
}

public static class Scenes
{
    public const string Title = "TitleScene";
    public const string Game = "GameScene";
    public const string Battle = "BattleScene";
}
