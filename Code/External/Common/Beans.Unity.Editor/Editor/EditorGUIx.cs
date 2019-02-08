using UnityEngine;

namespace Beans.Unity.Editor
{
	public static class EditorGUIx
	{
		public static void DrawOutlineRect (Rect rect, Color color, int width = 1, bool inner = false)
		{
			var leftSide = new Rect (rect);
			var rightSide = new Rect (rect);
			var bottomSide = new Rect (rect);
			var topSide = new Rect (rect);

			if (inner)
			{
				leftSide.xMax = leftSide.xMin + width;
				rightSide.xMin = rightSide.xMax - width;
				bottomSide.yMax = bottomSide.yMin + width;
				topSide.yMin = topSide.yMax - width;
			}
			else
			{
				leftSide.xMax = leftSide.xMin;
				leftSide.xMin -= width;

				rightSide.xMin = rightSide.xMax;
				rightSide.xMax += width;

				bottomSide.xMin -= width;
				bottomSide.xMax += width;
				bottomSide.yMax = bottomSide.yMin;
				bottomSide.yMin -= width;

				topSide.xMin -= width;
				topSide.xMax += width;
				topSide.yMin = topSide.yMax;
				topSide.yMax += width;
			}

			UnityEditor.EditorGUI.DrawRect (leftSide, color);
			UnityEditor.EditorGUI.DrawRect (rightSide, color);
			UnityEditor.EditorGUI.DrawRect (bottomSide, color);
			UnityEditor.EditorGUI.DrawRect (topSide, color);
		}
	}
}