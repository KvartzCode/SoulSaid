using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool condition = ((ReadOnlyAttribute)attribute).Env switch
            {
                ReadOnlyAttribute.Environment.Always => true,
                ReadOnlyAttribute.Environment.PlayMode => EditorApplication.isPlaying,
                ReadOnlyAttribute.Environment.EditMode => !EditorApplication.isPlaying,
                _ => throw new ArgumentOutOfRangeException(),
            };

            EditorGUI.BeginDisabledGroup(condition);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif

/// <summary>
/// Prevents editing this field in the inspector in the specified environment
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute
{
    public Environment Env { get; }

    public ReadOnlyAttribute(Environment environment = Environment.Always)
    {
        Env = environment;
    }

    public enum Environment
    {
        Always,
        PlayMode,
        EditMode,
    }
}