using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[AddComponentMenu ("Composition/Move NPC")]
public class MoveNPC : MonoBehaviour {
		
	public Path path;
	public float nextWaypointDistance = 0.5f;
	public float speed = 60f;
	public GameObject currentPathNode;
	public List<Transform> pathNodes;
	
	private Transform targetPosition;
	private Seeker seeker;
	private CharacterController motor;
	private int currentWaypoint = 0;	
	private float goalReachedDistance = 0.55f;
	
	private CompositionGraph graph;
	private ControllerGUI controller;
	private GameObject myCamera;
	private GameObject halo;
	private GameObject haloStart;
	private GameObject lineRenderer;

	// Use this for initialization
	void Start () {
		// Read dynamically the movement graph from the corresponding file, create
		// the CompositionGraph and add it to the graphs maintained by A* Pathfinding
		//string filename = Application.dataPath + "/Composition/CompositionGraphs/" + gameObject.name + "Graph.tgf";
		string filename = Application.dataPath + "/Composition/PathfindingGraphs/" + gameObject.name + "Graph.tgf";
		graph = new CompositionGraph(filename);
		graph.name = gameObject.name + " Graph";
		// Debug.Log ("Scanning " + gameObject.name + "...");
		AstarPath.active.astarData.AddGraph(graph);
		graph.Scan();
		
		// Fetch all the waypoints of the A* graph related to the GameObject
		pathNodes = graph.GetPathNodes();
		
		// Initialize some useful scripts
		seeker = GetComponent<Seeker>();
		motor = GetComponent<CharacterController>();
		controller = GameObject.Find("GUI").GetComponent<ControllerGUI>();
		myCamera = transform.Find("Camera").gameObject;
		halo = GameObject.Find("Halo");
		haloStart = GameObject.Find ("HaloStart");
		lineRenderer = GameObject.Find ("LineRenderer");

		// Put the NPC in the right starting position
		transform.position = pathNodes[0].transform.position;
		currentPathNode = pathNodes[0].gameObject;
	}
	
	void OnPathComplete (Path p) {
		// Debug.Log("Yay, we got a path. Did it have any error? " + p.error);
		if (! p.error)
		{
			path = p;
			currentWaypoint = 0;
			controller.ExecutingAction = true;
			myCamera.SetActive(true);

			halo.transform.position = targetPosition.position;
			halo.SetActive(true);

			haloStart.transform.position = gameObject.transform.position;
			haloStart.SetActive(true);	

			for(int i=0; i < path.vectorPath.Length; i++)
				lineRenderer.GetComponent<LineRenderer>().SetPosition(i, path.vectorPath[i]);
			lineRenderer.SetActive(true);
		}
	}
	
	public GameObject GetCurrentPathNode () 
	{
		return currentPathNode;
	}

	public bool CanGoTo (Transform nodeToCheck) {
		bool result = false;

		foreach (Transform node in pathNodes)
			if ((node.position.x == nodeToCheck.position.x) && (node.position.y == nodeToCheck.position.y) && (node.position.z == nodeToCheck.position.z))
				result = true;

		return result;
	}
	
	public void SetTargetPosition(Transform targetP)
	{
		// Set the new target position and try to find a path to reach it
		foreach (Transform t in pathNodes)
			if (t.position.x == targetP.position.x && t.position.y == targetP.position.y && t.position.z == targetP.position.z)
				targetPosition = t;
		
		// We set the current node as walkable, so it will be considered
		// while searching for a path
		graph.SetNodeAsWalkable(currentPathNode, true);
		//Debug.Log ("Is " + currentPathNode.name + " currently walkable? " + graph.GetGraphNode(currentPathNode).walkable)
		
		Path p = new Path(transform.position, targetPosition.position, OnPathComplete);

		// Find the right graph to perform A* on
		int graphMask = 0;
		for(int i = 0; i < AstarPath.active.astarData.graphs.Length; i++)
			if((gameObject.name + " Graph") == AstarPath.active.astarData.graphs[i].name)
				graphMask = i;
		p.nnConstraint.graphMask = 1 << graphMask;
		
		seeker.StartPath(p);
		
		// Debug.Log ("SetTargetPosition: " + targetP.name + " as new objective");
	}

	public Vector3 GetNextWaypoint()
	{
		return path.vectorPath[currentWaypoint];
	}
	
