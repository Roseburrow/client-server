//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Net;

public class Program
{
    static void Main(string[] args)
    {
        //Makes a new client.
        Client location = new Client();
        location.runClient(args);
        Console.ReadLine();
    }
}