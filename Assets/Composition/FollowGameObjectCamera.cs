using UnityEngine;
using System.Collections;

public class FollowGameObjectCamera : MonoBehaviour {

	public GameObject objectToFollow;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = objectToFollow.transform.position + new Vector3(0, 6, 2);
	}
}
