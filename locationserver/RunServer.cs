using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    class RunServer
    {
        static void Main(string[] args)
        {
            //Make a new server instance.
            LocationServer server = new LocationServer();
            server.runServer(args);   
        }
    }
}
