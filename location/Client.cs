﻿using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

public class Client
{
    TcpClient client;
    private string ip;
    private int port;
    private string name;
    private string location;
    private int locLen;

    private enum protocol { whois, h1, h9, h0 }

    private protocol protocolType = protocol.whois;

    public Client()
    {
        client = new TcpClient();
        ip = "whois.net.dcs.hull.ac.uk";
        port = 43;
        name = null;
        location = null;
    }

    public void runClient(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("No arguements supplied. Try again giving an arguement...");
            Console.WriteLine();
        }
        else
        {
            SplitArgs(args);

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
    }

    private void Request()
    {
        NetworkStream dataStream = client.GetStream();
        dataStream.ReadTimeout = 1000;

        StreamWriter sw = new StreamWriter(dataStream);
        StreamReader sr = new StreamReader(dataStream);

        if (location == null)
        {
            if (protocolType == protocol.whois)
                sw.Write(string.Format("{0}\r\n", name));

            else if (protocolType == protocol.h9)
                sw.Write(string.Format("GET /{0}\r\n", name));

            else if (protocolType == protocol.h0)
                sw.Write(string.Format("GET /?{0} HTTP/1.0\r\n\r\n", name));

            else if (protocolType == protocol.h1)
                sw.Write(string.Format("GET /?name={0} HTTP/1.1\r\nHost: "
                                     + "{1}\r\n\r\n", name, ip));
        }
        else if (location != null)
        {
            if (protocolType == protocol.whois)
                sw.Write(string.Format("{0} {1}\r\n", name, location));

            else if (protocolType == protocol.h9)
                sw.Write(string.Format("PUT /{0}\r\n\r\n{1}\r\n", name, location));

            else if (protocolType == protocol.h0)
                sw.Write(string.Format("POST /{0} HTTP/1.0\r\nContent-Length: "
                                     + "{1}\r\n\r\n{2}", name, locLen, location));

            else if (protocolType == protocol.h1)
                sw.Write(string.Format("POST / HTTP/1.1\r\nHost: {0}\r\nContent-Length: "
                                     + "{1}\r\n\r\nname={2}&location={3}",
                                        ip, locLen, name, location));
        }

        //flushes the request, sending it to the server.
        sw.Flush();

        /*This is required because there is a read timeout for the client and the server.
          It ensures that the response has been recieved before reying to read the response.*/
        Thread.Sleep(2000);
        string response = GetResponse(sr);

        if (port == 80)
            Console.WriteLine(response);

        else ReadResponse(response);
    }

    private string GetResponse(StreamReader sr)
    {
        string response = "";
        char c;

        while (true)
        {
            try
            {
                c = (char)sr.Read();
                if (c == '\uffff' || c == '\0')
                    break;

                response += c;
            }
            catch
            {
                break;
            }
        }
        return response;
    }

    private void ReadResponse(string response)
    {
        string recievedLocation;

        Regex nameWhoIsResponse = new Regex(@"(.*)\r\n");
        Regex locWhoIsResponse = new Regex(@"^OK\r\n$");

        Regex nameHTTPResponse = new Regex(@"^HTTP/([^ ]+) 200 OK\r\nContent-Type: "
                                           + @"text/plain\r\n\r\n(.*)\r\n$");
        Regex locHTTPResponse = new Regex(@"^HTTP/([^ ]+) 200 OK\r\nContent-Type: "
                                          + @"text/plain\r\n\r\n$");

        Regex whoIsNotFound = new Regex(@"^ERROR: no entries found\r\n$");
        Regex otherNotFound = new Regex(@"^HTTP/([^ ]+) 404 Not Found\r\nContent-Type: text/plain\r\n\r\n$$");

        if (whoIsNotFound.IsMatch(response) || otherNotFound.IsMatch(response))
        {
            Console.WriteLine("ERROR: no entries found");
        }
        else if (nameHTTPResponse.IsMatch(response))
        {
            recievedLocation = nameHTTPResponse.Match(response).Groups[2].Value;
            Console.WriteLine(String.Format("{0} is {1}", name, recievedLocation));
        }
        else if (locHTTPResponse.IsMatch(response))
        {
            Console.WriteLine(String.Format("{0} location changed to be {1}", name, location));
        }
        else if (locWhoIsResponse.IsMatch(response) && location != null)
        {
            Console.WriteLine(String.Format("{0} location changed to be {1}", name, location));
        }
        else if (nameWhoIsResponse.IsMatch(response))
        {
            recievedLocation = nameWhoIsResponse.Match(response).Groups[1].Value;
            Console.WriteLine(String.Format("{0} is {1}", name, recievedLocation));
        }
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
            else if (args[i] == "/t")
            {
                client.ReceiveTimeout = int.Parse(args[++i]);
                i++;
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
            else if (nameSet == false)
            {
                name = args[i];
                nameSet = true;
            }
            else
            {
                location = args[i];
                locLen = location.Length;
            }
        }
    }
}