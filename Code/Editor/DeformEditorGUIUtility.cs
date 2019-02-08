using UnityEngine;

namespace DeformEditor
{
	public static class DeformEditorGUIUtility
	{
		public static class Strings
		{
			public static readonly string AxisTooltip = "This is the transform that all the calculations will be relative local to. If left blank, the component's transform will be used.\n-\nAll vertices are converted to a space relative to this transform.";
			public static readonly string FactorTooltip = "Strength of the effect.";
			public static readonly string FalloffTooltip = "The sharpness of the effects' transition.";
			public static readonly string TopTooltip = "Any vertices above this will be unaffected.";
			public static readonly string BottomTooltip = "Any vertices below this will be unaffected.";
			public static readonly string SmoothTooltip = "Should the strength of this effect be smoothed near the bounds?";
		}

		public static class DefaultContent
		{
			public static readonly GUIContent Axis = new GUIContent
			(
				text: "Axis",
				tooltip: Strings.AxisTooltip
			);
			public static readonly GUIContent Factor = new GUIContent
			(
				text: "Factor",
				tooltip: Strings.FactorTooltip
			);
			public static readonly GUIContent Falloff = new GUIContent
			(
				text: "Falloff",
				tooltip: Strings.FalloffTooltip
			);
			public static readonly GUIContent Top = new GUIContent
			(
				text: "Top",
				tooltip: Strings.TopTooltip
			);
			public static readonly GUIContent Bottom = new GUIContent
			(
				text: "Bottom",
				tooltip: Strings.BottomTooltip
			);
			public static readonly GUIContent Smooth = new GUIContent
			(
				text: "Smooth",
				tooltip: Strings.SmoothTooltip
			);
		}
	}
}