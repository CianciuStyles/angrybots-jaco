using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using RestSharp;

public class RESTfulClient {
	
	private RestClient client;
	private string filePath;
	private string clientID;
	
	//public RESTfulClient (string uri, string filePath)
	public RESTfulClient(string uri)
	{
		this.client = new RestClient(uri);
		//this.filePath = filePath;	
	}
	
	public void Authorize()
	{
		RestRequest request = new RestRequest("auth", Method.GET);
		
		Debug.Log ("Sending Authorize() request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode + " - " + response.StatusDescription);
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(response.Content);
		XmlNodeList list = xmlDoc.GetElementsByTagName("client_id");
		clientID = list.Item(0).InnerText;
		Debug.Log ("Response: " + clientID);
	}

	public void SendNonPlayerCharacter(string behaviorXMLString)
	{
		RestRequest request = new RestRequest(clientID + "/behaviors", Method.POST);
		request.AddHeader("Content-Type", "text/xml");
		//request.AddParameter("text/xml", System.IO.File.ReadAllText(filePath + Path.DirectorySeparatorChar + filename), ParameterType.RequestBody);
		request.AddParameter("text/xml", behaviorXMLString, ParameterType.RequestBody);
		
		Debug.Log ("Sending SendNonPlayerCharacter request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode + " - " + response.StatusDescription);
	}
	
	public void GetNonPlayerCharacter(string name)
	{
		RestRequest request = new RestRequest("npcs/" + name, Method.GET);
		//request.AddHeader("Accept", "text/xml");
		//request.AddHeader("Accept", "application/xml");
		request.AddHeader("Accept", "application/json");
		
		Debug.Log ("Sending GetNonPlayerCharacter request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode);
		Debug.Log ("Response: " + response.Content);
	}
	
	public void GetCount ()
	{
		RestRequest request = new RestRequest("npcs/count", Method.GET);
		request.AddHeader("Accept", "text/plain");
		
		Debug.Log ("Sending GetCount request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode);
		Debug.Log ("Count: " + response.Content);
	}
	
	public void Clear()
	{
		RestRequest request = new RestRequest("npcs", Method.DELETE);
		//request.AddHeader("Accept", "text/plain");
		
		Debug.Log ("Sending Clear request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode);
	}
	
	public void SendTargetBehavior(string targetXmlString)
	{
		RestRequest request = new RestRequest(clientID + "/target", Method.POST);
		request.AddHeader("Content-Type", "text/xml");
		//request.AddParameter("text/xml", System.IO.File.ReadAllText(filePath + Path.DirectorySeparatorChar + filename), ParameterType.RequestBody);
		request.AddParameter("text/xml", targetXmlString, ParameterType.RequestBody);
		
		Debug.Log ("Sending SendTargetBehavior request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode + " - " + response.StatusDescription);
	}
	
	public void RequestComposition()
	{
		RestRequest request = new RestRequest(clientID + "/composition", Method.POST);
		//request.AddHeader("Content-Type", "text/xml");
		//request.AddParameter("text/xml", System.IO.File.ReadAllText(filePath + Path.DirectorySeparatorChar + filename), ParameterType.RequestBody);
		//request.AddParameter("text/xml", targetXmlString, ParameterType.RequestBody);
		
		Debug.Log ("Sending RequestComposition request...");
		RestResponse response = client.Execute(request) as RestResponse;
		Debug.Log ("Status Code: " + response.StatusCode + " - " + response.StatusDescription);
	}
	
	public string GetComposition()
	{
		RestResponse response = null;
		bool firstTime = true;
		string queue_number = null;
		
		do {
			if ( !firstTime )
				Thread.Sleep(2000);
			
			RestRequest request = new RestRequest(clientID + "/composition", Method.GET);
			request.AddHeader("Accept", "text/xml");
			//request.AddHeader("Accept", "application/json");
			//request.AddParameter("text/xml", System.IO.File.ReadAllText(filePath + Path.DirectorySeparatorChar + filename), ParameterType.RequestBody);
			//request.AddParameter("text/xml", targetXmlString, ParameterType.RequestBody);
			
			Debug.Log ("Sending GetComposition request...");
			response = client.Execute(request) as RestResponse;
			Debug.Log ("Status Code: " + response.StatusCode + " - " + response.StatusDescription);
			//Debug.Log (response.Content);
			
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(response.Content);
			XmlNodeList list = xmlDoc.GetElementsByTagName("queue_number");
			queue_number = list.Item(0).InnerText;
			Debug.Log ("Queue number: " + list.Item(0).InnerText);
			
			firstTime = false;
		} while (response.StatusCode == 0 || !queue_number.Equals("0") );
		
		return response.Content;
	}
}
