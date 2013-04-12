using Ionic.Zip;
using LitJson;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using UnityEngine;

public class ControllerGUI : MonoBehaviour {
	
	private static int xOffset = 10;
	private static int yOffset = 10;
	private static int boxWidth = 180;
	private static int buttonWidth = 160;
	private static int lineHeight = 35;
	private static int xOffsetButton = xOffset + (boxWidth - buttonWidth)/2;
	private static int yOffsetButton = yOffset + 25;
	
	private GameObject pathNodesGameObject;
	private GameObject npcsGameObject;
	private GameObject targetBehaviorGameObject;

	private IDictionary<string, GameObject> npcsList = new Dictionary<string, GameObject>();
	private IDictionary<string, Transform> nodesList = new Dictionary<string, Transform>();
	private int numberOfOptions;
	
	public ActionLookupTable actionLookupTable;
	private bool executingAction;
	public bool ExecutingAction
	{
		get {return executingAction;}
		set {executingAction = value;}
	}
	
	public bool showOptionList;
	
	public string CompositionGraphsDirectoryPath
	{
		get{return compositionGraphsDirectoryPath;}
		set{compositionGraphsDirectoryPath = value;}
	}
	
	private string compositionGraphsDirectoryPath = @"Assets/Composition/CompositionGraphs";
	public string finiteStateMachinesDirectoryPath = @"Assets/Composition/FiniteStateMachines";
	public string xmlDirectoryPath = @"Assets/Composition/XML/";
	
	private RESTfulClient webService;

	// Use this for initialization
	void Start () {
		// Build an hashtable of all the NPCs present in the scene
		npcsGameObject = GameObject.Find("NPCs");
		Transform[] npcs = npcsGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform npc in npcs) {
			try {
				// All the NPCs names start with "My"
				if(npc.gameObject.name.StartsWith("My")) {
					//npcsList.Add(npc.gameObject.name, npc.gameObject.GetComponent<MoveNPC>());
					npcsList.Add(npc.gameObject.name, npc.gameObject);
					//Debug.Log(node.gameObject.name + " found.");
				}
			} catch {
				// Hashtable launches an exception when we try to add an object that is already in the Hashtable
				// If the node we are trying to insert is already in the hashtable, we simply don't add it again
			}
		}
		Debug.Log(npcsList.Count + " NPCs found.");

		// Build an hashtable of all the possible points of interest, to be displayed in the GUI
		pathNodesGameObject = GameObject.Find("Path Nodes");
		Transform[] pathNodes = pathNodesGameObject.GetComponentsInChildren<Transform>();
		foreach (Transform node in pathNodes){
			try {
				// We are interested only in the real points of interest, those whose name is "NodeX" 
				if(node.gameObject.name.StartsWith("Node")) {
					nodesList.Add(node.gameObject.name, node);
					//Debug.Log(node.gameObject.name + " found.");
				}
			} catch {
				// Hashtable launches an exception when we try to add an object that is already in the Hashtable
				// If the node we are trying to insert is already in the hashtable, we simply don't add it again
			}
		}
		//numberOfOptions = nodesList.Count;
		Debug.Log(nodesList.Count + " nodes found.");

		targetBehaviorGameObject = GameObject.Find("TargetBehavior");
		Debug.Log(targetBehaviorGameObject.name + " found.");
		
		showOptionList = false;

		// Write the XML files to be sent to the SM4LL service
		//WriteXMLFiles();
		// Package XML files in ZIP files to be sent to the SM4ALL service
		//ZipXMLFiles();
		// Make the request to the SM4LL service (if it could be possible)
		// AskForComposition();
		// GenerateFiles();
		
