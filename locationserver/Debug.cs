using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Debug
{
    private bool debugEnabled;
    private string debugLine = "";

    public Debug(bool debugEnable)
    {
        this.debugEnabled = debugEnable;
    }

    public void SetDebug(bool debugEnable)
    {
        debugEnabled = debugEnable;
    }

    public void RequestMsg(string nameRequested, string requestType)
    {
        if (debugEnabled)
        {
            debugLine += (string.Format("Request Type: {0}\nRequest Data: {1}\n",requestType, nameRequested));
        }
    }

    public void LookupResponseMsg(string locationResponded)
    {
        if (debugEnabled)
        {
            if (locationResponded == null)
                debugLine += "Responded: Person not found in database\n";

            else debugLine += string.Format("Responded: {0}", locationResponded);
        }        
    }

    public void ChangeResponseMsg(string nameRequested,string locationResponded)
    {
        if (debugEnabled)
            debugLine += string.Format("Responded: {0} changed to be {1}", 
                                             nameRequested, locationResponded);
    }

    public void ActualInputMsg(string input)
    {
        debugLine += string.Format("Actual Request:\n{0}\n", input);
    }

    public void ActualResponseMsg(string response)
    {
        debugLine += string.Format("Actual Response:\n\n{0}\n", response);
    }

    public void OutputMsg()
    {
        Console.WriteLine(debugLine);
    }
}

