using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class StartStopGUI : MonoBehaviour {
	
	ControllerGUI controller;
	OptionsGUI opts;
	
	IDictionary<string, GameObject> npcsList = new Dictionary<string, GameObject>();
	GameObject targetBehavior;
	
	RESTfulClient webService;
	
	// Use this for initialization
	void Start () {
		controller = gameObject.GetComponent<ControllerGUI>();
		opts = gameObject.GetComponent<OptionsGUI>();
		
		GameObject npcsGameObject = GameObject.Find("NPCs");
		Transform[] npcs = npcsGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform npc in npcs) {
			try {
				if(npc.gameObject.name.StartsWith("My")) {
					npcsList.Add(npc.gameObject.name, npc.gameObject);
				}
			} catch {}
		}
		
		targetBehavior = GameObject.Find("TargetBehavior");
		webService = null;
	}
	
	// Update is called once per frame
	void Update () {}
	
	void OnGUI ()
	{
		GUILayout.BeginArea( new Rect(Screen.width * 0.4f, Screen.height - 50f, Screen.width * 0.2f, 30f) );
		GUILayout.BeginHorizontal("box");
	
			if ( GUILayout.Button("Start") )
			{
				Debug.Log ("Trying to connect to " + opts.jacoURI);
					
				webService = new RESTfulClient(opts.jacoURI);
				webService.Authorize();
				
				foreach(KeyValuePair<string, GameObject> npc in npcsList) {
					string npcXML = npc.Value.GetComponent<FiniteStateMachine>().Serialize();
					webService.SendNonPlayerCharacter(npcXML);
					Thread.Sleep(500);
				}
				
				string targetXML = targetBehavior.GetComponent<TargetBehavior>().Serialize();
				webService.SendTargetBehavior(targetXML);
				Thread.Sleep(500);
				
				webService.RequestComposition();
				
				string composition = webService.GetComposition();
				Debug.Log(composition);
			
				controller.actionLookupTable = new ActionLookupTable(composition, "xml");
				controller.showOptionList = true;
				Debug.Log("ActionLookupTable generated: " + controller.actionLookupTable.Count + " states found.");
			}
			
			if ( controller.ExecutingAction )
				GUI.enabled = false;
		
			if ( GUILayout.Button("Reset") )
			{
				foreach(KeyValuePair<string, GameObject> npc in npcsList)
				{
					npc.Value.GetComponent<FiniteStateMachine>().Reset();
					npc.Value.GetComponent<MoveNPC>().Reset();
				}
			}
		
			GUI.enabled = true;
			if ( GUILayout.Button("Options") )
			{
				opts.showOptions = !(opts.showOptions);
				if (!opts.showOptions)
					Time.timeScale = 1.0f;
			}
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
