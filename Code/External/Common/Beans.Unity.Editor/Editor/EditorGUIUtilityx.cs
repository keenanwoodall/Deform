using UnityEngine;

namespace Beans.Unity.Editor
{
	public static class EditorGUIUtilityx
	{
		public static Color BackgroundColor => UnityEditor.EditorGUIUtility.isProSkin ? new Color32 (56, 56, 56, 255) : new Color32 (194, 194, 194, 255);
		public static Color HighlightColor => UnityEditor.EditorGUIUtility.isProSkin ? new Color32 (91, 91, 91, 255) : new Color32 (222, 222, 222, 255);
		public static Color LowlightColor => UnityEditor.EditorGUIUtility.isProSkin ? new Color32 (30, 30, 30, 255) : new Color32 (153, 153, 153, 255);
		public static Color SelectedBlueColor => new Color32 (62, 125, 231, 255);
	}
}