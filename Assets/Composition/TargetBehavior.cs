using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;

public class TargetBehavior : FiniteStateMachine {

	// Use this for initialization
	public override void Start () {
		readNodes = new List<FSMNodeWithTransitions>();
		if (baseFolder == null)
			filename = @"Assets/Composition/FiniteStateMachines/" + gameObject.name + "FSM.tgf";
		else
			filename = baseFolder + "/TargetBehavior.tgf";
		StreamReader file = new StreamReader(filename);
		
		while(file.EndOfStream == false)
		{
			string line = file.ReadLine();
			// The character # divides the list of nodes from the list of edges
			if (line.Trim() == "#"){
				// We initialize the array of adjacency lists readEdges with the proper size
				//readEdges = new List<FSMTransitions>(readNodes.Count);
				//for(int i = 0; i < readNodes.Count; i++)
				//	readEdges.Add(new FSMTransitions(readNodes[i].GetNodeName()));
				//Debug.Log("Created List with " + readEdges.Count + " FSMTransitions objects");
				continue;
			}
			
			// This regular expression indicates a declaration of a node of the graph
			Regex nodeItem = new Regex(@"^(\d+)\s*([A-Za-z]\w*\s*\w*)");
			// This regular expression indicates a declaration of an edge of the graph
			Regex edgeItem = new Regex(@"^(\d+)\s*(\d+)\s*([A-Za-z]\w*)");
	
			// If the line currently read is a declaration of a node, add the Node object
			// correspoding to the node to the readNodes list
			Match matchNode = nodeItem.Match(line);
			if (matchNode.Success) {
				int nodeId = int.Parse(matchNode.Groups[1].Value);
				string nodeName = matchNode.Groups[2].Value;
				//Debug.Log("Node " + nodeId + ": " + nodeName + " found.");
				
				FSMNodeWithTransitions newNode = new FSMNodeWithTransitions(nodeName, nodeId);
				readNodes.Add(newNode);
				continue;
			}
			
			// If the line currently read is a declaration of an edge, add the Node object
			// correspoding to the second endpoint of the edge to the corresponging adjacency
			// list in the readEdges array
			Match matchEdge = edgeItem.Match(line);
			if (matchEdge.Success) {
				int firstNodeId = int.Parse(matchEdge.Groups[1].Value);
				int secondNodeId = int.Parse(matchEdge.Groups[2].Value);
				string actionName = matchEdge.Groups[3].Value;
				//Debug.Log("Edge <" + firstNodeId + ", " + secondNodeId + "> found.");

				//Debug.Log("Trying to insert " + actionName + ", " + readNodes[secondNodeId-1].NodeName + " in " + (firstNodeId-1));
				readNodes[firstNodeId-1].AddTransition(actionName, readNodes[secondNodeId-1].NodeName);
				continue;
			}
		}

		currentState = readNodes[0].NodeName;
		file.Close();
		
		Debug.Log ("TargetBehavior: " + readNodes.Count + " nodes found.");
	}
	
	// Update is called once per frame
	public override void Update () {
		/*
		Debug.Log ("The FSM has " + readNodes.Count + " nodes");
		
		foreach(FSMNodeWithTransitions node in readNodes){
			Debug.Log ("Transitions available from " + node.NodeName);

			foreach (FSMTransition t in node.Transitions)
				Debug.Log("\t" + node.NodeName + " -> " + t.ActionName + " -> " + t.Argument);
		}
		*/

		//Debug.Log ("CurrentState is: " + currentState);
	}

	public override void SaveAsXml ()
	{
		bool firstNode = true;

		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "\t";
		settings.OmitXmlDeclaration = false;

		XmlWriter writer = XmlWriter.Create(@"Assets/Composition/XML/" + gameObject.name + ".sbl.xml", settings);
		writer.WriteStartElement("service");
		//writer.WriteAttributeString("xmlns", "http://www.sm4all-project.eu/composition/sbl");
		writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
		writer.WriteAttributeString("xsi:schemaLocation", "http://www.sm4all-project.eu/composition/sbl ../sbl.xsd");
		writer.WriteAttributeString("name", gameObject.name);
		writer.WriteAttributeString("stid", "http://www.sm4all-project.eu/composition/sample/" + gameObject.name);
		writer.WriteAttributeString("class", "service-type");

		writer.WriteStartElement("ts");

		foreach (FSMNodeWithTransitions node in readNodes)
		{
			writer.WriteStartElement("state");
			writer.WriteAttributeString("name", node.NodeName);

			if (firstNode) {
				writer.WriteAttributeString("type", "initial-final");
				firstNode = false;
			} else {
				writer.WriteAttributeString("type", "final");
			}

			foreach (FSMTransition transition in node.Transitions)
			{
				writer.WriteStartElement("transition");
				writer.WriteAttributeString("action", transition.ActionName);
				
				/*
				writer.WriteStartElement("invocation-parameter");
				writer.WriteAttributeString("value", transition.Argument);
				writer.WriteEndElement(); // End of Invocation-Parametere node
				*/

				writer.WriteStartElement("target");
				writer.WriteAttributeString("state", transition.Argument);
				writer.WriteEndElement(); // End of Target node

				writer.WriteEndElement(); //End of Transition node
			}

			writer.WriteEndElement(); // End of State node
		}

		writer.WriteEndElement(); // End of TS node
		writer.WriteEndElement(); // End of Service node

		writer.Close();
	}
	
	public override string Serialize()
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = new UTF8Encoding(false);
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
		xmlWriterSettings.Indent = true;
		
		using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
		{
			xmlWriter.WriteStartElement("behavior");
			
				xmlWriter.WriteStartElement("name");
					xmlWriter.WriteString(gameObject.name);
				xmlWriter.WriteEndElement(); // End of Name node
			
				xmlWriter.WriteStartElement("finiteStateMachine");
					foreach (FSMNodeWithTransitions node in readNodes)
					{
						xmlWriter.WriteStartElement("state");
						xmlWriter.WriteAttributeString("node", node.NodeName);
						
						/*
						if (firstNode) {
							writer.WriteAttributeString("type", "initial-final");
							firstNode = false;
						} else {
							writer.WriteAttributeString("type", "final");
						}
						*/
				
						foreach (FSMTransition transition in node.Transitions)
						{
							xmlWriter.WriteStartElement("transition");
								//xmlWriter.WriteAttributeString("action", transition.ActionName + transition.Argument);
								xmlWriter.WriteAttributeString("action", transition.ActionName);
								
								/*
								writer.WriteStartElement("invocation-parameter");
								writer.WriteAttributeString("value", transition.Argument);
								writer.WriteEndElement(); // End of Invocation-Parametere node
								*/
				
								xmlWriter.WriteStartElement("target");
									xmlWriter.WriteString(transition.Argument);
								xmlWriter.WriteEndElement(); // End of Target node
			
							xmlWriter.WriteEndElement(); //End of Transition node
						}
			
						xmlWriter.WriteEndElement(); // End of State node
					}
				xmlWriter.WriteEndElement(); // End of FiniteStateMachine node
				
			xmlWriter.WriteEndElement(); // End of Behavior node
		}
		
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
