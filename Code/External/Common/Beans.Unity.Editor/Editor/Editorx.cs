using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Beans.Unity.Editor
{
	using Editor = UnityEditor.Editor;
	
	public static class Editorx
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="editor">The editor whose serialized properties will be automatically assigned</param>
		/// <param name="logWarningForUnfoundProperties">Should a warning be logged if a serialized property is not found?</param>
		public static void FindSerializedProperties(this Editor editor, bool logWarningForUnfoundProperties = true)
		{
			FieldInfo[] fields = editor.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic |
			                                                BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(SerializedProperty))
				{
					string propertyName = field.Name;

					if (propertyName.EndsWith("Property"))
						propertyName = propertyName.Remove(propertyName.Length - 8);
					if (propertyName.EndsWith("Prop"))
						propertyName = propertyName.Remove(propertyName.Length - 4);

					SerializedProperty property = editor.serializedObject.FindProperty(propertyName);
					if (logWarningForUnfoundProperties && property == null)
						Debug.LogWarning($"Could not find serialized property for the {editor.GetType().Name}.{propertyName} field.", editor.target);
					else
						field.SetValue(editor, property);
				}
			}
		}
		
		public static string Nicify(this string name)
		{
			return ObjectNames.NicifyVariableName(name);
		}
	}
}