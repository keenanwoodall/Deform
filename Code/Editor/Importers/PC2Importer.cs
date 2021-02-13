using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;
using Deform;

namespace DeformEditor
{
	[ScriptedImporter(version: Version, ext: Extension)]
	public class PC2Importer : ScriptedImporter
	{
		public const int Version = 0;
		public const string Extension = "pc2";
		public const string PointCacheID = "PointCache";
		
		public override void OnImportAsset(AssetImportContext ctx)
		{
			PointCache pointCache = ScriptableObject.CreateInstance<PointCache>();
			
			using (FileStream fs = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read))
			{
				using (BinaryReader br = new BinaryReader(fs))
				{
					string signature = new string(br.ReadChars(12));
					int fileVersion = br.ReadInt32();
					int perFramePointCount = br.ReadInt32();
					float startFrame = br.ReadSingle();
					float frameRate = br.ReadSingle();
					int frameCount = br.ReadInt32();

					Vector3[] points = new Vector3[frameCount * perFramePointCount];

					for (int i = 0; i < perFramePointCount; i++)
					{
						points[i] = new Vector3
						(
							br.ReadSingle(),
							br.ReadSingle(),
							br.ReadSingle()
						);
					}
					
					pointCache.Initialize(signature, fileVersion, startFrame, frameRate, frameCount, perFramePointCount, points);
				}
			}
			
			ctx.AddObjectToAsset(PointCacheID, pointCache);
			ctx.SetMainObject(pointCache);
		}
	}
}