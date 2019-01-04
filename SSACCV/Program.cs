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
		string sampleCroppedData = GetLocalPath(@"Data\Sample\croppedSample1mSmol.csv");
		string staypointOutput = GetLocalPath(@"Data\Output\Staypoints.csv");
		string pathOutput = GetLocalPath(@"Data\Output\Paths.csv");

		CsvManager csvManager = new CsvManager();
		ParticipantManager pManager = new ParticipantManager();

		Task<List<DataPoint>> read = csvManager.ReadAsync(sampleCroppedData);
		read.Wait();

		List<DataPoint> points = read.Result;

		pManager.AddDataPoints(points);

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

public static partial class Extensions
{
	#region Shuffle list

	static Random rng = new Random();

	/// <summary>
	/// https://stackoverflow.com/questions/273313/randomize-a-listt
	/// User: grenade
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	#endregion
}