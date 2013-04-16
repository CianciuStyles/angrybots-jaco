using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {
	
	Vector3 originalCameraPosition;
	public float xAxisSpeed;
	public float yAxisSpeed;
	public float zAxisSpeed;
		
	// Use this for initialization
	void Start () {
		Transform originalCamera = gameObject.transform;
		originalCameraPosition = new Vector3(originalCamera.position.x, originalCamera.position.y, originalCamera.position.z);
		
		xAxisSpeed = 5.0f;
		yAxisSpeed = 5.0f;
		zAxisSpeed = 35.0f;
	}
	
	// Update is called once per frame
	void Update () {
		
		if ( Input.GetKeyDown(KeyCode.C) )
			gameObject.transform.position = originalCameraPosition;
		
		if ( Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) )
			gameObject.transform.Translate(Vector3.up * xAxisSpeed * Time.deltaTime);
		
		if ( Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) )
			gameObject.transform.Translate(Vector3.up * -xAxisSpeed * Time.deltaTime);
		
		if ( Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) )
			gameObject.transform.Translate(Vector3.left * yAxisSpeed * Time.deltaTime);
		
		if ( Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) )
			gameObject.transform.Translate(Vector3.left * -yAxisSpeed * Time.deltaTime);
		
		if ( Input.GetAxis("Mouse ScrollWheel") > 0 )
		{
			if ( !gameObject.camera.orthographic )
			{
				gameObject.camera.transform.Translate(Vector3.forward * zAxisSpeed * Time.deltaTime);
				
				if ( gameObject.transform.position.y < 0.5f )
					gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.5f, gameObject.transform.position.z);
			}
		}
		
		if ( Input.GetAxis("Mouse ScrollWheel") < 0 )
		{
			if ( !gameObject.camera.orthographic )
			{
				gameObject.transform.Translate(Vector3.forward * -zAxisSpeed * Time.deltaTime);
				
				if ( gameObject.transform.position.y > 48.5f )
					gameObject.transform.position = new Vector3(gameObject.transform.position.x, 48.5f, gameObject.transform.position.z);
			}
		}
	}
}
