using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// The information that actually gets written out to the csv file.
/// Each object represents one staypoint group (a specific period of time stayed within the defined area).
/// Groups within the same staypoint share location information.
/// </summary>
public class StaypointOutput : IComparable<StaypointOutput>
{
	#region Fields

	[Name("User_ID")]
	public int UserID { get; set; }

	[Name("Staypoint_ID")]
	public int StaypointID { get; set; }

	[Name("Staypoint_Group_ID")]
	public int StaypointGroupID { get; set; }

	[Name("Loct_Start")]
	public DateTime StartDate { get; set; }

	[Name("Loct_End")]
	public DateTime EndDate { get; set; }

	[Name("Academic_Day_Start")]
	public string AcademicDayStart { get; set; }

	[Name("Academic_Day_End")]
	public string AcademicDayEnd { get; set; }

	[Name("Duration_minutes")]
	public double StayDurationMinutes { get; set; }

	[Name("Building_ID")]
	public string BuildingID { get; set; }

	[Name("Building_Name")]
	public string BuildingName { get; set; }

	[Name("Lat")]
	public decimal Lat { get; set; }

	[Name("Lon")]
	public decimal Lon { get; set; }

	[Name("YYC_X")]
	public decimal YYC_X { get; set; }

	[Name("YYC_Y")]
	public decimal YYC_Y { get; set; }

	[Name("Max_Temp_C")]
	public double MaxTemp { get; set; }

	[Name("Mean_Temp_C")]
	public double MeanTemp { get; set; }

	[Name("Total_Precip_mm")]
	public double TotalPrecip { get; set; }

	[Name("Snow_cm")]
	public int Snow { get; set; }

	[Name("Q_Score")]
	public double QuantityScore { get; set; }

	[Name("T_Score")]
	public double TemporalScore { get; set; }

	[Name("A_Score")]
	public double AccuracyScore { get; set; }

	[Name("Combined_Score")]
	public double CombinedScore { get; set; }

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
