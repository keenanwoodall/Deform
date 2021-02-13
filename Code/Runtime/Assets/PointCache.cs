using UnityEngine;

namespace Deform
{
	public class PointCache : ScriptableObject
	{
		[SerializeField] private string signature;
		[SerializeField] private int fileVersion;
		[SerializeField] private float startFrame;
		[SerializeField] private int frameCount;
		[SerializeField] private int framePointCount;
		[SerializeField] private float frameRate;
		[SerializeField] private Vector3[] points;

		public string Signature => signature;
		public int FileVersion => fileVersion;
		public float StartFrame => startFrame;
		public int FrameCount => frameCount;
		public int FramePointCount => framePointCount;
		public float FrameRate => frameRate;
		public Vector3[] Points => points;

		public void Initialize(string signature, int fileVersion, float startFrame, float frameRate, int frameCount, int framePointCount, Vector3[] points)
		{
			this.signature = signature;
			this.fileVersion = fileVersion;
			this.startFrame = startFrame;
			this.frameCount = frameCount;
			this.frameRate = frameRate;
			this.framePointCount = framePointCount;
			this.points = points;
		}
	}
}