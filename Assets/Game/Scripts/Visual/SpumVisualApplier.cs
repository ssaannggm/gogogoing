// Assets/Game/Scripts/Visuals/SpumVisualApplier.cs
using UnityEngine;
using Game.Data;
using System.Linq;

public class SpumVisualApplier : MonoBehaviour
{
    [Header("1. ������ UnitSO ���� (�����Ϳ�)")]
    [Tooltip("���� ���� UnitSO ���� ������ �̸��� �˴ϴ�.")]
    public string unitId = "new_unit";
    public string displayName = "New Unit";

    [Header("2. ������ �⺻ ��ü Sprite Renderers")]
    public SpriteRenderer Head;
    public SpriteRenderer Hair;
    public SpriteRenderer FaceHair;
    public SpriteRenderer Body;
    public SpriteRenderer L_Arm;
    public SpriteRenderer R_Arm;
    public SpriteRenderer L_Foot;
    public SpriteRenderer R_Foot;

    [Header("3. �� (Ȱ��ȭ�� �͸� ����)")]
    public SpriteRenderer[] eyeBacks_Active;
    public SpriteRenderer[] eyeFronts_Active;

    /// <summary>
    /// UnitSO�� ��������Ʈ ������ �� ĳ������ '�⺻ ����'�� �����մϴ�. (��Ÿ�ӿ�)
    /// </summary>
    public void ApplyVisuals(UnitSO unitData)
    {
        if (unitData == null)
        {
            Debug.LogError("������ UnitSO �����Ͱ� �����ϴ�.", gameObject);
            return;
        }

        var parts = unitData.bodyParts;
        Head.sprite = parts.Head;
        Hair.sprite = parts.Hair;
        FaceHair.sprite = parts.FaceHair;
        Body.sprite = parts.Body;
        L_Arm.sprite = parts.L_Arm;
        R_Arm.sprite = parts.R_Arm;
        L_Foot.sprite = parts.L_Foot;
        R_Foot.sprite = parts.R_Foot;

        var eyeData = unitData.eyeParts;
        if (eyeData.back != null)
        {
            foreach (var renderer in eyeBacks_Active)
            {
                if (renderer != null) renderer.sprite = eyeData.back;
            }
        }
        if (eyeData.front != null)
        {
            foreach (var renderer in eyeFronts_Active)
            {
                if (renderer != null) renderer.sprite = eyeData.front;
            }
        }
    }

    /// <summary>
    /// �ڽ� ������Ʈ�� �˻��Ͽ� SpriteRenderer �ʵ带 �ڵ����� ä��ϴ�. (�����Ϳ�)
    /// </summary>
    public void AutoLinkRenderers()
    {
        SpriteRenderer FindRenderer(string name)
        {
            var found = transform.GetComponentsInChildren<Transform>(true)
                                 .FirstOrDefault(t => t.name == name);
            return found ? found.GetComponent<SpriteRenderer>() : null;
        }

        Head = FindRenderer("5_Head");
        Hair = FindRenderer("7_Hair");
        FaceHair = FindRenderer("6_FaceHair");
        Body = FindRenderer("Body");
        L_Arm = FindRenderer("20_L_Arm");
        R_Arm = FindRenderer("-20_R_Arm");
        L_Foot = FindRenderer("_3L_Foot");
        R_Foot = FindRenderer("_12R_Foot");

        eyeBacks_Active = transform.GetComponentsInChildren<SpriteRenderer>(false)
                                   .Where(r => r.name == "Back").ToArray();
        eyeFronts_Active = transform.GetComponentsInChildren<SpriteRenderer>(false)
                                    .Where(r => r.name == "Front").ToArray();

        // ����� �α� �߰�
        if (Head != null) Debug.Log("SpumVisualApplier: SpriteRenderer �ڵ� ������ �Ϸ�Ǿ����ϴ�.");
        else Debug.LogError("SpumVisualApplier: �ڵ� ���� ����! �ڽ� ������Ʈ�� �̸��� ��Ȯ���� Ȯ�����ּ���. (��: 5_Head)");
    }
}