using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (SimplexNoiseDeformer)), CanEditMultipleObjects]
	public class SimplexNoiseDeformerEditor : NoiseDeformerEditor
	{
		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();
		}
	}
}