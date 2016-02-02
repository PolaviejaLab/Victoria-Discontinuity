using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using UnityOSC;

public class Logger : MonoBehaviour 
{
	StreamWriter writer;
    public OSCClient oscClient;

    public Logger()
    {
        oscClient = new OSCClient(IPAddress.Parse("127.0.0.1"), 4567);
    }

	public void OpenLog(string filename)
	{
		writer = new StreamWriter(filename, true);
		Write("Logger\tStarted logging");
	}
	
	public void CloseLog()
	{
		Write("Logger\tStopped logging");
		writer.Close();
	}

	public void Write(string message)
	{
		if(writer != null) {
			writer.WriteLine(DateTime.UtcNow.ToString("o") + "\t" + message);
			writer.Flush();
		}
        
        if(oscClient != null) {
            OSCMessage packet = new OSCMessage("/", message);
            oscClient.Send(packet);
        }
	}

}
