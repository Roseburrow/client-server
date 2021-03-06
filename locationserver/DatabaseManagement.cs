﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DatabaseManagement
{
    ConcurrentDictionary<string, string> serverDatabase;
    private static readonly object locker = new object();
    private string filePath;

    public DatabaseManagement()
    {
        serverDatabase = new ConcurrentDictionary<string, string>();
    }

    public void setFilePath(string filepath)
    {
        filePath = filepath;
    }

    public void SaveDatabse()
    {
        if (filePath == null)
            return;

        lock (locker)
        {
            StreamWriter writer = new StreamWriter(filePath, false);

            foreach (var entry in serverDatabase)
            {
                string line = string.Format("{0} {1}", entry.Key, entry.Value);
                writer.WriteLine(line);
            }
            writer.Close();
        }
    }

    public void LoadDatabase()
    {
        if (filePath == null || !File.Exists(filePath))
        {
            File.Create(filePath);
        }

        lock (locker)
        {
            StreamReader reader = new StreamReader(filePath);
            string[] lineSplit;
            bool endOfData = false;

            while (!endOfData)
            {
                try
                {
                    lineSplit = reader.ReadLine().Split();

                    string name = lineSplit[0];
                    string location = "";

                    for (int i = 1; i < lineSplit.Length; i++)
                    {
                        location += (lineSplit[i]);

                        if (i != lineSplit.Length - 1)
                            location += " ";
                    }

                    serverDatabase.GetOrAdd(name, location);
                }
                catch
                {
                    endOfData = true;
                }
            }
            reader.Close();
        }
    }

    public void Add(string username, string location)
    {
        serverDatabase.GetOrAdd(username, location);
    }

    public string lookupDatabase(string name)
    {
        try
        {
            if (serverDatabase.ContainsKey(name))
            {
                return serverDatabase[name];
            }
        }
        catch
        {
            Console.WriteLine("Null name requested in dictionary.");
        }
        return null;
    }

    public bool changeLocation(string name, string newLocation)
    {
        if (serverDatabase.ContainsKey(name))
        {
            serverDatabase[name] = newLocation;
            return true;
        }
        return false;
    }
}
