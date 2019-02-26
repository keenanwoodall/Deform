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
		public float top;
		/// <summary>
		/// The bottom value relative to the position.
		/// </summary>
		public float bottom;
		/// <summary>
		/// The origin of the handle.
		/// </summary>
		public Vector3 position;
		/// <summary>
		/// The rotation of the handle.
		/// </summary>
		public Quaternion rotation;
		/// <summary>
		/// The scale of the handle.
		/// </summary>
		public Vector3 scale;
		/// <summary>
		/// The direction of the handle, relative to its rotation.
		/// </summary>
		public Vector3 direction;
		/// <summary>
		/// The granularity of control.
		/// </summary>
		public float snap;
		/// <summary>
		/// How large should the handles in screenspace.
		/// </summary>
		public float screenspaceHandleSize = 1f;
		/// <summary>
		/// The color of the handles.
		/// </summary>
		public Color handleColor;
		/// <summary>
		/// The delegate for the handle caps.
		/// </summary>
		public Handles.CapFunction handleCapFunction = Handles.CircleHandleCap;
		/// <summary>
		/// The delegate for drawing a line between the handles.
		/// </summary>
		public LineMethod drawGuidelineCallback;

		/// <summary>
		/// Draws the handles.
		/// </summary>
		/// <returns>Returns true if the top or bottom was changed.</returns>
		public bool DrawHandle ()
		{
			var direction = this.direction.normalized;

			var handleSpace = Matrix4x4.TRS (position, rotation, scale);
			using (new Handles.DrawingScope (handleColor, handleSpace))
			{
				var topPosition = direction * top;
				var bottomPosition = direction * bottom;

				drawGuidelineCallback?.Invoke (topPosition, bottomPosition);

				var holdingCtrl = (Event.current.modifiers & EventModifiers.Control) > 0;
				var actualSnap = holdingCtrl ? 0.5f : snap;

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var bottomSize = HandleUtility.GetHandleSize (handleSpace.inverse.MultiplyPoint3x4 (bottomPosition)) * screenspaceHandleSize;
					var newBottomPosition = Handles.Slider (bottomPosition, direction, bottomSize, handleCapFunction, actualSnap);
					if (check.changed)
					{
						bottom = Vector3.Dot (direction, newBottomPosition);
						bottom = Mathf.Min (bottom, top);

						return true;
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var topSize = HandleUtility.GetHandleSize (handleSpace.inverse.MultiplyPoint3x4 (topPosition)) * screenspaceHandleSize;
					var newTopPosition = Handles.Slider (topPosition, direction, topSize, handleCapFunction, actualSnap);
					if (check.changed)
					{
						top = Vector3.Dot (direction, newTopPosition);
						top = Mathf.Max (top, bottom);

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
			this.top = top;
			this.bottom = bottom;
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
			this.direction = direction;

			return DrawHandle ();
		}

		/// <summary>
		/// Draws the handles.
		/// </summary>
		/// <returns>Returns true if the top or bottom was changed.</returns>
		public bool DrawHandle (float top, float bottom, Transform axis, Vector3 direction)
		{
			return DrawHandle (top, bottom, axis.position, axis.rotation, axis.localScale, direction);
		}
	}
}