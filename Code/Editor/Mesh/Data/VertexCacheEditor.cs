using System.Linq;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (VertexCache))]
	[CanEditMultipleObjects]
	public class VertexCacheEditor : Editor
	{
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			var firstVertexCache = target as VertexCache;

			var targetsHaveDifferentVertexCount = targets.Any (t => ((VertexCache)t).Data.Vertices.Length != firstVertexCache.Data.Vertices.Length);

			EditorGUI.showMixedValue = targetsHaveDifferentVertexCount;
			EditorGUILayout.LabelField ($"Vertex Count: {firstVertexCache.Data.Vertices.Length}");
			EditorGUI.showMixedValue = false;
		}
	}
}