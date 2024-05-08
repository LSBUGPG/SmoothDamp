using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Graph))]
public class GraphEditor : Editor
{
    void AddFloatLabel(VisualElement inspector, SerializedProperty property, string labelText)
    {
        Label label = new();
        inspector.TrackPropertyValue(property, p => label.text = $"{labelText}: {p.floatValue}");
        inspector.Add(label);
    }

    void AddIntLabel(VisualElement inspector, SerializedProperty property, string labelText)
    {
        Label label = new();
        inspector.TrackPropertyValue(property, p => label.text = $"{labelText}: {p.intValue}");
        inspector.Add(label);
    }
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();
        InspectorElement.FillDefaultInspector(inspector, serializedObject, this);
        AddIntLabel(inspector, serializedObject.FindProperty("inspectStep"), "Step");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectTime"), "Time");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectDistance"), "Distance");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectVelocity"), "Velocity");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectInput"), "Input");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectPosition"), "Position");
        AddFloatLabel(inspector, serializedObject.FindProperty("inspectTarget"), "Target");
        return inspector;
    }
}
