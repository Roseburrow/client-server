using System;
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
    private int nameLen;
    private int timeout = 1000;

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
            try
            {
                //Try to split the arguments into the data needed...
                SplitArgs(args);
            }
            catch
            {
                Console.WriteLine("One or more arguments were entered incorrectly. Double check your inputs...");
            }

            try
            {
                client.Connect(ip, port); //creates a socket connecting the local host to the named host and port.
                Request();
            }
            catch
            {
                Console.WriteLine(string.Format("Could not connect to server. IP: {0}  PORT: {1}", ip, port));
            }
        }
    }

    private void Request()
    {
        NetworkStream dataStream = client.GetStream();
        StreamWriter sw = new StreamWriter(dataStream);
        StreamReader sr = new StreamReader(dataStream);

        SetTimeout(dataStream);

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
            {
                int contentLen = locLen + nameLen + 15;
                sw.Write(string.Format("POST / HTTP/1.1\r\nHost: {0}\r\nContent-Length: "
                                     + "{1}\r\n\r\nname={2}&location={3}",
                                        ip, contentLen, name, location));
            }
        }

        //flushes the request, sending it to the server.
        sw.Flush();

        /*This is required because there is a read timeout for the client and the server.
          It ensures that the response has been recieved before trying to read the response.*/
        if (port != 80)
        {
            Thread.Sleep(2000);
        }

        string response = GetResponse(sr);
        //If it is a request to the internet print it in the correct format.

        if (port == 80)
            Console.WriteLine("{0} is {1}", name, response);

        else ReadResponse(response);
    }

    private void SetTimeout(NetworkStream dataStream)
    {
        if (timeout != 0)
            dataStream.ReadTimeout = timeout;

        else dataStream.ReadTimeout = Timeout.Infinite;
    }

    private string GetResponse(StreamReader sr)
    {
        string response = "";
        char[] characters = new char[3000];

        for (int i = 0; i < characters.Length; i++)
        {
            try
            {
                int numFromStream = sr.Read(characters, i, 1);

                if (numFromStream == 0)
                    break;
                else response += characters[i];
            }
            catch
            {
                Console.WriteLine("TIMED OUT");
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
        bool locationSet = false;

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
                timeout = int.Parse(args[i + 1]);
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
            else if (!nameSet)
            {
                name = args[i];
                nameLen = name.Length;
                nameSet = true;
            }
            else if (nameSet && !locationSet)
            {
                location = args[i];
                locLen = location.Length;
                locationSet = true;
            }
            else
            {
                //Do Nothing...
            }
        }
    }
}
