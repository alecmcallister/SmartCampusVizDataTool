﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Manages reading from/ writing to .csv files
/// </summary>
public class CsvManager
{
	#region Read

	/// <summary>
	/// Reads the .csv at the given filepath.
	/// Must be mapped properly within <see cref="DataPointMap"/>.
	/// </summary>
	/// <param name="filepath">The .csv path (full)</param>
	/// <returns>The .csv read into a list of <see cref="DataPoint"/> objects</returns>
	public async Task<List<DataPoint>> ReadAsync(string filepath)
	{
		Console.WriteLine("Loading dataset:");
		WritePathToConsole(filepath);

		DateTime loadStart = DateTime.Now;

		await Task.Yield();

		List<DataPoint> dataPoints = new List<DataPoint>();

		using (StreamReader sr = new StreamReader(new FileStream(filepath, FileMode.Open)))
		{
			using (CsvReader csv = new CsvReader(sr))
			{
				csv.Configuration.RegisterClassMap(new DataPointMap());
				await csv.ReadAsync();
				csv.ReadHeader();
				dataPoints = csv.GetRecords<DataPoint>().ToList();
			}
		}

		dataPoints.Sort();

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("\tComplete\n\t{0:0.00}s\n", (DateTime.Now - loadStart).TotalSeconds);
		Console.ResetColor();

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
		Console.WriteLine("Writing {0}:", typeof(T).Name);
		WritePathToConsole(filepath);

		DateTime loadStart = DateTime.Now;

		using (StreamWriter sr = new StreamWriter(new FileStream(filepath, FileMode.Create)))
		{
			using (CsvWriter csv = new CsvWriter(sr))
			{
				csv.WriteHeader<T>();
				csv.NextRecord();
				csv.WriteRecords(output);
			}
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("\tComplete\n\t{0:0.00}s\n", (DateTime.Now - loadStart).TotalSeconds);
		Console.ResetColor();
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