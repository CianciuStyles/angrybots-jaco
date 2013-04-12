using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using LitJson;

public class ActionLookupTable {
	private IDictionary<PossibleState, List<NextAction>> table;
	public int Count
	{
		get {return table.Count;}
	}

	public ActionLookupTable (string filename)
	{
		if (filename.EndsWith(".json"))
			ReadFromJson(filename);
		else if (filename.EndsWith(".xml"))
			ReadFromXML(filename);
	}
	
	public ActionLookupTable(string composition, string encoding)
	{
		if(encoding.Equals("xml")){
			//Debug.Log ("Going to parse the XML file");
			ParseXML(composition);
		}
	}
	
	
	private void ReadFromJson (string filename)
	{
		StreamReader sr = new StreamReader(filename);
		JsonData readTable = JsonMapper.ToObject(sr.ReadToEnd());
		readTable = readTable["table"];
		
		//Debug.Log ("ActionLookupTable created.");
		//Debug.Log (readTable.Count + " possible states found.");

		table = new Dictionary<PossibleState, List<NextAction>>(readTable.Count);
		for(int i = 0; i < readTable.Count; i++)
		{
			//Debug.Log ("Analyzing state " + i + "...");
			JsonData possibleSituation = readTable[i];

			PossibleState ps = new PossibleState(possibleSituation["currentState"]);
			//NextActions na = new NextActions(possibleSituation["nextActions"]);
			List<NextAction> na = NextAction.CreateNextActionsList(possibleSituation["nextActions"]);
			//Debug.Log(ps.GetHashCode());
			table.Add(ps, na);
			//Debug.Log (ps.MyEnemyMech);
			//Debug.Log (ps.MyEnemyMineBot);
		}

		//Debug.Log ("Internal Dictionary created.");
		//Debug.Log (table.Count + " possible states found.");

		/*
		Debug.Log("Let's try to retrieve a list of next actions given a possible state...");
		PossibleState myTry = new PossibleState("Node B", "Node I", "Node P");
		List<NextAction> myTry2 = table[myTry];
		Debug.Log("Have we found a correct object? " + (myTry2 != null));
		foreach(NextAction a in myTry2)
			Debug.Log(a);*/
		
	}
	

	public void ReadFromXML (string filename)
	{
		table = new Dictionary<PossibleState, List<NextAction>>();

		XmlReaderSettings settings = new XmlReaderSettings();
		settings.ConformanceLevel = ConformanceLevel.Document;

		//string myEnemyMechState = "";
		//string myEnemyMineBotState = "";
		//string myEnemyMineBot1State = "";
		PossibleState ps = new PossibleState();
		List<NextAction> na = new List<NextAction>();

		XmlReader reader = XmlReader.Create(filename, settings);
		while(reader.Read())
		{
			//Debug.Log(reader.Name);
			switch (reader.Name)
			{
				case "community-state":
					if(reader.IsStartElement())
						ps = new PossibleState();
						//ps = new PossibleState(myEnemyMechState, myEnemyMineBotState, myEnemyMineBot1State);
					break;
				case "service-conversational-state":
					ps.NpcStates.Add(reader.GetAttribute("service"), reader.GetAttribute("state"));
					/*switch(reader.GetAttribute("service"))
					{
						case ("MyEnemyMech"):
							//Debug.Log("Case MyEnemyMech selected.");
							myEnemyMechState = reader.GetAttribute("state");
							break;
						case ("MyEnemyMineBot"):
							myEnemyMineBotState = reader.GetAttribute("state");
							break;
						case ("MyEnemyMineBot1"):
							myEnemyMineBot1State = reader.GetAttribute("state");
							break;
						default:
							break;
					}*/
					break;
				case "conversational-orchestration-state":
					if(!reader.IsStartElement())
						try {
							table.Add(ps, na);
						} catch {
							foreach (NextAction n in na)
								table[ps].Add(n);
						}
					break;
				case "transition":
					if(reader.IsStartElement()) 
					{
						na = new List<NextAction>();
						string action = reader.GetAttribute("action");
						int index = action.StartsWith("MoveTo") ? 6 : 12;
						string npc = reader.GetAttribute("invoke");
						NextAction na1 = new NextAction(action.Substring(0, index), action.Substring(index), npc);
						na.Add(na1);
					}
					break;
				
				default:
					break;
			}
		}
		reader.Close();

		/*
		foreach (KeyValuePair<PossibleState, List<NextAction>> pair in table)
		{
			Debug.Log("Next Actions for State: " + pair.Key);
			foreach(NextAction act in pair.Value)
				Debug.Log("\t" + act);
		}
		*/
	}
	
	
	public void ParseXML (string composition)
	{
		table = new Dictionary<PossibleState, List<NextAction>>();

		XmlReaderSettings settings = new XmlReaderSettings();
		settings.ConformanceLevel = ConformanceLevel.Document;

		PossibleState ps = new PossibleState();
		List<NextAction> na = new List<NextAction>();

		//XmlReader reader = XmlReader.Create(new StringReader(composition), settings);
		XmlReader reader = XmlReader.Create(new StringReader(composition));
		while(reader.Read())
		{
			//Debug.Log(reader.Name);
			switch (reader.Name)
			{
				//case "targetState":
				case "possibleState":
					//Debug.Log ("read a targetState");
					if(reader.IsStartElement()){
						//
					} else {
						try {
							table.Add(ps, na);
						} catch {
							foreach (NextAction n in na)
								table[ps].Add(n);
						}
					}
					break;
				
				case "communityState":
					//Debug.Log ("read a communityState");
					if(reader.IsStartElement())
						ps = new PossibleState();
					break;
				
				case "behaviorState":
					//Debug.Log ("read a behaviorState");
					ps.NpcStates.Add(reader.GetAttribute("behavior"), reader.GetAttribute("state"));
					break;
				
				case "transitions":
					//Debug.Log ("read a transitions");
					if(reader.IsStartElement())
						na = new List<NextAction>();
					break;
				
				case "transition":
					//Debug.Log ("read a transition");
					string action = reader.GetAttribute("action");
					int index = action.StartsWith("MoveTo") ? 6 : 12;
					string npc = reader.GetAttribute("invoke");
					NextAction na1 = new NextAction(action.Substring(0, index), action.Substring(index), npc);
					na.Add(na1);
					break;
				
				default:
					break;

			}
		}
		reader.Close();
		
		/*
		foreach (KeyValuePair<PossibleState, List<NextAction>> pair in table)
		{
			Debug.Log("Next Actions for State: " + pair.Key);
			foreach(NextAction act in pair.Value)
				Debug.Log("\t" + act);
		}
		*/
		
	}

