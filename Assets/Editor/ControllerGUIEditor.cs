using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ControllerGUI))]
public class ControllerGUIEditor : Editor {
	/*
	private ControllerGUI target;
	public void Start() {
		target = (ControllerGUI) FindObjectOfType(typeof(ControllerGUI));
	}
	*/
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		//string path = EditorUtility.OpenFilePanel("Locate the XML folder...", "", "");
		//target.CompositionGraphsDirectoryPath = EditorUtility.OpenFolderPanel("Locate the CompositionGraphs folder", @"Assets/Composition", "CompositionGraphs");
		
		if (GUILayout.Button("Generate files...")) {
			ControllerGUI controller = GameObject.Find("ControllerGUI").GetComponent<ControllerGUI>();
			//controller.GenerateFiles();
		}
		
		//GUILayout.Button("Useless button");
	}

}
