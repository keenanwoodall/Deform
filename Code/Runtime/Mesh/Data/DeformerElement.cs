using UnityEngine;

namespace Deform
{
	/// <summary>
	/// Contains a reference to a deformer, and a bool, Active, that determines if the deformer should be allowed to process data.
	/// </summary>
	[System.Serializable]
	public class DeformerElement : IComponentElement<Deformer>
	{
		public Deformer Component { get => component; set => component = value; }
		public bool Active { get => active; set => active = value; }

		[SerializeField] private Deformer component;
		[SerializeField] private bool active = true;

		public DeformerElement () : this (null, true) { }
		public DeformerElement (Deformer component, bool active = true)
		{
			this.component = component;
			this.active = active;
		}

		public bool CanProcess ()
		{
			return Active && Component != null && Component.CanProcess ();
		}
	}
}