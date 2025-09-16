// Assets/Editor/SpumVisualApplierEditor.cs (통합 완료된 최종 코드)
using UnityEngine;
using UnityEditor;
using Game.Data;

[CustomEditor(typeof(SpumVisualApplier))]
public class SpumVisualApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var applier = (SpumVisualApplier)target;

        // --- 버튼 1: 자동 연결 ---
        if (GUILayout.Button("2. SpriteRenderer 자동 연결"))
        {
            Undo.RecordObject(applier, "Auto-Link Renderers");
            applier.AutoLinkRenderers();
        }

        GUILayout.Space(10); // 버튼 사이에 공간 추가

        // --- 버튼 2: UnitSO 추출 ---
        if (GUILayout.Button("3. UnitSO 생성 및 추출"))
        {
            ExtractUnitSO(applier);
        }
    }

    private void ExtractUnitSO(SpumVisualApplier applier)
    {
        if (applier.Head == null)
        {
            Debug.LogError("먼저 SpriteRenderer를 연결해야 합니다. '자동 연결' 버튼을 눌러주세요.");
            return;
        }

        var newUnitSO = ScriptableObject.CreateInstance<UnitSO>();

        newUnitSO.unitId = applier.unitId;
        newUnitSO.displayName = applier.displayName;

        newUnitSO.bodyParts.Head = applier.Head.sprite;
        newUnitSO.bodyParts.Hair = applier.Hair.sprite;
        newUnitSO.bodyParts.FaceHair = applier.FaceHair.sprite;
        newUnitSO.bodyParts.Body = applier.Body.sprite;
        newUnitSO.bodyParts.L_Arm = applier.L_Arm.sprite;
        newUnitSO.bodyParts.R_Arm = applier.R_Arm.sprite;
        newUnitSO.bodyParts.L_Foot = applier.L_Foot.sprite;
        newUnitSO.bodyParts.R_Foot = applier.R_Foot.sprite;

        if (applier.eyeBacks_Active.Length > 0)
            newUnitSO.eyeParts.back = applier.eyeBacks_Active[0].sprite;
        if (applier.eyeFronts_Active.Length > 0)
            newUnitSO.eyeParts.front = applier.eyeFronts_Active[0].sprite;

        // TODO: 아래 경로를 자신의 UnitSO 저장 경로에 맞게 수정해주세요!
        string savePath = $"Assets/So/Unit/{applier.unitId}.asset";

        AssetDatabase.CreateAsset(newUnitSO, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"UnitSO 생성 완료! 경로: {savePath}");

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newUnitSO;
    }
}