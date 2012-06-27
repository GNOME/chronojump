//llibre mono (notebook) pag 206

using System; //for environment
using System.IO;
using System.Web.Services;
//using System.Web;
using System.Web;

using System.Net; //getIP stuff

[WebService(Namespace="http://localhost:8080/", //work to connect to corall development from client (from browser works only when online)
//[WebService(Namespace="http://80.32.81.197:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT
//[WebService(Namespace="http://server.chronojump.org:8080/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
//[WebService(Namespace="http://server.chronojump.org/", //works to connect with pinux xen from client (from browser don't works) WORKS FROM CLIENT (important: needed the last '/')
	Description="ChronojumpServerAlive")]
[Serializable]
public class ChronojumpServerAlive {
	
	[WebMethod(Description="PingAlive")]
	public string PingAlive(string str)
	{
		Console.WriteLine("PingAlive string: " + str);
	
		return "OK";
	}
}
