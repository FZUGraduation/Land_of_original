using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;

public class PrefabComponentCopier : OdinEditorWindow
{
    [Title("源预制体")]
    [Required, Tooltip("拖拽源预制体到这里")]
    public GameObject sourcePrefab;

    [Title("目标预制体列表")]
    [Required, Tooltip("拖拽目标预制体到这里")]
    public GameObject[] targetPrefabs;

    [Title("组件名称")]
    [Required, Tooltip("输入需要复制的组件名称（包括命名空间）")]
    public string componentName = "BaseEnemy";

    [MenuItem("Tools/Editor/Prefab Component Copier")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabComponentCopier>();
        window.titleContent = new GUIContent("Prefab Component Copier");
        window.Show();
    }

    [Button("复制组件数据", ButtonSizes.Large)]
    private void CopyComponentData()
    {
        if (sourcePrefab == null)
        {
            Debug.LogError("源预制体未设置！");
            return;
        }

        if (targetPrefabs == null || targetPrefabs.Length == 0)
        {
            Debug.LogError("目标预制体列表为空！");
            return;
        }

        if (string.IsNullOrEmpty(componentName))
        {
            Debug.LogError("组件名称不能为空！");
            return;
        }

        // 获取类型
        Type type = GetTypeByName(componentName);
        if (type == null)
        {
            Debug.LogError($"未找到类型：{componentName}");
            return;
        }

        // 获取源预制体中的目标组件
        Component sourceComponent = sourcePrefab.GetComponent(type);
        if (sourceComponent == null)
        {
            Debug.LogError($"源预制体中未找到组件：{componentName}");
            return;
        }

        // 遍历目标预制体
        foreach (GameObject targetPrefab in targetPrefabs)
        {
            if (targetPrefab == null)
            {
                Debug.LogWarning("目标预制体为空，跳过！");
                continue;
            }

            // 获取目标预制体中的目标组件
            Component targetComponent = targetPrefab.GetComponent(type);
            if (targetComponent == null)
            {
                Debug.LogWarning($"目标预制体 {targetPrefab.name} 中未找到组件：{componentName}，跳过！");
                continue;
            }

            // 复制组件数据
            CopyComponentValues(sourceComponent, targetComponent);

            // 保存修改后的预制体
            PrefabUtility.SavePrefabAsset(targetPrefab);
            Debug.Log($"已更新预制体：{targetPrefab.name}");
        }
    }

    // 复制组件的字段值
    private static void CopyComponentValues(Component source, Component target)
    {
        var fields = source.GetType().GetFields();
        foreach (var field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }

        var properties = source.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                property.SetValue(target, property.GetValue(source));
            }
        }
    }
    private Type GetTypeByName(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
}