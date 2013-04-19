using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
    
public class MyBuildPostprocessor {
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		string baseFolder = pathToBuiltProject.Substring(0, pathToBuiltProject.Length - 4) + "_Data/";
		
        Debug.Log( "BuiltProject path " + baseFolder );
		
		Debug.Log ("Copying files...");
		
		Directory.CreateDirectory(baseFolder + "Composition/FiniteStateMachines");
		string[] fsms = Directory.GetFiles(Application.dataPath + "/Composition/FiniteStateMachines", "*.tgf");
		foreach (string tgfFile in fsms)
		{
			string fileName = Path.GetFileName(tgfFile);
			File.Copy (tgfFile, baseFolder + "Composition/FiniteStateMachines/" + fileName);
		}
		
		Directory.CreateDirectory(baseFolder + "Composition/PathfindingGraphs");
		string[] graphs = Directory.GetFiles(Application.dataPath + "/Composition/PathfindingGraphs", "*.tgf");
		foreach (string tgfFile in graphs)
		{
			string fileName = Path.GetFileName(tgfFile);
			File.Copy (tgfFile, baseFolder + "Composition/PathfindingGraphs/" + fileName);
		}
		
		Debug.Log ("Copying files... DONE!");
    }
}