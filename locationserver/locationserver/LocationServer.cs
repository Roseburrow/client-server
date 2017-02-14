using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
public class LocationServer
{
    Dictionary<string, string> serverDatabase;

    public LocationServer()
    {
        serverDatabase = new Dictionary<string, string>();
        serverDatabase.Add("csstsb", "rbb-312");
    }

    public string lookupDatabase(string name)
    {
        if (serverDatabase.ContainsKey(name))
        {
            return serverDatabase[name];
        }
        return "false";
    }

    protected bool changeLocation(string name, string newLocation)
    {
        if (serverDatabase.ContainsKey(name))
        {
            serverDatabase[name] = newLocation;
            return true;
        }
        return false;
    }

    public void runServer()
    {
        TcpListener listener;
        Socket connection;
        NetworkStream socketStream;

        try
        {
            listener = new TcpListener(IPAddress.Any, 43); //Creates server socket on port. (43 in this case...)
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                connection = listener.AcceptSocket();
                socketStream = new NetworkStream(connection);

                Console.WriteLine("Connected!");

                doRequest(socketStream);

                socketStream.Close();
                connection.Close();

                Console.WriteLine("Connection Closed!");
                Console.ReadLine();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Connection Failed");
            Console.WriteLine(e.ToString());
        }
    }

    protected void doRequest(NetworkStream socketStream)
    {
        StreamWriter sw = new StreamWriter(socketStream);
        StreamReader sr = new StreamReader(socketStream);

        string username = sr.ReadLine();
        Console.WriteLine("Requested Name: " + username);

        string newLocation = sr.ReadLine();
        Console.WriteLine("Requested Change: " + newLocation);

        string requestType = getRequestType(username, newLocation);

        if (requestType == "lookup")
        {
            if (lookupDatabase(username) == "false")
                return;
            else
            {
                sw.WriteLine(username + " is in " + lookupDatabase(username));
            }    
        }
        else if (requestType == "locationChange")
        {
            if (changeLocation(username, newLocation))
                sw.WriteLine(username + " location changed to be in " + newLocation);
        }
        else if (requestType == "ERROR")
            sw.WriteLine("Something went wrong");

        sw.Flush();

        Console.WriteLine(sr.ReadToEnd());
    }

    protected string getRequestType(string username, string location)
    {
        if (location == "" && username != "")
            return "lookup";

        else if (location != "" && username != "")
            return "locationChange";

        else return "ERROR";
    }
}
