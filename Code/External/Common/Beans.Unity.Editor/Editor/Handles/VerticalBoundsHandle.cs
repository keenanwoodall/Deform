using UnityEngine;
using UnityEditor;

namespace Beans.Unity.Editor
{
	public class VerticalBoundsHandle
	{
		public delegate void LineMethod (Vector3 a, Vector3 b);

		public float top;
		public float bottom;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
		public Vector3 direction;
		public float snap;
		public float screenspaceHandleSize = 1f;
		public Color handleColor;
		public Handles.CapFunction handleCap = Handles.CircleHandleCap;
		public LineMethod guideLine;

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

				guideLine?.Invoke (topPosition, bottomPosition);

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var bottomSize = HandleUtility.GetHandleSize (handleSpace.inverse.MultiplyPoint3x4 (bottomPosition)) * screenspaceHandleSize;
					var newBottomPosition = Handles.Slider (bottomPosition, direction, bottomSize, handleCap, snap);
					if (check.changed)
					{
						bottom = Vector3.Dot (direction, newBottomPosition);
						return true;
					}
				}

				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var topSize = HandleUtility.GetHandleSize (handleSpace.inverse.MultiplyPoint3x4 (topPosition)) * screenspaceHandleSize;
					var newTopPosition = Handles.Slider (topPosition, direction, topSize, handleCap, snap);
					if (check.changed)
					{
						top = Vector3.Dot (direction, newTopPosition);
						return true;
					}
				}

				return false;
			}
		}

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

		public bool DrawHandle (float top, float bottom, Transform axis, Vector3 direction)
		{
			return DrawHandle (top, bottom, axis.position, axis.rotation, axis.localScale, direction);
		}
	}
}