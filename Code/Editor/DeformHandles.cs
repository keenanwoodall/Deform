using UnityEngine;
using UnityEditor;

namespace DeformEditor
{
	/// <summary>
	/// Assists in drawing custom handles based on a consistent style.
	/// </summary>
	public static class DeformHandles
	{
		public const int DEF_CURVE_SEGMENTS = 120;

		public delegate void LineMethod (Vector3 a, Vector3 b);
		public enum LineMode { Solid, Light, SolidDotted, LightDotted }
		public enum CapType { Circle, Rectangle, Dot }

		private static LineMethod GetLineMethod (LineMode mode)
		{
			switch (mode)
			{
				default:
				case LineMode.Solid:
				case LineMode.Light:
					return Handles.DrawLine;
				case LineMode.SolidDotted:
				case LineMode.LightDotted:
					return (a, b) => Handles.DrawDottedLine (a, b, DeformEditorSettings.DottedLineSize);
			}
		}

		private static Color GetLineColor (LineMode mode)
		{
			switch (mode)
			{
				default:
				case LineMode.Solid:
				case LineMode.SolidDotted:
					return DeformEditorSettings.SolidHandleColor;
				case LineMode.Light:
				case LineMode.LightDotted:
					return DeformEditorSettings.LightHandleColor;
			}
		}

		public static void HandleCapFunction (int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType)
		{
			Handles.DotHandleCap (controlId, position, rotation, size, eventType);
		}

		public static void Line (Vector3 a, Vector3 b, LineMode mode)
		{
			using (new Handles.DrawingScope (GetLineColor (mode)))
				GetLineMethod (mode) (a, b);
		}
		public static void Line (Vector3 a, Vector3 b, LineMode mode, Color color)
		{
			using (new Handles.DrawingScope (color))
				GetLineMethod (mode) (a, b);
		}

