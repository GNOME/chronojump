//Carles Pina
//compile:  mcs json_ping.cs /reference:System.Json.dll
//Good reference: http://computerbeacon.net/blog/creating-jsonobjects-in-c

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Json;
using System.Text;

public class JsonTest
{
	static public void Main()
	{
		// Create a request using a URL that can receive a post. 
		WebRequest request = WebRequest.Create ("http://api.chronojump.org:8080/ping");

		// Set the Method property of the request to POST.
		request.Method = "POST";
		
		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json";

        // Creates the json object
        JsonObject json = new JsonObject();
        json.Add("os_version", "Linux");
        json.Add("cj_version", "0.99");

        // Converts it to a String
        String js = json.ToString();

        // Writes the json object into the request dataStream
        Stream dataStream = request.GetRequestStream ();
		dataStream.Write (Encoding.UTF8.GetBytes(js), 0, js.Length);
		
		dataStream.Close ();
		
		// Get the response.
		WebResponse response = request.GetResponse ();
		
		// Display the status (will be 201, CREATED)
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);
		
		// Clean up the streams.
		dataStream.Close ();
		response.Close ();    
	}
}
