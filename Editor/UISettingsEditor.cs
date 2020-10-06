using ToaruUnity.UI.Settings;
using UnityEditor;
using UnityEngine;

namespace ToaruUnityEditor.UI.Settings
{
    [CustomEditor(typeof(ToaruUISettings))]
    internal sealed class UISettingsEditor : Editor
    {
        private GUIContent m_CanvasTag;
        private GUIContent m_UIObjPoolSize;
        private GUIContent m_StackMinGrow;
        private GUIContent m_AutoClearWhenUnloadingScene;


        private void OnEnable()
        {
            m_CanvasTag = new GUIContent("Canvas Tag", "场景中Canvas对象的Tag，UIManager会根据这个Tag来自动寻找Canvas");
            m_UIObjPoolSize = new GUIContent("UI Obj Pool Size", "UI对象池的长度，该值必须大于或等于0。这个数值不应该过大");
            m_StackMinGrow = new GUIContent("Stack Min Grow", "栈长度不够时，重新分配的栈的长度的最小增长量，该值必须大于0");
            m_AutoClearWhenUnloadingScene = new GUIContent("Auto Clear When Unloading Scene", "如果开关被开启，则在场景被卸载时，会自动销毁所有缓存");
        }


        public override void OnInspectorGUI()
        {
            SerializedProperty canvasTag = serializedObject.FindProperty("m_CanvasTag");
            SerializedProperty uiObjPoolSize = serializedObject.FindProperty("m_UIObjPoolSize");
            SerializedProperty stackMinGrow = serializedObject.FindProperty("m_StackMinGrow");
            SerializedProperty autoClearWhenUnloadingScene = serializedObject.FindProperty("m_AutoClearWhenUnloadingScene");

            canvasTag.stringValue = EditorGUILayout.TagField(m_CanvasTag, canvasTag.stringValue);

            if(canvasTag.stringValue == "Untagged")
            {
                EditorGUILayout.HelpBox("Invalid Tag", MessageType.Error);
            }

            uiObjPoolSize.intValue = EditorGUILayout.IntSlider(m_UIObjPoolSize, uiObjPoolSize.intValue, 0, 20);
            stackMinGrow.intValue = EditorGUILayout.IntSlider(m_StackMinGrow, stackMinGrow.intValue, 1, 10);

            autoClearWhenUnloadingScene.boolValue = EditorGUILayout.ToggleLeft(m_AutoClearWhenUnloadingScene, autoClearWhenUnloadingScene.boolValue);

            if (!autoClearWhenUnloadingScene.boolValue)
            {
                EditorGUILayout.HelpBox("在场景被卸载时，需要手动销毁所有缓存，否则预制体等资源不会被释放", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}