
using UnityEditor;
using UnityEngine;

public class MarkAnimationClipAsLegacy
{
    [MenuItem("Tools/Editor/Mark AnimationClip As Legacy")]
    static void MarkAsLegacy()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip)
            {
                AnimationClip clip = obj as AnimationClip;
                SerializedObject serializedClip = new SerializedObject(clip);
                serializedClip.FindProperty("m_Legacy").boolValue = true;
                serializedClip.ApplyModifiedProperties();
                Debug.Log("Marked " + clip.name + " as Legacy");
            }
        }
    }
}
