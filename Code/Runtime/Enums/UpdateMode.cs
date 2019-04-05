namespace Deform
{
	/// <summary>
	/// Holds different update modes.
	/// Auto: Gets updated by a manager.
	/// Pause: Never updated or reset.
	/// Stop: Mesh is reverted to it's undeformed state until mode is switched.
	/// Custom: Allows updates, but not from a Deformable Manager.
	/// </summary>
	public enum UpdateMode
	{
		Auto,
		Pause,
		Stop,
		Custom
	}
}