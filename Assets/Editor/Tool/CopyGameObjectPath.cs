using UnityEditor;
using UnityEngine;

public static class CopyGameObjectPath
{
    [MenuItem("GameObject/复制路径", false, 0)]
    private static void CopyPath()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("未选中任何 GameObject！");
            return;
        }

        // 获取选中对象的 Transform
        Transform selectedTransform = Selection.activeGameObject.transform;

        // 生成层级路径
        string path = GetHierarchyPath(selectedTransform);

        // 复制到剪贴板
        GUIUtility.systemCopyBuffer = path;

        Debug.Log($"已复制路径: {path}");
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }
}