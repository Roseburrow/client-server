using System;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class Client
{
    TcpClient client;

    public Client()
    {
        client = new TcpClient();
    }

    public void runClient(string[] args, string ipToConnect, int portToConnect, string name, string locationStatus)
    {
        try
        {
            client.Connect(ipToConnect, portToConnect); //creates a socket connecting the local host to the named host and port.
        }
        catch
        {
            Console.WriteLine(String.Format("Could not connect to server. IP: {0}  PORT: {1}", ipToConnect, portToConnect));
        }

        WhoIsRequest(args, ipToConnect, portToConnect, name, locationStatus);
    }

    private void WhoIsRequest(string[] args, string ipToConnect, int portToConnect, string name, string locationStatus)
    {
        StreamWriter sw = new StreamWriter(client.GetStream());
        StreamReader sr = new StreamReader(client.GetStream());

        if (args.Length < 1)
        {
            Console.WriteLine("No arguements supplied. Try again giving an arguement...");
            Console.ReadLine();
        }
        else
        {
            sw.WriteLine(name);
            sw.WriteLine(locationStatus);
            sw.Flush();

            Console.WriteLine(sr.ReadLine());
            Console.WriteLine(sr.ReadToEnd());
        }
    }
}
