using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CommunityStaypointOutput : IComparable<CommunityStaypointOutput>
{
	#region Fields

	[Name("CommunityStaypoint_ID")]
	public int CommunityStaypointID { get; set; }

	[Name("CommunityStaypoint_Group_ID")]
	public int CommunityStaypointGroupID { get; set; }

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

	//[Name("Building_ID")]
	//public string BuildingID { get; set; }

	//[Name("Building_Name")]
	//public string BuildingName { get; set; }

	[Name("Lat")]
	public decimal Lat { get; set; }

	[Name("Lon")]
	public decimal Lon { get; set; }

	//[Name("Max_Temp_C")]
	//public double MaxTemp { get; set; }

	//[Name("Mean_Temp_C")]
	//public double MeanTemp { get; set; }

	//[Name("Total_Precip_mm")]
	//public double TotalPrecip { get; set; }

	//[Name("Snow_cm")]
	//public int Snow { get; set; }

	//[Name("Q_Score")]
	//public double QuantityScore { get; set; }

	//[Name("T_Score")]
	//public double TemporalScore { get; set; }

	//[Name("A_Score")]
	//public double AccuracyScore { get; set; }

	[Name("Combined_Score")]
	public double CombinedScore { get; set; }

	[Name("Centroid_Lat")]
	public decimal CentroidLat { get; set; }

	[Name("Centroid_Lon")]
	public decimal CentroidLon { get; set; }

	//[Name("Group_Centroid_Lat")]
	//public decimal GroupCentroidLat { get; set; }

	//[Name("Group_Centroid_Lon")]
	//public decimal GroupCentroidLon { get; set; }

	#endregion

	public int CompareTo(CommunityStaypointOutput other)
	{
		int val = CommunityStaypointID.CompareTo(other.CommunityStaypointID);

		if (val == 0)
			val = CommunityStaypointGroupID.CompareTo(other.CommunityStaypointGroupID);

		if (val == 0)
			val = StartDate.CompareTo(other.StartDate);

		return val;
	}
}