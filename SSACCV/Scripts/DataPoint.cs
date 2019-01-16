using CsvHelper;
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

	[Name("YYC_X"), TypeConverter(typeof(DecimalConvert))]
	public decimal yyc_x { get; set; }

	[Name("YYC_Y"), TypeConverter(typeof(DecimalConvert))]
	public decimal yyc_y { get; set; }

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
	public Vector2 yyc_location => new Vector2(yyc_x, yyc_y);

	[Ignore]
	public Vector2 location => new Vector2(lat, lon);

	[Ignore]
	public int staypointID { get; set; } = -1;

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
	/// Get the time period between points <paramref name="a"/> and <paramref name="b"/>
	/// </summary>
	/// <param name="a">Start point (earlier time)</param>
	/// <param name="b">End point (later time)</param>
	/// <returns>The total time (in minutes) between a.datetime_start and b.datetime_start</returns>
	public static double TimeDifference(DataPoint a, DataPoint b)
	{
		return (b.loct - a.loct).TotalMinutes;
	}

	/// <summary>
	/// Get the distance to <paramref name="b"/>
	/// </summary>
	/// <param name="b">The other point</param>
	/// <returns>The distance between the two points (in meters probably)</returns>
	public decimal DistanceTo(DataPoint b)
	{
		return (b.yyc_location - yyc_location).magnitude;
	}

	#endregion
}
