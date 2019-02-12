using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (CellularNoiseDeformer)), CanEditMultipleObjects]
	public class CellularNoiseDeformerEditor : NoiseDeformerEditor
	{
		protected override void OnEnable ()
		{
			base.OnEnable ();

			drawOffsetVectorOverride = DrawVector4PropertyAsVector3;
			drawOffsetSpeedVectorOverride = DrawVector4PropertyAsVector3;
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
		}

		private void DrawVector4PropertyAsVector3 (SerializedProperty property, GUIContent defaultContent)
		{
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				var newVector = EditorGUILayout.Vector3Field (defaultContent, property.vector4Value);
				if (check.changed)
					property.vector4Value = newVector;
			}
		}
	}
}