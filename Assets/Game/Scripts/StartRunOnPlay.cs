// 아무 빈 오브젝트에 붙여 테스트용으로 사용
using System.Collections;
using UnityEngine;
using Game.Services;
[DefaultExecutionOrder(-10)]
public class StartRunOnPlay : MonoBehaviour
{
    [SerializeField] int seed = 12345;

    [Header("Auto bootstrap")]
    [SerializeField] bool autoInstantiateManager = true;
    [SerializeField] string managerResourcePath = "_Core/GameManagerRoot"; // Resources 경로

    IEnumerator Start()
    {
        // 씬에 이미 있으면 한 프레임 대기(awake 설정 시간)
        if (GameManager.I == null)
        {
            var existing = FindObjectOfType<GameManager>();
            if (existing != null) yield return null;
        }

        // 없으면 프리팹에서 로드해 생성
        if (GameManager.I == null && autoInstantiateManager)
        {
            var prefab = Resources.Load<GameObject>(managerResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"StartRunOnPlay: GameManager 프리팹을 Resources/{managerResourcePath} 경로에 두세요.");
                yield break;
            }
            Instantiate(prefab);
            yield return null; // GameManager.Awake가 실행될 프레임 대기
        }

        if (GameManager.I == null)
        {
            Debug.LogError("StartRunOnPlay: GameManager가 준비되지 않았습니다.");
            yield break;
        }

        GameManager.I.StartNewRun(seed); // → BattleManager가 RunStarted 이벤트를 받아 전투 시작
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.I.EndRun(false);     // 런 종료
            GameManager.I.StartNewRun(seed); // 재시작
        }
    }
}
