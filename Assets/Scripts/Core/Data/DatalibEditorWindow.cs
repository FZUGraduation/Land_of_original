using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class DatalibEditorWindow : OdinMenuEditorWindow
{
    Datalib datalib;
    bool forceReload = true;
    [MenuItem("Window/LYWindow/Datalib _F8")]
    private static void OpenWindow()
    {
        DatalibEditorWindow window = GetWindow<DatalibEditorWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        window.datalib = new Datalib();
        window.datalib.CreateData();
        window.Show();
    }

    protected override void OnImGUI()
    {
        base.OnImGUI();
        var selected = this.MenuTree.Selection.FirstOrDefault();
        if (selected == null) return;
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.control)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Q:
                        if (selected.Value is null)
                        {
                            selected.Toggled = !selected.Toggled;
                            Event.current.Use(); // 防止事件传递到其他控件
                        }
                        else if (selected.Value is ConfigData)
                        {
                            selected.Parent.Toggled = !selected.Parent.Toggled;
                            Event.current.Use();
                        }
                        break;
                    case KeyCode.D:
                        if (selected.Value is ConfigData)
                        {
                            var source = selected.Value as ConfigData;
                            var newInstance = Instantiate(source);
                            newInstance.name = source.name;
                            datalib.AddData(newInstance);
                            Event.current.Use();
                        }
                        break;
                }
            }
            else
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Delete:
                        if (selected.Value is ConfigData)
                        {
                            var source = selected.Value as ConfigData;
                            datalib.RemoveData(source);
                            DestroyImmediate(source, true);
                            Event.current.Use();
                        }
                        break;
                }
            }

        }

        // Draw your OdinMenuTree here
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree();
        tree.DefaultMenuStyle.IconSize = 28.00f;
        tree.Config.DrawSearchToolbar = true;
        if (datalib == null)
        {
            datalib = new Datalib();
        }
        Debug.Log("LoadData");
        //等待loadData完成
        datalib.LoadData(forceReload);
        forceReload = false;
        // var sortedKeys = datalib.dataDict.Keys.ToList();
        // sortedKeys.Sort(CompareTypeOrder);

        tree.AddRange(datalib.creatorList, (v) =>
        {
            //Debug.Log(v.type.ToString());
            var path = Datalib.configTypeOrder.Find(item => item.type == v.type).path;
            //Debug.Log(path);
            return path;
        });
        var configDataType = typeof(ConfigData);
        foreach (var i in Datalib.configTypeOrder)
        {
            if (configDataType.IsAssignableFrom(i.type))
            {
                if (datalib.dataDict.TryGetValue(i.type, out var list))
                {
                    if (i.subPath != null)
                    {
                        // 添加二级目录
                        tree.AddRange(datalib.dataDict[i.type], (v) => $"{i.path}/{i.subPath(v)}/{v.key}{v.GetInstanceID()}");
                    }
                    else
                    {
                        tree.AddRange(datalib.dataDict[i.type], (v) => $"{i.path}/{v.key}{v.GetInstanceID()}");
                    }
                }
            }
            // else
            // {
            //     if (i.type == typeof(GlobalDB))
            //     {
            //         tree.Add(i.path, datalib.globalDB);
            //     }
            // }


        }



        tree.EnumerateTree().ForEach((item) =>
        {
            if (item.Value is ConfigData)
            {
                item.Name = "$";
            }
            // var newName = (item.Value as ConfigData)?.key;
            // if(newName != null){
            //     item.Name = "$";//newName;
            // }
        });
        tree.EnumerateTree().AddIcons<ConfigData>(x => x.icon);
        tree.EnumerateTree().Where(x => x.Value as ConfigData).ForEach(AddDragHandles);
        // Debug.Log("IconAdd");
        return tree;
    }
    private void AddDragHandles(OdinMenuItem menuItem)
    {
        menuItem.OnDrawItem += x => DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, false, false);
    }

    protected override void OnBeginDrawEditors()
    {
        var selected = this.MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;
        // Draws a toolbar with the name of the currently selected menu item.
        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            if (selected != null)
            {
                GUILayout.Label(selected.Name);
                if (selected.Value is ConfigData)
                {
                    if (GUILayout.Button("Duplicate"))
                    {
                        var source = selected.Value as ConfigData;
                        var newInstance = Instantiate(source);
                        newInstance.name = source.name;
                        datalib.AddData(newInstance);
                        // AssetDatabase.AddObjectToAsset(newInstance, datalib);
                        // AssetDatabase.SaveAssetIfDirty(datalib);
                        TrySelectMenuItemWithObject(newInstance);
                        //AssetDatabase.Refresh();
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        var source = selected.Value as ConfigData;
                        datalib.RemoveData(source);
                        // AssetDatabase.RemoveObjectFromAsset(source);
                        // EditorUtility.SetDirty(datalib);
                        // AssetDatabase.SaveAssetIfDirty(datalib);
                        // AssetDatabase.Refresh();
                        DestroyImmediate(source, true);
                    }
                }
                else if (selected.Value is EntityCreator)
                {
                    if (GUILayout.Button("Create"))
                    {
                        var source = selected.Value as EntityCreator;
                        var newInstance = ScriptableObject.CreateInstance(source.type) as ConfigData;
                        newInstance.name = source.type.Name;
                        datalib.AddData(newInstance);
                        TrySelectMenuItemWithObject(newInstance);
                    }
                }
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}
#endif