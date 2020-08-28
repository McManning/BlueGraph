using System;
using UnityEngine;
using UnityEditor;
using BlueGraph.Editor;
using BlueGraph;

namespace BlueGraphSamples
{
    /// <summary>
    /// Custom PropertyDrawer that adds an edit button next to graph asset references.
    /// 
    /// CAN work for the base Graph type (as long as the CustomPropertyDrawer is flagged to show for children as well)
    /// It's cool and all - but superseded by GraphAssetHandler for usability.
    /// </summary>
    [CustomPropertyDrawer(typeof(MonoBehaviourGraph))]
    public class MonoBehaviourGraphPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var scriptRect = new Rect(position.x, position.y, position.width - 90, position.height);
            var buttonRect = new Rect(position.x + position.width - 90, position.y, 90, position.height);

            EditorGUI.PropertyField(scriptRect, property);
            if (property.objectReferenceValue != null)
            {
                if (GUI.Button(buttonRect, "Edit Graph")) {
                    var editor = UnityEditor.Editor.CreateEditor(property.objectReferenceValue) as GraphEditor;
                    if (!editor)
                    {
                        Debug.LogWarning("No editor found for graph asset");
                    } 
                    else
                    {
                        editor.CreateOrFocusEditorWindow();
                    }
                }
            }
            
            EditorGUI.EndProperty();
        }
    }
}
