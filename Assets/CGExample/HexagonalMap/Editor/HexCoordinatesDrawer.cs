
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(HexCoordinatates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HexCoordinatates coordinatates = new HexCoordinatates(property.FindPropertyRelative("x").intValue,
                                                              property.FindPropertyRelative("z").intValue);

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinatates.ToString());
    }
}
