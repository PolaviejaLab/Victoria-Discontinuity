using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class Logger : MonoBehaviour 
{
	StreamWriter writer;

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
	}

}
