using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LSTGObject))]
public class LSTGObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var obj = (LSTGObject) target;
		EditorGUILayout.LabelField("Id", obj.Id.ToString());
		EditorGUILayout.LabelField("Group", LayerMask.LayerToName(obj.Group));

		EditorGUILayout.LabelField("Colli", obj.Colli.ToString());
	}
}