		/*
		//webService = new RESTfulClient("http://localhost:9799/jtlv");
		webService = new RESTfulClient("http://jaco.dis.uniroma1.it/1");
		
		webService.Authorize();
		
		foreach(KeyValuePair<string, GameObject> npc in npcsList) {
			//Debug.Log ("XML of " + npc.Key + ": " + npc.Value.GetComponent<FiniteStateMachine>().Serialize());
			string npcXML = npc.Value.GetComponent<FiniteStateMachine>().Serialize();
			webService.SendNonPlayerCharacter(npcXML);
			Thread.Sleep(500);
		}
		
		string targetXML = targetBehaviourGameObject.GetComponent<TargetBehaviour>().Serialize();
		webService.SendTargetBehavior(targetXML);
		Thread.Sleep(500);
		
		webService.RequestComposition();
		//string composition = webService.GetComposition();
		//Thread.Sleep(7000);
		
		string composition = webService.GetComposition();
		Debug.Log(composition);
		//webService.GetCount();
		//webService.Clear();
		//webService.GetCount();
		//webService.SendNonPlayerCharacter("prova1.xml");
		//webService.GetCount();
		//webService.GetNonPlayerCharacter("Stefano");

		// Create the ActionLookupTable from the JSON file or the XML Composition returned by the SM4LL service
		//ActionLookupTableGenerator.BuildTable();
		//actionLookupTable = new ActionLookupTable(@"Assets/Composition/ActionLookupTable.json");
		//actionLookupTable = new ActionLookupTable(@"Assets/Composition/XML/Composition.xml");
		actionLookupTable = new ActionLookupTable(composition, "xml");
		Debug.Log("ActionLookupTable generated: " + actionLookupTable.Count + " states found.");
		*/

