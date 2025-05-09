using UnityEngine;
using UnityEditor;

namespace OkapiKit.Editor
{
    [CustomEditor(typeof(ValueDisplay))]
    public class ValueDisplayEditor : OkapiBaseEditor
    {
        protected SerializedProperty valueSource;
        protected SerializedProperty valueHandler;
        protected SerializedProperty variable;
        protected SerializedProperty resource;

        ValueDisplay valueDisplay;
        protected virtual string typeOfDisplay { get; }

        protected override void OnEnable()
        {
            base.OnEnable();

            valueSource = serializedObject.FindProperty("valueSource"); ;
            valueHandler = serializedObject.FindProperty("valueHandler"); ;
            variable = serializedObject.FindProperty("variable"); ;
            resource = serializedObject.FindProperty("resource"); ;

            valueDisplay = target as ValueDisplay;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (WriteTitle())
            {
                StdEditor(false);
            }
        }

        protected virtual void StdEditor(bool useOriginalEditor = true, bool isFinal = true)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(valueSource, new GUIContent("Value Source", "What's the value source"));

            var source = (ValueDisplay.ValueSource)valueSource.enumValueIndex;

            switch (source)
            {
                case ValueDisplay.ValueSource.Variable:
                    EditorGUILayout.PropertyField(variable, new GUIContent("Variable", "Variable to display."), true);
                    break;
                case ValueDisplay.ValueSource.VariableInstance:
                    EditorGUILayout.PropertyField(valueHandler, new GUIContent("Variable Instance", "Variable instance to display."), true);
                    break;
                case ValueDisplay.ValueSource.Resource:
                    EditorGUILayout.PropertyField(resource, new GUIContent("Resource", "Resource to display."), true);
                    break;
                default:
                    break;
            }

            if (isFinal)
            {
                EditorGUILayout.PropertyField(propDescription, new GUIContent("Description", "This is for you to leave a comment for yourself or others."), true);

                EditorGUI.EndChangeCheck();

                serializedObject.ApplyModifiedProperties();
                (target as OkapiElement).UpdateExplanation();

                // Draw old editor, need it for now
                if (useOriginalEditor)
                {
                    base.OnInspectorGUI();
                }
            }
        }

        protected override GUIStyle GetTitleSyle()
        {
            return GUIUtils.GetActionTitleStyle();
        }

        protected override GUIStyle GetExplanationStyle()
        {
            return GUIUtils.GetActionExplanationStyle();
        }

        protected override string GetTitle()
        {
            string varName = "[UNDEFINED]";
            var source = (ValueDisplay.ValueSource)valueSource.enumValueIndex;

            switch (source)
            {
                case ValueDisplay.ValueSource.Variable:
                    varName = variable.objectReferenceValue?.name ?? "[UNDEFINED]";
                    break;
                case ValueDisplay.ValueSource.VariableInstance:
                    varName = valueHandler.objectReferenceValue?.name ?? "[UNDEFINED]";
                    break;
                case ValueDisplay.ValueSource.Resource:
                    varName = valueDisplay.GetResource()?.GetShortDescription(valueDisplay.gameObject);
                    break;
                default:
                    break;
            }

            return $"Display {varName} as {typeOfDisplay}";
        }

        protected override Texture2D GetIcon()
        {
            var varTexture = GUIUtils.GetTexture("VarDisplay");

            return varTexture;
        }

        protected override (Color, Color, Color) GetColors() => (GUIUtils.ColorFromHex("#fffaa7"), GUIUtils.ColorFromHex("#2f4858"), GUIUtils.ColorFromHex("#ffdf6e"));

    }
}