using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Log
{
    private string filePath = null;
    private bool logEnabled = false;
    private string host;
    private string request;
    private string response;

    private static readonly object locker = new object();

    public Log(string filePath, bool logEnabled)
    {
        this.filePath = filePath;
        this.logEnabled = logEnabled;
    }

    public void setHost(string hostToSet)
    {
        host = hostToSet;
    }

    public void setRequest(string reqToSet)
    {
        request = reqToSet;
    }

    public void setResponse(string respToSet)
    {
        response = respToSet;
    }

    public void SetLog(bool logEnable, string setpath)
    {
        logEnabled = logEnable;
        filePath = setpath;
    }

    public void WriteToFile()
    {
        if (logEnabled == true)
        {
            if (filePath == null)
                return;

            String line = host + " - - " + DateTime.Now.ToString() + " " 
                        + request + " " + response;

            lock (locker)
            {
                StreamWriter writer;
                writer = File.AppendText(filePath);
                writer.WriteLine(line);
                writer.Close();
            }
        }
    }
}