	// Update is called once per frame
	void FixedUpdate () {		
		if (path == null)
			return;
		
		if (currentWaypoint >= path.vectorPath.Length)
		{
			Debug.Log ("End of path reached");

			// Play the correct animation, depending on which kind of enemy the GameObject is
			if (gameObject.name.StartsWith("MyEnemyMech")) {
				// Play the idle animation when the mech reaches its target position
				GetComponentInChildren<Animation>().CrossFade("idle", 0.2f);
			} else if (gameObject.name.StartsWith("MyEnemyMineBot")) {
				// Play the awake animation when the minebot reaches its target position
				GetComponentInChildren<Animation>().CrossFade("awake", 0.2f);
			}	

			// Place the mech at the exact position of the path node
			transform.position = currentPathNode.transform.position;
			// Reset all the variables, so the mech stops moving
			path = null;
			targetPosition = null;
			
			// We set the node we arrived in as unwalkable, so other NPCs will not pass on this point
			// while trying to reach some other destination
			graph.SetNodeAsWalkable(currentPathNode, false);
			//Debug.Log ("Is " + currentPathNode.name + " currently walkable? " + graph.GetGraphNode(currentPathNode).walkable);

			GetComponent<FiniteStateMachine>().CurrentState = currentPathNode.name;
			controller.ExecutingAction = false;
			myCamera.SetActive(false);
			halo.SetActive(false);
			haloStart.SetActive(false);
			lineRenderer.SetActive(false);
			// Deactivate the AudioListener component of the corresponding camera
			// If two AudioListeners are active at the same time, Unity will complain in the Console
			// GameObject.Find(gameObject.name + " Camera").GetComponent<AudioListener>().enabled = false;

			return;
		}

		// Activate the AudioListener component of the corresponding camera
		// If two AudioListeners are active at the same time, Unity will complain in the Console
		// GameObject.Find(gameObject.name + " Camera").GetComponent<AudioListener>().enabled = true;

		// Calculate direction vector and make the NPC move
		Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
		dir *= speed * Time.fixedDeltaTime;
		motor.SimpleMove(dir);
		haloStart.transform.position = gameObject.transform.position;
		
		// Debug.Log (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]));
		/*
		if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < minDistance && 
			Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) > 0.1)
			minDistance = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
		Debug.Log(minDistance);
		*/

		// If we are close enough to the waypoint, let's go to the next one
		if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
		{	
			currentWaypoint++;
			return;
		} else {
			// Make the mech look in the direction he is moving in
			transform.LookAt(path.vectorPath[currentWaypoint]);

			// Play the correct animation, depending on which kind of enemy the GameObject is
			if (gameObject.name.StartsWith("MyEnemyMech")) {
				// Play the walk_forward animation while the mech moves
				GetComponentInChildren<Animation>().Play("walk_forward");
			} else if (gameObject.name.StartsWith("MyEnemyMineBot")) {
				// Play the forward animation while the mine bot moves
				GetComponentInChildren<Animation>().Play("forward");
			}			
				
			// Check if the mech reached one of the Path Nodes
			foreach (Transform t in pathNodes) {
				if (Vector3.Distance(transform.position, t.position) < goalReachedDistance){
					currentPathNode = t.gameObject;
				}
			}
		}
		
		/*
		foreach(Transform t in pathNodes){
			Debug.Log ("Is " + t.gameObject.name + " currently walkable?" + graph.GetGraphNode(t.gameObject).walkable);
		}	
		*/
	}
	
	public void Reset()
	{
		transform.position = pathNodes[0].transform.position;
		currentPathNode = pathNodes[0].gameObject;
		
		if (gameObject.name.StartsWith("MyEnemyMech")) {
			// Play the idle animation when the mech reaches its target position
			GetComponentInChildren<Animation>().CrossFade("idle", 0.2f);
		} else if (gameObject.name.StartsWith("MyEnemyMineBot")) {
			// Play the awake animation when the minebot reaches its target position
			GetComponentInChildren<Animation>().CrossFade("awake", 0.2f);
		}
		
		path = null;
		targetPosition = null;
		graph.SetNodeAsWalkable(currentPathNode, false);

		controller.ExecutingAction = false;
		myCamera.SetActive(false);
		halo.SetActive(false);
		haloStart.SetActive(false);
		lineRenderer.SetActive(false);
	}
}