using CsvHelper.Configuration.Attributes;
using System;

/// <summary>
/// The information that actually gets written out to the csv file.
/// Each object represents one staypoint group (a specific period of time stayed within the defined area).
/// Groups within the same staypoint share location information (Lat + Lon), but have different group centroid locations.
/// </summary>
public class StaypointOutput : StaypointOutputBase, IComparable<StaypointOutput>
{
	#region Fields

	[Name("User_ID")]
	public int UserID { get; set; }

	[Name("Staypoint_ID")]
	public int StaypointID { get; set; }

	[Name("Staypoint_Group_ID")]
	public int StaypointGroupID { get; set; }

	[Name("Group_Centroid_Lat")]
	public decimal GroupCentroidLat { get; set; }

	[Name("Group_Centroid_Lon")]
	public decimal GroupCentroidLon { get; set; }

	#endregion

	public int CompareTo(StaypointOutput other)
	{
		int val = UserID.CompareTo(other.UserID);

		if (val == 0)
			val = StaypointID.CompareTo(other.StaypointID);

		if (val == 0)
			val = StaypointGroupID.CompareTo(other.StaypointGroupID);

		if (val == 0)
			val = StartDate.CompareTo(other.StartDate);

		return val;
	}
}

/// <summary>
/// Anonymous base class.
/// The information that actually gets written out to the csv file.
/// Each object represents one staypoint (a specific period of time stayed within the defined area).
/// </summary>
public class StaypointOutputBase : IComparable<StaypointOutputBase>
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

	public int CompareTo(StaypointOutputBase other)
	{
		return StartDate.CompareTo(other.StartDate);
	}
}
