using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class InspectorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("box");
        DrawInspectorButtons();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        DrawDefaultInspector();
    }

    private void DrawInspectorButtons()
    {
        IEnumerable<MethodInfo> methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<InspectorButtonAttribute>(true) != null)
            .Where(m => m.GetParameters().Length == 0)
            .Where(m => m.ReturnType == typeof(void));

        foreach (MethodInfo method in methods)
        {
            InspectorButtonAttribute attribute = method.GetCustomAttribute<InspectorButtonAttribute>(true);
            string buttonLabel = string.IsNullOrWhiteSpace(attribute.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : attribute.Label;

            bool enabled = !(attribute.PlayModeOnly && !Application.isPlaying);

            if (attribute.EditModeOnly && Application.isPlaying)
                enabled = false;

            using (new EditorGUI.DisabledScope(!enabled))
            {
                if (!GUILayout.Button(buttonLabel)) continue;
                foreach (Object selectedTarget in targets)
                {
                    InvokeMethod(selectedTarget, method);
                }
            }
        }
    }

    private void InvokeMethod(Object selectedTarget, MethodInfo method)
    {
        Undo.RecordObject(selectedTarget, method.Name);
        method.Invoke(selectedTarget, null);
        EditorUtility.SetDirty(selectedTarget);
    }
}