//Carles Pina
//compile:  mcs json_post.cs /reference:System.Json.dll

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Json;

public class JsonTest
{
	static private string getLogsDir() 
	{
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "logs");
	}

	static private byte[] readFile(string filePath)
	{
		return System.IO.File.ReadAllBytes(filePath); 
	}

	static public void Main()
	{
		string filePath = getLogsDir() + Path.DirectorySeparatorChar + "log_chronojump.txt";

		if(! File.Exists(filePath)) {
			Console.WriteLine("Could not send file. It does not exist.");
			return;
		}

		// Create a request using a URL that can receive a post. 
		WebRequest request = WebRequest.Create ("http://api.chronojump.org:8080/backtrace");

		// Set the Method property of the request to POST.
		request.Method = "POST";
		
		// Create POST data and convert it to a byte array.
		byte[] byteArray = readFile(filePath);
		
		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/x-www-form-urlencoded";
		
		// Set the ContentLength property of the WebRequest.
		request.ContentLength = byteArray.Length;
		
		// Get the request stream.
		Stream dataStream;
		try {
			 dataStream = request.GetRequestStream ();
		} catch {
			Console.WriteLine("Could not send file. You are not connected to the Internet or server is down.");
			return;
		}
		
		// Write the data to the request stream.
		dataStream.Write (byteArray, 0, byteArray.Length);
		
		// Close the Stream object.
		dataStream.Close ();
		
		// Get the response.
		WebResponse response = request.GetResponse ();
		
		// Display the status.
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);
		
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
		string crash_id = result["crash_id"];
		Console.WriteLine("crash_id: "+ crash_id);
	}
}
