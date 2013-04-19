using UnityEngine;
using System.Collections;

public class OptionsGUI : MonoBehaviour {
	
	public bool showOptions;
	public bool groupActions;
	public string jacoURI;
	public string baseFolder;
	public GUISkin metalGUISkin;
	
	public FileBrowser m_fileBrowser;
	public Texture2D	m_directoryImage, m_fileImage;
	
	// Use this for initialization
	void Start () 
	{
		groupActions = false;
		showOptions = false;
		//jacoURI = "http://jaco.dis.uniroma1.it/1";
		jacoURI = "http://localhost:8080/JaCO";
		baseFolder = Application.dataPath + "/Composition/FiniteStateMachines";			
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
			
			GUI.skin = metalGUISkin;
			
			GUILayout.BeginArea(new Rect(Screen.width * 0.33f, Screen.height * 0.1f, Screen.width * 0.33f, Screen.height * 0.8f));
			//GUILayout.BeginScrollView(new Vector2(Screen.width * 0.33f, Screen.height * 0.1f), GUILayout.Width(Screen.width * 0.33f), GUILayout.Height(Screen.height * 0.8f));		
			
				GUILayout.BeginVertical("Options", GUI.skin.box);
					GUILayout.Space(30);
					groupActions = GUILayout.Toggle(groupActions, "Group actions?");
			
					GUILayout.Space(15);
					GUILayout.Label("URL of the JaCO web service:");
					jacoURI = GUILayout.TextField(jacoURI);
			
					GUILayout.Space(15);
					GUILayout.Label("Folder with the .TGF files:");
			
					//GUILayout.BeginHorizontal();
					baseFolder = GUILayout.TextField(baseFolder);
					//GUILayout.FlexibleSpace();
					if (GUILayout.Button("Select New Folder", GUILayout.ExpandWidth(false))) {
						m_fileBrowser = new FileBrowser(new Rect(Screen.width * 0.25f, Screen.height * 0.1f, Screen.width * 0.5f, Screen.height * 0.8f), "Choose Folder...", FileSelectedCallback);
						m_fileBrowser.BrowserType = FileBrowserType.Directory;
						m_fileBrowser.SelectionPattern = "*";
						m_fileBrowser.DirectoryImage = m_directoryImage;
						m_fileBrowser.FileImage = m_fileImage;
					}
					//GUILayout.EndHorizontal();
			
					GUILayout.Space(15);
					    GUILayout.BeginVertical();
						    GUILayout.BeginHorizontal();
							    GUILayout.FlexibleSpace();
							    if (GUILayout.Button("OK"))
								{
									showOptions = false;
									Time.timeScale = 1.0f;
								}	
							    GUILayout.FlexibleSpace();
						    GUILayout.EndHorizontal();
					    GUILayout.EndVertical();
		        GUILayout.EndVertical();
			
			//GUILayout.EndScrollView();
			GUILayout.EndArea();
			
			if (m_fileBrowser != null)
				m_fileBrowser.OnGUI();
		}
	}
	
	protected void FileSelectedCallback(string path) {
		m_fileBrowser = null;
		if (!path.Equals(""))
			baseFolder = path;
	}
}
