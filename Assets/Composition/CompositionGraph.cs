using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Pathfinding;

public class CompositionGraph : PointGraph {
	
	public string graphFilename;
	
	private List<CompositionNode> readNodes = new List<CompositionNode>();
	private List<CompositionNode>[] readEdges;
	
	public CompositionGraph (string filename) {
		graphFilename = filename;
	}
	
	public override void Scan () 
	{
		//Debug.Log("In Scan() function...");
		
		// We call ParseFile(), that will read the input coming from the TGF file
		// and initialize some accessory private Lists accordingly
		ParseFile();
		
		/*
		foreach (CompositionNode n in readNodes)
			Debug.Log (n.GetNode().name);
		
		foreach (List<CompositionNode> list in readEdges)
			foreach(CompositionNode node in list)
				Debug.Log ("Edge with " + node.GetNode().name);
		*/
		
		// We create the set of nodes to be maintained by tha A* Pathfinding system
		// For each node we set its position, the other nodes it is connected to
		// and the costs (distances) of these connections
		nodes = CreateNodes(readNodes.Count);
		for (int i = 0; i < readNodes.Count; i++) {
			Node currentNode = nodes[i];
			currentNode.position = (Int3) readNodes[i].GetNode().transform.position;
			currentNode.walkable = true;
		
			Node[] connections = new Node[readEdges[i].Count];
			int[] connectionCosts = new int[readEdges[i].Count];
			for(int j = 0; j < readEdges[i].Count; j++) {
				connections[j] = nodes[(readEdges[i][j].GetId())-1];
				connectionCosts[j] = (currentNode.position - connections[j].position).costMagnitude;
			}
			
			currentNode.connections = connections;
			currentNode.connectionCosts = connectionCosts;
		}
		
		/*
		foreach(Node n in nodes) {
			Debug.Log ("Analyzing " + n.position + "...");
			for (int i = 0; i < n.connections.Length; i++)
				Debug.Log ("Node " + n.position + " is connected to " + n.connections[i].position + " with cost " + n.connectionCosts[i]);
		}
		*/
	}
	
	public void ParseFile ()
	{
		StreamReader file = new StreamReader(graphFilename);
		
		while(file.EndOfStream == false)
		{
			string line = file.ReadLine();
			// The character # divides the list of nodes from the list of edges
			if (line.Trim() == "#"){
				// We initialize the array of adjacency lists readEdges with the proper size
				readEdges = new List<CompositionNode>[readNodes.Count];
				for(int i = 0; i<readEdges.Length; i++)
					readEdges[i] = new List<CompositionNode>();
				continue;
			}
			
			// This regular expression indicates a declaration of a node of the graph
			Regex nodeItem = new Regex(@"^(\d+)\s*([A-Za-z]\w*)");
			// This regular expression indicates a declaration of an edge of the graph
			Regex edgeItem = new Regex(@"^(\d+)\s*(\d+)\s*(\w*)");
	
			// If the line currently read is a declaration of a node, add the Node object
			// correspoding to the node to the readNodes list
			Match matchNode = nodeItem.Match(line);
			if (matchNode.Success) {
				int nodeId = int.Parse(matchNode.Groups[1].Value);
				string nodeName = matchNode.Groups[2].Value;
				//Debug.Log("Node " + nodeId + ": " + nodeName + " found.");
				
				CompositionNode newNode = new CompositionNode(GameObject.Find(/*"Node" + */nodeName), nodeId);
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
				//Debug.Log("Edge <" + firstNodeId + ", " + secondNodeId + "> found.");
				
				readEdges[firstNodeId-1].Add(readNodes[secondNodeId-1]);
				readEdges[secondNodeId-1].Add(readNodes[firstNodeId-1]);
				continue;
			}
		}
		file.Close();
	}
	
	// This function will be called by the MoveNPC script to retrieve
	// the list of the waypoint the NPC can go to
	public List<Transform> GetPathNodes()
	{
		List<Transform> result = new List<Transform>();
		for(int i = 0; i < readNodes.Count; i++)
			result.Add(readNodes[i].GetNode().transform);
		
		return result;
	}
	
	public void SetNodeAsWalkable (GameObject node, bool walkable)
	{
		foreach(NavGraph graph in AstarPath.active.astarData.graphs)
			foreach(Node n in graph.nodes)
				if (n.position == (Int3) node.transform.position)
					n.walkable = walkable;
	}
	
	public Node GetGraphNode (GameObject node)
	{
		Node result = null;
		foreach(NavGraph graph in AstarPath.active.astarData.graphs)
			foreach(Node n in graph.nodes)
				if (n.position == (Int3) node.transform.position){
					result = n;
					break;
			}
		
		return result;
	}
}

class CompositionNode {
	private GameObject node;
	private int id;
	
	public CompositionNode (GameObject n, int i) {
		node = n;
		id = i;
	}
	
	public GameObject GetNode () {
		return node;
	}
	
	public int GetId () {
		return id;
	}
}
