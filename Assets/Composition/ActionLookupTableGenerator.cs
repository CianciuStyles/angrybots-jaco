using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class ActionLookupTableGenerator{

	public static void BuildTable () {
		/*
		IDictionary<string, FiniteStateMachine> npcFSM = new Dictionary<string, FiniteStateMachine>();
		GameObject npcsGameObject = GameObject.Find("NPCs");
		Transform[] npcs = npcsGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform npc in npcs) {
			try {
				// All the NPCs names start with "My"
				if(npc.gameObject.name.StartsWith("My")) {
					npcsList.Add(npc.gameObject.name, npc.gameObject.GetComponent<FiniteStateMachine>());
					//Debug.Log(node.gameObject.name + " found.");
				}
			} catch {
				// Hashtable launches an exception when we try to add an object that is already in the Hashtable
				// If the node we are trying to insert is already in the hashtable, we simply don't add it again
			}
		}
		*/
		
		Debug.Log ("Building the Action Lookup Table...");
		
		StringBuilder sb = new StringBuilder();
		JsonWriter writer = new JsonWriter(sb);
		
		FiniteStateMachine mechFSM = GameObject.Find("MyEnemyMech").GetComponent<FiniteStateMachine>();
		FiniteStateMachine mineBotFSM = GameObject.Find("MyEnemyMineBot").GetComponent<FiniteStateMachine>();
		FiniteStateMachine mineBot1FSM = GameObject.Find("MyEnemyMineBot1").GetComponent<FiniteStateMachine>();
	
		//Debug.Log ("Trying to fetch FSM...");
		IList<FSMNodeWithTransitions> mechStates = mechFSM.ReadNodes;
		IList<FSMNodeWithTransitions> mineBotStates = mineBotFSM.ReadNodes;
		IList<FSMNodeWithTransitions> mineBot1States = mineBot1FSM.ReadNodes;
		//Debug.Log ("Trying to fetch FSM... DONE!");

		writer.WriteObjectStart();
		writer.WritePropertyName("table");
		writer.WriteArrayStart();
	
		foreach (FSMNodeWithTransitions mechState in mechStates) {
			mechFSM.CurrentState = mechState.NodeName;
			IList<FSMTransition> mechNextActions = mechFSM.NextActions;
			
			foreach (FSMNodeWithTransitions mineBotState in mineBotStates) {
				mineBotFSM.CurrentState = mineBotState.NodeName;
				IList<FSMTransition> mineBotNextActions = mineBotFSM.NextActions;
				
				foreach (FSMNodeWithTransitions mineBot1State in mineBot1States) {
					mineBot1FSM.CurrentState = mineBot1State.NodeName;
					IList<FSMTransition> mineBot1NextActions = mineBot1FSM.NextActions;
					
					writer.WriteObjectStart();
					writer.WritePropertyName("currentState");
					writer.WriteArrayStart(); //Modified
					writer.WriteObjectStart();
					writer.WritePropertyName("npc");
					writer.Write(mechFSM.gameObject.name);
					writer.WritePropertyName("state");
					writer.Write(mechFSM.CurrentState);
					writer.WriteObjectEnd();
					writer.WriteObjectStart();
					writer.WritePropertyName("npc");
					writer.Write(mineBotFSM.gameObject.name);
					writer.WritePropertyName("state");
					writer.Write(mineBotFSM.CurrentState);
					writer.WriteObjectEnd();
					writer.WriteObjectStart();
					writer.WritePropertyName("npc");
					writer.Write(mineBot1FSM.gameObject.name);
					writer.WritePropertyName("state");
					writer.Write(mineBot1FSM.CurrentState);
					writer.WriteObjectEnd();
					writer.WriteArrayEnd(); //Modified
					writer.WritePropertyName("nextActions");
					writer.WriteArrayStart();
		
					foreach (FSMTransition nextAction in mechNextActions) {
						writer.WriteObjectStart();
						writer.WritePropertyName("actionName");
						writer.Write(nextAction.ActionName);
						writer.WritePropertyName("argument");
						writer.Write(nextAction.Argument);
						writer.WritePropertyName("npc");
						writer.Write (mechFSM.gameObject.name);
						writer.WriteObjectEnd();
					}
					
					foreach (FSMTransition nextAction in mineBotNextActions) {
						writer.WriteObjectStart();
						writer.WritePropertyName("actionName");
						writer.Write(nextAction.ActionName);
						writer.WritePropertyName("argument");
						writer.Write(nextAction.Argument);
						writer.WritePropertyName("npc");
						writer.Write (mineBotFSM.gameObject.name);
						writer.WriteObjectEnd();
					}
					
					foreach (FSMTransition nextAction in mineBot1NextActions) {
						writer.WriteObjectStart();
						writer.WritePropertyName("actionName");
						writer.Write(nextAction.ActionName);
						writer.WritePropertyName("argument");
						writer.Write(nextAction.Argument);
						writer.WritePropertyName("npc");
						writer.Write (mineBot1FSM.gameObject.name);
						writer.WriteObjectEnd();
					}
					
					writer.WriteArrayEnd();
					writer.WriteObjectEnd();
						
				}
			}
		}
		
		writer.WriteArrayEnd();
		writer.WriteObjectEnd();
		StreamWriter sw = new StreamWriter(@"Assets/Composition/ActionLookupTable.json");
		Debug.Log ("Writing the JSON file...");
		sw.Write(sb.ToString());
		Debug.Log ("Writing the JSON file... DONE!");
		sw.Close();
	
	}
}
