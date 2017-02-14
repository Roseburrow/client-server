//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class Program
{
    static void Main(string[] args)
    {
        // IP and port to connect to...
        string ipToConnect;
        int portToConnect;
        string name;
        string locationStatus;

        //Makes a new client.
        Client location = new Client();                                                                                                                                                                                             

        //Gets the ip and port from args.
        Tuple <string, int, string, string> argsData = SortArgs(args);
        ipToConnect = argsData.Item1;
        portToConnect = argsData.Item2;
        name = argsData.Item3;
        locationStatus = argsData.Item4;

        location.runClient(args, ipToConnect, portToConnect, name, locationStatus);
        Console.ReadLine();
    }

    static Tuple<string, int, string, string> SortArgs(string[] args)
    {
        string ip = "localhost";
        int port = 43;
        string locationStatus = null;
        string name = null;
        bool nameSet = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "/h")
            {
                ip = args[i + 1];
                i++;
            }
            else if (args[i] == "/p")
            {
                port = int.Parse(args[i + 1]);
                i++;
            }
            else if (nameSet == false)
            {
                name = args[i];
                nameSet = true;
            }
            else { locationStatus = args[i]; }
        }
        return Tuple.Create(ip, port, name, locationStatus);
    }
}