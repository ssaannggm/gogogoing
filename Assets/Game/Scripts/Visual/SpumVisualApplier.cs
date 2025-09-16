// Assets/Game/Scripts/Visuals/SpumVisualApplier.cs
using UnityEngine;
using Game.Data;
using System.Linq;

public class SpumVisualApplier : MonoBehaviour
{
    [Header("1. 추출할 UnitSO 정보 (에디터용)")]
    [Tooltip("새로 만들 UnitSO 에셋 파일의 이름이 됩니다.")]
    public string unitId = "new_unit";
    public string displayName = "New Unit";

    [Header("2. 적용할 기본 신체 Sprite Renderers")]
    public SpriteRenderer Head;
    public SpriteRenderer Hair;
    public SpriteRenderer FaceHair;
    public SpriteRenderer Body;
    public SpriteRenderer L_Arm;
    public SpriteRenderer R_Arm;
    public SpriteRenderer L_Foot;
    public SpriteRenderer R_Foot;

    [Header("3. 눈 (활성화된 것만 연결)")]
    public SpriteRenderer[] eyeBacks_Active;
    public SpriteRenderer[] eyeFronts_Active;

    /// <summary>
    /// UnitSO의 스프라이트 정보로 이 캐릭터의 '기본 외형'을 설정합니다. (런타임용)
    /// </summary>
    public void ApplyVisuals(UnitSO unitData)
    {
        if (unitData == null)
        {
            Debug.LogError("적용할 UnitSO 데이터가 없습니다.", gameObject);
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
    /// 자식 오브젝트를 검색하여 SpriteRenderer 필드를 자동으로 채웁니다. (에디터용)
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

        // 디버그 로그 추가
        if (Head != null) Debug.Log("SpumVisualApplier: SpriteRenderer 자동 연결이 완료되었습니다.");
        else Debug.LogError("SpumVisualApplier: 자동 연결 실패! 자식 오브젝트의 이름이 정확한지 확인해주세요. (예: 5_Head)");
    }
}