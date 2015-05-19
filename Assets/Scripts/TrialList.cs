using System.Collections.Generic;
using System.IO;

public class TrialList
{
	private Queue<Dictionary<string, string>> trials;


	public TrialList(string filename)
	{
		StreamReader reader = new StreamReader(filename);		
		string[] header = reader.ReadLine().Split(',');
		
		trials = new Queue<Dictionary<string, string>>();
		
		while(!reader.EndOfStream)
		{
			string line = reader.ReadLine();
			string[] columns = line.Split(',');
			
			Dictionary<string, string> trial = new Dictionary<string, string>();
			
			for(int i = 0; i < columns.Length; i++) {
				trial[header[i].Trim ()] = columns[i].Trim();
			}
			
			trials.Enqueue(trial);
		}		
	}
	
	
	public bool HasMore()
	{
		return trials.Count != 0;
	}
	
	
	public int Count()
	{
		return trials.Count;
	}
	

	public Dictionary<string, string> Pop()
	{
		return trials.Dequeue();
	}
}