		// At the beginning, we are not performing any action, so we'd like to see all possible options
		executingAction = false;
	} 
	
	public void GenerateFiles ()
	{
		//Debug.Log ("Writing XML files...");
		WriteXMLFiles();
		//Debug.Log ("Writing XML files...DONE!");
		
		//Debug.Log ("Writing ZIP files...");
		ZipXMLFiles();
		//Debug.Log ("Writing ZIP files...DONE!");
		
		//Debug.Log ("Cleaning XML files...");
		CleanXMLFiles();
		//Debug.Log ("Cleaning XML files...DONE!");
	}
	
	void WriteXMLFiles ()
	{
		// Save the XML files for the NPCs' FSMs
		foreach(KeyValuePair<string, GameObject> npc in npcsList)
		{
			FiniteStateMachine fsm = npc.Value.GetComponent<FiniteStateMachine>();
			fsm.SaveAsXml();
		}

		// Create the XML file for the service definitions
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "\t";

		XmlWriter writer = XmlWriter.Create(xmlDirectoryPath + "services.sdd.xml", settings);
		writer.WriteStartElement("home");
		writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
		writer.WriteAttributeString("xsi:schemaLocation", "http://www.sm4all-project.eu/composition/sdd ../sdd.xsd");
		writer.WriteAttributeString("name", "NPCs Service Description");

		foreach(KeyValuePair<string, GameObject> npc in npcsList)
		{
			writer.WriteStartElement("service-instance");
			writer.WriteAttributeString("name", npc.Value.name);
			writer.WriteAttributeString("description", npc.Value.name);
			writer.WriteAttributeString("type", npc.Value.name + ".sbl.xml");
			writer.WriteAttributeString("siid", npc.Value.name);
			writer.WriteAttributeString("wsa", npc.Value.name);
			writer.WriteEndElement(); // End of the Service-Instance node
		}

		writer.WriteEndElement(); // End of the Home node
		writer.Close();

		// Save the XML file for the Target Behaviour
		TargetBehavior tb = targetBehaviorGameObject.GetComponent<TargetBehavior>();
		tb.SaveAsXml();

	}

	void ZipXMLFiles () 
	{
		// Create the zip file with the FSMs and the services definition
		using (ZipFile zip = new ZipFile())
		{
			foreach (KeyValuePair<string, GameObject> npc in npcsList)
			{
				ZipEntry e = zip.AddFile(xmlDirectoryPath + npc.Value.name + ".sbl.xml");
				e.FileName = npc.Value.name + ".sbl.xml";
			}

			ZipEntry entry = zip.AddFile(xmlDirectoryPath + "services.sdd.xml");
			entry.FileName = "services.sdd.xml";

			zip.Save(xmlDirectoryPath + "services.zip");
		}

		// Create the zip file with the TargetBehaviour
		using (ZipFile zip = new ZipFile())
		{
			ZipEntry e = zip.AddFile(xmlDirectoryPath + "TargetBehaviour.sbl.xml");
			e.FileName = "TargetBehaviour.sbl.xml";
			zip.Save(xmlDirectoryPath + "target.zip");
		}
	}
	
	void CleanXMLFiles () {
		
		foreach(KeyValuePair<string, GameObject> npc in npcsList)
			File.Delete(xmlDirectoryPath + npc.Value.name + ".sbl.xml");
		
		File.Delete(xmlDirectoryPath + "services.sdd.xml");
		File.Delete(xmlDirectoryPath + targetBehaviorGameObject.name + ".sbl.xml");
	}

	// Update is called once per frame
	void Update () {
		//Debug.Log ("There are currently " + AstarPath.active.astarData.graphs.Length + " graphs in the system.");
		//foreach(Pathfinding.NavGraph graph in AstarPath.active.astarData.graphs)
		//	Debug.Log (graph.name);
	}
	
	void OnGUI () {		
		/*
		int currentOption = 0;
		foreach (DictionaryEntry node in nodesList)
		{
			Transform myNode = node.Value as Transform;
			//Debug.Log("Analyzing " + myNode.name + "...");
			bool isNodeOccupied = false;

			// We see if some point of interest is already occupied
			// If it is, we can take a snapshot
			// If it isn't, we can command a NPC to reach it
			foreach (DictionaryEntry npc in npcsList)
			{
				MoveNPC myNPC = npc.Value as MoveNPC;
				// Debug.Log("Analyzing " + myNPC.gameObject.name + "...");
				
				if (myNode == myNPC.GetCurrentPathNode().transform)
					isNodeOccupied = true;
			}

			if (isNodeOccupied) {
				if (GUI.Button (new Rect(xOffsetButton, yOffsetButton + currentOption * lineHeight, buttonWidth, 30), "Take snapshot of " + myNode.name))
				{
					Debug.Log("Click!");
				}
			} else {
				if (GUI.Button (new Rect(xOffsetButton, yOffsetButton + currentOption * lineHeight, buttonWidth, 30), "Move to " + myNode.name))
				{
					// We see how many NPC are capable of reaching the point we are interested in
					List<MoveNPC> canMove = new List<MoveNPC>();
					foreach (DictionaryEntry npc in npcsList)
					{
						MoveNPC myNPC = npc.Value as MoveNPC;
						//Debug.Log("Can " + myNPC.gameObject.name + " get to " + myNode.gameObject.name + "?");
						
						if (myNPC.CanGoTo(myNode))
						{
							//Debug.Log(myNPC.gameObject.name + " can reach " + myNode.gameObject.name);
							canMove.Add(myNPC);
						}
					}

					// If only one NPC can reach the point, set him the new target position
					if (canMove.Count == 1)
						canMove[0].SetTargetPosition(myNode);
					else {
					// If more than one can, select one of them randomly and set him the new target position
						System.Random random = new System.Random();
						canMove[random.Next(canMove.Count)].SetTargetPosition(myNode);
					}
				}
			}

			currentOption++;
		}
		*/
		
		/*
		string MyEnemyMechState = npcsList["MyEnemyMech"].GetComponent<FiniteStateMachine>().CurrentState;
		string MyEnemyMineBotState = npcsList["MyEnemyMineBot"].GetComponent<FiniteStateMachine>().CurrentState;
		string MyEnemyMineBot1State = npcsList["MyEnemyMineBot1"].GetComponent<FiniteStateMachine>().CurrentState;
		*/
		
		if (showOptionList)
		{
			IDictionary<string, string> currentStates = new SortedDictionary<string, string>();
			foreach(KeyValuePair<string, GameObject> npc in npcsList) 
				currentStates.Add(npc.Key, npc.Value.GetComponent<FiniteStateMachine>().CurrentState);
			
			List<NextAction> nextActions = actionLookupTable.GetNextActions(currentStates);
			//List<NextAction> nextActions = actionLookupTable.GetNextActions(MyEnemyMechState, MyEnemyMineBotState, MyEnemyMineBot1State);
			//nextActions.Sort();
			numberOfOptions = nextActions.Count;
			
			if (executingAction) {
				GUI.Label( new Rect(xOffset, yOffset, 250, 30), "An action is being executed...");
			} else {
				GUI.Box(new Rect(xOffset, yOffset, boxWidth, (numberOfOptions+1)*lineHeight), "Controller Option List");
				int currentOption = 0;
				foreach (NextAction na in nextActions)
				{
					if (GUI.Button (new Rect(xOffsetButton, yOffsetButton + currentOption * lineHeight, buttonWidth, 30), na.actionName + " " + na.argument)) {
						if (na.actionName == "TakeSnapshot")
							Debug.Log("Click!");
						else if (na.actionName == "MoveTo")
							npcsList[na.npc].GetComponent<MoveNPC>().SetTargetPosition(nodesList[na.argument]);
					}
					currentOption++;
				}
			}
		}
	}
}