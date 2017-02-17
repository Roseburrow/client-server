using System;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class Client
{
    TcpClient client;
    private string ip = "localhost";
    private int port = 43;
    private string name = null;
    private string location = null;
    private int locLen;

    private enum protocol { whois, h1, h9, h0 }

    private protocol protocolType = protocol.whois;

    public Client()
    {
        client = new TcpClient();
    }

    public void runClient(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("No arguements supplied. Try again giving an arguement...");
            Console.ReadLine();
        }
        else
        {
            SplitArgs(args);
        }

        try
        {
            client.Connect(ip, port); //creates a socket connecting the local host to the named host and port.
            Request();
        }
        catch
        {
            Console.WriteLine(String.Format("Could not connect to server. IP: {0}  PORT: {1}", ip, port));
        }
    }

    private void Request()
    {
        StreamWriter sw = new StreamWriter(client.GetStream());
        StreamReader sr = new StreamReader(client.GetStream());

        if (protocolType == protocol.whois && location == null)
        {
            sw.Write(string.Format("{0}\r\n", name));
        }
        else if (protocolType == protocol.whois && location != null)
        {
            sw.Write(string.Format("{0} {1}\r\n", name, location));
        }
        else if (protocolType == protocol.h9 && location == null)
        {
            sw.Write(string.Format("GET /{0}\r\n", name));
        }
        else if (protocolType == protocol.h9 && location != null)
        {
            sw.Write(string.Format("PUT /{0}\r\n\r\n{1}\r\n", name, location));
        }
        else if (protocolType == protocol.h0 && location == null)
        {
            sw.Write(string.Format("GET /?{0} HTTP/1.0\r\n\r\n", name));
        }
        else if (protocolType == protocol.h0 && location != null)
        {
            sw.Write(string.Format("POST /{0} HTTP/1.0\r\nContent-Length: {1}\r\n\r\n{2}", 
                                    name, locLen, location));
        }
        else if (protocolType == protocol.h1 && location == null)
        {
            sw.Write(string.Format("GET /?name={0} HTTP/1.1\r\nHost: {1}\r\n\r\n", name, ip));
        }
        else if (protocolType == protocol.h1 && location != null)
        {
            sw.Write(string.Format("POST / HTTP/1.1\r\nHost: {0}\r\nContent-Length: " 
                                    + "{1}\r\n\r\nname={2}&location={3}",
                                    ip, locLen, name, location));
        }

        sw.Flush();

        Console.WriteLine(sr.ReadLine());
        Console.WriteLine(sr.ReadToEnd());
    }

    private void SplitArgs(string[] args)
    {
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
            else if (args[i] == "/h9")
            {
                protocolType = protocol.h9;
            }
            else if (args[i] == "/h1")
            {
                protocolType = protocol.h1;
            }
            else if (args[i] == "/h0")
            {
                protocolType = protocol.h0;
            }
            else
            {
                location = args[i];
                locLen = location.Length;
            }
        }
    }
}
