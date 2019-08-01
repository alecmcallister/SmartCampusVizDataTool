using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Manages reading from/ writing to .csv files
/// </summary>
public class CsvManager
{
	public static event Action<bool> BusyChanged = new Action<bool>(b => { });
	static bool _busy;
	public static bool Busy { get { return _busy; } set { if (_busy != value) { _busy = value; BusyChanged(value); } } }

	#region Read

	/// <summary>
	/// Reads the .csv at the given filepath.
	/// Must be mapped properly within <see cref="DataPointMap"/>.
	/// </summary>
	/// <param name="filepath">The .csv path (full)</param>
	/// <returns>The .csv read into a list of <see cref="DataPoint"/> objects</returns>
	public async Task<List<DataPoint>> ReadAsync(string filepath)
	{
		Busy = true;

		ConsoleLog.LogStart("Loading dataset:");
		WritePathToConsole(filepath);
		Console.WriteLine("This may take a while... (~1-2 minutes)");
		List<DataPoint> dataPoints = new List<DataPoint>();

		try
		{
			using (StreamReader sr = new StreamReader(new FileStream(filepath, FileMode.Open)))
			{
				using (CsvReader csv = new CsvReader(sr))
				{
					csv.Read();
					csv.ReadHeader();
					dataPoints = csv.GetRecords<DataPoint>().ToList();
				}
			}

			dataPoints.Sort();
		}
		catch
		{
			Console.Error.WriteLine("Encountered an error while loading data.\nPlease try again with another dataset.");
		}

		ConsoleLog.LogStop();

		Busy = false;

		return dataPoints;
	}

	#endregion

	#region Write

	/// <summary>
	/// Writes a list of objects to a .csv at the given filepath.
	/// Overwrites the file if it already exists.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="filepath">The .csv path (full)</param>
	/// <param name="output">The list of (T)objects to write</param>
	public void Write<T>(string filepath, List<T> output)
	{
		if (output == null)
			return;

		Busy = true;

		ConsoleLog.LogStart(string.Format("Writing {0}:", typeof(T).Name));
		WritePathToConsole(filepath);

		using (StreamWriter sr = new StreamWriter(new FileStream(filepath, FileMode.Create)))
		{
			using (CsvWriter csv = new CsvWriter(sr))
			{
				csv.WriteHeader<T>();
				csv.NextRecord();
				csv.WriteRecords(output);
			}
		}

		ConsoleLog.LogStop();

		Busy = false;
	}

	#endregion

	#region Pretty colors

	/// <summary>
	/// Used for pretty colors.
	/// </summary>
	/// <param name="path">The thing</param>
	public void WritePathToConsole(string path)
	{
		Console.ForegroundColor = ConsoleColor.Magenta;
		Console.WriteLine(path);
		Console.ResetColor();
	}

	#endregion
}
