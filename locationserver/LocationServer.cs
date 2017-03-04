using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
public class LocationServer
{
    DatabaseManagement database;
    Log log = new Log(null, false);

    private string databaseFilePath = null;
    private bool setDebugger = false;

    public LocationServer()
    {
        database = new DatabaseManagement();
    }

    private void SplitArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "/d")
            {
                setDebugger = true;
            }
            else if (args[i] == "/l")
            {
                string filepath = args[++i];
                log.SetLog(true, filepath);
            }
            else if (args[i] == "/f")
            {
                try
                {
                    databaseFilePath = args[++i];
                    database.setFilePath(databaseFilePath);
                    database.LoadDatabase();
                }
                catch
                {
                    Console.WriteLine("No file specified!");
                }
            }
        }
    }

    public void runServer(string[] args)
    {
        TcpListener listener;
        RequestHandler Handler;

        SplitArgs(args);

        try
        {
            listener = new TcpListener(IPAddress.Any, 43); //Creates server socket on port. (43 in this case...)
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                log.setHost(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

                Handler = new RequestHandler(database, log, setDebugger);

                Thread thread = new Thread(() => Handler.doRequest(client));
                thread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Connection Failed");
            Console.WriteLine(e.ToString());
        }
    }
}

public class RequestHandler
{
    Debug debugger = new Debug(false);
    DatabaseManagement database;
    Log logger;

    private enum protocol { whois, h1, h9, h0 };
    private protocol activeProtocol = protocol.whois;

    private enum requestType { lookup, changeLocation }
    private requestType request;

    private static readonly object locker = new object();

    private string username = null;
    private string location = null;
    private string response = "";

    public RequestHandler(DatabaseManagement serverDatabase, Log logToSet, bool setDebugger)
    {
        database = serverDatabase;
        debugger.SetDebug(setDebugger);
        logger = logToSet;
    }

    public void doRequest(TcpClient connection)
    {
        NetworkStream socketStream = connection.GetStream();
        StreamWriter sw = new StreamWriter(socketStream);
        StreamReader sr = new StreamReader(socketStream);

        socketStream.ReadTimeout = 1000;
        logger.setResponse("OK");

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Connected.");
        Console.BackgroundColor = ConsoleColor.Black;

        string input = GetReaderData(sr, connection, socketStream);

        if (input == null)
        {
            Console.WriteLine("TIMED OUT READING DATA");
            return;
        }

        if (!RegexInputChecking(input))
            return;

        debugger.ActualInputMsg(input);
        debugger.RequestMsg(username, request.ToString());

        WriteResponse(request);

        sw.Write(response);

        sw.Flush();

        debugger.OutputMsg();
        logger.WriteToFile();

        socketStream.Close();
        connection.Close();

        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine("Connection Closed.");
        Console.BackgroundColor = ConsoleColor.Black;
    }

    private void WriteResponse(requestType request)
    {
        if (request == requestType.lookup)
        {
            LookupNameResponse();
            logger.setRequest(string.Format("GET \"{0}\"", username));
        }
        else if (request == requestType.changeLocation)
        {
            ChangeLocationResponse();
            database.SaveDatabse();

            debugger.ChangeResponseMsg(username, location);
            logger.setRequest(string.Format("PUT \"{0} {1}\"", username, location));
        }
    }

    private string GetReaderData(StreamReader sr, TcpClient hostConnection, NetworkStream socketStream)
    {
        string input = "";
        char[] characters = new char[1000];

        do
        {
            try
            {
                int numFromStream = sr.Read(characters, 0, characters.Length);

                for (int i = 0; i < numFromStream; i++)
                {
                    input += characters[i];
                }
            }
            catch
            {
                if (input == "")
                {
                    return null;
                }
                Console.WriteLine("Timout");
                break;
            }
        } while (hostConnection.Connected && socketStream.DataAvailable);

        return input;
    }

    private void LookupNameResponse()
    {
        //Looks up a person and returns the location of that person into personsLocation.
        string personsLocation = database.lookupDatabase(username);

        if (personsLocation != null)
        {
            if (activeProtocol == protocol.whois)
            {
                response = string.Format("{0}\r\n", personsLocation);
            }
            else if (activeProtocol == protocol.h9)
            {
                response = string.Format("HTTP/0.9 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation);

            }
            else if (activeProtocol == protocol.h0)
            {
                response = string.Format("HTTP/1.0 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation);
            }
            else if (activeProtocol == protocol.h1)
            {
                response = string.Format("HTTP/1.1 200 OK\r\nContent-Type: "
                                     + "text/plain\r\n\r\n{0}\r\n", personsLocation);
            }
        }
        else if (personsLocation == null)
        {
            if (activeProtocol == protocol.whois)
            {
                response = "ERROR: no entries found\r\n";
            }
            else if (activeProtocol == protocol.h9)
            {
                response = "HTTP/0.9 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }
            else if (activeProtocol == protocol.h0)
            {
                response = "HTTP/1.0 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }
            else if (activeProtocol == protocol.h1)
            {
                response = "HTTP/1.1 404 Not Found\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }

            logger.setResponse("RESPONSE UNKNOWN");
        }
        debugger.LookupResponseMsg(personsLocation);
    }

    private void ChangeLocationResponse()
    {
        if (database.changeLocation(username, location))
        {
            if (activeProtocol == protocol.whois)
            {
                response = "OK\r\n";
            }
            else if (activeProtocol == protocol.h9)
            {
                response = "HTTP/0.9 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }
            else if (activeProtocol == protocol.h0)
            {
                response = "HTTP/1.0 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }
            else if (activeProtocol == protocol.h1)
            {
                response = "HTTP/1.1 200 OK\r\nContent-Type: "
                       + "text/plain\r\n\r\n";
            }
            else
            {
                logger.setResponse("RESPONSE UNKNOWN");
            }
            return;
        }
        else if (!database.changeLocation(username, location))
        {
            database.Add(username, location);
            ChangeLocationResponse();
        }
    }

    private bool RegexInputChecking(string input)
    {
        Regex nameH9 = new Regex(@"^GET /(.*)\r\n$");
        Regex locationH9 = new Regex(@"^PUT /(.*)\r\n\r\n(.*)\r\n$");

        Regex nameH0 = new Regex(@"^GET /\?(.*) HTTP/1.0\r\n(.*)\r\n$");
        Regex locationH0 = new Regex(@"^POST /(.*) HTTP/1.0\r\n"
                                   + @"Content-Length: (\d+)\r\n(.*)\r\n(.*)$");

        Regex nameH1 = new Regex(@"^GET \/\?name=(.*) HTTP/1.1\r\n.*Host:"
                                + @" (.*)\r\n(.*)\r\n$");
        Regex locationH1 = new Regex(@"^POST / HTTP/1.1\r\nHost: (.*)\r\n"
                                    + @"Content-Length: (\d+)\r\n(.*)\r\n"
                                    + @"name=(.*)&location=(.*)$");

        Regex nameWhoIs = new Regex(@"^(.*)\r\n$");
        Regex locationWhoIs = new Regex(@"^([^ ]*) (.*)\r\n$");

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
        else
        {
            Console.WriteLine("Unrecognised Request");
            logger.setResponse("UNKNOWN REQUEST");
        }

        if (username == null)
        {
            return false;
        }
        return true;
    }
}
