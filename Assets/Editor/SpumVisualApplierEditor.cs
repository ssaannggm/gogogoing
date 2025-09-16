// Assets/Editor/SpumVisualApplierEditor.cs (���� �Ϸ�� ���� �ڵ�)
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

        // --- ��ư 1: �ڵ� ���� ---
        if (GUILayout.Button("2. SpriteRenderer �ڵ� ����"))
        {
            Undo.RecordObject(applier, "Auto-Link Renderers");
            applier.AutoLinkRenderers();
        }

        GUILayout.Space(10); // ��ư ���̿� ���� �߰�

        // --- ��ư 2: UnitSO ���� ---
        if (GUILayout.Button("3. UnitSO ���� �� ����"))
        {
            ExtractUnitSO(applier);
        }
    }

    private void ExtractUnitSO(SpumVisualApplier applier)
    {
        if (applier.Head == null)
        {
            Debug.LogError("���� SpriteRenderer�� �����ؾ� �մϴ�. '�ڵ� ����' ��ư�� �����ּ���.");
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

        // TODO: �Ʒ� ��θ� �ڽ��� UnitSO ���� ��ο� �°� �������ּ���!
        string savePath = $"Assets/So/Unit/{applier.unitId}.asset";

        AssetDatabase.CreateAsset(newUnitSO, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"UnitSO ���� �Ϸ�! ���: {savePath}");

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newUnitSO;
    }
}