using UnityEditor;

namespace DeformEditor
{
	public class DeformModelPostProcessor : AssetPostprocessor
	{
		private void OnPreprocessModel()
		{
			if (DeformEditorSettings.ModelsReadableByDefault && assetImporter is ModelImporter m)
			{
				if (m.importSettingsMissing)
					m.isReadable = true;
			}
		}
	}
}