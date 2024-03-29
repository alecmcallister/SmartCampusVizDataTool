﻿using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a row of data belonging to a user
/// </summary>
public class DataPoint : IComparable<DataPoint>
{
	#region Fields/ Columns

	[Name("User_ID")]
	public int userid { get; set; }

	[Name("Accuracy")]
	public int accuracy { get; set; }

	[Name("Loct"), TypeConverter(typeof(DateConverter))]
	public DateTime loct { get; set; }

	[Name("Academic_Day")]
	public string academic_day { get; set; }

	[Name("Building_ID")]
	public string building_id { get; set; }

	[Name("Building_Name")]
	public string building_name { get; set; }

	[Name("Lat"), TypeConverter(typeof(DecimalConvert))]
	public decimal lat { get; set; }

	[Name("Lon"), TypeConverter(typeof(DecimalConvert))]
	public decimal lon { get; set; }

	[Name("Distance"), TypeConverter(typeof(DoubleConvert))]
	public double distance { get; set; }

	[Name("Speed"), TypeConverter(typeof(DoubleConvert))]
	public double speed { get; set; }

	[Name("Max_Temp_C"), TypeConverter(typeof(DoubleConvert))]
	public double max_temp { get; set; }

	[Name("Mean_Temp_C"), TypeConverter(typeof(DoubleConvert))]
	public double mean_temp { get; set; }

	[Name("Total_Precip_mm"), TypeConverter(typeof(DoubleConvert))]
	public double total_precip { get; set; }

	[Name("Snow_cm")]
	public int snow { get; set; }

	#endregion

	[Ignore]
	public Vector2 location => new Vector2(lat, lon);

	#region Compare

	/// <summary>
	/// Compare ID, and start date
	/// </summary>
	/// <param name="other">The point to compare against</param>
	/// <returns>If different ID: 1 if higher than other, -1 if lower than other. If same ID: </returns>
	public int CompareTo(DataPoint other)
	{
		if (other == null)
			return 1;

		if (userid != other.userid)
			return userid.CompareTo(other.userid);

		else
			return DateTime.Compare(loct, other.loct);
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Get the time period between this point, and <paramref name="b"/>
	/// </summary>
	/// <param name="b">End point (later time)</param>
	/// <returns>The total time (in minutes) between a.datetime_start and b.datetime_start</returns>
	public double TimeDifference(DataPoint b)
	{
		return (b.loct - loct).TotalMinutes;
	}

	/// <summary>
	/// Get the distance to <paramref name="b"/>
	/// </summary>
	/// <param name="b">The other point</param>
	/// <returns>The distance between the two points (in meters probably)</returns>
	public double DistanceTo(DataPoint b)
	{
		return Vector2.AzimuthDistance(location, b.location);
	}

	#endregion
}
