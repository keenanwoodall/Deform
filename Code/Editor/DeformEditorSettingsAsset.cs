using UnityEngine;

namespace DeformEditor
{
	public class DeformEditorSettingsAsset : ScriptableObject
	{
		[Header ("Scene")]
		public Color solidHandleColor = new Color (1f, 0.4f, 0f, 1f);
		public Color lightHandleColor = new Color (1f, 0.4f, 0f, 0.5f);
		public float dottedLineSize = 5f;
		public float screenspaceHandleCapSize = 0.0275f;
		public float screenspaceAngleHandleSize = 1.25f;

		[Header("Importer")]
		public bool modelsReadableByDefault = false;
	}
}