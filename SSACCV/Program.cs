using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
		string sampleCroppedData = GetLocalPath(@"Data\Sample\croppedSample1mSmol.csv");
		string masterData = GetLocalPath(@"Data\Sample\CroppedMaster_Jan14.csv");

		string staypointOutput = GetLocalPath(@"Data\Output\Staypoints_" + DateTime.Today.Date.ToString("MMMdd") + ".csv");
		string pathOutput = GetLocalPath(@"Data\Output\Paths_" + DateTime.Today.Date.ToString("MMMdd") + ".csv");

		CsvManager csvManager = new CsvManager();
		ParticipantManager pManager = new ParticipantManager();

		Task<List<DataPoint>> read = csvManager.ReadAsync(masterData);
		read.Wait();

		List<DataPoint> points = read.Result;

		pManager.AddDataPoints(points);

		//csvManager.Write(staypointOutput, pManager.GetStayPointOutput());

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
