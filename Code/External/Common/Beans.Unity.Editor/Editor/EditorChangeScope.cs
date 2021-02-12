using System;
using UnityEngine;
using UnityEditor;

namespace Beans.Unity.Editor
{
	using Editor = UnityEditor.Editor;
	
	public struct EditorChangeScope : IDisposable
	{
		private readonly Editor editor;
		
		/// <summary>
		/// Creates a scope that updates the serialized object when constructed and applies modified properties when disposed.
		/// </summary>
		public EditorChangeScope(Editor editor)
		{
			this.editor = editor;
			if (editor)
				editor.serializedObject.UpdateIfRequiredOrScript();
		}
		public void Dispose()
		{
			if (editor)
				editor.serializedObject.ApplyModifiedProperties();
			else
				Debug.LogWarning($"{nameof(EditorChangeScope)} created without assigning editor when constructed. This does nothing.");
		}
	}
}