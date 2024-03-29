﻿using CsvHelper.Configuration.Attributes;
using System;

/// <summary>
/// The information that actually gets written out to the csv file.
/// Each object represents one restpoint group (a specific period of time stayed within the defined area).
/// Groups within the same restpoint share location information (Lat + Lon), but have different group centroid locations.
/// </summary>
public class RestpointOutput : RestpointOutputBase, IComparable<RestpointOutput>
{
	#region Fields

	[Name("User_ID")]
	public int UserID { get; set; }

	[Name("Restpoint_ID")]
	public int RestpointID { get; set; }

	[Name("Restpoint_Group_ID")]
	public int RestpointGroupID { get; set; }

	[Name("Group_Centroid_Lat")]
	public decimal GroupCentroidLat { get; set; }

	[Name("Group_Centroid_Lon")]
	public decimal GroupCentroidLon { get; set; }

	#endregion

	/// <summary>
	/// Used for sorting.
	/// Sorts on user ID, then respoint ID, then restpoint group ID, then start date.
	/// </summary>
	/// <param name="other">The restpoint we are comparing against</param>
	/// <returns>An integer with the comparison result</returns>
	public int CompareTo(RestpointOutput other)
	{
		int val = UserID.CompareTo(other.UserID);

		if (val == 0)
			val = RestpointID.CompareTo(other.RestpointID);

		if (val == 0)
			val = RestpointGroupID.CompareTo(other.RestpointGroupID);

		if (val == 0)
			val = StartDate.CompareTo(other.StartDate);

		return val;
	}
}

/// <summary>
/// Anonymous base class.
/// The information that actually gets written out to the csv file.
/// Each object represents one restpoint (a specific period of time stayed within the defined area).
/// </summary>
public class RestpointOutputBase : IComparable<RestpointOutputBase>
{
	#region Fields

	[Name("Loct_Start")]
	public DateTime StartDate { get; set; }

	[Name("Loct_End")]
	public DateTime EndDate { get; set; }

	[Name("Academic_Day_Start")]
	public string AcademicDayStart { get; set; }

	[Name("Academic_Day_End")]
	public string AcademicDayEnd { get; set; }

	[Name("Duration_minutes")]
	public float StayDurationMinutes { get; set; }

	[Name("Building_ID")]
	public string BuildingID { get; set; }

	[Name("Building_Name")]
	public string BuildingName { get; set; }

	[Name("Lat")]
	public decimal Lat { get; set; }

	[Name("Lon")]
	public decimal Lon { get; set; }

	[Name("Centroid_Lat")]
	public decimal CentroidLat { get; set; }

	[Name("Centroid_Lon")]
	public decimal CentroidLon { get; set; }

	[Name("Max_Temp_C")]
	public float MaxTemp { get; set; }

	[Name("Mean_Temp_C")]
	public float MeanTemp { get; set; }

	[Name("Total_Precip_mm")]
	public float TotalPrecip { get; set; }

	[Name("Snow_cm")]
	public int Snow { get; set; }

	[Name("Q_Score")]
	public float QuantityScore { get; set; }

	[Name("T_Score")]
	public float TemporalScore { get; set; }

	[Name("A_Score")]
	public float AccuracyScore { get; set; }

	[Name("Combined_Score")]
	public float CombinedScore { get; set; }

	#endregion

	/// <summary>
	/// Used for sorting.
	/// Sorts on start date.
	/// </summary>
	/// <param name="other">The restpoint we are comparing against</param>
	/// <returns>An integer with the comparison result</returns>
	public int CompareTo(RestpointOutputBase other)
	{
		return StartDate.CompareTo(other.StartDate);
	}
}
