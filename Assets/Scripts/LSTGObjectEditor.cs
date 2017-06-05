using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LSTGObject))]
public class LSTGObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var obj = (LSTGObject) target;

		var type = typeof(LSTGObject);
		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
		{
			switch (prop.Name)
			{
				case "Group":
					EditorGUILayout.LabelField("Group", LayerMask.LayerToName(obj.Group));
					break;
				case "Layer":
					EditorGUILayout.LabelField("Layer", SortingLayer.IDToName(obj.Layer));
					break;
				default:
					var value = prop.GetValue(obj, null);
					EditorGUILayout.LabelField(prop.Name, value != null ? value.ToString() : "(null)");
					break;
			}
		}
	}
}
