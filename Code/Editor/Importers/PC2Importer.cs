using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;
using Deform;
using NUnit.Framework;

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

			try
			{
				using (FileStream fs = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader br = new BinaryReader(fs))
					{
						string signature = new string(br.ReadChars(12));
						int fileVersion = br.ReadInt32();
						int frameSize = br.ReadInt32();
						float startFrame = br.ReadSingle();
						float frameRate = br.ReadSingle();
						int frameCount = br.ReadInt32();

						var points = new Vector3[frameSize * frameCount];

						for (int i = 0; i < points.Length; i++)
							points[i] = new Vector3
							(
								br.ReadSingle(),
								br.ReadSingle(),
								br.ReadSingle()
							);

						pointCache.Initialize(signature, fileVersion, startFrame, frameRate, frameCount, frameSize, points);
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log(e);
			}

			ctx.AddObjectToAsset(PointCacheID, pointCache);
			ctx.SetMainObject(pointCache);
		}
	}
}