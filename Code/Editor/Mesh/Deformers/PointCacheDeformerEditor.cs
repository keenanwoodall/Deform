using UnityEditor;
using UnityEngine;
using Deform;

namespace DeformEditor
{
	[CustomEditor(typeof(PointCacheDeformer))]
	public class PointCacheDeformerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (target is PointCacheDeformer pc)
			{
				if (pc.PointCache != null)
					EditorGUILayout.LabelField(pc.PointCache.FrameSize.ToString(), EditorStyles.centeredGreyMiniLabel);
			}
		}
	}
}