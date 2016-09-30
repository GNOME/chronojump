//Carles Pina
//compile:  mcs json_get.cs /reference:System.Json.dll

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Json;

public class JsonTest
{
	static public void Main()
	{
		// Create a request using a URL that can receive a GET. 
		WebRequest request = WebRequest.Create ("http://api.chronojump.org:8080/version");

		// Set the Method property of the request to POST.
		request.Method = "GET";
		
		// Get the response.
		WebResponse response = request.GetResponse ();
		
		// Display the status.
		Console.WriteLine ("StatusDescription:" + ((HttpWebResponse)response).StatusDescription);

        Stream dataStream;
		
		// Get the stream containing content returned by the server.
		dataStream = response.GetResponseStream ();
		
		// Open the stream using a StreamReader for easy access.
		StreamReader reader = new StreamReader (dataStream);
		
		// Read the content.
		string responseFromServer = reader.ReadToEnd ();
		
		// Display the content.
		Console.WriteLine (responseFromServer);
		
		// Clean up the streams.
		reader.Close ();
		dataStream.Close ();
		response.Close ();    

		JsonValue result = JsonValue.Parse(responseFromServer);
		string stable_version = result["stable"];
		Console.WriteLine("stable version is: "+ stable_version);
	}
}
