﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
public class LocationServer
{
    Dictionary<string, string> serverDatabase;

    private enum protocol { whois, h1, h9, h0 };
    private protocol activeProtocol = protocol.whois;

    private enum requestType { lookup, changeLocation }
    private requestType request;

    private string username = null;
    private string location = null;

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
        return null;
    }

    private bool changeLocation(string name, string newLocation)
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

                handleRequest(socketStream);

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

    private void handleRequest(NetworkStream socketStream)
    {
        string input = null;

        StreamWriter sw = new StreamWriter(socketStream);
        StreamReader sr = new StreamReader(socketStream);

        socketStream.ReadTimeout = 1000;

        input = GetReaderData(sr);

        RegexInputChecking(input);

        if (request == requestType.lookup)
        {
            sw = LookupNameResponse(sw);
        }
        else if (request == requestType.changeLocation)
        {
            sw = ChangeLocationResponse(sw);
        }

        sw.Flush();
    }

    private string GetReaderData(StreamReader sr)
    {
        string input = "";
        char c;
        bool readFail = false;

        do
        {
            try
            {
                c = (char)sr.Read();
                input += c;
            }
            catch
            {
                readFail = true;
                break;
            }
        }
        while (readFail == false);

        return input;
    }

    private StreamWriter LookupNameResponse(StreamWriter sw)
    {
        //Looks up a person and returns the location of that person into personsLocation.
        string personsLocation = lookupDatabase(username);

        if (personsLocation != null)
        {
            if (activeProtocol == protocol.whois)
            {
                sw.Write(string.Format("{0}\r\n", personsLocation));
            }
            else if (activeProtocol == protocol.h9)
            {
                sw.Write(string.Format("HTTP/0.9 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation));
            }
            else if (activeProtocol == protocol.h0)
            {
                sw.Write(string.Format("HTTP/1.0 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation));
            }
            else if (activeProtocol == protocol.h1)
            {
                sw.Write(string.Format("HTTP/1.1 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation));
            }
        }
        else if (personsLocation == null)
        {
            if (activeProtocol == protocol.whois)
            {
                sw.Write("ERROR: no entries found\r\n");
            }
            else if (activeProtocol == protocol.h9)
            {
                sw.Write("HTTP/0.9 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
            else if (activeProtocol == protocol.h0)
            {
                sw.Write("HTTP/1.0 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
            else if (activeProtocol == protocol.h1)
            {
                sw.Write("HTTP/1.1 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
        }
        return sw;
    }

    private StreamWriter ChangeLocationResponse(StreamWriter sw)
    {
        if (changeLocation(username, location))
        {
            if (activeProtocol == protocol.whois)
            {
                sw.Write("OK\r\n");
            }
            else if (activeProtocol == protocol.h9)
            {
                sw.Write("HTTP/0.9 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
            else if (activeProtocol == protocol.h0)
            {
                sw.Write("HTTP/1.0 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
            else if (activeProtocol == protocol.h1)
            {
                sw.Write("HTTP/1.1 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n");
            }
        }
        else if (!changeLocation(username, location))
        {
            serverDatabase.Add(username, location);
            sw.Write("OK\r\n");
        }
        return sw;
    }

    private void RegexInputChecking(string input)
    {
        Regex nameH9 = new Regex(@"^GET /?(.*)\r\n$");
        Regex locationH9 = new Regex(@"^PUT /(.*)\r\n\r\n(.*)\r\n$");

        Regex nameH0 = new Regex(@"^GET /\?(.*) HTTP/1.0\r\n(.*)\r\n$");
        Regex locationH0 = new Regex(@"^POST /(.*) HTTP/1.0\r\n"
                                   + @"Content-Length: (\d+)\r\n(.*)\r\n(.*)$");

        Regex nameH1 = new Regex(@"^GET \/\?name=(.*) HTTP/1.1\r\nHost:"
                                + @" (.*)\r\n(.*)\r\n$");
        Regex locationH1 = new Regex(@"^POST / HTTP/1.1\r\nHost: (.*)\r\n"
                                    + @"Content-Length: (\d+)\r\n(.*)\r\n"
                                    + @"name=(.*)&location=(.*)$");

        Regex nameWhoIs = new Regex(@"^(.*)\r\n$");
        Regex locationWhoIs = new Regex(@"^([^ ]+) (.*)\r\n$");

        if (locationH9.IsMatch(input))
        {
            activeProtocol = protocol.h9;
            request = requestType.changeLocation;
            username = locationH9.Match(input).Groups[1].Value;
            location = locationH9.Match(input).Groups[2].Value;
        }
        else if (nameH9.IsMatch(input))
        {
            activeProtocol = protocol.h9;
            request = requestType.lookup;
            username = nameH9.Match(input).Groups[1].Value;
        }
        else if (locationH0.IsMatch(input))
        {
            activeProtocol = protocol.h0;
            request = requestType.changeLocation;
            username = locationH0.Match(input).Groups[1].Value;
            location = locationH0.Match(input).Groups[4].Value;
        }
        else if (nameH0.IsMatch(input))
        {
            activeProtocol = protocol.h0;
            request = requestType.lookup;
            username = nameH0.Match(input).Groups[1].Value;
        }
        else if (locationH1.IsMatch(input))
        {
            activeProtocol = protocol.h1;
            request = requestType.changeLocation;
            username = locationH1.Match(input).Groups[4].Value;
            location = locationH1.Match(input).Groups[5].Value;
        }
        else if (nameH1.IsMatch(input))
        {
            activeProtocol = protocol.h1;
            request = requestType.lookup;
            username = nameH1.Match(input).Groups[1].Value;
        }
        else if (locationWhoIs.IsMatch(input))
        {
            activeProtocol = protocol.whois;
            request = requestType.changeLocation;
            username = locationWhoIs.Match(input).Groups[1].Value;
            location = locationWhoIs.Match(input).Groups[2].Value;
        }
        else if (nameWhoIs.IsMatch(input))
        {
            activeProtocol = protocol.whois;
            request = requestType.lookup;
            username = nameWhoIs.Match(input).Groups[1].Value;
        }
    }
}
