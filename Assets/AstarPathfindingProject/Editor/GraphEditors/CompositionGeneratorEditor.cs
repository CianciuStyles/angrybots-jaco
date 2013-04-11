using UnityEngine;
using UnityEditor;
using System.Collections;
using Pathfinding;

[CustomGraphEditor (typeof(CompositionGraph), "Composition Graph")]
public class CompositionGeneratorEditor : GraphEditor {

	//Here goes the GUI
	public override void OnInspectorGUI (NavGraph target) {
		CompositionGraph graph = target as CompositionGraph;

		graph.graphFilename = EditorGUILayout.TextField ("Graph to load: ", graph.graphFilename);
	}
}
