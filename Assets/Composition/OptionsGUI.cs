using UnityEngine;
using System.Collections;

public class OptionsGUI : MonoBehaviour {
	
	public bool showOptions;
	public bool groupActions;
	public string jacoURI;

	// Use this for initialization
	void Start () 
	{
		groupActions = false;
		showOptions = false;
		jacoURI = "http://jaco.dis.uniroma1.it/1";
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			showOptions = !showOptions;
			if (!showOptions)
				Time.timeScale = 1.0f;
		}
	}
	
	void OnGUI ()
	{
		if (showOptions)
		{
			Time.timeScale = 0.0f;

			GUILayout.BeginArea(new Rect(Screen.width * 0.33f, Screen.height * 0.1f, Screen.width * 0.33f, Screen.height * 0.8f));
			GUILayout.BeginScrollView(new Vector2(Screen.width * 0.33f, Screen.height * 0.1f), GUILayout.Width(Screen.width * 0.33f), GUILayout.Height(Screen.height * 0.8f));		
			
				GUILayout.BeginVertical("Options", GUI.skin.box);
					GUILayout.Space(30);
					groupActions = GUILayout.Toggle(groupActions, "Group actions?");
			
					GUILayout.Space(15);
					GUILayout.Label("URL of the JaCO web service:");
					jacoURI = GUILayout.TextField(jacoURI);
		        GUILayout.EndVertical();
			
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
