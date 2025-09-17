// Assets/Editor/HierarchyDumper.cs
using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;

public static class HierarchyDumper
{
    [MenuItem("Tools/Dump Selected Hierarchy %#h")]
    public static void DumpSelectedHierarchy()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("No GameObject selected."); return; }

        var sb = new StringBuilder();
        Dump(go.transform, sb, 0);

        var text = sb.ToString();
        EditorGUIUtility.systemCopyBuffer = text;
        var path = Path.Combine(Application.dataPath, "../HierarchyDump.txt");
        File.WriteAllText(path, text, Encoding.UTF8);
        Debug.Log($"Hierarchy dumped to clipboard and {path}");
    }

    static void Dump(Transform t, StringBuilder sb, int depth)
    {
        sb.Append(' ', depth * 2);
        sb.AppendLine(t.name);
        for (int i = 0; i < t.childCount; i++)
            Dump(t.GetChild(i), sb, depth + 1);
    }
}
