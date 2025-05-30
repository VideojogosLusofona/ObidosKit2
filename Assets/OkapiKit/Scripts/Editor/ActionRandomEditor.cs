using UnityEditor;
using UnityEngine;

namespace OkapiKit.Editor
{
    [CustomEditor(typeof(ActionRandom))]
    public class ActionRandomEditor : ActionEditor
    {
        SerializedProperty propActions;

        protected override void OnEnable()
        {
            base.OnEnable();

            propActions = serializedObject.FindProperty("actions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (WriteTitle())
            {
                StdEditor(false);

                var action = (target as ActionRandom);
                if (action == null) return;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(propActions, new GUIContent("Choices", "What are the options of actions to run?"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    action.UpdateExplanation();
                }
            }
        }
    }
}