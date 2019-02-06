using UnityEngine;

namespace Deform
{
	public interface IComponentElement<T> where T : Component
	{
		T Component { get; set; }
		bool Active { get; set; }
	}
}