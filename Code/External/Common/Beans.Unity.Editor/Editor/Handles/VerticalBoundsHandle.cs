using UnityEngine;
using UnityEditor;

namespace Beans.Unity.Editor
{
	public class VerticalBoundsHandle
	{
		public delegate void LineMethod (Vector3 a, Vector3 b);

		/// <summary>
		/// The top value relative to the position.
		/// </summary>
		public float Top { get => top; set => top = value; }
		/// <summary>
		/// The bottom value relative to the position.
		/// </summary>
		public float Bottom { get => bottom; set => bottom = value; }
		/// <summary>
		/// The origin of the handle.
		/// </summary>
		public Vector3 Position { get => position; set => position = value; }
		/// <summary>
		/// The rotation of the handle.
		/// </summary>
		public Quaternion Rotation { get => rotation; set => rotation = value; }
		/// <summary>
		/// The scale of the handle.
		/// </summary>
		public Vector3 Scale { get => scale; set => scale = value; }
		/// <summary>
		/// The direction of the handle, relative to its rotation.
		/// </summary>
		public Vector3 Direction { get => direction; set => direction = value; }
		/// <summary>
		/// The granularity of control.
		/// </summary>
		public float Snap { get => snap; set => snap = value; }
		/// <summary>
		/// How large should the handles in screenspace.
		/// </summary>
		public float ScreenspaceHandleSize { get => screenspaceHandleSize; set => screenspaceHandleSize = value; }
		/// <summary>
		/// The color of the handles.
		/// </summary>
		public Color HandleColor { get => handleColor; set => handleColor = value; }
		/// <summary>
		/// The delegate for the handle caps.
		/// </summary>
		public Handles.CapFunction HandleCapFunction { get => handleCapFunction; set => handleCapFunction = value; }
		/// <summary>
		/// The delegate for drawing a line between the handles.
		/// </summary>
		public LineMethod DrawGuidelineCallback { get => drawGuidelineCallback; set => drawGuidelineCallback = value; }

		private float top;
		private float bottom;
		private Vector3 position;
		private Quaternion rotation;
		private Vector3 scale;
		private Vector3 direction;
		private float snap;
		private float screenspaceHandleSize = 1f;
		private Color handleColor;
		private Handles.CapFunction handleCapFunction = Handles.CircleHandleCap;
		private LineMethod drawGuidelineCallback;

		/// <summary>
		/// Draws the handles.
		/// </summary>
		/// <returns>Returns true if the top or bottom was changed.</returns>
		public bool DrawHandle ()
		{
			var direction = this.Direction.normalized;

			var handleSpace = Matrix4x4.TRS (Position, Rotation, Scale);
			using (new Handles.DrawingScope (HandleColor, handleSpace))
			{
				var topPosition = direction * Top;
				var bottomPosition = direction * Bottom;

				DrawGuidelineCallback?.Invoke (topPosition, bottomPosition);

				var holdingCtrl = (Event.current.modifiers & EventModifiers.Control) > 0;
				var actualSnap = holdingCtrl ? 0.5f : Snap;

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var bottomSize = HandleUtility.GetHandleSize (bottomPosition) * ScreenspaceHandleSize;
					var newBottomPosition = Handles.Slider (bottomPosition, direction, bottomSize, HandleCapFunction, actualSnap);
					if (check.changed)
					{
						Bottom = Vector3.Dot (direction, newBottomPosition);
						Bottom = Mathf.Min (Bottom, Top);

						return true;
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var topSize = HandleUtility.GetHandleSize (topPosition) * ScreenspaceHandleSize;
					var newTopPosition = Handles.Slider (topPosition, direction, topSize, HandleCapFunction, actualSnap);
					if (check.changed)
					{
						Top = Vector3.Dot (direction, newTopPosition);
						Top = Mathf.Max (Top, Bottom);

						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Draws the handles.
		/// </summary>
		/// <returns>Returns true if the top or bottom was changed.</returns>
		public bool DrawHandle (float top, float bottom, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 direction)
		{
			this.Top = top;
			this.Bottom = bottom;
			this.Position = position;
			this.Rotation = rotation;
			this.Scale = scale;
			this.Direction = direction;

			return DrawHandle ();
		}

		/// <summary>
		/// Draws the handles.
		/// </summary>
		/// <returns>Returns true if the top or bottom was changed.</returns>
		public bool DrawHandle (float top, float bottom, Transform axis, Vector3 direction)
		{
			return DrawHandle (top, bottom, axis.position, axis.rotation, axis.lossyScale, direction);
		}
	}
}