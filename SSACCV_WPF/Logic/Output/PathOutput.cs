using CsvHelper.Configuration.Attributes;
using System;

/// <summary>
/// Path information to be written to the csv file.
/// Each object represents a point on a path, grouped by <see cref="PathOutput.PathID"/>.
/// </summary>
public class PathOutput : PathOutput_Base, IComparable<PathOutput>
{
	#region Fields

	[Name("User_ID")]
	public int UserID { get; set; }

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

/// <summary>
/// Anonymous base class.
/// Path information to be written to the csv file.
/// Each object represents a point on a path, grouped by <see cref="PathOutput.PathID"/>.
/// </summary>
public class PathOutput_Base : IComparable<PathOutput_Base>
{
	#region Fields

	[Name("Path_ID")]
	public int PathID { get; set; }

	[Name("Path_Point_ID")]
	public int PathPointID { get; set; }

	[Name("Loct")]
	public DateTime Date { get; set; }
	
	[Name("Loct_Year")]
	public int Year { get; set; }

	[Name("Loct_Month")]
	public int Month { get; set; }

	[Name("Loct_Day")]
	public int Day { get; set; }

	[Name("Loct_Hour")]
	public int Hour { get; set; }

	[Name("Loct_Minute")]
	public int Minute { get; set; }

	[Name("Loct_Second")]
	public int Second { get; set; }

	[Name("Weekday")]
	public int Weekday { get; set; }

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
	public float DistanceToNextPoint { get; set; }

	[Name("Minutes_To_Next")]
	public float MinutesToNextPoint { get; set; }

	[Name("Minutes_From_Last")]
	public float MinutesFromLast { get; set; }

	[Name("Max_Temp_C")]
	public float MaxTemp { get; set; }

	[Name("Mean_Temp_C")]
	public float MeanTemp { get; set; }

	[Name("Total_Precip_mm")]
	public float TotalPrecip { get; set; }

	[Name("Snow_cm")]
	public int Snow { get; set; }

	[Name("Azimuth_Path")]
	public float AzimuthPath { get; set; }

	[Name("Azimuth_Segment")]
	public float AzimuthSegment { get; set; }

	[Name("Speed")]
	public float Speed { get; set; }

	[Ignore]
	public DateTime VerboseDate
	{
		get => Date;
		set
		{
			Date = value;
			Year = value.Year;
			Month = value.Month;
			Day = value.Day;
			Hour = value.Hour;
			Minute = value.Minute;
			Second = value.Second;
			Weekday = (int)value.DayOfWeek;
		}
	}

	#endregion

	public int CompareTo(PathOutput_Base other)
	{
		int val = PathID.CompareTo(other.PathID);

		if (val == 0)
			val = PathPointID.CompareTo(other.PathPointID);

		return val;
	}
}