		/// <summary>
		/// Draws an arc.
		/// </summary>
		public static void WireArc (Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
		{
			using (new Handles.DrawingScope (DeformEditorSettings.LightHandleColor))
				Handles.DrawWireArc (center, normal, from, angle, radius);
		}

		/// <summary>
		/// Draws a full circle.
		/// </summary>
		public static void Circle (Vector3 center, Vector3 normal, Vector3 from, float radius)
		{
			using (new Handles.DrawingScope (DeformEditorSettings.LightHandleColor))
				Handles.DrawWireArc (center, normal, from, 360f, radius);
		}

		/// <summary>
		/// Draws a handle that can be dragged along an axis.
		/// </summary>
		public static Vector3 Slider (Vector3 position, Vector3 direction)
		{
			var size = HandleUtility.GetHandleSize (position) * DeformEditorSettings.ScreenspaceSliderHandleCapSize;
			using (new Handles.DrawingScope (DeformEditorSettings.SolidHandleColor))
				return Handles.Slider (position, direction, size, HandleCapFunction, 0f);
		}

		/// <summary>
		/// Draws a small handle that can be dragged along an axis.
		/// </summary>
		public static Vector3 MiniSlider (Vector3 position, Vector3 direction)
		{
			var size = HandleUtility.GetHandleSize (position) * DeformEditorSettings.ScreenspaceSliderHandleCapSize * 0.5f;
			using (new Handles.DrawingScope (DeformEditorSettings.SolidHandleColor))
				return Handles.Slider (position, direction, size, HandleCapFunction, 0f);
		}

		/// <summary>
		/// Draws a radius handle.
		/// </summary>
		public static float Radius (Quaternion rotation, Vector3 position, float radius)
		{
			using (new Handles.DrawingScope (DeformEditorSettings.SolidHandleColor))
				return Handles.RadiusHandle (rotation, position, radius);
		}

		/// <summary>
		/// Draws an animation curve in the scene, in 3D space.
		/// </summary>
		public static void Curve (AnimationCurve curve, Vector3 position, Quaternion rotation, Vector3 scale, float magnitude, float xOffset, float yOffset, int segments = DEF_CURVE_SEGMENTS)
		{
			if (curve == null || curve.length == 0)
				return;

			var lastPoint = Vector3.zero;
			var lastPointSet = false;

			for (int i = 0; i < segments; i++)
			{
				var height = curve.Evaluate (i / (float)segments);

				var point = Vector3.Lerp (Vector3.zero, Vector3.forward, i / (float)segments);
				point.z -= xOffset;
				point.y = (height * magnitude) + yOffset;

				var worldPoint = Matrix4x4.TRS (position, rotation, scale).MultiplyPoint3x4 (point);
				//axis.localToWorldMatrix.MultiplyPoint3x4 (point);

				if (lastPointSet)
					Line (lastPoint, worldPoint, LineMode.Solid);

				lastPoint = worldPoint;
				lastPointSet = true;
			}
		}

		public static void Curve (AnimationCurve curve, Transform axis, float magnitude, float xOffset, float yOffset, int segments = DEF_CURVE_SEGMENTS)
		{
			Curve (curve, axis.position, axis.rotation, axis.lossyScale, magnitude, xOffset, yOffset, segments);
		}

		public static void Bounds(Bounds bounds, Matrix4x4 matrix, LineMode mode) => Bounds(bounds, matrix, mode, GetLineColor(mode));
		public static void Bounds (Bounds bounds, Matrix4x4 matrix, LineMode mode, Color color)
		{
			var leftTopFront		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 (-bounds.extents.x,  bounds.extents.y,  bounds.extents.z));
			var rightTopFront		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 ( bounds.extents.x,  bounds.extents.y,  bounds.extents.z));
			var leftBottomFront		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 (-bounds.extents.x, -bounds.extents.y,  bounds.extents.z));
			var rightBottomFront	= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 ( bounds.extents.x, -bounds.extents.y,  bounds.extents.z));
			var leftTopBack			= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 (-bounds.extents.x,  bounds.extents.y, -bounds.extents.z));
			var rightTopBack		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 ( bounds.extents.x,  bounds.extents.y, -bounds.extents.z));
			var leftBottomBack		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 (-bounds.extents.x, -bounds.extents.y, -bounds.extents.z));
			var rightBottomBack		= matrix.MultiplyPoint3x4 (bounds.center + new Vector3 ( bounds.extents.x, -bounds.extents.y, -bounds.extents.z));

			// Front
			Line (leftTopFront,		rightTopFront,		mode, color);
			Line (rightTopFront,	rightBottomFront,	mode, color);
			Line (rightBottomFront, leftBottomFront,	mode, color);
			Line (leftBottomFront,	leftTopFront,		mode, color);
			// Back
			Line (leftTopBack,		rightTopBack,		mode, color);
			Line (rightTopBack,		rightBottomBack,	mode, color);
			Line (rightBottomBack,	leftBottomBack,		mode, color);
			Line (leftBottomBack,	leftTopBack,		mode, color);
			// Side
			Line (leftTopBack,		leftTopFront,		mode, color);
			Line (rightTopBack,		rightTopFront,		mode, color);
			Line (rightBottomBack,	rightBottomFront,	mode, color);
			Line (leftBottomBack,	leftBottomFront,	mode, color);
		}

		public static void TransformToolHandle (Transform target, float scale = 1f)
		{
			var matrix = Matrix4x4.Scale (Vector3.one * scale);

			var newPosition = matrix.inverse.MultiplyPoint3x4 (target.position);
			var newRotation = target.rotation;
			var newScale = target.localScale;

			using (new Handles.DrawingScope (matrix))
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				switch (Tools.current)
				{
					default:
					case Tool.Move:
						if (Tools.pivotRotation == PivotRotation.Local)
							newPosition = Handles.PositionHandle (matrix.inverse.MultiplyPoint3x4 (target.position), target.rotation);
						else
							newPosition = Handles.PositionHandle (matrix.inverse.MultiplyPoint3x4 (target.position), Quaternion.identity);
						break;
					case Tool.Rotate:
						newRotation = Handles.RotationHandle(target.rotation, matrix.inverse.MultiplyPoint3x4 (target.position));
						break;
					case Tool.Scale:
						newScale = Handles.ScaleHandle (target.localScale, matrix.inverse.MultiplyPoint3x4 (target.position), target.rotation, HandleUtility.GetHandleSize (target.position));
						break;
				}
				if (check.changed)
				{
					Undo.RecordObject (target, "Changed Transform");
					target.SetPositionAndRotation (matrix.MultiplyPoint3x4 (newPosition), newRotation);
					target.localScale = newScale;
				}
			}
		}
	}
}