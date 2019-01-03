using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Program
{
	static void Main(string[] args)
	{
		Console.CursorVisible = false;

		string sampleData = GetLocalPath(@"Data\Sample\sample1m.csv");
		string staypointOutput = GetLocalPath(@"Data\Output\Staypoints_" + DateTime.Now.ToShortDateString() + ".csv");
		string pathOutput = GetLocalPath(@"Data\Output\Paths_" + DateTime.Now.ToShortDateString() + ".csv");

		CsvManager csvManager = new CsvManager();
		ParticipantManager pManager = new ParticipantManager();

		Task<List<DataPoint>> read = csvManager.ReadAsync(sampleData);
		read.Wait();

		pManager.AddDataPoints(read.Result);

		csvManager.Write(staypointOutput, pManager.GetStayPointOutput());

		csvManager.Write(pathOutput, pManager.GetPathOutput());

		Console.CursorVisible = true;
		Console.ReadLine();
	}

	static string GetLocalPath(string path)
	{
		return
			System.IO.Path.GetDirectoryName(
			System.IO.Path.GetDirectoryName(
			System.IO.Path.GetDirectoryName(
			AppDomain.CurrentDomain.BaseDirectory))) + @"\" + path;
	}
}
