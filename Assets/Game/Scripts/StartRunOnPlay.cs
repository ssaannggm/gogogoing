// �ƹ� �� ������Ʈ�� �ٿ� �׽�Ʈ������ ���
using System.Collections;
using UnityEngine;
using Game.Services;
[DefaultExecutionOrder(-10)]
public class StartRunOnPlay : MonoBehaviour
{
    [SerializeField] int seed = 12345;

    [Header("Auto bootstrap")]
    [SerializeField] bool autoInstantiateManager = true;
    [SerializeField] string managerResourcePath = "_Core/GameManagerRoot"; // Resources ���

    IEnumerator Start()
    {
        // ���� �̹� ������ �� ������ ���(awake ���� �ð�)
        if (GameManager.I == null)
        {
            var existing = FindObjectOfType<GameManager>();
            if (existing != null) yield return null;
        }

        // ������ �����տ��� �ε��� ����
        if (GameManager.I == null && autoInstantiateManager)
        {
            var prefab = Resources.Load<GameObject>(managerResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"StartRunOnPlay: GameManager �������� Resources/{managerResourcePath} ��ο� �μ���.");
                yield break;
            }
            Instantiate(prefab);
            yield return null; // GameManager.Awake�� ����� ������ ���
        }

        if (GameManager.I == null)
        {
            Debug.LogError("StartRunOnPlay: GameManager�� �غ���� �ʾҽ��ϴ�.");
            yield break;
        }

        GameManager.I.StartNewRun(seed); // �� BattleManager�� RunStarted �̺�Ʈ�� �޾� ���� ����
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.I.EndRun(false);     // �� ����
            GameManager.I.StartNewRun(seed); // �����
        }
    }
}