	public List<NextAction> GetNextActions (IDictionary<string, string> states)
	{
		return table[new PossibleState(states)];
	}
}

public class PossibleState : IEquatable<PossibleState>, IEqualityComparer<PossibleState> {
	/*
	private IDictionary<string, string> states;

	public PossibleState (JsonData json)
	{
		foreach (JsonData npcState in json) {

		}
	}
	*/

	//public string MyEnemyMech;
	//public string MyEnemyMineBot;
	//public string MyEnemyMineBot1;
	private IDictionary<string, string> npcStates;
	public IDictionary<string, string> NpcStates
	{
		get{return npcStates;}
		set{}
	}
	
	public PossibleState(IDictionary<string, string> states)
	{
		npcStates = new SortedDictionary<string, string>();
		foreach(KeyValuePair<string, string> state in states)
			npcStates.Add(state.Key, state.Value);
	}
	
	
	public PossibleState(JsonData json)
	{	
		/*
		MyEnemyMech = (string) json["MyEnemyMech"];
		MyEnemyMineBot = (string) json["MyEnemyMineBot"];
		MyEnemyMineBot1 = (string) json["MyEnemyMineBot1"];
		*/
		
		npcStates = new SortedDictionary<string, string>();
		for(int i = 0; i < json.Count; i++) {
			JsonData npc = json[i];
			npcStates.Add((string) npc["npc"], (string) npc["state"]);
			//Debug.Log ((string) npc["npc"] + ", " + (string) npc["state"]);
		}
	}
	
	/*
	public PossibleState(string s1, string s2, string s3)
	{
		MyEnemyMech = s1;
		MyEnemyMineBot = s2;
		MyEnemyMineBot1 = s3;
	}
	*/
	
	public PossibleState ()
	{
		npcStates = new SortedDictionary<string, string>();
	}

	public override string ToString ()
	{
		//return "MyEnemyMech: " + MyEnemyMech + ", MyEnemyMineBot: " + MyEnemyMineBot + ", MyEnemyMineBot1: " + MyEnemyMineBot1;
		string result = "";
		
		foreach(KeyValuePair<string, string> state in npcStates)
			result += state.Key + ": " + state.Value;
		
		return result;
	}

	public override int GetHashCode ()
	{
		string stringHashcode = "";
		
		foreach(KeyValuePair<string, string> state in npcStates)
			stringHashcode += state.Value;
		//Debug.Log(stringHashcode);
		
		return stringHashcode.GetHashCode();
	}

	public override bool Equals (System.Object obj)
	{
		return Equals (obj as PossibleState);
	}

	public bool Equals (PossibleState obj)
	{
		return obj != null && obj.GetHashCode() == this.GetHashCode();
	}

	public bool Equals(PossibleState p1, PossibleState p2)
	{
		return p1.GetHashCode() == p2.GetHashCode();
	}

	public int GetHashCode(PossibleState obj)
	{
		return obj.GetHashCode();
	}
}

public class NextAction : IComparable<NextAction> {

	public string actionName;
	public string argument;
	public string npc;

	public static List<NextAction> CreateNextActionsList(JsonData json)
	{
		List<NextAction> result = new List<NextAction>(json.Count);
		for (int i = 0; i < json.Count; i++)
		{
			JsonData nextAction = json[i];
			result.Add(new NextAction((string) nextAction["actionName"], 
									  (string) nextAction["argument"],
									  (string) nextAction["npc"]));
		}

		return result;
	}

	public NextAction(string aN, string a, string n)
	{
		actionName = aN;
		argument = a;
		npc = n;
	}

	public override string ToString()
	{
		return "Action Name: " + actionName + ", Argument: " + argument + ", NPC: " + npc;
	}

	public int CompareTo(NextAction other)
	{
		if (this.actionName != other.actionName)
			return this.actionName.CompareTo(other.actionName);
		else 
			return this.argument.CompareTo(other.argument);
	}
}