using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Path information to be written to the csv file.
/// Each object represents a point on a path, grouped by <see cref="PathOutput.PathID"/>.
/// </summary>
public class PathOutput : IComparable<PathOutput>
{
	#region Fields

	[Name("User_ID")]
	public int UserID { get; set; }

	[Name("Path_ID")]
	public int PathID { get; set; }

	[Name("Path_Point_ID")]
	public int PathPointID { get; set; }

	[Name("Loct")]
	public DateTime Date { get; set; }

	[Name("Academic_Day")]
	public string AcademicDay { get; set; }

	[Name("Building_ID")]
	public string BuildingID { get; set; }

	[Name("Building_Name")]
	public string BuildingName { get; set; }

	[Name("Lat")]
	public decimal Lat { get; set; }

	[Name("Lon")]
	public decimal Lon { get; set; }

	[Name("Distance_To_Next")]
	public double DistanceToNextPoint { get; set; }

	[Name("Minutes_To_Next")]
	public double MinutesToNextPoint { get; set; }

	[Name("Max_Temp_C")]
	public double MaxTemp { get; set; }

	[Name("Mean_Temp_C")]
	public double MeanTemp { get; set; }

	[Name("Total_Precip_mm")]
	public double TotalPrecip { get; set; }

	[Name("Snow_cm")]
	public int Snow { get; set; }

	[Name("Azimuth_Path")]
	public double AzimuthPath { get; set; }

	[Name("Azimuth_Segment")]
	public double AzimuthSegment { get; set; }

	[Name("Speed")]
	public double Speed { get; set; }

	#endregion

	public int CompareTo(PathOutput other)
	{
		int val = UserID.CompareTo(other.UserID);

		if (val == 0)
			val = PathID.CompareTo(other.PathID);

		if (val == 0)
			val = PathPointID.CompareTo(other.PathPointID);

		return val;
	}
}
