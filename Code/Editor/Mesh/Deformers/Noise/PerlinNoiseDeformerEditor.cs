using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (PerlinNoiseDeformer)), CanEditMultipleObjects]
	public class PerlinNoiseDeformerEditor : NoiseDeformerEditor
	{
		public override void OnInspectorGUI () => base.OnInspectorGUI ();
	}
}