using System.Linq;
using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (Deformable)), CanEditMultipleObjects]
	public class DeformableEditor : Editor
	{
		private class Content
		{
			public GUIContent UpdateMode, NormalsRecalculation, BoundsRecalculation, ColliderRecalculation, Manager, ClearDeformers, CleanDeformers, SaveObj, SaveAsset;

			public void Update ()
			{
				UpdateMode = new GUIContent
				(
					text: "Update Mode",
					tooltip: "Auto: Gets updated by a manager.\nPause: Never updated or reset.\nStop: Mesh is reverted to it's undeformed state until mode is switched.\nCustom: Allows updates, but not from a Deformable Manager."
				);
				NormalsRecalculation = new GUIContent
				(
					text: "Normals Recalculation",
					tooltip: "Auto: Normals are auto calculated after the mesh is deformed; overwriting any changes made by deformers.\nNone: Normals aren't modified by the Deformable."
				);
				BoundsRecalculation = new GUIContent
				(
					text: "Bounds Recalculation",
					tooltip: "Auto: Bounds are recalculated for any deformers that need it, and at the end after all the deformers finish.\nNever: Bounds are never recalculated.\nOnce At The End: Deformers that needs updated bounds are ignored and bounds are only recalculated at the end."
				);
				ColliderRecalculation = new GUIContent
				(
					text: "Collider Recalculation",
					tooltip: "Auto: Collider's mesh is updated when the rendered mesh is updated.\nNone: Collider's mesh isn't updated."
				);
				Manager = new GUIContent
				(
					text: "Manager",
					tooltip: "The manager that will update this deformable. If none is assigned a default one will be created at Start."
				);
				ClearDeformers = new GUIContent
				(
					text: "Clear",
					tooltip: "Remove all deformers from the deformer list."
				);
				CleanDeformers = new GUIContent
				(
					text: "Clean",
					tooltip: "Remove all null deformers from the deformer list."
				);
				SaveObj = new GUIContent
				(
					text: "Save Obj",
					tooltip: "Save the current mesh as a .obj file in the project. (Doesn't support vertex colors)"
				);
				SaveAsset = new GUIContent
				(
					text: "Save Asset",
					tooltip: "Save the current mesh as a mesh asset file in the project."
				);
			}
		}

		private class Properties
		{
			public SerializedProperty UpdateMode, NormalsRecalculation, BoundsRecalculation, ColliderRecalculation, MeshCollider, Manager;

			public void Update (SerializedObject obj)
			{
				UpdateMode				= obj.FindProperty ("updateMode");
				NormalsRecalculation	= obj.FindProperty ("normalsRecalculation");
				BoundsRecalculation		= obj.FindProperty ("boundsRecalculation");
				ColliderRecalculation	= obj.FindProperty ("colliderRecalculation");
				MeshCollider			= obj.FindProperty ("meshCollider");
				Manager					= obj.FindProperty ("manager");
			}
		}

		[SerializeField]
		private static bool ShowDebug;

		private Content content = new Content ();
		private Properties properties = new Properties ();

		private DeformerListEditor deformerList;

		private void OnEnable ()
		{
			content.Update ();
			properties.Update (serializedObject);

			deformerList = new DeformerListEditor (serializedObject, serializedObject.FindProperty ("deformerElements"));
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.UpdateMode, content.UpdateMode);
				if (check.changed)
					foreach (var t in targets)
						((Deformable)t).UpdateMode = (UpdateMode)properties.UpdateMode.enumValueIndex;
			}

			EditorGUILayout.PropertyField (properties.NormalsRecalculation, content.NormalsRecalculation);
			EditorGUILayout.PropertyField (properties.BoundsRecalculation, content.BoundsRecalculation);

			using (new EditorGUI.DisabledScope (targets.Any (t => ((Deformable)t).MeshCollider == null)))
				EditorGUILayout.PropertyField (properties.ColliderRecalculation, content.ColliderRecalculation);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.Manager, content.Manager);
				if (check.changed)
					foreach (var t in targets)
						((Deformable)t).Manager = (DeformableManager)properties.Manager.objectReferenceValue;
			}

			deformerList.DoLayoutList ();
			serializedObject.ApplyModifiedProperties ();

			EditorGUILayout.Space ();

			var newDeformers = DeformEditorGUILayout.DragAndDropComponentArea<Deformer> ();
			if (newDeformers != null && newDeformers.Count > 0)
			{
				Undo.RecordObjects (targets, "Added Deformers");
				foreach (var t in targets)
				{
					var elements = ((Deformable)t).DeformerElements;
					foreach (var newDeformer in newDeformers)
						elements.Add (new DeformerElement (newDeformer));
				}

				// I'd like to give a massive thanks and credit to @vertx aka Thomas Ingram for taking time out of his day to
				// solve an abomination of a bug and find this fix. He truly is an editor scripting legend.
				/*
                                       
                                                                              -/``                  ``                     ``.` `.      `                                                               
                                                                          `   `-`    ``````         .-                ```.. ``         ``                                                               
                                                                              `    .----::-.``` `````: `               ``     `      `.`    .                                                           
                                                                                 `-sso+++o+/:.-````..--.`   `    `                   ``     `` ``  ``                                                   
                                                                               ./oossssssss++::---::...--.`  `..``   `````.                  `.-`` ``                                                   
                                                        ``              `  `.--+++o+syyyyysso+/:::/+/::/-::-` `:-....`..--.    `             `` ``     `                                                
                                                        ..``````````  ``.:-:+ooyyysssyyyyyysoo+osoo/+oo:::://:.`-:-::.-.-/:-..:-:.`         :+:` `                                                      
                                                        .-.``````````..--///+sssyhyysyssoo++++oyyhysoyys+/::/+/:-:.::-.````---..``.`        :yo:`                                                       
                                                        .--.````````...--/+/+oossysoooo++/+/oosossyhmmdhyss+////:/:....```       ..```` `-``odho-``                                                     
                                                       `.---...`````...--:/+++oooooooso+//+/++oyydmmmNNNmhdhysso+//:----/oso/-..  `.... `.-hmNNdo.``     `                                              
                                                      ``----......`...--:://+/++ossosssssyyhhdddmmmmmmNNmmmmNNNmmmmmmNNNNMMMNmhhsoshhhyhddmNNmNd+.``                                                    
                                                     ```.----.........--/:://:/+osossyhhhhhddddmmmmmNNNNNmNNNNNNMNMMMMMMMMMMMMMMMNMMMNNNNNNNNmmd-`                                                      
                                                   ``..`..---.........--::-:::/+ooossshhhhddhdmmmmmmNNmNNNNNNNNNMMMMMMMMMMMMMMMMMMMMMNNNNNNNNmmy`                                                       
                                                   .--.....--.-.......------:-:/++ossyhhhddddmmmmmmmmmmmNNmNNNNNMMMMMMMMMMMMMMMMMMMMMNMMNNMNNNms`                                                       
                                                  `.---..`..-.-...........---:-:++osssyhdddmmdmmmmmmmmmmmNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMNNNMNNmds.                                                       
                                                  ..--:-..`.-......`........-:-///+++osydddmmmmmmmmmmmmmNNNNNMMMMMMMMMMMMMMMMMMMMMMNNNNNNNNNNmdh-                                                       
                                                  .-::--.........````````...--:::/++ooyyhddmmmmNNNmmmmmNNNNMMMMMMMMMMMMMMNNNNNmNNNNMNNNNNNNNNmdm:``                                                     
                                                  ---:---.......```````````.---:/+ooosshhhdmmmmNNNNNNNNNNNMNNNMMNNNNNNNNmmmddmdhhhdmmmmNNNNNNmmmy`     -o                                               
                                                 .---:--....`..````       ``..-//oossssyhhhmmmmNmNmNNNNNNNNNNNNmhdysssssso+oo+ysyo+o+oohmNNNNmmmm-`   .md                                               
                                                 .------..```````          ```.-:/++ossyyhhdmmmmmmmmNNNNNmdhyhdys+-````.---::/++++//::--hmmNNNNmm/    `m+                                               
                                                 ....-....`````                ``..--:/+oyyhddmdddmmmNmddddhsoyy+/:---:+oosyyyyhyysyhhs:/dNNNNNmmy`    y/                                               
                                                `.........```                       ```.-:+oyyysyyhdmmNNmdhyhhdhyyyyyyyhhhddddmmmmNNNmmhydmNNNNNNd.   `m+                                               
                                                `.........``         `...-:::-.`        `.-:/+//++sdmNNNNNmdhdhys+:-..--/+syysssyhdmmmNmmmmNNNNNmm:   ym+                                               
                                                ``.....`.```     ```..-://///::--.`       ``...-:osdNNMMNNNmdy/.````     . `::/ososydmNmNNNNNNNNmd:  .NN:                                               
                                        ``     ``........```````````````        `-.``          `.:shmNNMNNmmmh:.../-`   `o`.Ndo..-ohdmNNNMMMNNNNmd+  +NM-                                               
                                       `-`      `........````````      `       s-od/`          ``-ohdNNNNNNMNyo-.-//.`   `:hmdo++oyhdmNNNMMMMNNNNm+  hMM/                                               
                                       .`       `.......`````        `--.`     .`hms.          ``.oymNNNNNNMNmdh+-::::://ooyhyhhdmNNNMMMMMMMMNNNmm/  mNM+                                               
                                       `        ........`````     ``.` `````  `-syo-.          ..-+ydmNNNNNNNNmhy+++sssyhdmmNNNNNNNNMMMMMMMMMMNNmm: `dNM/                                               
                                       `        ........`..```   ``...````````.--::.`          ...+shmNNNNNNNNNmmmhyyyhdmmNNNNNNMMNNNMMMMMMMNNNNmd: +mNN`                                               
                                       ``       ....-....``..````````.`.`.---::--.`           ``../oymNNNNNNNNNNNNmNNNNNNNMMMMMMMMMMMMMMMMMMMNNNmm-oNMm-                                                
                                        ``      `.....-.......```````````..--..``           ``````:sdmNNNNNNNNNNMMMMNNNNNNNMMMMMMMMMMMMMMMMNNNNmdd+NMh                                                  
                                         ```     ........`.....`....-------..`````       `````````:shmmmNmNNNNNMMMMMNMNNNNMNMMMMMMMMMMMMMMMNNNNmsd/NN.                                                  
                                           `.    ........``........------.....```        `````   `:oddmmmmmNNNNMMMMMMMMNNMNNMMMMMMMMMMMMMMMNNNNmdh+Nd                                                   
                                            .`  `.....--.`...`.....--------...``         ````   `.:shdmmddmNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMNNNmdddoMo                                                   
                                            ...``......-.............------....`         `` `   `-+ohdmmmmddmmNNNMMMMMMMMMMMMMMMMMMMMMMMMNNNNNmdh+yy                                                    
                                            `... ``........`......``....----..`                `.:+shdmdmmmmdhmNMMMMMMMMMMMMMMMMMMMMMMMMMMNNNmddy/:                                                     
                                             ...  -........````````....----..``             ` ``-/shmmmNNNNmmdhdNMMMMMMMMMMMMMMMMMMMMMMMMNNNNmdhy.                                                      
                                              ``  ```.....``..```......----..``           ``..../oydmNNNMMNNNNmddNNNNNNMMMMMMMMMMMMMMMMMMNNmmmdh+.                                                      
                                                   .`-.....``.``........-----.`       ```.`````./syddmNNNNmmmmmmdmNNNNNNNMMMMMMMMMMMMMMNNNNdmmh+:                                                       
                                                 . ``......`..`........-:::::--.`     ``      ``-+syyhdmddy+:.-odNNNNNNNMMMMMMMMMMMMMMNNNNmdhdh/`                                                       
                                                 ````..``.``.`........--::/+++/:.`              `.-:/oyhmmmdhosdNMMNNNMNMMMMMMMMMMMMMMNNNNmddhy/`                                                       
                                                  ``````````.`......--:://ossso/-.           ```.---:+ydmNmmmNNNMMMMNNMMMMMMMMMMMMMMMNNNNmdddso-                                                        
                                                  ```````.````.....--://+syyyo+-.`          ``:-ooyo:+yhhdhhdmNNNNMMMNMMMMMMMMMMMMMNNNMNNhhhh+/`                                                        
                                                   ````````````....--:/+ossso/:.`          ` `.-:+o-`-+oosoo+yddmNNNMMMMMMMMMMMMMMMMNdNNNhhhy:-                                                         
                                                    ```````````.``..-/+oooo/:.`              `.-/s:``-+/++::.-://oyymNNNMMMMMMMMMMMNNNNNmddho:`                                                         
                                                     ````````````...-/++++/-`           `  `.`-++o:` .+osso+/://:o::/shmNMMMMMMMMNMNNmNNddyh/:`                                                         
                                                      `  ``.``````..-:++/:-       ``` `````..-osss/.:+hdmddddydhhhso/++ymNMMMMMMMNMNNhmmdhys:``                                                         
                                                      ` `````.``.``..:///:`   ``` ``.`````..`-:osyoohdmddddhhhddmdddyo/odmNMMMNNNNNmmyhhhyso`                                                           
                                                        ``````` ``...-:::/.  `.`` `            `..::++/:/:/++++//+o//+sydNNNNNmNmmmmdyodyy+/                                                            
                                                     `   ` `.`````.`..-::/- `.``          .....--...::/oyhhyhsshdNNmsoshdNNNNNNmddhdhyoyy+:                                                             
                                                     .     ``````````.----```...    ```````.-/:+o/oossyddhsyhdmNNMMNdhhhdmNNNNmmdshsysyo+.`                                                             
                                                     -``   ` `   ``.``.`````.-..`     `....--://+ssyyyhdddmNNNMMMMNNmmdhdNNNNmmNdyysyo//`.                                                              
                                                    `--`           ````````.-:-.`` ``  .-/++o++//+oyyydmNNNNNMMMMMNNNddhdmmNNNmmdosooo/-``                                                              
                                                    `---`         ``     ```-..````  `..:/+yosyy+/+hshhddmNNNNMNNNmdddddyydmmmmdys+oyo--                                                                
                                                    `---.         `       ``` `...````..:+:+s+oo-:/soshdmNNNNNMNmhoo/ssh//yddyyhoo/y+```                                                                
                                                    `---.``                   .`.---..-:+/:+/+::.-/.-/oydmmNNNNdho/::/+/:+os+s+y+oo+.. `/`                                                              
                                                     ---...`     `   `          ``...---::-:/::-..`-:-+osyyhddhhssyso/oo:ohy/+ssyo/`-.  ./::```                                                         
                                                     .-....```                ``` ``````.:/s+ss+ss+o+oshyhsydyssyosyho:/:/+oooooo/`/d:   :-..:-..`  ``                                                  
                                                      .....````                  `    ``..../+yyyshyyy+oyhysyyhyoy/ohso://:.-/:-..sdN.    o:-+.`...--.::..`                                             
                                                       ...`````                     ```````--:syyyyoo+oysdyosyyooso+:o:/s.-.`.:`omNmd     `+`..:o:-//-/:.-..:-  `                                       
                                                        ````````                        `.``./ooso/-/+o/+o:::sy++ss/.---.`` .`-+mNNNy   `  :-`-o-.-/::+--/..+:.:-..`                                    
                                                           ```                         ``` ..:/-/os----../-:-:-:-/+:...` ```:/mNNNNN+    . `+  .::o/-/:-++-:-.:/..--`--`..`                             
                                                                                          ...-.:-//-`-.``.-.-.``...` ``` ``+hmNMNNNN.    `` /-  /+ -h/-/s--:/:/:`-/.-:.`-.`..````                       
              `                                                                           `` ` -`-.:`````-````  ```.  ``.:hNMNMMMNNm     `. `o  `o.`h:..`-o/-/-.-o/`:.`./-.::`..``.```                  
   `.``                                                                                        ``` -`    `         ```-+hNNMMMMMMNNs      -. +-  :- +o `-s``/o-/+...-`:/.-::..:-..-``.```               
				*/
				// Changing fields directly with multiple objects selected doesn't dirty the serialized object.
				// To force it to be dirty you have to call this method.
				serializedObject.SetIsDifferentCacheDirty ();
				serializedObject.Update ();
			}

			EditorGUILayout.Space ();

			using (new EditorGUILayout.HorizontalScope ())
			{
				if (GUILayout.Button (content.ClearDeformers, EditorStyles.miniButtonLeft))
				{
					Undo.RecordObjects (targets, "Cleared Deformers");
					foreach (var t in targets)
						((Deformable)t).DeformerElements.Clear ();
				}
				if (GUILayout.Button (content.CleanDeformers, EditorStyles.miniButtonMid))
				{
					Undo.RecordObjects (targets, "Cleaned Deformers");
					foreach (var t in targets)
						((Deformable)t).DeformerElements.RemoveAll (d => d.Deformer == null);
				}
				if (GUILayout.Button (content.SaveObj, EditorStyles.miniButtonMid))
				{
					foreach (var t in targets)
					{
						var deformable = t as Deformable;

						// C:/...Deform/Assets/
						var projectPath = Application.dataPath + "/";
						// We have to generate the full asset path starting from the Assets folder for GeneratorUniqueAssetPath to work,
						var assetPath = AssetDatabase.GenerateUniqueAssetPath ($"Assets/{deformable.name}.obj");
						// Now that we have a unique asset path we can remove the "Assets/" and ".obj" to get the unique name.
						var fileName = assetPath;
						// It's pretty gross, but it works and this code doesn't need to be performant.
						fileName = fileName.Remove (0, 7);
						fileName = fileName.Remove (fileName.Length - 4, 4);

						ObjExporter.SaveMesh (deformable.GetMesh (), deformable.GetRenderer (), projectPath, fileName);
						AssetDatabase.Refresh (ImportAssetOptions.ForceSynchronousImport);
					}
				}
				if (GUILayout.Button (content.SaveAsset, EditorStyles.miniButtonRight))
				{
					foreach (var t in targets)
					{
						var deformable = t as Deformable;

						var assetPath = AssetDatabase.GenerateUniqueAssetPath ($"Assets/{deformable.name}.asset");
						// Now that we have a unique asset path we can remove the "Assets/" and ".obj" to get the unique name.
						var fileName = assetPath;
						// It's pretty gross, but it works and this code doesn't need to be performant.
						fileName = fileName.Remove (0, 7);
						fileName = fileName.Remove (fileName.Length - 4, 4);

						AssetDatabase.CreateAsset (Instantiate (deformable.GetMesh ()), assetPath);
						AssetDatabase.SaveAssets ();
					}
				}
			}

			EditorGUILayout.Space ();

			if (ShowDebug = EditorGUILayout.Foldout (ShowDebug, "Debug Info"))
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					var vertexCount = 0;
					var modifiedData = DataFlags.None;
					foreach (var t in targets)
					{
						var deformable = t as Deformable;
						var mesh = deformable.GetMesh ();

						if (mesh != null)
							vertexCount += deformable.GetMesh ().vertexCount;
						modifiedData |= deformable.ModifiedDataFlags;
					}
					EditorGUILayout.LabelField ($"Vertex Count: {vertexCount}");
					EditorGUILayout.LabelField ($"Modified Data: {modifiedData.ToString ()}");
				}
			}

			serializedObject.ApplyModifiedProperties ();
			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (ShowDebug)
			{
				var deformable = target as Deformable;

				DeformHandles.Bounds (deformable.GetMesh ().bounds, deformable.transform.localToWorldMatrix, DeformHandles.LineMode.LightDotted);
			}
		}
	}
}