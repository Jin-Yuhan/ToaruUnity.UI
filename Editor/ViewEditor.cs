using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ToaruUnity.UI;
using UnityEditorInternal;
using System;

namespace ToaruUnityEditor.UI
{
    [CustomEditor(typeof(AbstractView), true)]
    internal sealed class ViewEditor : Editor
    {
        private bool m_DebugFoldout = false;

        public override void OnInspectorGUI()
        {
            AbstractView view = target as AbstractView;

            ViewGUIUtility.DrawViewDebugInfo(view, ref m_DebugFoldout);

            SerializedProperty p = serializedObject.GetIterator();

            p.NextVisible(true);
            p.NextVisible(false);
            SerializedProperty stateChangedEvent = p.Copy();

            if (view is FadeInOutUGUIView)
            {
                EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);

                p.NextVisible(false);
                EditorGUILayout.PropertyField(p);
                p.NextVisible(false);
                EditorGUILayout.PropertyField(p);

                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

            while (p.NextVisible(false))
            {
                EditorGUILayout.PropertyField(p);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(stateChangedEvent);
            
            serializedObject.ApplyModifiedProperties();
        }
    }

    public static class ViewGUIUtility
    {

        public static void DrawViewDebugInfo(AbstractView view, ref bool foldout)
        {
            if (!EditorApplication.isPlaying)
                return;

            // test awake etc

            if (PrefabUtility.IsPartOfPrefabAsset(view))
                return;

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Runtime");

            if (foldout)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField($"Key: {GetObjString(view.InternalKey)}");

                    EditorGUILayout.LabelField("ViewState", EditorStyles.boldLabel);
                    if (view.IsTransformingState)
                    {
                        EditorGUILayout.HelpBox($"Transforming({view.RemainingTransformStateTaskCount} Remains)", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(view.State.ToString(), MessageType.Info);
                    }

                    if (view.Actions != null)
                    {
                        EditorGUILayout.LabelField("Action Center", EditorStyles.boldLabel);

                        EditorGUILayout.LabelField(view.Actions.GetType().ToString());
                        EditorGUILayout.LabelField($"{view.Actions.ExecutingCoroutineCount} Executing Coroutines");

                        if (view.Actions.ActionCount > 0)
                        {
                            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

                            foreach (KeyValuePair<int, Delegate> pair in view.Actions.ActionMap)
                            {
                                EditorGUILayout.LabelField($"[{pair.Key}] {pair.Value.Method.Name}");
                            }
                        }
                    }

                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static string GetObjString(object obj)
        {
            return obj == null ? "Null" : obj.ToString();
        }
    }
}