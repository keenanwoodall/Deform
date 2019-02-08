using System.Linq;
using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (Deformable)), CanEditMultipleObjects]
	public class DeformableEditor : Editor
	{
		private class Styles
		{
			public static readonly GUIStyle ButtonLeftStyle, ButtonMidStyle, ButtonRightStyle;

			static Styles ()
			{
				ButtonLeftStyle = new GUIStyle (EditorStyles.miniButtonLeft);
				ButtonMidStyle = new GUIStyle (EditorStyles.miniButtonMid);
				ButtonRightStyle = new GUIStyle (EditorStyles.miniButtonRight);

				var leftMargin = ButtonLeftStyle.margin;
				var midMargin = ButtonMidStyle.margin;
				var rightMargin = ButtonRightStyle.margin;

				midMargin.left--;
				rightMargin.left--;
				midMargin.right--;
				leftMargin.right--;
			}
		}

		private class Content
		{
			public static readonly GUIContent UpdateMode = new GUIContent (text: "Update Mode", tooltip: "Auto: Gets updated by a manager.\nPause: Never updated or reset.\nStop: Mesh is reverted to it's undeformed state until mode is switched.\nCustom: Allows updates, but not from a Deformable Manager.");
			public static readonly GUIContent NormalsRecalculation = new GUIContent (text: "Normals Recalculation", tooltip: "Auto: Normals are auto calculated after the mesh is deformed; overwriting any changes made by deformers.\nNone: Normals aren't modified by the Deformable.");
			public static readonly GUIContent BoundsRecalculation = new GUIContent (text: "Bounds Recalculation", tooltip: "Auto: Bounds are recalculated for any deformers that need it, and at the end after all the deformers finish.\nNever: Bounds are never recalculated.\nOnce At The End: Deformers that needs updated bounds are ignored and bounds are only recalculated at the end.");
			public static readonly GUIContent ColliderRecalculation = new GUIContent (text: "Collider Recalculation", tooltip: "Auto: Collider's mesh is updated when the rendered mesh is updated.\nNone: Collider's mesh isn't updated.");
			public static readonly GUIContent Manager = new GUIContent (text: "Manager", tooltip: "The manager that will update this deformable. If none is assigned a default one will be created at Start.");
			public static readonly GUIContent ClearDeformers = new GUIContent (text: "Clear", tooltip: "Remove all deformers from the deformer list.");
			public static readonly GUIContent CleanDeformers = new GUIContent (text: "Clean", tooltip: "Remove all null deformers from the deformer list.");
			public static readonly GUIContent SaveObj = new GUIContent (text: "Save Obj", tooltip: "Save the current mesh as a .obj file in the project. (Doesn't support vertex colors)");
			public static readonly GUIContent SaveAsset = new GUIContent (text: "Save Asset", tooltip: "Save the current mesh as a mesh asset file in the project.");
		}

		private class Properties
		{
			public SerializedProperty UpdateMode;
			public SerializedProperty NormalsRecalculation;
			public SerializedProperty BoundsRecalculation;
			public SerializedProperty ColliderRecalculation;
			public SerializedProperty MeshCollider;
			public SerializedProperty Manager;

			public Properties (SerializedObject obj)
			{
				UpdateMode				= obj.FindProperty ("updateMode");
				NormalsRecalculation	= obj.FindProperty ("normalsRecalculation");
				BoundsRecalculation		= obj.FindProperty ("boundsRecalculation");
				ColliderRecalculation	= obj.FindProperty ("colliderRecalculation");
				MeshCollider			= obj.FindProperty ("meshCollider");
				Manager					= obj.FindProperty ("manager");
			}
		}

		private bool ShowDebug;

		private Properties properties;

		private ReorderableComponentElementList<Deformer> deformerList;

		private void OnEnable ()
		{
			properties = new Properties (serializedObject);

			deformerList = new ReorderableComponentElementList<Deformer> (serializedObject, serializedObject.FindProperty ("deformerElements"));
		}

		private void OnDisable ()
		{
			deformerList.Dispose ();
		}

		private Rect rect;

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.UpdateIfRequiredOrScript ();
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.UpdateMode, Content.UpdateMode);
				if (check.changed)
				{
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((Deformable)t).UpdateMode = (UpdateMode)properties.UpdateMode.enumValueIndex;
				}
			}

			EditorGUILayout.PropertyField (properties.NormalsRecalculation, Content.NormalsRecalculation);
			EditorGUILayout.PropertyField (properties.BoundsRecalculation, Content.BoundsRecalculation);

			using (new EditorGUI.DisabledScope (targets.Any (t => ((Deformable)t).MeshCollider == null)))
				EditorGUILayout.PropertyField (properties.ColliderRecalculation, Content.ColliderRecalculation);

			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.Manager, Content.Manager);
				if (check.changed)
				{
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((Deformable)t).Manager = (DeformableManager)properties.Manager.objectReferenceValue;
				}
			}

			deformerList.DoLayoutList ();

			EditorGUILayout.Space ();

			var newDeformers = EditorGUILayoutx.DragAndDropComponentArea<Deformer> ();
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
				if (GUILayout.Button (Content.ClearDeformers, Styles.ButtonLeftStyle))
				{
					Undo.RecordObjects (targets, "Cleared Deformers");
					foreach (var t in targets)
						((Deformable)t).DeformerElements.Clear ();
				}
				if (GUILayout.Button (Content.CleanDeformers, Styles.ButtonMidStyle))
				{
					Undo.RecordObjects (targets, "Cleaned Deformers");
					foreach (var t in targets)
						((Deformable)t).DeformerElements.RemoveAll (d => d.Component == null);
				}
				if (GUILayout.Button (Content.SaveObj, Styles.ButtonMidStyle))
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
				if (GUILayout.Button (Content.SaveAsset, Styles.ButtonRightStyle))
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

			using (var foldout = new EditorGUILayoutx.FoldoutWideScope (ref ShowDebug, "Debug Info"))
			{
				if (foldout.isOpen)
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
